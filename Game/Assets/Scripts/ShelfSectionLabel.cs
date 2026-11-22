using UnityEngine;

public sealed class ShelfSectionLabel : MonoBehaviour
{
    [SerializeField] private string genre = "ROCK";
    [SerializeField] private string artistRange = "A-G";
    [SerializeField] private Color textColor = new(0.96f, 0.93f, 0.83f, 1f);

    private void Start()
    {
        CreateLabel("Shelf Genre Text", genre, new Vector3(0f, 1.31f, -0.455f), SizeForText(genre, 0.043f, 5));
        CreateLabel("Shelf Artist Range Text", artistRange, new Vector3(-0.52f, 0.96f, -0.455f), SizeForText(artistRange, 0.033f, 3));
    }

    public void Configure(string newGenre, string newArtistRange)
    {
        genre = newGenre.ToUpperInvariant();
        artistRange = newArtistRange.ToUpperInvariant();
        // The old scene serialized black text. A field default alone would not
        // override it, so set the cream colour for every runtime shelf instance.
        textColor = new Color(0.96f, 0.93f, 0.83f, 1f);
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
