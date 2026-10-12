// Put this file in Assets/Editor/
using System.IO;
using UnityEditor;
using UnityEngine;

public class PixelWaterBaker : EditorWindow
{
    Material material;
    int size = 128;          // output resolution (128 = the shader's virtual canvas)
    int frames = 64;         // number of frames in one seamless loop
    string outFolder = "Assets/Sprite/WaterFrames";
    bool makeSpriteSheet = true;

    [MenuItem("Tools/Pixel Water Baker")]
    static void Open()
    {
        GetWindow<PixelWaterBaker>("Pixel Water Baker");
    }

    void OnGUI()
    {
        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
        size = EditorGUILayout.IntField("Size (px)", size);
        frames = EditorGUILayout.IntField("Frames", frames);
        outFolder = EditorGUILayout.TextField("Output Folder", outFolder);
        makeSpriteSheet = EditorGUILayout.Toggle("Also make sprite sheet", makeSpriteSheet);

        EditorGUILayout.HelpBox(
            "Frames cover exactly one seamless loop (phase 0 -> 1). " +
            "Loop length when played = Frames / FPS.", MessageType.Info);

        GUI.enabled = material != null && size > 0 && frames > 0;
        if (GUILayout.Button("Bake")) Bake();
        GUI.enabled = true;
    }

    void Bake()
    {
        // Work on a copy so the original material is not modified.
        Material m = new Material(material);
        m.SetFloat("_UseManualPhase", 1f);

        Directory.CreateDirectory(outFolder);

        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Point;
        RenderTexture prev = RenderTexture.active;

        Texture2D sheet = makeSpriteSheet
            ? new Texture2D(size * frames, size, TextureFormat.RGBA32, false)
            : null;

        for (int i = 0; i < frames; i++)
        {
            EditorUtility.DisplayProgressBar("Baking water", $"Frame {i + 1}/{frames}", (float)i / frames);

            m.SetFloat("_ManualPhase", (float)i / frames);
            Graphics.Blit(null, rt, m);

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();

            File.WriteAllBytes(Path.Combine(outFolder, $"water_{i:000}.png"), tex.EncodeToPNG());

            if (sheet != null)
                sheet.SetPixels(i * size, 0, size, size, tex.GetPixels());

            DestroyImmediate(tex);
        }

        if (sheet != null)
        {
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(outFolder, "water_sheet.png"), sheet.EncodeToPNG());
            DestroyImmediate(sheet);
        }

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        DestroyImmediate(m);
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        Debug.Log($"Pixel Water baked {frames} frames to {outFolder}");
    }
}
