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
        FinishDisplayFurniture();
        RemoveOldMovementMarker();
        CreateCheckoutCounter();
        CreateStaffDoor();
        CreateClosedEntrance();
        CreateMusicDepartmentDetails();
        CreatePostSaleAtmosphere();
        TuneLighting();
        CreateShopAccents();
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

    private void FinishDisplayFurniture()
    {
        // Visible structural supports: previously the display surfaces floated.
        const float legY = 0.37f;
        foreach (float x in new[] { -0.74f, 0.74f })
        {
            foreach (float z in new[] { 0.31f, 1.00f })
            {
                CreateCube("Display Table Leg", new Vector3(x, legY, z),
                    new Vector3(0.105f, 0.74f, 0.105f), counterMaterial);
            }
        }

        CreateCube("Display Table Lower Shelf",
            new Vector3(0f, 0.19f, 0.65f),
            new Vector3(1.56f, 0.06f, 0.83f), fixtureMaterial);
        CreateCube("Display Table Edge",
            new Vector3(0f, 0.84f, 0.20f),
            new Vector3(1.70f, 0.035f, 0.04f), counterMaterial);
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
        CreateText("Music Department Label", "RECORDS  •  CD", new Vector3(0f, 2.38f, 4.08f), 0.025f);

        CreateCube("Low Browser Bin Left", new Vector3(-4.55f, 0.46f, 1.25f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        CreateCube("Low Browser Bin Right", new Vector3(4.45f, 0.46f, 0.85f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        // Both loose-stock tables now have proper bodies instead of hovering.
        CreateCube("Left Browser Bin Base", new Vector3(-4.55f, 0.22f, 1.25f),
            new Vector3(1.12f, 0.44f, 0.78f), counterMaterial);
        CreateCube("Right Browser Bin Base", new Vector3(4.45f, 0.22f, 0.85f),
            new Vector3(1.12f, 0.44f, 0.78f), counterMaterial);
        CreateCube("Left Browser Bin Lip", new Vector3(-4.55f, 0.64f, 0.82f),
            new Vector3(1.24f, 0.075f, 0.055f), counterMaterial);
        CreateCube("Right Browser Bin Lip", new Vector3(4.45f, 0.64f, 0.42f),
            new Vector3(1.24f, 0.075f, 0.055f), counterMaterial);
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
            0.033f, Quaternion.Euler(0f, 180f, 0f), Color.white);

        CreateCube("Front Sale Poster Left", new Vector3(-4.9f, 1.8f, -4.82f),
            new Vector3(1.6f, 1.15f, 0.05f), saleRed);
        CreateText("Sale Poster Left Text", "FINAL SALE", new Vector3(-4.9f, 1.82f, -4.86f),
            0.024f, Quaternion.Euler(0f, 180f, 0f), Color.white);

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

    private void CreateShopAccents()
    {
        // The greybox had an open sky, making even finished shelves look
        // like exterior props. A non-shadow-casting ceiling closes the room
        // while the URP directional and warm overhead lighting stay usable.
        GameObject ceiling = CreateCube("Retail Shop Ceiling",
            new Vector3(0f, 3.27f, 0f),
            new Vector3(14f, 0.12f, 10.1f), signageMaterial);
        MeshRenderer ceilingRenderer = ceiling.GetComponent<MeshRenderer>();
        if (ceilingRenderer != null)
        {
            ceilingRenderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        Collider ceilingCollider = ceiling.GetComponent<Collider>();
        if (ceilingCollider != null)
        {
            ceilingCollider.enabled = false;
        }

        Material charcoal = CreateMaterial("Charcoal Wall Trim",
            new Color(0.24f, 0.25f, 0.26f));
        Material agedPaper = CreateMaterial("Aged Ticket Paper",
            new Color(0.86f, 0.81f, 0.69f));

        // A few restrained retail fixtures help the floor feel like an actual
        // post-sale high-street record shop rather than a flat prototype room.
        CreateCube("Back Wall Skirting", new Vector3(0f, 0.09f, 4.84f),
            new Vector3(13.7f, 0.18f, 0.07f), charcoal);
        CreateCube("Entrance Skirting Left", new Vector3(-4.55f, 0.09f, -4.83f),
            new Vector3(4.3f, 0.18f, 0.07f), charcoal);
        CreateCube("Entrance Skirting Right", new Vector3(4.55f, 0.09f, -4.83f),
            new Vector3(4.3f, 0.18f, 0.07f), charcoal);

        CreateCube("Store Discount Ticket", new Vector3(-1.10f, 0.86f, 0.29f),
            new Vector3(0.28f, 0.018f, 0.20f), agedPaper);
        CreateCube("Returns Ticket", new Vector3(4.64f, 0.936f, -2.40f),
            new Vector3(0.23f, 0.015f, 0.14f), agedPaper);

        // Two broad diffuse ceiling lights, no extra shadow-casting lights.
        for (int i = 0; i < 2; i++)
        {
            float z = i == 0 ? -1.6f : 2.0f;
            CreateCube("Ceiling Light Diffuser " + (i + 1),
                new Vector3(0f, 3.14f, z),
                new Vector3(3.0f, 0.04f, 0.23f), signageMaterial);

            GameObject lightObject = new GameObject("Warm Overhead Light " + (i + 1));
            lightObject.transform.position = new Vector3(0f, 2.85f, z);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 0.35f;
            light.shadows = LightShadows.None;
            light.color = new Color(1f, 0.94f, 0.81f);
        }
    }

    private static void TuneLighting()
    {
        GameObject sunObject = GameObject.Find("Directional Light");
        Light mainLight = sunObject != null ? sunObject.GetComponent<Light>() : null;
        if (mainLight != null)
        {
            mainLight.intensity = 1.0f;
            mainLight.shadowStrength = 0.43f;
            mainLight.color = new Color(1f, 0.92f, 0.81f);
            mainLight.shadows = LightShadows.Soft;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.69f, 0.67f, 0.61f);
    }

    // Seven browsable rows, four deep at each cover. Real CD-store tiers
    // have a shallow rise towards the back, retaining lips, and bold but
    // compact alphabet dividers. Persistent shelf IDs are never changed.
    public static void StyleShelf(Transform shelf, string genre)
    {
        if (shelf == null)
        {
            return;
        }

        if (shelfFrameMaterial == null)
        {
            shelfFrameMaterial = CreateMaterial("Powder Coated Dark Frame",
                new Color(0.25f, 0.27f, 0.29f));
            shelfBackMaterial = CreateMaterial("Dark Vinyl Browsing Decks",
                new Color(0.16f, 0.18f, 0.20f));
            rockHeaderMaterial = CreateMaterial("Rock Burgundy",
                new Color(0.57f, 0.21f, 0.34f));
            indieHeaderMaterial = CreateMaterial("Indie Petrol",
                new Color(0.13f, 0.42f, 0.43f));
            shelfRangeMaterial = CreateMaterial("Alphabet Divider Slate",
                new Color(0.34f, 0.38f, 0.39f));
        }

        Material department = string.Equals(genre, "Indie",
            System.StringComparison.OrdinalIgnoreCase)
            ? indieHeaderMaterial : rockHeaderMaterial;

        // Reuse and resize the original primitives. The old bookcase silhouette
        // must disappear entirely, rather than sitting behind the new trays.
        StylePart(shelf, "Shelf Back", new Vector3(0f, 0.83f, 0.43f),
            new Vector3(2.13f, 1.48f, 0.055f), shelfBackMaterial);
        StylePart(shelf, "Shelf Board", new Vector3(0f, 0.72f, -0.08f),
            new Vector3(2.12f, 0.06f, 1.02f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Top", new Vector3(0f, 1.81f, 0.37f),
            new Vector3(2.12f, 0.038f, 0.12f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Left Side", new Vector3(-1.08f, 1.19f, -0.045f),
            new Vector3(0.045f, 1.14f, 1.20f), shelfFrameMaterial);
        StylePart(shelf, "Shelf Right Side", new Vector3(1.08f, 1.19f, -0.045f),
            new Vector3(0.045f, 1.14f, 1.20f), shelfFrameMaterial);
        StylePart(shelf, "ROCK Shelf Sign Panel", new Vector3(0f, 1.66f, 0.397f),
            new Vector3(2.05f, 0.22f, 0.026f), department);
        StylePart(shelf, "B Shelf Sign Panel", new Vector3(0.73f, 1.66f, 0.364f),
            new Vector3(0.37f, 0.175f, 0.027f), shelfRangeMaterial);

        for (int row = 0; row < ShelfSlot.TierCount; row++)
        {
            float y = 0.735f + row * 0.085f;
            float z = -0.47f + row * 0.130f;

            CreateRackPart(shelf, "Browsing Deck " + (row + 1),
                new Vector3(0f, y, z + 0.028f),
                new Vector3(2.06f, 0.032f, 0.134f), shelfBackMaterial);
            CreateRackPart(shelf, "Browsing Lip " + (row + 1),
                new Vector3(0f, y + 0.040f, z - 0.063f),
                new Vector3(2.06f, 0.042f, 0.020f), shelfFrameMaterial);

            // Small aisle markers, not giant geometric fins hiding the covers.
            for (int divider = 0; divider < 2; divider++)
            {
                float x = divider == 0 ? -0.43f : 0.43f;
                CreateRackPart(shelf, "Genre Divider " + row + "-" + divider,
                    new Vector3(x, y + 0.115f, z + 0.093f),
                    new Vector3(0.018f, 0.050f, 0.045f), department);
            }
        }

        CreateRackPart(shelf, "Closed Plinth",
            new Vector3(0f, 0.33f, -0.07f),
            new Vector3(2.06f, 0.65f, 1.07f), shelfFrameMaterial);
        CreateRackPart(shelf, "Front Kick Panel",
            new Vector3(0f, 0.45f, -0.615f),
            new Vector3(2.06f, 0.56f, 0.035f), shelfBackMaterial);
        CreateRackPart(shelf, "Front Price Rail",
            new Vector3(0f, 0.715f, -0.628f),
            new Vector3(2.06f, 0.038f, 0.027f), department);

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
            slot.transform.localPosition = new Vector3(0f, 1.145f, -0.054f);
            slot.transform.localRotation = Quaternion.identity;
            slot.transform.localScale = new Vector3(2.01f, 0.91f, 1.17f);

            Collider volume = slot.GetComponent<Collider>();
            if (volume != null)
            {
                volume.isTrigger = true;
            }

            MeshRenderer visual = slot.GetComponent<MeshRenderer>();
            if (visual != null)
            {
                visual.enabled = false;
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
