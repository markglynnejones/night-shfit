using System;
using System.Collections.Generic;
using UnityEngine;

// Shared, original early-2000s-inspired cover art for 248 distinct albums.
// Legible artist and album text is rasterised into the cover texture itself;
// no per-CD TextMesh children, font assets or external artwork are needed.
[RequireComponent(typeof(MediaItem))]
public sealed class AlbumCoverArt : MonoBehaviour
{
    private sealed class Sleeve
    {
        public Material Cover;
        public Material Spine;
    }

    private static readonly Dictionary<string, Sleeve> Cached = new();
    private const int Size = 256;

    // Each letter is encoded as seven rows of five dots. This tiny bitmap
    // alphabet renders consistent, high-contrast lettering on any platform.
    private static readonly Dictionary<char, byte[]> Font = new()
    {
        ['A'] = new byte[] {14,17,17,31,17,17,17},
        ['B'] = new byte[] {30,17,17,30,17,17,30},
        ['C'] = new byte[] {14,17,16,16,16,17,14},
        ['D'] = new byte[] {30,17,17,17,17,17,30},
        ['E'] = new byte[] {31,16,16,30,16,16,31},
        ['F'] = new byte[] {31,16,16,30,16,16,16},
        ['G'] = new byte[] {14,17,16,23,17,17,14},
        ['H'] = new byte[] {17,17,17,31,17,17,17},
        ['I'] = new byte[] {31,4,4,4,4,4,31},
        ['J'] = new byte[] {7,2,2,2,18,18,12},
        ['K'] = new byte[] {17,18,20,24,20,18,17},
        ['L'] = new byte[] {16,16,16,16,16,16,31},
        ['M'] = new byte[] {17,27,21,21,17,17,17},
        ['N'] = new byte[] {17,25,21,19,17,17,17},
        ['O'] = new byte[] {14,17,17,17,17,17,14},
        ['P'] = new byte[] {30,17,17,30,16,16,16},
        ['Q'] = new byte[] {14,17,17,17,21,18,13},
        ['R'] = new byte[] {30,17,17,30,20,18,17},
        ['S'] = new byte[] {15,16,16,14,1,1,30},
        ['T'] = new byte[] {31,4,4,4,4,4,4},
        ['U'] = new byte[] {17,17,17,17,17,17,14},
        ['V'] = new byte[] {17,17,17,17,17,10,4},
        ['W'] = new byte[] {17,17,17,21,21,21,10},
        ['X'] = new byte[] {17,17,10,4,10,17,17},
        ['Y'] = new byte[] {17,17,10,4,4,4,4},
        ['Z'] = new byte[] {31,1,2,4,8,16,31},
        ['0'] = new byte[] {14,17,19,21,25,17,14},
        ['1'] = new byte[] {4,12,4,4,4,4,14},
        ['2'] = new byte[] {14,17,1,2,4,8,31},
        ['3'] = new byte[] {30,1,1,14,1,1,30},
        ['4'] = new byte[] {2,6,10,18,31,2,2},
        ['5'] = new byte[] {31,16,30,1,1,17,14},
        ['6'] = new byte[] {6,8,16,30,17,17,14},
        ['7'] = new byte[] {31,1,2,4,8,8,8},
        ['8'] = new byte[] {14,17,17,14,17,17,14},
        ['9'] = new byte[] {14,17,17,15,1,2,12},
        ['-'] = new byte[] {0,0,0,31,0,0,0},
        ['&'] = new byte[] {12,18,20,8,21,18,13},
        ['.'] = new byte[] {0,0,0,0,0,12,12},
        [':'] = new byte[] {0,12,12,0,12,12,0},
        ['\''] = new byte[] {12,12,8,0,0,0,0},
        ['!'] = new byte[] {4,4,4,4,4,0,4},
        ['?'] = new byte[] {14,17,1,2,4,0,4},
        ['/'] = new byte[] {1,2,2,4,8,8,16},
        [' '] = new byte[] {0,0,0,0,0,0,0}
    };

    private void Start()
    {
        MediaItem media = GetComponent<MediaItem>();
        if (media == null || media.AlbumDefinition == null)
        {
            return;
        }

        AlbumDefinition album = media.AlbumDefinition;
        if (!Cached.TryGetValue(album.CatalogueId, out Sleeve sleeve))
        {
            sleeve = CreateSleeve(album);
            if (sleeve == null)
            {
                return;
            }

            Cached.Add(album.CatalogueId, sleeve);
        }

        Transform cover = transform.Find("Placeholder Cover Card");
        Transform spine = transform.Find("Placeholder Spine Strip");

        if (cover != null && cover.TryGetComponent(out MeshRenderer coverRenderer))
        {
            coverRenderer.sharedMaterial = sleeve.Cover;
        }

        if (spine != null && spine.TryGetComponent(out MeshRenderer spineRenderer))
        {
            spineRenderer.sharedMaterial = sleeve.Spine;
        }
    }

    private static Sleeve CreateSleeve(AlbumDefinition album)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            Debug.LogError("The project's URP cover shader is missing.");
            return null;
        }

        uint hash = StableHash(album.CatalogueId);
        bool indie = string.Equals(album.Genre, "Indie", StringComparison.OrdinalIgnoreCase);
        float hue = (indie ? 0.40f : 0.95f) + (hash % 90u) / 850f;
        hue %= 1f;
        Color dark = Color.HSVToRGB(hue, 0.63f, 0.23f);
        Color mid = Color.HSVToRGB((hue + 0.16f) % 1f, 0.70f, 0.65f);
        Color bright = Color.HSVToRGB((hue + 0.47f) % 1f, 0.55f, 0.94f);
        Color ink = new(0.97f, 0.95f, 0.85f, 1f);

        Color32[] pixels = new Color32[Size * Size];
        int variation = (int)(hash % 6u);
        for (int y = 0; y < Size; y++)
        {
            float fy = y / (float)Size;
            for (int x = 0; x < Size; x++)
            {
                float fx = x / (float)Size;
                float dx = fx - 0.52f;
                float dy = fy - 0.54f;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                Color color = Color.Lerp(dark, mid, Mathf.Clamp01(fx * 0.24f + fy * 0.48f));
                bool accent = false;

                switch (variation)
                {
                    case 0:
                        accent = Mathf.Abs(Mathf.Sin((fx + fy * 0.38f) * 38f)) > 0.82f;
                        break;
                    case 1:
                        accent = Mathf.Abs(Mathf.Sin(distance * 39f)) > 0.80f;
                        break;
                    case 2:
                        accent = Mathf.Abs(fy - 0.51f - Mathf.Sin(fx * 15f) * 0.14f) < 0.035f;
                        break;
                    case 3:
                        accent = (Mathf.Abs(dx) < 0.21f && Mathf.Abs(dy) < 0.26f
                            && (x + y) % 21 < 4);
                        break;
                    case 4:
                        accent = Mathf.Abs(Mathf.Sin(fx * 17f) * Mathf.Cos(fy * 13f)) > 0.68f;
                        break;
                    default:
                        accent = Mathf.Abs(Mathf.Sin((dx * dx - dy * dy) * 61f)) > 0.84f;
                        break;
                }

                if (accent)
                {
                    color = Color.Lerp(color, bright, 0.82f);
                }

                // Two clean type bands keep the album names legible even when
                // the geometric art is bright and busy.
                if (y >= 179 || y < 77)
                {
                    color = Color.Lerp(dark, Color.black, 0.35f);
                }

                if ((y == 76 || y == 178) && x > 10 && x < Size - 10)
                {
                    color = bright;
                }

                pixels[y * Size + x] = color;
            }
        }

        List<string> artistLines = Wrap(album.ArtistName.ToUpperInvariant(), 16, 2);
        for (int i = 0; i < artistLines.Count; i++)
        {
            DrawLine(pixels, artistLines[i], 214 - i * 29, 3, ink);
        }

        List<string> titleLines = Wrap(album.AlbumTitle.ToUpperInvariant(), 20, 2);
        for (int i = 0; i < titleLines.Count; i++)
        {
            DrawLine(pixels, titleLines[i], 43 - i * 23, 3, ink);
        }

        // The original cube's front-face UVs are rotated 180 degrees in the
        // current scene template. Rotate the raster once, at creation time,
        // rather than changing the physical case transform (which would break
        // shelf/hand orientation). This corrects upside-down artist/title text
        // in both the rack and the selected carried CD.
        RotateForCaseFrontUV(pixels);

        Texture2D texture = new(Size, Size, TextureFormat.RGBA32, false)
        {
            name = "SIDE B / " + album.CatalogueId,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        Material cover = new(shader) { name = "Cover " + album.CatalogueId };
        if (cover.HasProperty("_BaseColor")) cover.SetColor("_BaseColor", Color.white);
        if (cover.HasProperty("_BaseMap")) cover.SetTexture("_BaseMap", texture);
        Material spine = new(shader) { name = "Spine " + album.CatalogueId };
        if (spine.HasProperty("_BaseColor")) spine.SetColor("_BaseColor", mid);

        return new Sleeve { Cover = cover, Spine = spine };
    }

    private static List<string> Wrap(string value, int maxCharacters, int maxLines)
    {
        List<string> lines = new();
        string current = string.Empty;
        foreach (string word in value.Split(' '))
        {
            if (string.IsNullOrEmpty(word))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(current)
                && current.Length + word.Length + 1 > maxCharacters)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = string.IsNullOrEmpty(current) ? word : current + " " + word;
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            lines.Add(current);
        }

        if (lines.Count > maxLines)
        {
            // Preserve the album's last distinctive words, shortening only
            // the excessively long first/second line when necessary.
            string remainder = string.Join(" ", lines.GetRange(maxLines - 1,
                lines.Count - maxLines + 1));
            lines[maxLines - 1] = remainder;
            lines.RemoveRange(maxLines, lines.Count - maxLines);
        }

        return lines;
    }

    private static void DrawLine(Color32[] pixels, string text, int y,
        int maximumScale, Color color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        int scale = Mathf.Max(1, Mathf.Min(maximumScale,
            (Size - 24) / (text.Length * 6)));
        int width = text.Length * 6 * scale;
        int startX = (Size - width) / 2;
        Color32 ink = color;

        for (int letter = 0; letter < text.Length; letter++)
        {
            char c = text[letter];
            if (!Font.TryGetValue(c, out byte[] glyph))
            {
                glyph = Font['?'];
            }

            int x = startX + letter * 6 * scale;
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if ((glyph[row] & (1 << (4 - col))) == 0)
                    {
                        continue;
                    }

                    for (int offsetY = 0; offsetY < scale; offsetY++)
                    {
                        for (int offsetX = 0; offsetX < scale; offsetX++)
                        {
                            int px = x + col * scale + offsetX;
                            int py = y + (6 - row) * scale + offsetY;
                            if (px >= 0 && px < Size && py >= 0 && py < Size)
                            {
                                pixels[py * Size + px] = ink;
                            }
                        }
                    }
                }
            }
        }
    }

    // 180-degree image rotation: flip both texture axes to compensate for
    // the case's front-face UV orientation, without allocating another array.
    private static void RotateForCaseFrontUV(Color32[] pixels)
    {
        for (int i = 0, opposite = pixels.Length - 1; i < opposite; i++, opposite--)
        {
            Color32 current = pixels[i];
            pixels[i] = pixels[opposite];
            pixels[opposite] = current;
        }
    }

    private static uint StableHash(string value)
    {
        uint hash = 2166136261u;
        for (int i = 0; i < value.Length; i++)
        {
            hash ^= value[i];
            hash *= 16777619u;
        }

        return hash;
    }
}
