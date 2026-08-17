using UnityEngine;

[CreateAssetMenu(fileName = "AlbumDefinition", menuName = "Night Shift/Album Definition")]
public sealed class AlbumDefinition : ScriptableObject
{
    [SerializeField] private string catalogueId = string.Empty;
    [SerializeField] private string artistName = string.Empty;
    [SerializeField] private string albumTitle = string.Empty;
    [SerializeField] private string genre = string.Empty;
    [SerializeField] private string sortKey = string.Empty;
    [SerializeField] private Sprite coverArtwork = null;

    public string CatalogueId => catalogueId;
    public string ArtistName => artistName;
    public string AlbumTitle => albumTitle;
    public string Genre => genre;
    public string SortKey => sortKey;
    public Sprite CoverArtwork => coverArtwork;
}
