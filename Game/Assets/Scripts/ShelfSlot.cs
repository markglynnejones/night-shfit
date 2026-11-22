using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShelfSlot : MonoBehaviour
{
    [SerializeField] private string shelfSectionId = string.Empty;
    [SerializeField] private string acceptedGenre = "Rock";
    [SerializeField] private string artistRangeStart = "A";
    [SerializeField] private string artistRangeEnd = "G";
    [SerializeField] private Transform snapPoint = null;
    [SerializeField] private float itemSpacing = 0.11f;
    [SerializeField] private int maxItems = 18;

    // Seven stepped rows of nine face-out CDs, with four physical copies behind
    // each front-facing position. The shelf order remains a single persistent
    // list; the next layer fills only when the front layer is occupied.
    private static readonly Vector3 ShelvedEulerAngles = new(28f, 0f, 0f);
    public const int CoversPerTier = 9;
    public const int TierCount = 7;
    public const int MaxDepth = 4;
    public const int FacesPerLayer = CoversPerTier * TierCount;
    private const float FirstCoverX = -0.84f;
    private const float CoverHorizontalSpacing = 0.21f;
    private const float FirstTierZ = -0.47f;
    private const float TierDepthSpacing = 0.13f;
    private const float FirstTierHeight = 0.81f;
    private const float TierHeightSpacing = 0.085f;
    private const float DeepCaseOffsetZ = 0.026f;
    private const float DeepCaseOffsetY = 0.013f;

    private readonly List<PhysicalInteractable> placedItems = new();
    private GameObject placementHint;

    public string ShelfSectionId => shelfSectionId;
    public int ItemCount => placedItems.Count;
    public int Capacity => maxItems;
    public string AcceptedGenre => acceptedGenre;

    // O(n log n) longest nondecreasing ordered subsequence. Duplicates remain
    // valid adjacent stock. Checking 1,000+ cases must not stutter the HUD.
    public int CountCorrectlyOrganisedItems()
    {
        List<AlbumDefinition> tails = new();
        foreach (PhysicalInteractable item in placedItems)
        {
            AlbumDefinition album = GetAlbum(item);
            if (!CanAccept(album))
            {
                continue;
            }

            int low = 0;
            int high = tails.Count;
            while (low < high)
            {
                int middle = (low + high) / 2;
                if (CompareAlbumOrder(tails[middle], album) <= 0)
                {
                    low = middle + 1;
                }
                else
                {
                    high = middle;
                }
            }

            if (low == tails.Count)
            {
                tails.Add(album);
            }
            else
            {
                tails[low] = album;
            }
        }

        return tails.Count;
    }

    public bool IsFullyOrganised()
    {
        return CountCorrectlyOrganisedItems() == placedItems.Count;
    }

    public void Configure(string genre, string firstArtistInitial, string lastArtistInitial)
    {
        Configure(string.Empty, genre, firstArtistInitial, lastArtistInitial);
    }

    public void Configure(string persistentShelfSectionId, string genre, string firstArtistInitial, string lastArtistInitial)
    {
        shelfSectionId = persistentShelfSectionId;
        acceptedGenre = genre;
        artistRangeStart = firstArtistInitial;
        artistRangeEnd = lastArtistInitial;
        placedItems.Clear();
    }

    public void ConfigureJewelCaseSpacing()
    {
        itemSpacing = CoverHorizontalSpacing;
        maxItems = FacesPerLayer * MaxDepth;
    }

    public bool TryPlace(PhysicalInteractable item)
    {
        return TryPlace(item, transform.position);
    }

    public bool TryPlace(PhysicalInteractable item, Vector3 placementWorldPoint)
    {
        if (placedItems.Count >= maxItems)
        {
            Debug.Log("This shelf section is full.");
            return false;
        }

        if (!TryGetKnownAlbum(item, out AlbumDefinition albumDefinition))
        {
            return false;
        }

        // The player may misfile a CD. Correct order is feedback, not a placement rule.
        PlaceItem(item, InsertionIndexFor(placementWorldPoint));

        Debug.Log($"Shelved {albumDefinition.ArtistName} - {albumDefinition.AlbumTitle} in {acceptedGenre} / {ArtistRangeLabel()}.");
        PrototypeSaveLoadController.NotifyPersistentStateChanged();
        return true;
    }

    public bool TryPlaceStartingItem(PhysicalInteractable item, bool deliberatelyMisfiled = false)
    {
        if (placedItems.Count >= maxItems || !TryGetKnownAlbum(item, out _))
        {
            return false;
        }

        // Correct reference stock begins in sort order; misplaced items preserve authored chaos.
        PlaceItem(item, deliberatelyMisfiled ? placedItems.Count : SortedInsertionIndexFor(item));
        return true;
    }

    public bool CanAccept(AlbumDefinition albumDefinition)
    {
        return albumDefinition != null
            && Matches(albumDefinition.Genre, acceptedGenre)
            && IsArtistInRange(albumDefinition.ArtistName);
    }

    public void ShowPlacementHintFor(PhysicalInteractable item, Vector3 placementWorldPoint)
    {
        if (placedItems.Count >= maxItems || !TryGetKnownAlbum(item, out _))
        {
            HidePlacementHint();
            return;
        }

        int insertionIndex = InsertionIndexFor(placementWorldPoint);
        EnsurePlacementHint();
        Transform referenceTransform = snapPoint != null ? snapPoint : transform;
        Vector3 hintPosition = LocalOffsetForIndex(
            Mathf.Clamp(insertionIndex, 0, maxItems - 1));
        hintPosition.y += 0.013f;
        hintPosition.z -= 0.024f;

        placementHint.transform.SetParent(referenceTransform, false);
        placementHint.transform.localPosition = hintPosition;
        placementHint.transform.localRotation = Quaternion.Euler(ShelvedEulerAngles);
        placementHint.SetActive(true);
    }

    public void HidePlacementHint()
    {
        if (placementHint != null)
        {
            placementHint.SetActive(false);
        }
    }

    public void ClearIfHolding(PhysicalInteractable item)
    {
        if (placedItems.Remove(item))
        {
            ReflowPlacedItems();
        }
    }

    public int IndexOf(PhysicalInteractable item)
    {
        return placedItems.IndexOf(item);
    }

    public void ClearPlacedItemsForLoad()
    {
        placedItems.Clear();
        HidePlacementHint();
    }

    public bool RestorePlacedItem(PhysicalInteractable item)
    {
        if (item == null || placedItems.Contains(item) || placedItems.Count >= maxItems)
        {
            return false;
        }

        placedItems.Add(item);
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform, this, Vector3.zero, ShelvedEulerAngles);
        ReflowPlacedItems();
        return true;
    }

    private bool TryGetKnownAlbum(PhysicalInteractable item, out AlbumDefinition albumDefinition)
    {
        albumDefinition = null;
        if (item == null || placedItems.Contains(item))
        {
            return false;
        }

        MediaItem mediaItem = item.GetComponent<MediaItem>();
        if (mediaItem == null || mediaItem.AlbumDefinition == null)
        {
            return false;
        }

        albumDefinition = mediaItem.AlbumDefinition;
        return true;
    }

    private void PlaceItem(PhysicalInteractable item, int insertionIndex)
    {
        placedItems.Insert(Mathf.Clamp(insertionIndex, 0, placedItems.Count), item);
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform, this, Vector3.zero, ShelvedEulerAngles);
        ReflowPlacedItems();
    }

    private void ReflowPlacedItems()
    {
        for (int i = 0; i < placedItems.Count; i++)
        {
            placedItems[i].MoveWithinShelf(LocalOffsetForIndex(i));
        }
    }

    private Vector3 LocalOffsetForIndex(int index)
    {
        return LocalOffsetForIndex(index, placedItems.Count);
    }

    private Vector3 LocalOffsetForIndex(int index, int itemCount)
    {
        int slot = Mathf.Clamp(index, 0, maxItems - 1);
        int depth = slot / FacesPerLayer;
        int face = slot % FacesPerLayer;
        int tier = face / CoversPerTier;
        int column = face % CoversPerTier;
        return new Vector3(
            FirstCoverX + column * CoverHorizontalSpacing,
            FirstTierHeight + tier * TierHeightSpacing + depth * DeepCaseOffsetY,
            FirstTierZ + tier * TierDepthSpacing + depth * DeepCaseOffsetZ);
    }

    private void EnsurePlacementHint()
    {
        if (placementHint != null)
        {
            return;
        }

        placementHint = new GameObject("Placement Hint");
        CreateHintCube("Placement Hint Case Outline",
            new Vector3(0f, 0f, 0f), new Vector3(0.170f, 0.130f, 0.008f),
            CreateHintMaterial(new Color(0.54f, 0.78f, 0.76f, 1f)));
        placementHint.SetActive(false);
    }

    private void CreateHintCube(string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject hintPart = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hintPart.name = name;
        hintPart.transform.SetParent(placementHint.transform, false);
        hintPart.transform.localPosition = localPosition;
        hintPart.transform.localRotation = Quaternion.identity;
        hintPart.transform.localScale = localScale;

        Collider hintCollider = hintPart.GetComponent<Collider>();
        if (hintCollider != null)
        {
            Destroy(hintCollider);
        }

        MeshRenderer renderer = hintPart.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static Material CreateHintMaterial(Color hintColor)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        Material material = new(shader)
        {
            name = "Shelf Placement Hint"
        };

        material.SetColor("_BaseColor", hintColor);
        material.SetColor("_Color", hintColor);
        return material;
    }

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase);
    }

    private static AlbumDefinition GetAlbum(PhysicalInteractable item)
    {
        MediaItem media = item != null ? item.GetComponent<MediaItem>() : null;
        return media != null ? media.AlbumDefinition : null;
    }

    private int InsertionIndexFor(Vector3 placementWorldPoint)
    {
        if (placedItems.Count == 0)
        {
            return 0;
        }

        Transform referenceTransform = snapPoint != null ? snapPoint : transform;
        Vector3 local = referenceTransform.InverseTransformPoint(placementWorldPoint);
        int column = Mathf.Clamp(
            Mathf.RoundToInt((local.x - FirstCoverX) / CoverHorizontalSpacing),
            0, CoversPerTier - 1);
        // The non-solid tray collider is hit at its front face. Its Z is not
        // the case row, so use the aim height to choose one of seven tiers.
        int tier = Mathf.Clamp(
            Mathf.RoundToInt((local.y - FirstTierHeight) / TierHeightSpacing),
            0, TierCount - 1);

        // The player is choosing a real space within a browsing tray. An
        // insertion still preserves every other copy's authored order.
        return Mathf.Min(tier * CoversPerTier + column, placedItems.Count);
    }

    // Browse an individual four-deep column when empty-handed. Rotating the
    // visible first case to the back changes only that column and persists.
    // The face is always determined from the same aim location as insertion.
    public bool FlipCoverAt(Vector3 worldPoint, bool backwards = false)
    {
        if (placedItems.Count <= FacesPerLayer)
        {
            return false;
        }

        Transform reference = snapPoint != null ? snapPoint : transform;
        Vector3 local = reference.InverseTransformPoint(worldPoint);
        int column = Mathf.Clamp(
            Mathf.RoundToInt((local.x - FirstCoverX) / CoverHorizontalSpacing),
            0, CoversPerTier - 1);
        int tier = Mathf.Clamp(
            Mathf.RoundToInt((local.y - FirstTierHeight) / TierHeightSpacing),
            0, TierCount - 1);
        int face = tier * CoversPerTier + column;
        int count = 0;
        for (int layer = 0; layer < MaxDepth; layer++)
        {
            if (face + layer * FacesPerLayer >= placedItems.Count)
            {
                break;
            }

            count++;
        }

        if (count < 2)
        {
            return false;
        }

        int first = face;
        int last = face + (count - 1) * FacesPerLayer;
        PhysicalInteractable moving = placedItems[backwards ? last : first];

        if (backwards)
        {
            for (int layer = count - 1; layer > 0; layer--)
            {
                placedItems[face + layer * FacesPerLayer] =
                    placedItems[face + (layer - 1) * FacesPerLayer];
            }

            placedItems[first] = moving;
        }
        else
        {
            for (int layer = 0; layer < count - 1; layer++)
            {
                placedItems[face + layer * FacesPerLayer] =
                    placedItems[face + (layer + 1) * FacesPerLayer];
            }

            placedItems[last] = moving;
        }

        ReflowPlacedItems();
        PrototypeSaveLoadController.NotifyPersistentStateChanged();
        return true;
    }

    private int SortedInsertionIndexFor(PhysicalInteractable item)
    {
        for (int i = 0; i < placedItems.Count; i++)
        {
            if (CompareShelfOrder(item, placedItems[i]) < 0)
            {
                return i;
            }
        }

        return placedItems.Count;
    }

    private static int CompareShelfOrder(PhysicalInteractable left, PhysicalInteractable right)
    {
        MediaItem leftMediaItem = left != null ? left.GetComponent<MediaItem>() : null;
        MediaItem rightMediaItem = right != null ? right.GetComponent<MediaItem>() : null;
        AlbumDefinition leftAlbum = leftMediaItem != null ? leftMediaItem.AlbumDefinition : null;
        AlbumDefinition rightAlbum = rightMediaItem != null ? rightMediaItem.AlbumDefinition : null;
        return CompareAlbumOrder(leftAlbum, rightAlbum);
    }

    private static int CompareAlbumOrder(AlbumDefinition left, AlbumDefinition right)
    {
        string leftArtistSortName = left != null ? ArtistSortName(left.ArtistName) : string.Empty;
        string rightArtistSortName = right != null ? ArtistSortName(right.ArtistName) : string.Empty;

        int artistComparison = string.Compare(leftArtistSortName, rightArtistSortName, StringComparison.OrdinalIgnoreCase);
        if (artistComparison != 0)
        {
            return artistComparison;
        }

        string leftAlbumTitle = left != null ? left.AlbumTitle : string.Empty;
        string rightAlbumTitle = right != null ? right.AlbumTitle : string.Empty;
        return string.Compare(leftAlbumTitle, rightAlbumTitle, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsArtistInRange(string artistName)
    {
        char artistInitial = SortInitial(artistName);
        return artistInitial >= SortInitial(artistRangeStart) && artistInitial <= SortInitial(artistRangeEnd);
    }

    private string ArtistRangeLabel()
    {
        return $"{artistRangeStart.ToUpperInvariant()}-{artistRangeEnd.ToUpperInvariant()}";
    }

    private static char SortInitial(string artistName)
    {
        string sortName = ArtistSortName(artistName);
        return string.IsNullOrEmpty(sortName) ? '#' : char.ToUpperInvariant(sortName[0]);
    }

    private static string ArtistSortName(string artistName)
    {
        string trimmedName = artistName != null ? artistName.Trim() : string.Empty;
        const string leadingArticle = "The ";

        if (trimmedName.StartsWith(leadingArticle, StringComparison.OrdinalIgnoreCase))
        {
            return trimmedName[leadingArticle.Length..].TrimStart();
        }

        return trimmedName;
    }
}
