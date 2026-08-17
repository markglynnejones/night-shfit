using System.Collections.Generic;
using UnityEngine;

public sealed class PrototypeSortingSetup : MonoBehaviour
{
    [SerializeField] private PhysicalInteractable cdTemplate = null;
    [SerializeField] private Transform shelfTemplate = null;

    private readonly AlbumSpawn[] albums =
    {
        new("Blue Day", "International Clever Person", "Rock", "B", new Vector3(0f, 1.13f, 0.65f), -8f, false),
        new("Lincoln Gardens", "Hybrid Hypothesis", "Rock", "L", new Vector3(-4.5f, 0.31f, -1.6f), 28f, false),
        new("Tropical Apes", "Whatever People Think I Am", "Indie", "T", new Vector3(5.05f, 1.23f, -2.35f), -10f, false),
        new("Basement Signals", "Emergency Telephones", "Rock", "B", Vector3.zero, 0f, true),
        new("Late Night Chemistry", "Youth Club Rules", "Rock", "L", Vector3.zero, 0f, true),
        new("Tape Parade", "Songs for Taxi Queues", "Indie", "T", Vector3.zero, 0f, true),
        new("Northern Exit", "Last Train Home", "Indie", "N", Vector3.zero, 0f, true),
        new("Motorway Service Station", "Heartbreak at Junction 12", "Rock", "M", new Vector3(-4.55f, 0.93f, 1.25f), 12f, false),
        new("Neon Weekends", "Photobooth Alibis", "Indie", "N", new Vector3(4.45f, 0.93f, 0.85f), -32f, false),
        new("Paper Plan Committee", "Plans We Made At Midnight", "Indie", "P", new Vector3(-1.1f, 0.31f, -3.3f), 16f, false)
    };

    private readonly ShelfSpawn[] shelves =
    {
        new("Rock", "B", new Vector3(-5.2f, 0f, 3.65f)),
        new("Rock", "L", new Vector3(-2.6f, 0f, 3.65f)),
        new("Rock", "M", new Vector3(0f, 0f, 3.65f)),
        new("Indie", "N", new Vector3(2.6f, 0f, 3.65f)),
        new("Indie", "P", new Vector3(5.2f, 0f, 3.65f)),
        new("Indie", "T", new Vector3(0f, 0f, 2.15f))
    };

    private void Awake()
    {
        ResolveTemplates();

        if (cdTemplate == null || shelfTemplate == null)
        {
            Debug.LogWarning("Prototype sorting setup could not find the CD or shelf template.");
            return;
        }

        Dictionary<string, ShelfSlot> shelfSlots = new();
        ConfigureShelf(shelfTemplate, shelves[0]);
        AddShelfSlot(shelfSlots, shelfTemplate, shelves[0]);

        for (int i = 1; i < shelves.Length; i++)
        {
            Transform shelfInstance = Instantiate(shelfTemplate);
            ConfigureShelf(shelfInstance, shelves[i]);
            AddShelfSlot(shelfSlots, shelfInstance, shelves[i]);
        }

        ConfigureCd(cdTemplate.gameObject, albums[0], shelfSlots);
        for (int i = 1; i < albums.Length; i++)
        {
            GameObject cdInstance = Instantiate(cdTemplate.gameObject);
            ConfigureCd(cdInstance, albums[i], shelfSlots);
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

    private static void ConfigureCd(GameObject cdObject, AlbumSpawn album, Dictionary<string, ShelfSlot> shelfSlots)
    {
        cdObject.name = $"{album.Artist} CD Case";
        cdObject.transform.position = album.Position;
        cdObject.transform.rotation = Quaternion.Euler(0f, album.Yaw, 0f);

        AlbumInfo albumInfo = cdObject.GetComponent<AlbumInfo>();
        albumInfo.Configure(album.Artist, album.Album, album.Genre, album.SortKey);

        ShiftItem shiftItem = cdObject.GetComponent<ShiftItem>();
        if (shiftItem == null)
        {
            shiftItem = cdObject.AddComponent<ShiftItem>();
        }

        shiftItem.Configure(!album.StartsShelved, album.StartsShelved);
        ShiftManager.Instance?.RegisterItem(shiftItem);

        if (!album.StartsShelved)
        {
            return;
        }

        if (shelfSlots.TryGetValue(KeyFor(album.Genre, album.SortKey), out ShelfSlot shelfSlot))
        {
            shelfSlot.TryPlaceStartingItem(cdObject.GetComponent<PhysicalInteractable>());
        }
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

    private static void AddShelfSlot(Dictionary<string, ShelfSlot> shelfSlots, Transform shelf, ShelfSpawn shelfData)
    {
        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot == null)
        {
            return;
        }

        shelfSlots[KeyFor(shelfData.Genre, shelfData.SortKey)] = slot;
    }

    private static string KeyFor(string genre, string sortKey)
    {
        return $"{genre.ToUpperInvariant()}:{sortKey.ToUpperInvariant()}";
    }

    private readonly struct AlbumSpawn
    {
        public AlbumSpawn(string artist, string album, string genre, string sortKey, Vector3 position, float yaw, bool startsShelved)
        {
            Artist = artist;
            Album = album;
            Genre = genre;
            SortKey = sortKey;
            Position = position;
            Yaw = yaw;
            StartsShelved = startsShelved;
        }

        public string Artist { get; }
        public string Album { get; }
        public string Genre { get; }
        public string SortKey { get; }
        public Vector3 Position { get; }
        public float Yaw { get; }
        public bool StartsShelved { get; }
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
