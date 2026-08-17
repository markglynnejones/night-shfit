using UnityEngine;

public sealed class PrototypeStoreLayout : MonoBehaviour
{
    private Material wallMaterial;
    private Material fixtureMaterial;
    private Material counterMaterial;
    private Material signageMaterial;
    private Material shutterMaterial;
    private Material doorMaterial;

    private void Awake()
    {
        CreateMaterials();
        AdjustExistingDisplayTable();
        CreateCheckoutCounter();
        CreateStaffDoor();
        CreateClockOutPoint();
        CreateClosedEntrance();
        CreateMusicDepartmentDetails();
    }

    private void CreateMaterials()
    {
        wallMaterial = CreateMaterial("Prototype Wall Warm Grey", new Color(0.58f, 0.55f, 0.52f));
        fixtureMaterial = CreateMaterial("Prototype Fixture Grey", new Color(0.68f, 0.67f, 0.64f));
        counterMaterial = CreateMaterial("Prototype Counter Grey", new Color(0.48f, 0.48f, 0.46f));
        signageMaterial = CreateMaterial("Prototype Signage Off White", new Color(0.86f, 0.84f, 0.78f));
        shutterMaterial = CreateMaterial("Prototype Shutter Blue Grey", new Color(0.28f, 0.34f, 0.39f));
        doorMaterial = CreateMaterial("Prototype Staff Door", new Color(0.38f, 0.43f, 0.42f));
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

    private void CreateClockOutPoint()
    {
        GameObject clockOutPoint = CreateCube("Clock Out Point", new Vector3(6.35f, 1.05f, 2.85f), new Vector3(0.08f, 0.55f, 0.45f), signageMaterial);
        clockOutPoint.AddComponent<ClockOutPoint>();
        CreateText("Clock Out Label", "CLOCK\nOUT", new Vector3(6.29f, 1.08f, 2.85f), 0.024f, Quaternion.Euler(0f, -90f, 0f));
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
        CreateCube("Music Department Header", new Vector3(0f, 2.35f, 4.15f), new Vector3(5.9f, 0.22f, 0.06f), signageMaterial);
        CreateText("Music Department Label", "MUSIC", new Vector3(0f, 2.38f, 4.08f), 0.06f);

        CreateCube("Low Browser Bin Left", new Vector3(-4.55f, 0.46f, 1.25f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        CreateCube("Low Browser Bin Right", new Vector3(4.45f, 0.46f, 0.85f), new Vector3(1.25f, 0.32f, 0.9f), fixtureMaterial);
        CreateCube("Aisle End Cap", new Vector3(-5.8f, 0.95f, 3.0f), new Vector3(0.65f, 1.1f, 0.8f), fixtureMaterial);
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
        GameObject textObject = new GameObject(name);
        textObject.transform.position = position;
        textObject.transform.rotation = rotation;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.color = Color.black;
        textMesh.richText = false;

        PrototypeTextMaterial.Apply(textMesh, Color.black);
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

        material.SetColor("_BaseColor", color);
        material.SetColor("_Color", color);
        return material;
    }
}
