using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class PrototypeSortingSetup : MonoBehaviour
{
    [SerializeField] private PhysicalInteractable cdTemplate = null;
    [SerializeField] private Transform shelfTemplate = null;

    private readonly CopySpawn[] copies =
    {
        new("amuse-absolution-ish", "cd-amuse-01", Vector3.zero, 0f, true),
        new("black-afternoon-broken-television", "cd-black-afternoon-01", Vector3.zero, 0f, true),
        new("black-afternoon-broken-television", "cd-black-afternoon-02", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-clever-person-01", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-clever-person-02", Vector3.zero, 0f, true),
        new("blue-day-international-clever-person", "cd-blue-day-clever-person-03", new Vector3(0f, 1.13f, 0.65f), -8f, false),
        new("blue-day-warning-ish", "cd-blue-day-warning-01", Vector3.zero, 0f, true),
        new("blue-day-warning-ish", "cd-blue-day-warning-02", new Vector3(4.85f, 0.31f, -0.65f), -24f, false),
        new("grey-parade-the-grey-parade", "cd-grey-parade-01", Vector3.zero, 0f, true),
        new("grey-parade-the-grey-parade", "cd-grey-parade-02", Vector3.zero, 0f, true),
        new("grey-parade-the-grey-parade", "cd-grey-parade-04", new Vector3(-4.55f, 0.93f, 1.25f), 12f, false),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-01", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-02", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-03", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-04", Vector3.zero, 0f, true),
        new("lincoln-gardens-hybrid-hypothesis", "cd-lincoln-gardens-05", new Vector3(-4.5f, 0.31f, -1.6f), 28f, false),
        new("late-returns-platforms", "cd-late-returns-01", Vector3.zero, 0f, true),
        new("late-returns-platforms", "cd-late-returns-02", Vector3.zero, 0f, true),
        new("late-returns-platforms", "cd-late-returns-03", new Vector3(4.45f, 0.93f, 0.85f), -32f, false),
        new("tropical-apes-whatever-people-think-i-am", "cd-tropical-apes-01", Vector3.zero, 0f, true),
        new("tropical-apes-whatever-people-think-i-am", "cd-tropical-apes-02", new Vector3(5.05f, 1.23f, -2.35f), -10f, false)
    };

    private readonly ShelfSpawn[] shelves =
    {
        new("rock-a-g", "Rock", "A", "G", new Vector3(-4.6f, 0f, 3.65f)),
        new("rock-l-m", "Rock", "L", "M", new Vector3(0f, 0f, 3.65f)),
        new("indie-t-t", "Indie", "T", "T", new Vector3(4.6f, 0f, 3.65f))
    };

    public static event Action SetupCompleted;
    public static bool HasCompletedSetup { get; private set; }

    private void Awake()
    {
        HasCompletedSetup = false;
        ResolveTemplates();

        if (cdTemplate == null || shelfTemplate == null)
        {
            Debug.LogWarning("Prototype sorting setup could not find the CD or shelf template.");
            MarkSetupCompleted();
            return;
        }

        Dictionary<string, AlbumDefinition> albumDefinitions = LoadAlbumDefinitions();
        List<ShelfSlot> shelfSlots = new();
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

        MarkSetupCompleted();
    }

    private static void MarkSetupCompleted()
    {
        HasCompletedSetup = true;
        SetupCompleted?.Invoke();
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
            GameObject shelfObject = GameObject.Find("ROCK A-G Shelf Section");
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
        List<ShelfSlot> shelfSlots)
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

        if (!copy.StartsShelved)
        {
            return;
        }

        ShelfSlot shelfSlot = FindShelfSlotFor(albumDefinition, shelfSlots);
        if (shelfSlot != null)
        {
            shelfSlot.TryPlaceStartingItem(cdObject.GetComponent<PhysicalInteractable>());
        }
    }

    private static void ConfigureShelf(Transform shelf, ShelfSpawn shelfData)
    {
        shelf.name = $"{shelfData.Genre.ToUpperInvariant()} {shelfData.ArtistRangeLabel.ToUpperInvariant()} Shelf Section";
        shelf.position = shelfData.Position;

        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot != null)
        {
            slot.name = $"{shelfData.Genre.ToUpperInvariant()} {shelfData.ArtistRangeLabel.ToUpperInvariant()} Placement Slot";
            slot.Configure(shelfData.ShelfId, shelfData.Genre, shelfData.ArtistRangeStart, shelfData.ArtistRangeEnd);
        }

        ShelfSectionLabel label = shelf.GetComponent<ShelfSectionLabel>();
        if (label != null)
        {
            label.Configure(shelfData.Genre, shelfData.ArtistRangeLabel);
        }
    }

    private static void AddShelfSlot(List<ShelfSlot> shelfSlots, Transform shelf, ShelfSpawn shelfData)
    {
        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot == null)
        {
            return;
        }

        shelfSlots.Add(slot);
    }

    private static ShelfSlot FindShelfSlotFor(AlbumDefinition albumDefinition, List<ShelfSlot> shelfSlots)
    {
        for (int i = 0; i < shelfSlots.Count; i++)
        {
            ShelfSlot shelfSlot = shelfSlots[i];
            if (shelfSlot != null && shelfSlot.CanAccept(albumDefinition))
            {
                return shelfSlot;
            }
        }

        return null;
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
        public ShelfSpawn(string shelfId, string genre, string artistRangeStart, string artistRangeEnd, Vector3 position)
        {
            ShelfId = shelfId;
            Genre = genre;
            ArtistRangeStart = artistRangeStart;
            ArtistRangeEnd = artistRangeEnd;
            Position = position;
        }

        public string ShelfId { get; }
        public string Genre { get; }
        public string ArtistRangeStart { get; }
        public string ArtistRangeEnd { get; }
        public string ArtistRangeLabel => $"{ArtistRangeStart}-{ArtistRangeEnd}";
        public Vector3 Position { get; }
    }
}
