using UnityEngine;

public sealed class PrototypeSortingSetup : MonoBehaviour
{
    [SerializeField] private PhysicalInteractable cdTemplate = null;
    [SerializeField] private Transform shelfTemplate = null;

    private readonly AlbumSpawn[] albums =
    {
        new("Blue Day", "International Clever Person", "Rock", "B", new Vector3(0f, 1.13f, 0.65f), -8f),
        new("Lincoln Gardens", "Hybrid Hypothesis", "Rock", "L", new Vector3(-4.5f, 0.31f, -1.6f), 28f),
        new("Tropical Apes", "Whatever People Think I Am", "Indie", "T", new Vector3(4.6f, 1.23f, -2.6f), -18f)
    };

    private readonly ShelfSpawn[] shelves =
    {
        new("Rock", "B", new Vector3(-2.55f, 0f, 3.65f)),
        new("Rock", "L", new Vector3(0f, 0f, 3.65f)),
        new("Indie", "T", new Vector3(2.55f, 0f, 3.65f))
    };

    private void Awake()
    {
        ResolveTemplates();

        if (cdTemplate == null || shelfTemplate == null)
        {
            Debug.LogWarning("Prototype sorting setup could not find the CD or shelf template.");
            return;
        }

        ConfigureCd(cdTemplate.gameObject, albums[0]);
        for (int i = 1; i < albums.Length; i++)
        {
            GameObject cdInstance = Instantiate(cdTemplate.gameObject);
            ConfigureCd(cdInstance, albums[i]);
        }

        ConfigureShelf(shelfTemplate, shelves[0]);
        for (int i = 1; i < shelves.Length; i++)
        {
            Transform shelfInstance = Instantiate(shelfTemplate);
            ConfigureShelf(shelfInstance, shelves[i]);
        }
    }

    private void ResolveTemplates()
    {
        if (cdTemplate == null)
        {
            GameObject cdObject = GameObject.Find("Prototype CD Case");
            cdTemplate = cdObject != null ? cdObject.GetComponent<PhysicalInteractable>() : null;
        }

        if (shelfTemplate == null)
        {
            GameObject shelfObject = GameObject.Find("ROCK B Shelf Section");
            shelfTemplate = shelfObject != null ? shelfObject.transform : null;
        }
    }

    private static void ConfigureCd(GameObject cdObject, AlbumSpawn album)
    {
        cdObject.name = $"{album.Artist} CD Case";
        cdObject.transform.position = album.Position;
        cdObject.transform.rotation = Quaternion.Euler(0f, album.Yaw, 0f);

        AlbumInfo albumInfo = cdObject.GetComponent<AlbumInfo>();
        albumInfo.Configure(album.Artist, album.Album, album.Genre, album.SortKey);
    }

    private static void ConfigureShelf(Transform shelf, ShelfSpawn shelfData)
    {
        shelf.name = $"{shelfData.Genre.ToUpperInvariant()} {shelfData.SortKey.ToUpperInvariant()} Shelf Section";
        shelf.position = shelfData.Position;

        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot != null)
        {
            slot.name = $"{shelfData.Genre.ToUpperInvariant()} {shelfData.SortKey.ToUpperInvariant()} Placement Slot";
            slot.Configure(shelfData.Genre, shelfData.SortKey);
        }

        ShelfSectionLabel label = shelf.GetComponent<ShelfSectionLabel>();
        if (label != null)
        {
            label.Configure(shelfData.Genre, shelfData.SortKey);
        }
    }

    private readonly struct AlbumSpawn
    {
        public AlbumSpawn(string artist, string album, string genre, string sortKey, Vector3 position, float yaw)
        {
            Artist = artist;
            Album = album;
            Genre = genre;
            SortKey = sortKey;
            Position = position;
            Yaw = yaw;
        }

        public string Artist { get; }
        public string Album { get; }
        public string Genre { get; }
        public string SortKey { get; }
        public Vector3 Position { get; }
        public float Yaw { get; }
    }

    private readonly struct ShelfSpawn
    {
        public ShelfSpawn(string genre, string sortKey, Vector3 position)
        {
            Genre = genre;
            SortKey = sortKey;
            Position = position;
        }

        public string Genre { get; }
        public string SortKey { get; }
        public Vector3 Position { get; }
    }
}
