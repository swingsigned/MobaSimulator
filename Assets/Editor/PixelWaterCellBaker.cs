// Put this file in Assets/Editor/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PixelWaterCellBaker : EditorWindow
{
    Material material;
    Transform root;
    string nameFilter = "Water";
    float pixelsPerUnit = 128f;
    int frames = 16;
    string outFolder = "Assets/Resources/WaterCells";

    class Cell
    {
        public SpriteRenderer sr;
        public Bounds b;
    }

    [MenuItem("Tools/Pixel Water Cell Baker")]
    static void Open()
    {
        GetWindow<PixelWaterCellBaker>("Water Cell Baker");
    }

    void OnGUI()
    {
        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);
        root = (Transform)EditorGUILayout.ObjectField("Cells Root (MAP)", root, typeof(Transform), true);
        nameFilter = EditorGUILayout.TextField("Name contains", nameFilter);
        pixelsPerUnit = EditorGUILayout.FloatField("Pixels per world unit", pixelsPerUnit);
        frames = EditorGUILayout.IntField("Frames per loop", frames);
        outFolder = EditorGUILayout.TextField("Output Folder", outFolder);

        EditorGUILayout.HelpBox(
            "Run this while the cells exist in the scene (usually in Play Mode, after the map is spawned). " +
            "Each water cell gets its own sprite sheet, cut from one shared pixel grid so neighbours match. " +
            "The cell's current sprite is used as the shape mask (hex).",
            MessageType.Info);

        GUI.enabled = material != null && root != null && frames > 0 && pixelsPerUnit > 0f;
        if (GUILayout.Button("Bake all water cells")) Bake();
        GUI.enabled = true;
    }

    void Bake()
    {
        // 1. Collect the water cells under root.
        List<Cell> cells = new List<Cell>();
        foreach (Transform child in root)
        {
            if (!child.name.Contains(nameFilter)) continue;
            SpriteRenderer sr = child.GetComponentInChildren<SpriteRenderer>();
            if (sr == null || sr.sprite == null) continue;
            cells.Add(new Cell { sr = sr, b = sr.bounds });
        }

        if (cells.Count == 0)
        {
            EditorUtility.DisplayDialog("Water Cell Baker", "No cells found. Check the Root and the name filter.", "OK");
            return;
        }

        // 2. Common origin of the shared pixel grid.
        Vector2 mapMin = new Vector2(cells[0].b.min.x, cells[0].b.min.y);
        foreach (Cell c in cells)
            mapMin = new Vector2(Mathf.Min(mapMin.x, c.b.min.x), Mathf.Min(mapMin.y, c.b.min.y));

        Directory.CreateDirectory(outFolder);

        Material m = new Material(material);
        m.SetFloat("_UseManualPhase", 1f);

        RenderTexture prevActive = RenderTexture.active;
        HashSet<string> seen = new HashSet<string>();
        int baked = 0;

        for (int ci = 0; ci < cells.Count; ci++)
        {
            Cell c = cells[ci];
            string key = PixelWaterCell.Key(c.b);
            if (!seen.Add(key)) continue; // two cells at the same position

            EditorUtility.DisplayProgressBar("Baking water cells", key, (float)ci / cells.Count);

            int w = Mathf.Max(1, Mathf.RoundToInt(c.b.size.x * pixelsPerUnit));
            int h = Mathf.Max(1, Mathf.RoundToInt(c.b.size.y * pixelsPerUnit));

            if (w * frames > 16384)
            {
                Debug.LogError("Sheet would be wider than 16384 px. Lower Frames or Pixels per world unit.");
                break;
            }

            Vector2 origin = new Vector2(
                Mathf.Round((c.b.min.x - mapMin.x) * pixelsPerUnit),
                Mathf.Round((c.b.min.y - mapMin.y) * pixelsPerUnit));

            m.SetVector("_Res", new Vector4(w, h, 0f, 0f));
            m.SetVector("_Origin", new Vector4(origin.x, origin.y, 0f, 0f));

            float[] mask = ReadAlphaMask(c.sr.sprite, w, h);

            RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Point;

            Texture2D sheet = new Texture2D(w * frames, h, TextureFormat.RGBA32, false);
            Texture2D tmp = new Texture2D(w, h, TextureFormat.RGBA32, false);

            for (int f = 0; f < frames; f++)
            {
                m.SetFloat("_ManualPhase", (float)f / frames);
                Graphics.Blit(null, rt, m);

                RenderTexture.active = rt;
                tmp.ReadPixels(new Rect(0, 0, w, h), 0, 0);

                Color[] px = tmp.GetPixels();
                if (mask != null)
                    for (int i = 0; i < px.Length; i++) px[i].a = mask[i];

                sheet.SetPixels(f * w, 0, w, h, px);
            }

            sheet.Apply();
            File.WriteAllBytes(Path.Combine(outFolder, key + "_f" + frames + ".png"), sheet.EncodeToPNG());

            RenderTexture.active = prevActive;
            RenderTexture.ReleaseTemporary(rt);
            DestroyImmediate(sheet);
            DestroyImmediate(tmp);
            baked++;
        }

        RenderTexture.active = prevActive;
        DestroyImmediate(m);
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        Debug.Log("Pixel Water: baked " + baked + " cells x " + frames + " frames into " + outFolder);
    }

    // Reads the alpha of a sprite (works even if the texture is not Read/Write enabled)
    // and resamples it (nearest) to w x h.
    static float[] ReadAlphaMask(Sprite s, int w, int h)
    {
        Texture2D src = s.texture;
        Rect r = s.textureRect;
        int rw = Mathf.RoundToInt(r.width);
        int rh = Mathf.RoundToInt(r.height);
        if (rw <= 0 || rh <= 0) return null;

        RenderTexture tmpRT = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, tmpRT);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = tmpRT;

        Texture2D read = new Texture2D(rw, rh, TextureFormat.RGBA32, false);
        read.ReadPixels(new Rect(r.x, r.y, rw, rh), 0, 0);
        read.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(tmpRT);

        Color32[] sp = read.GetPixels32();
        DestroyImmediate(read);

        float[] a = new float[w * h];
        for (int y = 0; y < h; y++)
        {
            int sy = Mathf.Min(rh - 1, (int)((y + 0.5f) / h * rh));
            for (int x = 0; x < w; x++)
            {
                int sx = Mathf.Min(rw - 1, (int)((x + 0.5f) / w * rw));
                a[y * w + x] = sp[sy * rw + sx].a / 255f;
            }
        }
        return a;
    }
}

// Auto import settings for the baked sheets: pixel-perfect, no compression.
public class PixelWaterCellImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("WaterCells")) return;

        TextureImporter ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Default;
        ti.filterMode = FilterMode.Point;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.alphaIsTransparency = true;
        ti.maxTextureSize = 16384;
    }
}
