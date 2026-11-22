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

    // Runtime entries share a definition across all physical copies. The prototype
    // authoring catalogue is deterministic, so these IDs survive save/reload.
    public void ConfigureRuntime(string id, string artist, string title, string category)
    {
        catalogueId = id;
        artistName = artist;
        albumTitle = title;
        genre = category;
        sortKey = artist + " / " + title;
    }
}
