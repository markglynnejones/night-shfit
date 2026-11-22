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

        RenderSettings.ambientLight = new Color(0.57f, 0.54f, 0.49f);
    }

    // Keep the slot collider/snap transform intact. Only the oversized fixture
    // meshes and visible signs shrink: this preserves existing shelf IDs and saves.
    public static void StyleShelf(Transform shelf, string genre)
    {
        if (shelf == null)
        {
            return;
        }

        if (shelfFrameMaterial == null)
        {
            shelfFrameMaterial = CreateMaterial("Shelf Frame Ash Wood", new Color(0.49f, 0.45f, 0.39f));
            shelfBackMaterial = CreateMaterial("Shelf Back Warm Charcoal", new Color(0.24f, 0.27f, 0.28f));
            rockHeaderMaterial = CreateMaterial("Rock Section Faded Burgundy", new Color(0.50f, 0.21f, 0.24f));
            indieHeaderMaterial = CreateMaterial("Indie Section Muted Petrol", new Color(0.16f, 0.40f, 0.42f));
            shelfRangeMaterial = CreateMaterial("Shelf Range Slate", new Color(0.27f, 0.31f, 0.32f));
        }

        StylePart(shelf, "Shelf Back", new Vector3(0f, 0.75f, 0.12f),
            new Vector3(2.1f, 1.24f, 0.09f), shelfBackMaterial);
        StylePart(shelf, "Shelf Board", new Vector3(0f, 0.43f, -0.04f),
            new Vector3(2.1f, 0.05f, 0.51f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Top", new Vector3(0f, 1.16f, -0.04f),
            new Vector3(2.1f, 0.05f, 0.51f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Left Side", new Vector3(-1.06f, 0.75f, -0.04f),
            new Vector3(0.045f, 1.24f, 0.51f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Right Side", new Vector3(1.06f, 0.75f, -0.04f),
            new Vector3(0.045f, 1.24f, 0.51f), shelfFrameMaterial);
        StylePart(shelf, "ROCK Shelf Sign Panel", new Vector3(0f, 1.31f, -0.43f),
            new Vector3(1.25f, 0.19f, 0.025f),
            string.Equals(genre, "Indie", System.StringComparison.OrdinalIgnoreCase)
                ? indieHeaderMaterial : rockHeaderMaterial);
        StylePart(shelf, "B Shelf Sign Panel", new Vector3(-0.52f, 0.96f, -0.43f),
            new Vector3(0.23f, 0.18f, 0.025f), shelfRangeMaterial);

        Transform snap = shelf.Find("ROCK A-G CD Snap Point");
        if (snap != null)
        {
            snap.localPosition = new Vector3(0f, 0.53f, -0.38f);
        }

        ShelfSlot slot = shelf.GetComponentInChildren<ShelfSlot>();
        if (slot != null)
        {
            slot.transform.localPosition = new Vector3(0f, 0.53f, -0.34f);
            slot.transform.localScale = new Vector3(1.75f, 0.31f, 0.02f);

            // The large bright prototype slot should never cover the CD spines.
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
