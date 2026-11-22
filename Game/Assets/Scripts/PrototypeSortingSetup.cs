using System;
using System.Collections.Generic;
using UnityEngine;

// Runtime-authored vertical slice: one shared definition per album and stable IDs per copy.
// The original scene CD and shelf are templates; there is no hand-placed stock duplication.
public sealed class PrototypeSortingSetup : MonoBehaviour
{
    [SerializeField] private PhysicalInteractable cdTemplate;
    [SerializeField] private Transform shelfTemplate;

    private static readonly AlbumSeed[] AdditionalAlbums =
    {
        new("aerial-broadcast-frequency", "Aerial Broadcast", "Frequency of Rain", "Rock"),
        new("autumn-engine-radiator", "Autumn Engine", "Radiator Hearts", "Rock"),
        new("brass-mercury-starlight", "Brass Mercury", "Starlight Motorway", "Rock"),
        new("clockwork-hearts-sunday", "Clockwork Hearts", "Sunday Static", "Rock"),
        new("circuit-breakers-colour", "Circuit Breakers", "Colour Television", "Rock"),
        new("echo-hours-afterglow", "Echo Hours", "Afterglow Avenue", "Rock"),
        new("electric-orchard-lights", "Electric Orchard", "Lights Below", "Rock"),
        new("faded-signals-tunnel", "Faded Signals", "Tunnel Vision", "Rock"),
        new("far-horizon-third", "Far Horizon", "The Third Exit", "Rock"),
        new("glass-harbour-weather", "Glass Harbour", "Bad Weather Radio", "Rock"),
        new("hush-parade-northern", "Hush Parade", "Northern Cities", "Rock"),
        new("iron-foxes-rivals", "Iron Foxes", "Rivals at Dawn", "Rock"),
        new("midnight-static-midway", "Midnight Static", "The Midway", "Rock"),
        new("northbound-parallel", "Northbound", "Parallel Lines", "Rock"),
        new("paper-satellites-silver", "Paper Satellites", "Silver Antennas", "Rock"),
        new("velvet-static-loose", "Velvet Static", "Loose Connections", "Rock"),
        new("amber-arcade-postcards", "Amber Arcade", "Postcards in June", "Indie"),
        new("birdwatchers-night", "Birdwatchers", "Night Windows", "Indie"),
        new("breakfast-tourist-morning", "Breakfast Tourist", "Morning Service", "Indie"),
        new("daylight-cinema-seaside", "Daylight Cinema", "Seaside Stories", "Indie"),
        new("eastbound-ghosts-stations", "Eastbound Ghosts", "Empty Stations", "Indie"),
        new("fern-house-mountains", "Fern House", "Mountains in Miniature", "Indie"),
        new("golden-coast-bicycles", "Golden Coast", "Bicycles and Rain", "Indie"),
        new("half-past-eleven-pink", "Half Past Eleven", "The Pink Hour", "Indie"),
        new("little-comets-ordinary", "Little Comets", "Ordinary Satellites", "Indie"),
        new("morning-forecast-portraits", "Morning Forecast", "Pocket Portraits", "Indie"),
        new("neon-postcards-harbour", "Neon Postcards", "Harbour Street", "Indie"),
        new("orchard-avenue-quiet", "Orchard Avenue", "A Quiet Departure", "Indie"),
        new("penny-atlas-cartography", "Penny Atlas", "Cartography Club", "Indie"),
        new("silver-lanterns-summer", "Silver Lanterns", "Summer in Reverse", "Indie"),
        new("summer-library-polaroids", "The Summer Library", "Polaroids", "Indie"),
        new("winter-registry-paper", "Winter Registry", "Paper Planes", "Indie")
    };

    // Five Rock bays along the back wall, six Indie bays on the side walls.
    // Yaw rotates the original back-wall shelf to face inward on side walls.
    private static readonly ShelfSeed[] Shelves =
    {
        new("rock-a-d", "Rock", "A", "D", -4.78f, 3.65f, 0f),
        new("rock-e-h", "Rock", "E", "H", -2.39f, 3.65f, 0f),
        new("rock-i-m", "Rock", "I", "M", 0f, 3.65f, 0f),
        new("rock-n-s", "Rock", "N", "S", 2.39f, 3.65f, 0f),
        new("rock-t-z", "Rock", "T", "Z", 4.78f, 3.65f, 0f),
        new("indie-a-d", "Indie", "A", "D", -6.4f, -3.4f, -90f),
        new("indie-e-h", "Indie", "E", "H", -6.4f, -1.05f, -90f),
        new("indie-i-m", "Indie", "I", "M", -6.4f, 1.3f, -90f),
        new("indie-n-r", "Indie", "N", "R", -6.4f, 3.65f, -90f),
        new("indie-s-v", "Indie", "S", "V", 6.4f, 0.45f, 90f),
        new("indie-w-z", "Indie", "W", "Z", 6.4f, 2.85f, 90f)
    };

    // Surface heights include half the thickness of a CD lying face-up.
    // The scene furniture is fixed: display table, two low bins, and a checkout.
    private static readonly PileSpot[] Piles =
    {
        new(-0.41f, 0.86f, 0.52f), new(0.41f, 0.86f, 0.52f),
        new(-0.41f, 0.86f, 0.82f), new(0.41f, 0.86f, 0.82f),
        new(-4.80f, 0.66f, 1.12f), new(-4.30f, 0.66f, 1.12f),
        new(-4.80f, 0.66f, 1.38f), new(-4.30f, 0.66f, 1.38f),
        new(4.20f, 0.66f, 0.75f), new(4.70f, 0.66f, 0.75f),
        new(4.20f, 0.66f, 1.00f), new(4.70f, 0.66f, 1.00f),
        new(4.15f, 0.96f, -2.65f), new(4.96f, 0.96f, -2.65f),
        new(-3.10f, 0.04f, -2.65f), new(-2.15f, 0.04f, -2.75f),
        new(-3.20f, 0.04f, -0.45f), new(-1.45f, 0.04f, -0.32f),
        new(1.75f, 0.04f, -0.45f), new(2.65f, 0.04f, -2.55f),
        new(2.40f, 0.04f, 1.40f), new(-2.75f, 0.04f, 1.55f),
        new(0.70f, 0.04f, 2.20f), new(-0.82f, 0.04f, 2.25f)
    };

    public static event Action SetupCompleted;
    public static bool HasCompletedSetup { get; private set; }

    private void Awake()
    {
        HasCompletedSetup = false;
        ResolveTemplates();

        if (cdTemplate == null || shelfTemplate == null)
        {
            Debug.LogError("Rock/Indie slice is missing its CD/shelf scene templates.");
            CompleteSetup();
            return;
        }

        Dictionary<string, AlbumDefinition> definitions = LoadAlbumDefinitions();
        for (int i = 0; i < AdditionalAlbums.Length; i++)
        {
            AlbumSeed seed = AdditionalAlbums[i];
            if (definitions.ContainsKey(seed.Id))
            {
                continue;
            }

            AlbumDefinition definition = ScriptableObject.CreateInstance<AlbumDefinition>();
            definition.name = seed.Id;
            definition.ConfigureRuntime(seed.Id, seed.Artist, seed.Title, seed.Genre);
            definitions.Add(seed.Id, definition);
        }

        List<AlbumDefinition> orderedCatalogue = new(definitions.Values);
        orderedCatalogue.Sort((a, b) => string.Compare(a.CatalogueId, b.CatalogueId, StringComparison.Ordinal));

        List<ShelfSlot> shelfSlots = CreateShelves();
        List<PhysicalInteractable> correctlyShelved = new();
        List<PhysicalInteractable> misfiled = new();
        int looseCount = 0;
        int physicalId = 0;

        for (int albumIndex = 0; albumIndex < orderedCatalogue.Count; albumIndex++)
        {
            AlbumDefinition definition = orderedCatalogue[albumIndex];
            // 2, 5, 4, 3 copies repeated: 40 titles -> 140 physical cases.
            int quantity = 2 + ((albumIndex * 7) % 4);

            for (int copy = 0; copy < quantity; copy++)
            {
                GameObject cd = physicalId == 0 ? cdTemplate.gameObject : Instantiate(cdTemplate.gameObject);
                physicalId++;
                string stableId = "slice-" + physicalId.ToString("D4");
                ConfigureCd(cd, definition, stableId);
                PhysicalInteractable interactable = cd.GetComponent<PhysicalInteractable>();

                if (physicalId % 7 == 0) // About 14% correctly shelved as clues.
                {
                    correctlyShelved.Add(interactable);
                }
                else if (physicalId % 8 == 0) // Some stock misfiled on the wrong shelves.
                {
                    misfiled.Add(interactable);
                }
                else
                {
                    PileSpot spot = Piles[looseCount % Piles.Length];
                    int layer = looseCount / Piles.Length;
                    float yaw = ((looseCount * 37) % 23) - 11;
                    cd.transform.SetPositionAndRotation(
                        new Vector3(spot.X, spot.Y + layer * 0.071f, spot.Z),
                        Quaternion.Euler(90f, yaw, 0f));
                    interactable.SettleLooseStock();
                    looseCount++;
                }
            }
        }

        // Build orderly reference stock before inserting deliberately incorrect items.
        for (int i = 0; i < correctlyShelved.Count; i++)
        {
            AlbumDefinition album = correctlyShelved[i].GetComponent<MediaItem>().AlbumDefinition;
            ShelfSlot destination = FindCorrectShelf(album, shelfSlots);
            if (destination == null || !destination.TryPlaceStartingItem(correctlyShelved[i]))
            {
                Debug.LogWarning("No space for reference stock: " + album.CatalogueId);
            }
        }

        for (int i = 0; i < misfiled.Count; i++)
        {
            AlbumDefinition album = misfiled[i].GetComponent<MediaItem>().AlbumDefinition;
            ShelfSlot incorrectShelf = FindIncorrectShelf(album, shelfSlots, i);
            if (incorrectShelf == null || !incorrectShelf.TryPlaceStartingItem(misfiled[i], true))
            {
                Debug.LogWarning("No space for misfiled stock: " + album.CatalogueId);
            }
        }

        if (FindAnyProgressHud() == null)
        {
            new GameObject("Organisation Progress").AddComponent<PrototypeOrganisationHud>();
        }

        Debug.Log("Rock/Indie slice: " + orderedCatalogue.Count + " albums, "
            + physicalId + " cases, " + shelfSlots.Count + " shelving bays. "
            + correctlyShelved.Count + " reference copies, "
            + misfiled.Count + " misfiled copies, " + looseCount + " loose.");
        CompleteSetup();
    }

    private static PrototypeOrganisationHud FindAnyProgressHud()
    {
        return FindAnyObjectByType<PrototypeOrganisationHud>();
    }

    private static void CompleteSetup()
    {
        HasCompletedSetup = true;
        SetupCompleted?.Invoke();
    }

    private void ResolveTemplates()
    {
        if (cdTemplate == null)
        {
            GameObject source = GameObject.Find("Prototype CD Case");
            cdTemplate = source != null ? source.GetComponent<PhysicalInteractable>() : null;
        }

        if (shelfTemplate == null)
        {
            GameObject source = GameObject.Find("ROCK A-G Shelf Section");
            shelfTemplate = source != null ? source.transform : null;
        }
    }

    private static Dictionary<string, AlbumDefinition> LoadAlbumDefinitions()
    {
        AlbumDefinition[] assets = Resources.LoadAll<AlbumDefinition>("Albums");
        Dictionary<string, AlbumDefinition> definitions = new();
        foreach (AlbumDefinition asset in assets)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.CatalogueId))
            {
                continue;
            }

            if (definitions.ContainsKey(asset.CatalogueId))
            {
                Debug.LogWarning("Duplicate catalogue ID: " + asset.CatalogueId);
                continue;
            }

            definitions.Add(asset.CatalogueId, asset);
        }

        return definitions;
    }

    private List<ShelfSlot> CreateShelves()
    {
        List<ShelfSlot> slots = new();
        for (int i = 0; i < Shelves.Length; i++)
        {
            Transform shelf = i == 0 ? shelfTemplate : Instantiate(shelfTemplate);
            ShelfSeed seed = Shelves[i];

            shelf.name = seed.Genre.ToUpperInvariant() + " " + seed.Start + "-" + seed.End + " Shelf Section";
            shelf.SetPositionAndRotation(new Vector3(seed.X, 0f, seed.Z), Quaternion.Euler(0f, seed.Yaw, 0f));

            ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
            if (slot == null)
            {
                Debug.LogError("Shelf template does not contain a ShelfSlot.");
                continue;
            }

            slot.name = "Placement Slot " + seed.Id;
            slot.Configure(seed.Id, seed.Genre, seed.Start, seed.End);
            ShelfSectionLabel label = shelf.GetComponent<ShelfSectionLabel>();
            if (label != null)
            {
                label.Configure(seed.Genre, seed.Start + "-" + seed.End);
            }

            slots.Add(slot);
        }

        return slots;
    }

    private static void ConfigureCd(GameObject cd, AlbumDefinition definition, string stableId)
    {
        cd.name = definition.ArtistName + " - " + definition.AlbumTitle + " [" + stableId + "]";
        MediaItem media = cd.GetComponent<MediaItem>();
        if (media == null)
        {
            media = cd.AddComponent<MediaItem>();
        }

        media.Configure(definition, stableId);
        if (cd.GetComponent<AlbumCoverArt>() == null)
        {
            cd.AddComponent<AlbumCoverArt>();
        }
    }

    private static ShelfSlot FindCorrectShelf(AlbumDefinition album, List<ShelfSlot> slots)
    {
        foreach (ShelfSlot slot in slots)
        {
            if (slot.CanAccept(album))
            {
                return slot;
            }
        }

        return null;
    }

    private static ShelfSlot FindIncorrectShelf(AlbumDefinition album, List<ShelfSlot> slots, int seed)
    {
        for (int offset = 0; offset < slots.Count; offset++)
        {
            ShelfSlot slot = slots[(seed * 3 + offset) % slots.Count];
            if (!slot.CanAccept(album) && slot.ItemCount < 18)
            {
                return slot;
            }
        }

        return null;
    }

    private readonly struct AlbumSeed
    {
        public readonly string Id;
        public readonly string Artist;
        public readonly string Title;
        public readonly string Genre;
        public AlbumSeed(string id, string artist, string title, string genre)
        {
            Id = id;
            Artist = artist;
            Title = title;
            Genre = genre;
        }
    }

    private readonly struct ShelfSeed
    {
        public readonly string Id;
        public readonly string Genre;
        public readonly string Start;
        public readonly string End;
        public readonly float X;
        public readonly float Z;
        public readonly float Yaw;
        public ShelfSeed(string id, string genre, string start, string end, float x, float z, float yaw)
        {
            Id = id; Genre = genre; Start = start; End = end;
            X = x; Z = z; Yaw = yaw;
        }
    }

    private readonly struct PileSpot
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        public PileSpot(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }
}
