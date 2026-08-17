using UnityEngine;

public sealed class AlbumInfo : MonoBehaviour
{
    [SerializeField] private string artist = "Blue Day";
    [SerializeField] private string album = "International Clever Person";
    [SerializeField] private string genre = "Rock";
    [SerializeField] private string sortKey = "B";

    public string Artist => artist;
    public string Album => album;
    public string Genre => genre;
    public string SortKey => sortKey;
}
