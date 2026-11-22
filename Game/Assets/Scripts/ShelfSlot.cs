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

    private static readonly Vector3 ShelvedEulerAngles = new(0f, -90f, 0f);
    private const float ShelfDepthOffset = 0.38f;
    private const float HintFrontZ = -0.12f;

    private readonly List<PhysicalInteractable> placedItems = new();
    private GameObject placementHint;

    public string ShelfSectionId => shelfSectionId;
    public int ItemCount => placedItems.Count;
    public string AcceptedGenre => acceptedGenre;

    // Longest ordered subsequence: stock that can remain while wrongly placed stock is removed.
    // This gives useful incremental progress without requiring an exact hard-coded slot.
    public int CountCorrectlyOrganisedItems()
    {
        int count = placedItems.Count;
        int[] best = new int[count];
        int longest = 0;

        for (int i = 0; i < count; i++)
        {
            AlbumDefinition current = GetAlbum(placedItems[i]);
            if (!CanAccept(current))
            {
                continue;
            }

            best[i] = 1;
            for (int j = 0; j < i; j++)
            {
                if (best[j] > 0 && CompareAlbumOrder(GetAlbum(placedItems[j]), current) <= 0)
                {
                    best[i] = Mathf.Max(best[i], best[j] + 1);
                }
            }

            longest = Mathf.Max(longest, best[i]);
        }

        return longest;
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
        Vector3 hintPosition = LocalOffsetForIndex(insertionIndex, placedItems.Count + 1);
        hintPosition.z = HintFrontZ;

        placementHint.transform.SetParent(referenceTransform, false);
        placementHint.transform.localPosition = hintPosition;
        placementHint.transform.localRotation = Quaternion.identity;
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
        float centeredIndex = index - ((itemCount - 1) * 0.5f);
        return new Vector3(centeredIndex * itemSpacing, 0f, ShelfDepthOffset);
    }

    private void EnsurePlacementHint()
    {
        if (placementHint != null)
        {
            return;
        }

        placementHint = new GameObject("Placement Hint");
        CreateHintCube("Placement Hint Spine", new Vector3(0f, 0f, 0f), new Vector3(0.09f, 0.76f, 0.055f), CreateHintMaterial(new Color(0.45f, 0.73f, 0.86f, 1f)));
        CreateHintCube("Placement Hint Top Tab", new Vector3(0f, 0.42f, -0.015f), new Vector3(0.22f, 0.065f, 0.1f), CreateHintMaterial(new Color(0.82f, 0.88f, 0.9f, 1f)));
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
        float localX = referenceTransform.InverseTransformPoint(placementWorldPoint).x;

        for (int i = 0; i < placedItems.Count; i++)
        {
            if (localX < LocalOffsetForIndex(i).x)
            {
                return i;
            }
        }

        return placedItems.Count;
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
