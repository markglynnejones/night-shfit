using UnityEngine;

public sealed class ShelfSlot : MonoBehaviour
{
    [SerializeField] private string acceptedGenre = "Rock";
    [SerializeField] private string acceptedSortKey = "B";
    [SerializeField] private Transform snapPoint = null;
    [SerializeField] private float itemSpacing = 0.11f;
    [SerializeField] private int maxItems = 14;

    private static readonly Vector3 ShelvedEulerAngles = new(0f, -90f, 0f);
    private const float ShelfDepthOffset = 0.38f;

    private readonly System.Collections.Generic.List<PhysicalInteractable> placedItems = new();

    public void Configure(string genre, string sortKey)
    {
        acceptedGenre = genre;
        acceptedSortKey = sortKey;
        placedItems.Clear();
    }

    public bool TryPlace(PhysicalInteractable item)
    {
        if (placedItems.Count >= maxItems)
        {
            Debug.Log("This shelf section is full.");
            return false;
        }

        if (!CanAccept(item))
        {
            Debug.Log("That CD does not belong in this shelf slot.");
            return false;
        }

        PlaceItem(item);
        ShiftManager.Instance?.NotifyItemPlaced(item);

        AlbumDefinition albumDefinition = item.GetComponent<MediaItem>().AlbumDefinition;
        Debug.Log($"Placed {albumDefinition.ArtistName} - {albumDefinition.AlbumTitle} in {acceptedGenre} / {acceptedSortKey}.");
        return true;
    }

    public bool TryPlaceStartingItem(PhysicalInteractable item)
    {
        if (!CanAccept(item))
        {
            return false;
        }

        PlaceItem(item);
        return true;
    }

    public void ClearIfHolding(PhysicalInteractable item)
    {
        if (placedItems.Remove(item))
        {
            ReflowPlacedItems();
        }
    }

    private bool CanAccept(PhysicalInteractable item)
    {
        if (item == null || placedItems.Contains(item))
        {
            return false;
        }

        MediaItem mediaItem = item.GetComponent<MediaItem>();
        if (mediaItem == null || mediaItem.AlbumDefinition == null)
        {
            return false;
        }

        AlbumDefinition albumDefinition = mediaItem.AlbumDefinition;
        return Matches(albumDefinition.Genre, acceptedGenre) && Matches(albumDefinition.SortKey, acceptedSortKey);
    }

    private void PlaceItem(PhysicalInteractable item)
    {
        placedItems.Add(item);
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform, this, Vector3.zero, ShelvedEulerAngles);
        ReflowPlacedItems();
    }

    private void ReflowPlacedItems()
    {
        placedItems.Sort(CompareShelfOrder);

        for (int i = 0; i < placedItems.Count; i++)
        {
            placedItems[i].MoveWithinShelf(LocalOffsetForIndex(i));
        }
    }

    private Vector3 LocalOffsetForIndex(int index)
    {
        float centeredIndex = index - ((placedItems.Count - 1) * 0.5f);
        return new Vector3(centeredIndex * itemSpacing, 0f, ShelfDepthOffset);
    }

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareShelfOrder(PhysicalInteractable left, PhysicalInteractable right)
    {
        MediaItem leftMediaItem = left != null ? left.GetComponent<MediaItem>() : null;
        MediaItem rightMediaItem = right != null ? right.GetComponent<MediaItem>() : null;
        string leftCatalogueId = leftMediaItem != null && leftMediaItem.AlbumDefinition != null ? leftMediaItem.AlbumDefinition.CatalogueId : string.Empty;
        string rightCatalogueId = rightMediaItem != null && rightMediaItem.AlbumDefinition != null ? rightMediaItem.AlbumDefinition.CatalogueId : string.Empty;

        int catalogueComparison = string.Compare(leftCatalogueId, rightCatalogueId, System.StringComparison.OrdinalIgnoreCase);
        if (catalogueComparison != 0)
        {
            return catalogueComparison;
        }

        string leftPhysicalId = leftMediaItem != null ? leftMediaItem.PhysicalItemId : string.Empty;
        string rightPhysicalId = rightMediaItem != null ? rightMediaItem.PhysicalItemId : string.Empty;
        return string.Compare(leftPhysicalId, rightPhysicalId, System.StringComparison.OrdinalIgnoreCase);
    }
}
