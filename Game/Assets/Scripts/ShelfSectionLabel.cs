using UnityEngine;

public sealed class ShelfSectionLabel : MonoBehaviour
{
    [SerializeField] private string genre = "ROCK";
    [SerializeField] private string artistRange = "A-G";
    [SerializeField] private Color textColor = Color.black;

    private void Start()
    {
        CreateLabel("Shelf Genre Text", genre, new Vector3(0f, 1.43f, -0.455f), SizeForText(genre, 0.024f, 5));
        CreateLabel("Shelf Artist Range Text", artistRange, new Vector3(-0.52f, 1.03f, -0.455f), SizeForText(artistRange, 0.026f, 3));
    }

    public void Configure(string newGenre, string newArtistRange)
    {
        genre = newGenre.ToUpperInvariant();
        artistRange = newArtistRange.ToUpperInvariant();
    }

    private void CreateLabel(
        string name,
        string text,
        Vector3 localPosition,
        float characterSize)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.color = textColor;
        textMesh.richText = false;

        PrototypeTextMaterial.Apply(textMesh, textColor);
    }

    private static float SizeForText(string text, float maximumSize, int comfortableLength)
    {
        float scale = comfortableLength / (float)Mathf.Max(1, text.Length);
        return maximumSize * Mathf.Min(1f, scale);
    }
}
