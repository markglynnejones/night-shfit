using UnityEngine;

public sealed class PrototypeStoreLayout : MonoBehaviour
{
    private Material wallMaterial;
    private Material fixtureMaterial;
    private Material counterMaterial;
    private Material signageMaterial;
    private Material shutterMaterial;
    private Material doorMaterial;
    private Material floorMaterial;

    private static Material shelfFrameMaterial;
    private static Material shelfBackMaterial;
    private static Material rockHeaderMaterial;
    private static Material indieHeaderMaterial;
    private static Material shelfRangeMaterial;

    private void Awake()
    {
        CreateMaterials();
        ApplyExistingRoomMaterials();
        AdjustExistingDisplayTable();
        RemoveOldMovementMarker();
        CreateCheckoutCounter();
        CreateStaffDoor();
        CreateClosedEntrance();
        CreateMusicDepartmentDetails();
        CreatePostSaleAtmosphere();
        TuneLighting();
    }

    private void CreateMaterials()
    {
        wallMaterial = CreateMaterial("Soft Warm Shop Walls", new Color(0.72f, 0.69f, 0.62f));
        floorMaterial = CreateMaterial("Warm Charcoal Shop Floor", new Color(0.31f, 0.31f, 0.29f));
        fixtureMaterial = CreateMaterial("Natural Pale Ash Fixtures", new Color(0.66f, 0.60f, 0.50f));
        counterMaterial = CreateMaterial("Smoky Walnut Checkout", new Color(0.37f, 0.36f, 0.35f));
        signageMaterial = CreateMaterial("Cream Retail Signage", new Color(0.94f, 0.89f, 0.76f));
        shutterMaterial = CreateMaterial("Dusty Blue Shutter", new Color(0.31f, 0.40f, 0.44f));
        doorMaterial = CreateMaterial("Muted Green Staff Door", new Color(0.30f, 0.42f, 0.39f));
    }

    private void ApplyExistingRoomMaterials()
    {
        AssignExistingMaterial("Floor", floorMaterial);
        AssignExistingMaterial("Back Wall", wallMaterial);
        AssignExistingMaterial("Front Wall", wallMaterial);
        AssignExistingMaterial("Left Wall", wallMaterial);
        AssignExistingMaterial("Right Wall", wallMaterial);
        AssignExistingMaterial("CD Display Surface", fixtureMaterial);
    }

    private static void AssignExistingMaterial(string objectName, Material material)
    {
        GameObject gameObject = GameObject.Find(objectName);
        MeshRenderer renderer = gameObject != null ? gameObject.GetComponent<MeshRenderer>() : null;
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void RemoveOldMovementMarker()
    {
        GameObject marker = GameObject.Find("Movement Reference Block");
        if (marker != null)
        {
            marker.SetActive(false);
        }
    }

    private void AdjustExistingDisplayTable()
    {
        GameObject displayTable = GameObject.Find("CD Display Surface");
        if (displayTable == null)
        {
            return;
        }

        displayTable.transform.position = new Vector3(0f, 0.75f, 0.65f);
        displayTable.transform.localScale = new Vector3(1.7f, 0.14f, 0.9f);
    }

    private void CreateCheckoutCounter()
    {
        CreateCube("Checkout Counter Base", new Vector3(4.65f, 0.42f, -2.65f), new Vector3(1.9f, 0.84f, 0.7f), counterMaterial);
        CreateCube("Checkout Counter Top", new Vector3(4.65f, 0.88f, -2.65f), new Vector3(2.05f, 0.08f, 0.82f), fixtureMaterial);
        CreateCube("Small Till Block", new Vector3(4.25f, 1.05f, -2.7f), new Vector3(0.42f, 0.25f, 0.32f), counterMaterial);
        CreateText("Checkout Label", "TILL", new Vector3(4.65f, 1.08f, -3.08f), 0.035f);
    }

    private void CreateStaffDoor()
    {
        CreateCube("Staff Room Door", new Vector3(6.88f, 1.05f, 2.1f), new Vector3(0.05f, 2.1f, 0.95f), doorMaterial);
        CreateCube("Staff Door Handle", new Vector3(6.82f, 1.05f, 1.8f), new Vector3(0.06f, 0.08f, 0.08f), signageMaterial);
        CreateText("Staff Door Label", "STAFF", new Vector3(6.78f, 1.8f, 2.1f), 0.03f, Quaternion.Euler(0f, -90f, 0f));
    }

    private void CreateClosedEntrance()
    {
        CreateCube("Closed Entrance Shutter", new Vector3(0f, 1.45f, -4.88f), new Vector3(3.7f, 2.4f, 0.04f), shutterMaterial);

        for (int i = 0; i < 7; i++)
        {
            float y = 0.45f + i * 0.3f;
            CreateCube($"Shutter Slat {i + 1}", new Vector3(0f, y, -4.92f), new Vector3(3.8f, 0.035f, 0.04f), signageMaterial);
        }

        CreateText("Closed Entrance Label", "CLOSED", new Vector3(0f, 2.55f, -4.95f), 0.055f, Quaternion.Euler(0f, 180f, 0f));
    }

    private void CreateMusicDepartmentDetails()
    {
        CreateCube("Music Department Header", new Vector3(0f, 2.35f, 4.15f), new Vector3(12f, 0.22f, 0.06f), signageMaterial);
        CreateText("Music Department Label", "MUSIC", new Vector3(0f, 2.38f, 4.08f), 0.06f);

        CreateCube("Low Browser Bin Left", new Vector3(-4.55f, 0.46f, 1.25f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        CreateCube("Low Browser Bin Right", new Vector3(4.45f, 0.46f, 0.85f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        // The old aisle end cap obstructed the new left-wall browsing bays.
    }

    private void CreatePostSaleAtmosphere()
    {
        Material saleRed = CreateMaterial("Faded Sale Red", new Color(0.71f, 0.22f, 0.19f));
        Material midnight = CreateMaterial("Night Shift Midnight", new Color(0.08f, 0.16f, 0.22f));
        Material gold = CreateMaterial("Warm Retail Gold", new Color(0.83f, 0.69f, 0.36f));

        // In-universe fictional shop identity and aftermath of a huge music sale.
        CreateCube("Shop Name Header", new Vector3(0f, 2.6f, -4.72f),
            new Vector3(4.5f, 0.36f, 0.07f), midnight);
        CreateText("Shop Identity", "SIDE B RECORDS", new Vector3(0f, 2.62f, -4.78f),
            0.047f, Quaternion.Euler(0f, 180f, 0f), Color.white);

        CreateCube("Front Sale Poster Left", new Vector3(-4.9f, 1.8f, -4.82f),
            new Vector3(1.6f, 1.15f, 0.05f), saleRed);
        CreateText("Sale Poster Left Text", "FINAL SALE", new Vector3(-4.9f, 1.82f, -4.86f),
            0.035f, Quaternion.Euler(0f, 180f, 0f), Color.white);

        CreateCube("Front Sale Poster Right", new Vector3(4.9f, 1.8f, -4.82f),
            new Vector3(1.6f, 1.15f, 0.05f), gold);
        CreateText("Sale Poster Right Text", "ALL STOCK\nMUST GO", new Vector3(4.9f, 1.82f, -4.86f),
            0.028f, Quaternion.Euler(0f, 180f, 0f));

        CreateCube("Rock Department Strip", new Vector3(-2.38f, 2.1f, 4.07f),
            new Vector3(5.7f, 0.15f, 0.035f), saleRed);
        CreateCube("Indie Department Strip", new Vector3(3.7f, 2.1f, 4.07f),
            new Vector3(3.9f, 0.15f, 0.035f), midnight);

        CreateCube("Faded Checkout Sign", new Vector3(4.65f, 1.48f, -3.08f),
            new Vector3(1.55f, 0.22f, 0.04f), saleRed);
        CreateText("Checkout Returns Message", "RETURNS / EXCHANGES",
            new Vector3(4.65f, 1.48f, -3.12f), 0.017f, Quaternion.identity, Color.white);

        // Lighting is tuned separately so the warm palette has predictable contrast.
    }

    private static void TuneLighting()
    {
        GameObject sunObject = GameObject.Find("Directional Light");
        Light mainLight = sunObject != null ? sunObject.GetComponent<Light>() : null;
        if (mainLight != null)
        {
            mainLight.intensity = 1.2f;
            mainLight.color = new Color(1f, 0.92f, 0.81f);
            mainLight.shadows = LightShadows.Soft;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.57f, 0.54f, 0.49f);
    }

    // Keep the slot collider/snap transform intact. Only the oversized fixture
    // meshes and visible signs shrink: this preserves existing shelf IDs and saves.
    // Build shallow, waist-height CD flip-bins. This preserves the original
    // ShelfSlot, snap transform and persistent section ID, but replaces the
    // greybox bookcase silhouette with a familiar early-2000s music-shop rack.
    public static void StyleShelf(Transform shelf, string genre)
    {
        if (shelf == null)
        {
            return;
        }

        if (shelfFrameMaterial == null)
        {
            shelfFrameMaterial = CreateMaterial("Flip-Bin Dark Metal Frame",
                new Color(0.23f, 0.25f, 0.26f));
            shelfBackMaterial = CreateMaterial("Flip-Bin Charcoal Deck",
                new Color(0.14f, 0.16f, 0.17f));
            rockHeaderMaterial = CreateMaterial("Rock and Pop Burgundy Header",
                new Color(0.55f, 0.18f, 0.30f));
            indieHeaderMaterial = CreateMaterial("Indie Petrol Header",
                new Color(0.13f, 0.37f, 0.40f));
            shelfRangeMaterial = CreateMaterial("Divider Matte Slate",
                new Color(0.31f, 0.35f, 0.36f));
        }

        bool isIndie = string.Equals(genre, "Indie",
            System.StringComparison.OrdinalIgnoreCase);
        Material header = isIndie ? indieHeaderMaterial : rockHeaderMaterial;

        // Reuse the scene's model parts rather than layering a new rack on top.
        StylePart(shelf, "Shelf Back", new Vector3(0f, 0.48f, 0.36f),
            new Vector3(2.13f, 0.92f, 0.065f), shelfBackMaterial);
        StylePart(shelf, "Shelf Board", new Vector3(0f, 0.74f, -0.03f),
            new Vector3(2.12f, 0.07f, 0.84f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Top", new Vector3(0f, 1.53f, 0.36f),
            new Vector3(2.14f, 0.035f, 0.065f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Left Side", new Vector3(-1.08f, 1.05f, -0.03f),
            new Vector3(0.05f, 0.65f, 0.86f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Right Side", new Vector3(1.08f, 1.05f, -0.03f),
            new Vector3(0.05f, 0.65f, 0.86f), shelfFrameMaterial);

        // A high-contrast header and alphabet tab, like the photo reference.
        StylePart(shelf, "ROCK Shelf Sign Panel", new Vector3(0f, 1.38f, 0.31f),
            new Vector3(2.04f, 0.245f, 0.025f), header);
        StylePart(shelf, "B Shelf Sign Panel", new Vector3(0.69f, 1.38f, 0.282f),
            new Vector3(0.42f, 0.195f, 0.025f), shelfRangeMaterial);

        // Three stepped decks expose the actual front covers. Six physical
        // cases sit across each deck; shallow rails and index tabs suggest the
        // CD browsing troughs used in real high-street music shops.
        for (int tier = 0; tier < 3; tier++)
        {
            float y = 0.79f + tier * 0.10f;
            float z = -0.30f + tier * 0.24f;

            CreateRackPart(shelf, "Browsing Deck " + (tier + 1),
                new Vector3(0f, y, z),
                new Vector3(2.07f, 0.043f, 0.235f), shelfBackMaterial);
            CreateRackPart(shelf, "Browsing Front Lip " + (tier + 1),
                new Vector3(0f, y + 0.028f, z - 0.124f),
                new Vector3(2.07f, 0.049f, 0.025f), shelfFrameMaterial);

            // Each small tab marks a divider rather than behaving like a CD.
            for (int divider = 0; divider < 2; divider++)
            {
                float x = divider == 0 ? -0.60f : 0.285f;
                CreateRackPart(shelf, "Index Divider " + (tier + 1) + "-" + (divider + 1),
                    new Vector3(x, y + 0.09f, z + 0.038f),
                    new Vector3(0.027f, 0.19f, 0.082f), header);
            }
        }

        CreateRackPart(shelf, "Browsing Bin Plinth",
            new Vector3(0f, 0.34f, -0.02f),
            new Vector3(2.07f, 0.64f, 0.78f), shelfFrameMaterial);
        CreateRackPart(shelf, "Browsing Bin Front Panel",
            new Vector3(0f, 0.47f, -0.425f),
            new Vector3(2.07f, 0.58f, 0.045f), shelfBackMaterial);

        // Snap stays a direct child of the shelf, with absolute in-rack
        // offsets computed by ShelfSlot.LocalOffsetForIndex. No saved IDs move.
        Transform snap = shelf.Find("ROCK A-G CD Snap Point");
        if (snap != null)
        {
            snap.localPosition = Vector3.zero;
            snap.localRotation = Quaternion.identity;
            snap.localScale = Vector3.one;
        }

        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot != null)
        {
            slot.transform.localPosition = new Vector3(0f, 1.04f, -0.06f);
            slot.transform.localRotation = Quaternion.identity;
            slot.transform.localScale = new Vector3(2.02f, 0.57f, 0.75f);

            // A large but invisible placement hit volume makes realistic-size
            // cases easy to put down; the cases themselves remain individually
            // pickable, with their own colliders.
            MeshRenderer slotVisual = slot.GetComponent<MeshRenderer>();
            if (slotVisual != null)
            {
                slotVisual.enabled = false;
            }
        }
    }

    private static void StylePart(Transform parent, string name, Vector3 position,
        Vector3 scale, Material material)
    {
        Transform part = parent.Find(name);
        if (part == null)
        {
            return;
        }

        part.localPosition = position;
        part.localScale = scale;
        MeshRenderer renderer = part.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void CreateRackPart(Transform parent, string name,
        Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = name;
        piece.transform.SetParent(parent, false);
        piece.transform.localPosition = localPosition;
        piece.transform.localScale = localScale;

        // The rack is display geometry. ShelfSlot owns the placement physics;
        // extra collider faces here would obstruct aiming at small CD covers.
        Collider collider = piece.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        MeshRenderer renderer = piece.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }
    }

    private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;

        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        return cube;
    }

    private void CreateText(string name, string text, Vector3 position, float characterSize)
    {
        CreateText(name, text, position, characterSize, Quaternion.identity);
    }

    private void CreateText(string name, string text, Vector3 position, float characterSize, Quaternion rotation)
    {
        CreateText(name, text, position, characterSize, rotation, Color.black);
    }

    private void CreateText(string name, string text, Vector3 position,
        float characterSize, Quaternion rotation, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.position = position;
        textObject.transform.rotation = rotation;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.color = color;
        textMesh.richText = false;

        PrototypeTextMaterial.Apply(textMesh, color);
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material material = new Material(shader)
        {
            name = name
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        return material;
    }
}
