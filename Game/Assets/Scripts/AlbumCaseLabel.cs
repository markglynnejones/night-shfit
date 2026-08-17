using UnityEngine;

[RequireComponent(typeof(AlbumInfo))]
public sealed class AlbumCaseLabel : MonoBehaviour
{
    [SerializeField] private Color frontTextColor = Color.white;
    [SerializeField] private Color spineTextColor = Color.black;

    private const float FrontZ = -0.047f;
    private const float SpineZ = -0.054f;

    private void Awake()
    {
        AlbumInfo albumInfo = GetComponent<AlbumInfo>();

        CreateText(
            "Front Artist Label",
            albumInfo.Artist.ToUpperInvariant(),
            new Vector3(0.03f, 0.14f, FrontZ),
            Quaternion.identity,
            0.018f,
            frontTextColor,
            transform);

        CreateText(
            "Front Album Label",
            WrapTitle(albumInfo.Album),
            new Vector3(0.03f, 0f, FrontZ),
            Quaternion.identity,
            0.013f,
            frontTextColor,
            transform);

        CreateText(
            "Spine Album Label",
            FormatSpineText(albumInfo),
            new Vector3(-0.34f, 0f, SpineZ),
            Quaternion.Euler(0f, 0f, 90f),
            0.0034f,
            spineTextColor,
            transform);
    }

    private static void CreateText(
        string name,
        string text,
        Vector3 localPosition,
        Quaternion localRotation,
        float characterSize,
        Color color,
        Transform parent)
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
        textMesh.lineSpacing = 0.85f;
    }

    private static string WrapTitle(string title)
    {
        const int maxLineLength = 18;

        if (title.Length <= maxLineLength)
        {
            return title;
        }

        int splitIndex = title.LastIndexOf(' ', maxLineLength);
        if (splitIndex < 0)
        {
            splitIndex = title.IndexOf(' ', maxLineLength);
        }

        if (splitIndex < 0)
        {
            return title;
        }

        return title.Remove(splitIndex, 1).Insert(splitIndex, "\n");
    }

    private static string FormatSpineText(AlbumInfo albumInfo)
    {
        return $"{albumInfo.Artist} \u2022 {albumInfo.Album}".ToUpperInvariant();
    }
}
