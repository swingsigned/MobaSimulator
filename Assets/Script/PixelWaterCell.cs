// Put this file in Assets/Script/ (NOT in an Editor folder).
// Add it to the same GameObject that has the water cell's SpriteRenderer.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PixelWaterCell : MonoBehaviour
{
    public float fps = 8f;                      // loop length = frames / fps
    public string resourcesFolder = "WaterCells";

    class Sheet
    {
        public Texture2D tex;
        public int frames;
    }

    static Dictionary<string, Sheet> sheets;

    SpriteRenderer sr;
    Sprite[] frames;
    int last = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        sheets = null;
    }

    // Same key is used by the baker, so a cell finds its own texture by its position.
    public static string Key(Bounds b)
    {
        return "cell_" + Mathf.RoundToInt(b.center.x * 100f) + "_" + Mathf.RoundToInt(b.center.y * 100f);
    }

    static void LoadSheets(string folder)
    {
        if (sheets != null) return;
        sheets = new Dictionary<string, Sheet>();

        foreach (Texture2D t in Resources.LoadAll<Texture2D>(folder))
        {
            int i = t.name.LastIndexOf("_f");
            int n;
            if (i < 0 || !int.TryParse(t.name.Substring(i + 2), out n) || n <= 0) continue;
            sheets[t.name.Substring(0, i)] = new Sheet { tex = t, frames = n };
        }
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr.sprite == null) { enabled = false; return; }

        LoadSheets(resourcesFolder);

        Bounds b = sr.bounds;
        Sheet sheet;
        if (!sheets.TryGetValue(Key(b), out sheet)) { enabled = false; return; }

        int w = sheet.tex.width / sheet.frames;
        int h = sheet.tex.height;

        // Make the new sprite cover exactly the old sprite's rectangle.
        Vector3 s = transform.lossyScale;
        float localW = b.size.x / Mathf.Max(0.0001f, Mathf.Abs(s.x));
        float ppu = w / localW;
        Vector2 pivot = new Vector2(
            (transform.position.x - b.min.x) / b.size.x,
            (transform.position.y - b.min.y) / b.size.y);

        frames = new Sprite[sheet.frames];
        for (int i = 0; i < frames.Length; i++)
        {
            frames[i] = Sprite.Create(
                sheet.tex, new Rect(i * w, 0, w, h),
                pivot, ppu, 0, SpriteMeshType.FullRect);
        }
    }

    void Update()
    {
        if (frames == null) return;

        // Global clock: every water cell shows the same frame at the same time.
        int idx = (int)(Time.time * fps) % frames.Length;
        if (idx != last)
        {
            sr.sprite = frames[idx];
            last = idx;
        }
    }
}
