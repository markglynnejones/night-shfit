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
    // Each CD owns a fixed physical position (face + 63 * depth). Removing
    // one case compacts only its own four-deep column, never adjacent rows.
    private readonly Dictionary<PhysicalInteractable, int> physicalSlots = new();
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
        physicalSlots.Clear();
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
        int face = FaceIndexForPoint(placementWorldPoint);
        if (placedItems.Count >= maxItems || CountInFace(face) >= MaxDepth)
        {
            Debug.Log("This CD facing is full. Choose another position.");
            return false;
        }

        if (!TryGetKnownAlbum(item, out AlbumDefinition albumDefinition))
        {
            return false;
        }

        // The selected CD becomes the new front cover. Only cases already in
        // this one four-deep stack move back by one physical position.
        foreach (PhysicalInteractable other in placedItems)
        {
            int slot = physicalSlots[other];
            if (slot % FacesPerLayer == face)
            {
                physicalSlots[other] = slot + FacesPerLayer;
            }
        }

        placedItems.Add(item);
        physicalSlots[item] = face;
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform,
            this, Vector3.zero, ShelvedEulerAngles);
        ReflowPlacedItems();

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

        // Scene creation happens before gameplay. Build the reference fixture
        // in the legacy layer-first order, then keep every position stable
        // once the player starts touching individual stacks.
        placedItems.Insert(deliberatelyMisfiled
            ? placedItems.Count : SortedInsertionIndexFor(item), item);
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform,
            this, Vector3.zero, ShelvedEulerAngles);
        for (int i = 0; i < placedItems.Count; i++)
        {
            physicalSlots[placedItems[i]] = i;
        }

        ReflowPlacedItems();
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
        int facing = FaceIndexForPoint(placementWorldPoint);
        if (placedItems.Count >= maxItems || CountInFace(facing) >= MaxDepth
            || !TryGetKnownAlbum(item, out _))
        {
            HidePlacementHint();
            return;
        }

        int insertionIndex = facing;
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
        if (!physicalSlots.TryGetValue(item, out int removedSlot))
        {
            return;
        }

        placedItems.Remove(item);
        physicalSlots.Remove(item);

        // A gap is filled by the CDs physically behind this particular face.
        // No neighbouring cover, row or shelf may move.
        int face = removedSlot % FacesPerLayer;
        foreach (PhysicalInteractable other in placedItems)
        {
            int slot = physicalSlots[other];
            if (slot % FacesPerLayer == face && slot > removedSlot)
            {
                physicalSlots[other] = slot - FacesPerLayer;
            }
        }

        ReflowPlacedItems();
    }

    public int IndexOf(PhysicalInteractable item)
    {
        return placedItems.IndexOf(item);
    }

    public int PhysicalSlotOf(PhysicalInteractable item)
    {
        return item != null && physicalSlots.TryGetValue(item, out int slot)
            ? slot : -1;
    }

    public void ClearPlacedItemsForLoad()
    {
        placedItems.Clear();
        physicalSlots.Clear();
        HidePlacementHint();
    }

    // physicalSlot is optional for backwards-compatible v2 saves. For older
    // saves, pass the historical shelfOrder (layer-first flat index).
    public bool RestorePlacedItem(PhysicalInteractable item, int physicalSlot = -1)
    {
        if (item == null || placedItems.Contains(item) || placedItems.Count >= maxItems)
        {
            return false;
        }

        int slot = physicalSlot >= 0 && physicalSlot < maxItems
            && !physicalSlots.ContainsValue(physicalSlot)
                ? physicalSlot : FirstFreePhysicalSlot();
        if (slot < 0)
        {
            return false;
        }

        placedItems.Add(item);
        physicalSlots[item] = slot;
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform,
            this, Vector3.zero, ShelvedEulerAngles);
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

    private void ReflowPlacedItems()
    {
        // This list is also our serialized shelf-order. Keep it in physical
        // facing/depth order so saved order and quiet progress agree with view.
        placedItems.Sort((left, right) =>
            physicalSlots[left].CompareTo(physicalSlots[right]));
        foreach (PhysicalInteractable item in placedItems)
        {
            item.MoveWithinShelf(LocalOffsetForIndex(physicalSlots[item]));
        }
    }

    private int FirstFreePhysicalSlot()
    {
        for (int slot = 0; slot < maxItems; slot++)
        {
            if (!physicalSlots.ContainsValue(slot))
            {
                return slot;
            }
        }

        return -1;
    }

    private int CountInFace(int face)
    {
        int count = 0;
        foreach (int slot in physicalSlots.Values)
        {
            if (slot % FacesPerLayer == face)
            {
                count++;
            }
        }

        return count;
    }

    private PhysicalInteractable ItemAtSlot(int physicalSlot)
    {
        foreach (PhysicalInteractable item in placedItems)
        {
            if (physicalSlots[item] == physicalSlot)
            {
                return item;
            }
        }

        return null;
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

    private int FaceIndexForPoint(Vector3 placementWorldPoint)
    {
        Transform reference = snapPoint != null ? snapPoint : transform;
        Vector3 local = reference.InverseTransformPoint(placementWorldPoint);
        int column = Mathf.Clamp(
            Mathf.RoundToInt((local.x - FirstCoverX) / CoverHorizontalSpacing),
            0, CoversPerTier - 1);
        int tier = Mathf.Clamp(
            Mathf.RoundToInt((local.y - FirstTierHeight) / TierHeightSpacing),
            0, TierCount - 1);
        return tier * CoversPerTier + column;
    }

    // Test the ACTUAL front-cover rectangle instead of the distance to an
    // approximate face centre. Seven rows are close together, so the former
    // 16cm-radius centre test could steal aim from neighbouring rows.
    // Pick the first visible cover plane crossed by the centre-screen ray.
    public bool TryGetAimedFacePoint(Ray sightLine, float maxDistance,
        out Vector3 worldFacingPoint)
    {
        worldFacingPoint = Vector3.zero;
        Transform reference = snapPoint != null ? snapPoint : transform;
        Quaternion caseRotation = reference.rotation * Quaternion.Euler(ShelvedEulerAngles);
        Quaternion inverseCaseRotation = Quaternion.Inverse(caseRotation);
        Vector3 coverNormal = caseRotation * Vector3.back;
        float denominator = Vector3.Dot(sightLine.direction, coverNormal);

        // Covers are browsed from the front of the rack, not through its back.
        if (denominator >= -0.05f)
        {
            return false;
        }

        // The case is roughly 17.2 x 13.6 cm. The small border allowance
        // makes aiming at realistic-sized cases usable without crossing into
        // the next row (unlike the previous 16cm spherical tolerance).
        const float halfCoverWidth = 0.103f;
        const float halfCoverHeight = 0.076f;
        const float coverFrontOffset = 0.008f;
        float nearestHitDistance = float.MaxValue;
        int bestFace = -1;

        for (int face = 0; face < FacesPerLayer; face++)
        {
            Vector3 centre = reference.TransformPoint(LocalOffsetForIndex(face));
            Vector3 frontPlaneCentre = centre + coverNormal * coverFrontOffset;
            float hitDistance = Vector3.Dot(
                frontPlaneCentre - sightLine.origin, coverNormal) / denominator;
            if (hitDistance <= 0.12f || hitDistance >= nearestHitDistance
                || hitDistance > maxDistance)
            {
                continue;
            }

            Vector3 pointOnCover = sightLine.GetPoint(hitDistance);
            Vector3 localOnCover = inverseCaseRotation
                * (pointOnCover - frontPlaneCentre);
            if (Mathf.Abs(localOnCover.x) > halfCoverWidth
                || Mathf.Abs(localOnCover.y) > halfCoverHeight)
            {
                continue;
            }

            nearestHitDistance = hitDistance;
            bestFace = face;
        }

        if (bestFace < 0)
        {
            return false;
        }

        worldFacingPoint = reference.TransformPoint(LocalOffsetForIndex(bestFace));
        return true;
    }

    // Share the same exact face mapping with pickup prompts and in-Editor tests.
    public Vector3 GetWorldFacingCentre(int row, int column)
    {
        int face = Mathf.Clamp(row, 0, TierCount - 1) * CoversPerTier
            + Mathf.Clamp(column, 0, CoversPerTier - 1);
        Transform reference = snapPoint != null ? snapPoint : transform;
        return reference.TransformPoint(LocalOffsetForIndex(face));
    }

    public int RowNumberForPoint(Vector3 worldFacingPoint)
    {
        return FaceIndexForPoint(worldFacingPoint) / CoversPerTier + 1;
    }

    public int ColumnNumberForPoint(Vector3 worldFacingPoint)
    {
        return FaceIndexForPoint(worldFacingPoint) % CoversPerTier + 1;
    }

    public int CountAtFacing(Vector3 worldFacingPoint)
    {
        return CountInFace(FaceIndexForPoint(worldFacingPoint));
    }

    // Let the interaction UI identify the front-facing CD and show how
    // many distinct physical copies are behind that cover. This read-only
    // query never alters the authored ordering or persistent state.
    public bool TryGetFrontCaseAt(Vector3 worldPoint,
        out PhysicalInteractable front, out int countInColumn)
    {
        int face = FaceIndexForPoint(worldPoint);
        front = ItemAtSlot(face);
        countInColumn = CountInFace(face);
        return front != null;
    }

    // Rotate only the selected vertical four-deep column. Other columns keep
    // their exact physical position, and changed positions are serialized.
    public bool FlipCoverAt(Vector3 worldPoint, bool backwards = false)
    {
        int face = FaceIndexForPoint(worldPoint);
        int count = CountInFace(face);
        if (count < 2)
        {
            return false;
        }

        PhysicalInteractable[] stack = new PhysicalInteractable[count];
        for (int depth = 0; depth < count; depth++)
        {
            stack[depth] = ItemAtSlot(face + depth * FacesPerLayer);
            if (stack[depth] == null)
            {
                return false; // protect a malformed legacy stack
            }
        }

        for (int depth = 0; depth < count; depth++)
        {
            int source = backwards
                ? (depth + count - 1) % count
                : (depth + 1) % count;
            physicalSlots[stack[source]] = face + depth * FacesPerLayer;
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
