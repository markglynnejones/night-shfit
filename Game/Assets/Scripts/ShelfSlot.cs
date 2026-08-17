using UnityEngine;

public sealed class ShelfSlot : MonoBehaviour
{
    [SerializeField] private string acceptedGenre = "Rock";
    [SerializeField] private string acceptedSortKey = "B";
    [SerializeField] private Transform snapPoint = null;

    private PhysicalInteractable placedItem;

    public void Configure(string genre, string sortKey)
    {
        acceptedGenre = genre;
        acceptedSortKey = sortKey;
        placedItem = null;
    }

    public bool TryPlace(PhysicalInteractable item)
    {
        if (!CanAccept(item))
        {
            Debug.Log("That CD does not belong in this shelf slot.");
            return false;
        }

        placedItem = item;
        item.PlaceOnShelf(snapPoint != null ? snapPoint : transform, this);

        AlbumInfo albumInfo = item.GetComponent<AlbumInfo>();
        Debug.Log($"Placed {albumInfo.Artist} - {albumInfo.Album} in {acceptedGenre} / {acceptedSortKey}.");
        return true;
    }

    public void ClearIfHolding(PhysicalInteractable item)
    {
        if (placedItem == item)
        {
            placedItem = null;
        }
    }

    private bool CanAccept(PhysicalInteractable item)
    {
        if (placedItem != null || item == null)
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

    private static bool Matches(string actual, string expected)
    {
        return string.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase);
    }
}
