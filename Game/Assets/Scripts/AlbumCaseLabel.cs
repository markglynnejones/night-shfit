using UnityEngine;

[RequireComponent(typeof(AlbumInfo))]
public sealed class AlbumCaseLabel : MonoBehaviour
{
    [SerializeField] private Color frontTextColor = Color.white;
    [SerializeField] private Color spineTextColor = Color.black;

    private const float FrontZ = -0.047f;
    private const float SpineZ = -0.054f;

    private void Start()
    {
        AlbumInfo albumInfo = GetComponent<AlbumInfo>();
        string wrappedArtist = WrapText(albumInfo.Artist.ToUpperInvariant(), 9);
        string wrappedAlbumTitle = WrapText(albumInfo.Album, 14);
        string spineText = FormatSpineText(albumInfo);

        CreateText(
            "Front Artist Label",
            wrappedArtist,
            new Vector3(0.03f, 0.15f, FrontZ),
            Quaternion.identity,
            SizeForText(wrappedArtist, 0.011f, 9),
            frontTextColor,
            transform,
            0.8f);

        CreateText(
            "Front Album Label",
            wrappedAlbumTitle,
            new Vector3(0.03f, -0.055f, FrontZ),
            Quaternion.identity,
            SizeForText(wrappedAlbumTitle, 0.0088f, 13),
            frontTextColor,
            transform,
            0.82f);

        CreateText(
            "Spine Album Label",
            spineText,
            new Vector3(-0.34f, 0f, SpineZ),
            Quaternion.Euler(0f, 0f, 90f),
            SizeForText(spineText, 0.0024f, 34),
            spineTextColor,
            transform,
            0.9f);
    }

    private static void CreateText(
        string name,
        string text,
        Vector3 localPosition,
        Quaternion localRotation,
        float characterSize,
        Color color,
        Transform parent,
        float lineSpacing)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = localRotation;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 64;
        textMesh.color = color;
        textMesh.richText = false;
        textMesh.lineSpacing = lineSpacing;

        PrototypeTextMaterial.Apply(textMesh, color);
    }

    private static string WrapText(string text, int maxLineLength)
    {
        string[] words = text.Split(' ');
        string wrappedText = string.Empty;
        string currentLine = string.Empty;

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            string candidate = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";

            if (candidate.Length > maxLineLength && !string.IsNullOrEmpty(currentLine))
            {
                wrappedText = AppendLine(wrappedText, currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = candidate;
            }
        }

        return AppendLine(wrappedText, currentLine);
    }

    private static string AppendLine(string existingText, string line)
    {
        if (string.IsNullOrEmpty(existingText))
        {
            return line;
        }

        return $"{existingText}\n{line}";
    }

    private static string FormatSpineText(AlbumInfo albumInfo)
    {
        return $"{albumInfo.Artist} \u2022 {albumInfo.Album}".ToUpperInvariant();
    }

    private static float SizeForText(string text, float maximumSize, int comfortableLineLength)
    {
        int longestLineLength = LongestLineLength(text);
        float scale = comfortableLineLength / (float)Mathf.Max(1, longestLineLength);
        return maximumSize * Mathf.Min(1f, scale);
    }

    private static int LongestLineLength(string text)
    {
        string[] lines = text.Split('\n');
        int longestLineLength = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            longestLineLength = Mathf.Max(longestLineLength, lines[i].Length);
        }

        return longestLineLength;
    }
}
