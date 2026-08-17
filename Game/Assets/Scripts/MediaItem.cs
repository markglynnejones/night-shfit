using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicalInteractable))]
public sealed class MediaItem : MonoBehaviour
{
    [SerializeField] private AlbumDefinition albumDefinition = null;
    [SerializeField] private string physicalItemId = string.Empty;

    public AlbumDefinition AlbumDefinition => albumDefinition;
    public string PhysicalItemId => physicalItemId;

    public void Configure(AlbumDefinition newAlbumDefinition, string newPhysicalItemId)
    {
        albumDefinition = newAlbumDefinition;
        physicalItemId = newPhysicalItemId;
    }
}
