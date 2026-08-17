using System.Collections.Generic;
using UnityEngine;

public sealed class PrototypeSortingSetup : MonoBehaviour
{
    [SerializeField] private PhysicalInteractable cdTemplate = null;
    [SerializeField] private Transform shelfTemplate = null;

    private readonly CopySpawn[] copies =
    {
        new("blue-day-international-clever-person", "cd-blue-day-01", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-02", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-03", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-04", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-05", new Vector3(0f, 1.13f, 0.65f), -8f, false),
        new("basement-signals-emergency-telephones", "cd-basement-signals-01", Vector3.zero, 0f, true),
        new("basement-signals-emergency-telephones", "cd-basement-signals-02", Vector3.zero, 0f, true),
        new("basement-signals-emergency-telephones", "cd-basement-signals-03", Vector3.zero, 0f, true),
        new("basement-signals-emergency-telephones", "cd-basement-signals-04", new Vector3(4.85f, 0.31f, -0.65f), -24f, false),
        new("grey-parade-the-grey-parade", "cd-grey-parade-01", Vector3.zero, 0f, true),
        new("grey-parade-the-grey-parade", "cd-grey-parade-02", Vector3.zero, 0f, true),
        new("grey-parade-the-grey-parade", "cd-grey-parade-03", Vector3.zero, 0f, true),
        new("grey-parade-the-grey-parade", "cd-grey-parade-04", new Vector3(-4.55f, 0.93f, 1.25f), 12f, false),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-01", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-02", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-03", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-04", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-05", new Vector3(-4.5f, 0.31f, -1.6f), 28f, false),
        new("late-returns-platforms", "cd-late-returns-01", Vector3.zero, 0f, true),
        new("late-returns-platforms", "cd-late-returns-02", Vector3.zero, 0f, true),
        new("late-returns-platforms", "cd-late-returns-03", new Vector3(4.45f, 0.93f, 0.85f), -32f, false),
        new("motorway-service-station-heartbreak-at-junction-12", "cd-motorway-service-station-01", Vector3.zero, 0f, true),
        new("motorway-service-station-heartbreak-at-junction-12", "cd-motorway-service-station-02", Vector3.zero, 0f, true),
        new("motorway-service-station-heartbreak-at-junction-12", "cd-motorway-service-station-03", Vector3.zero, 0f, true),
        new("motorway-service-station-heartbreak-at-junction-12", "cd-motorway-service-station-04", new Vector3(1.15f, 0.31f, -3.75f), -18f, false),
        new("tropical-apes-whatever-people-think-i-am", "cd-tropical-apes-01", Vector3.zero, 0f, true),
        new("tropical-apes-whatever-people-think-i-am", "cd-tropical-apes-02", new Vector3(5.05f, 1.23f, -2.35f), -10f, false),
        new("amuse-absolution-ish", "cd-amuse-01", Vector3.zero, 0f, true),
        new("amuse-absolution-ish", "cd-amuse-02", Vector3.zero, 0f, true),
        new("amuse-absolution-ish", "cd-amuse-03", Vector3.zero, 0f, true)
    };

    private readonly ShelfSpawn[] shelves =
    {
        new("Rock", "A", new Vector3(-5.75f, 0f, 3.65f)),
        new("Rock", "B", new Vector3(-3.45f, 0f, 3.65f)),
        new("Rock", "G", new Vector3(-1.15f, 0f, 3.65f)),
        new("Rock", "L", new Vector3(1.15f, 0f, 3.65f)),
        new("Rock", "M", new Vector3(3.45f, 0f, 3.65f)),
        new("Indie", "T", new Vector3(5.75f, 0f, 3.65f))
    };

    private void Awake()
    {
        ResolveTemplates();

        if (cdTemplate == null || shelfTemplate == null)
        {
            Debug.LogWarning("Prototype sorting setup could not find the CD or shelf template.");
            return;
        }

        Dictionary<string, AlbumDefinition> albumDefinitions = LoadAlbumDefinitions();
        Dictionary<string, ShelfSlot> shelfSlots = new();
        ConfigureShelf(shelfTemplate, shelves[0]);
        AddShelfSlot(shelfSlots, shelfTemplate, shelves[0]);

        for (int i = 1; i < shelves.Length; i++)
        {
            Transform shelfInstance = Instantiate(shelfTemplate);
            ConfigureShelf(shelfInstance, shelves[i]);
            AddShelfSlot(shelfSlots, shelfInstance, shelves[i]);
        }

        ConfigureCd(cdTemplate.gameObject, copies[0], albumDefinitions, shelfSlots);
        for (int i = 1; i < copies.Length; i++)
        {
            GameObject cdInstance = Instantiate(cdTemplate.gameObject);
            ConfigureCd(cdInstance, copies[i], albumDefinitions, shelfSlots);
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

    private static Dictionary<string, AlbumDefinition> LoadAlbumDefinitions()
    {
        AlbumDefinition[] definitions = Resources.LoadAll<AlbumDefinition>("Albums");
        Dictionary<string, AlbumDefinition> definitionsById = new();

        for (int i = 0; i < definitions.Length; i++)
        {
            AlbumDefinition definition = definitions[i];
            if (definition == null || string.IsNullOrWhiteSpace(definition.CatalogueId))
            {
                continue;
            }

            definitionsById[definition.CatalogueId] = definition;
        }

        return definitionsById;
    }

    private static void ConfigureCd(
        GameObject cdObject,
        CopySpawn copy,
        Dictionary<string, AlbumDefinition> albumDefinitions,
        Dictionary<string, ShelfSlot> shelfSlots)
    {
        if (!albumDefinitions.TryGetValue(copy.CatalogueId, out AlbumDefinition albumDefinition))
        {
            Debug.LogWarning($"Missing album definition for catalogue ID '{copy.CatalogueId}'.");
            cdObject.SetActive(false);
            return;
        }

        cdObject.name = $"{albumDefinition.ArtistName} CD Case ({copy.PhysicalItemId})";
        cdObject.transform.position = copy.Position;
        cdObject.transform.rotation = Quaternion.Euler(0f, copy.Yaw, 0f);

        MediaItem mediaItem = cdObject.GetComponent<MediaItem>();
        if (mediaItem == null)
        {
            mediaItem = cdObject.AddComponent<MediaItem>();
        }

        mediaItem.Configure(albumDefinition, copy.PhysicalItemId);

        ShiftItem shiftItem = cdObject.GetComponent<ShiftItem>();
        if (shiftItem == null)
        {
            shiftItem = cdObject.AddComponent<ShiftItem>();
        }

        shiftItem.Configure(!copy.StartsShelved, copy.StartsShelved);
        ShiftManager.Instance?.RegisterItem(shiftItem);

        if (!copy.StartsShelved)
        {
            return;
        }

        if (shelfSlots.TryGetValue(KeyFor(albumDefinition.Genre, albumDefinition.SortKey), out ShelfSlot shelfSlot))
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

    private readonly struct CopySpawn
    {
        public CopySpawn(string catalogueId, string physicalItemId, Vector3 position, float yaw, bool startsShelved)
        {
            CatalogueId = catalogueId;
            PhysicalItemId = physicalItemId;
            Position = position;
            Yaw = yaw;
            StartsShelved = startsShelved;
        }

        public string CatalogueId { get; }
        public string PhysicalItemId { get; }
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
