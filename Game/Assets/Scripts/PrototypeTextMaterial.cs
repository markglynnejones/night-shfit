using UnityEngine;
using UnityEngine.Rendering;

public static class PrototypeTextMaterial
{
    public static void Apply(TextMesh textMesh, Color color)
    {
        MeshRenderer renderer = textMesh.GetComponent<MeshRenderer>();
        if (renderer == null || textMesh.font == null || textMesh.font.material == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            return;
        }

        Material material = new Material(shader)
        {
            name = "Prototype Depth Tested Text",
            renderQueue = (int)RenderQueue.AlphaTest
        };

        Texture fontTexture = textMesh.font.material.mainTexture;
        material.SetTexture("_BaseMap", fontTexture);
        material.SetColor("_BaseColor", color);
        material.SetFloat("_AlphaClip", 1f);
        material.SetFloat("_Cutoff", 0.5f);
        material.EnableKeyword("_ALPHATEST_ON");

        renderer.sharedMaterial = material;
        textMesh.color = Color.white;
    }
}
