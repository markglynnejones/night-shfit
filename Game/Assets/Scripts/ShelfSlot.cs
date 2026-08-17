using UnityEngine;

public sealed class ShelfSlot : MonoBehaviour
{
    [SerializeField] private string acceptedGenre = "Rock";
    [SerializeField] private string acceptedSortKey = "B";
    [SerializeField] private Transform snapPoint = null;
    [SerializeField] private float itemSpacing = 0.12f;

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
        if (!CanAccept(item))
        {
            Debug.Log("That CD does not belong in this shelf slot.");
            return false;
        }

        PlaceItem(item);
        ShiftManager.Instance?.NotifyItemPlaced(item);

        AlbumInfo albumInfo = item.GetComponent<AlbumInfo>();
        Debug.Log($"Placed {albumInfo.Artist} - {albumInfo.Album} in {acceptedGenre} / {acceptedSortKey}.");
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

        AlbumInfo albumInfo = item.GetComponent<AlbumInfo>();
        if (albumInfo == null)
        {
            return false;
        }

        return Matches(albumInfo.Genre, acceptedGenre) && Matches(albumInfo.SortKey, acceptedSortKey);
    }

    private void PlaceItem(PhysicalInteractable item)
    {
        placedItems.Add(item);
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
        float centeredIndex = index - ((placedItems.Count - 1) * 0.5f);
        return new Vector3(centeredIndex * itemSpacing, 0f, ShelfDepthOffset);
    }

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase);
    }
}
