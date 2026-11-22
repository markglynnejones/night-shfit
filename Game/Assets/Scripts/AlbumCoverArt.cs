using System;
using System.Collections.Generic;
using UnityEngine;

// Original procedural sleeves for the first vertical slice.
// Shared per catalogue ID, so hundreds of stock copies don't allocate duplicate textures.
[RequireComponent(typeof(MediaItem))]
public sealed class AlbumCoverArt : MonoBehaviour
{
    private sealed class Sleeve
    {
        public Material Cover;
        public Material Spine;
    }

    private static readonly Dictionary<string, Sleeve> Cached = new();

    private void Start()
    {
        MediaItem item = GetComponent<MediaItem>();
        if (item == null || item.AlbumDefinition == null)
        {
            return;
        }

        string id = item.AlbumDefinition.CatalogueId;
        if (!Cached.TryGetValue(id, out Sleeve sleeve))
        {
            sleeve = GenerateSleeve(id);
            if (sleeve == null)
            {
                return;
            }

            Cached.Add(id, sleeve);
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

    private static Sleeve GenerateSleeve(string id)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            Debug.LogError("Could not create album artwork: missing URP shader.");
            return null;
        }

        uint hash = StableHash(id);
        float hue = (hash % 360u) / 360f;
        Color shadow = Color.HSVToRGB(hue, 0.65f, 0.35f);
        Color mid = Color.HSVToRGB((hue + 0.09f) % 1f, 0.63f, 0.60f);
        Color highlight = Color.HSVToRGB((hue + 0.52f) % 1f, 0.44f, 0.93f);

        const int size = 128;
        Color32[] pixels = new Color32[size * size];
        int variation = (int)(hash % 4u);
        float skew = 0.6f + ((hash >> 4) % 9u) * 0.12f;

        for (int y = 0; y < size; y++)
        {
            float fy = y / (float)(size - 1);
            for (int x = 0; x < size; x++)
            {
                float fx = x / (float)(size - 1);
                Color color = Color.Lerp(shadow, mid, Mathf.Clamp01(fy * 0.6f + fx * 0.3f));
                float distance = Vector2.Distance(new Vector2(fx, fy), new Vector2(0.5f, 0.55f));

                if (variation == 0)
                {
                    if (Mathf.Sin((fx * skew + fy * 0.7f) * 32f) > 0.55f)
                    {
                        color = Color.Lerp(color, highlight, 0.68f);
                    }
                }
                else if (variation == 1)
                {
                    if (Mathf.Abs(Mathf.Sin(distance * 25f)) > 0.8f)
                    {
                        color = Color.Lerp(color, highlight, 0.88f);
                    }
                }
                else if (variation == 2)
                {
                    float horizon = 0.4f + Mathf.Sin(fx * 11f + skew) * 0.12f;
                    if (Mathf.Abs(fy - horizon) < 0.08f || fy > 0.86f)
                    {
                        color = Color.Lerp(color, highlight, 0.78f);
                    }
                }
                else
                {
                    float horizontal = Mathf.Abs(Mathf.Sin((fy + skew * fx) * 24f));
                    float vertical = Mathf.Abs(Mathf.Sin((fx - fy * 0.15f) * 19f));
                    if (horizontal > 0.9f || vertical > 0.94f)
                    {
                        color = Color.Lerp(color, highlight, 0.75f);
                    }
                }

                // A little printed-paper texture, not expensive runtime noise.
                float grain = ((x * 17 + y * 31 + (int)(hash & 255u)) % 19) / 210f;
                pixels[y * size + x] = Color.Lerp(color, Color.white, grain);
            }
        }

        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            name = "Fictional Sleeve " + id,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        Material cover = new(shader) { name = "Sleeve " + id };
        cover.SetColor("_BaseColor", Color.white);
        cover.SetTexture("_BaseMap", texture);
        Material spine = new(shader) { name = "Spine " + id };
        spine.SetColor("_BaseColor", highlight);

        return new Sleeve { Cover = cover, Spine = spine };
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
