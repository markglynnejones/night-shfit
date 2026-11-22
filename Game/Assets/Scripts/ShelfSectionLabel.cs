using UnityEngine;

public sealed class ShelfSectionLabel : MonoBehaviour
{
    [SerializeField] private string genre = "ROCK";
    [SerializeField] private string artistRange = "A-G";
    [SerializeField] private Color textColor = new(0.96f, 0.93f, 0.83f, 1f);

    private void Start()
    {
        // Billboard behind the three browsing tiers; keep the genre and A-Z
        // range in a single eye-readable header, not on an empty bookcase.
        // Smaller type sits within the compact two-metre shop sign. The
        // old large TextMesh values obscured the CD rows from a standing view.
        CreateLabel("Shelf Genre Text", genre,
            new Vector3(-0.26f, 1.66f, 0.375f), SizeForText(genre, 0.026f, 5));
        CreateLabel("Shelf Artist Range Text", artistRange,
            new Vector3(0.73f, 1.66f, 0.341f), SizeForText(artistRange, 0.022f, 3));
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
