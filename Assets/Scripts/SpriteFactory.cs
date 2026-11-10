using UnityEngine;

/// <summary>
/// Creates every graphic used by the game procedurally (no image files needed).
/// Basic shapes are generated once; characters, trees, obstacles etc. are composed from them.
/// </summary>
public static class SpriteFactory
{
    static Sprite square, circle, triangle, glow, ring, gradV, rounded, horseshoe;
    static Material meshMat;

    public static Sprite Square { get { if (square == null) square = BuildSquare(); return square; } }
    public static Sprite Circle { get { if (circle == null) circle = BuildCircle(); return circle; } }
    public static Sprite Triangle { get { if (triangle == null) triangle = BuildTriangle(); return triangle; } }
    public static Sprite Glow { get { if (glow == null) glow = BuildGlow(); return glow; } }
    public static Sprite Ring { get { if (ring == null) ring = BuildRing(); return ring; } }
    public static Sprite GradientV { get { if (gradV == null) gradV = BuildGradV(); return gradV; } }
    public static Sprite Rounded { get { if (rounded == null) rounded = BuildRounded(); return rounded; } }
    public static Sprite Horseshoe { get { if (horseshoe == null) horseshoe = BuildHorseshoe(); return horseshoe; } }

    // ---------------------------------------------------------------- colour helpers
    public static Color Hex(string s) { Color c; ColorUtility.TryParseHtmlString(s, out c); return c; }
    public static Color Darken(Color c, float k) { return new Color(c.r * (1f - k), c.g * (1f - k), c.b * (1f - k), c.a); }
    public static Color Lighten(Color c, float k) { return new Color(Mathf.Lerp(c.r, 1f, k), Mathf.Lerp(c.g, 1f, k), Mathf.Lerp(c.b, 1f, k), c.a); }
    public static Color Alpha(Color c, float a) { return new Color(c.r, c.g, c.b, a); }

    // ---------------------------------------------------------------- composition helpers
    public static SpriteRenderer Part(Transform parent, Sprite s, Color c, float x, float y, float sx, float sy, float rot, int order, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(x, y, 0f);
        go.transform.localRotation = Quaternion.Euler(0f, 0f, rot);
        go.transform.localScale = new Vector3(sx, sy, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = c;
        sr.sortingOrder = order;
        return sr;
    }

    public static SpriteRenderer R(Transform p, Color c, float x, float y, float w, float h, int order = 0, float rot = 0f)
    { return Part(p, Square, c, x, y, w, h, rot, order, "r"); }

    public static SpriteRenderer C(Transform p, Color c, float x, float y, float w, float h, int order = 0, float rot = 0f)
    { return Part(p, Circle, c, x, y, w, h, rot, order, "c"); }

    public static SpriteRenderer T(Transform p, Color c, float x, float y, float w, float h, int order = 0, float rot = 0f)
    { return Part(p, Triangle, c, x, y, w, h, rot, order, "t"); }

    public static SpriteRenderer G(Transform p, Color c, float x, float y, float size, int order = 0)
    { return Part(p, Glow, c, x, y, size, size, 0f, order, "g"); }

    public static void AddOrder(Transform root, int add)
    {
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += add;
    }

    // ---------------------------------------------------------------- meshes (sky, ground, hills)
    static Material MeshMaterial
    {
        get
        {
            if (meshMat == null)
            {
                var sh = Shader.Find("Sprites/Default");
                meshMat = new Material(sh);
            }
            return meshMat;
        }
    }

    public static Mesh NewMesh(string name, Transform parent, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var mesh = new Mesh();
        mesh.name = name;
        mf.sharedMesh = mesh;
        mr.sharedMaterial = MeshMaterial;
        mr.sortingOrder = order;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
        return mesh;
    }

    // ---------------------------------------------------------------- texture builders
    static Texture2D NewTex(int w, int h)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    static Sprite Finish(Texture2D t, Color32[] px, float ppu, Vector4 border)
    {
        t.SetPixels32(px);
        t.Apply();
        return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect, border);
    }

    static Color32 W(float a) { return new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f)); }

    static Sprite BuildSquare()
    {
        var t = NewTex(4, 4);
        var px = new Color32[16];
        for (int i = 0; i < 16; i++) px[i] = W(1f);
        return Finish(t, px, 4f, Vector4.zero);
    }

    static Sprite BuildCircle()
    {
        const int N = 128;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float r = N * 0.5f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                px[y * N + x] = W(r - d);
            }
        return Finish(t, px, N, Vector4.zero);
    }

    static Sprite BuildTriangle()
    {
        const int N = 128;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float r = N * 0.5f;
        for (int y = 0; y < N; y++)
        {
            float h = (y + 0.5f) / N;
            float half = (1f - h) * r;
            for (int x = 0; x < N; x++)
            {
                float a = half - Mathf.Abs(x + 0.5f - r) + 0.5f;
                px[y * N + x] = W(a);
            }
        }
        return Finish(t, px, N, Vector4.zero);
    }

    static Sprite BuildGlow()
    {
        const int N = 128;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float r = N * 0.5f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / r;
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2f);
                px[y * N + x] = W(a);
            }
        return Finish(t, px, N, Vector4.zero);
    }

    static Sprite BuildRing()
    {
        const int N = 128;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float r = N * 0.5f, rin = r * 0.86f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = x + 0.5f - r, dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                px[y * N + x] = W(Mathf.Clamp01(r - d) * Mathf.Clamp01(d - rin));
            }
        return Finish(t, px, N, Vector4.zero);
    }

    static Sprite BuildGradV()
    {
        var t = NewTex(2, 64);
        var px = new Color32[2 * 64];
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 2; x++)
                px[y * 2 + x] = W((y + 0.5f) / 64f);
        return Finish(t, px, 64f, Vector4.zero);
    }

    static Sprite BuildRounded()
    {
        const int N = 64;
        const float rad = 24f;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float c = N * 0.5f;
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + 0.5f - c) - (c - rad), 0f);
                float dy = Mathf.Max(Mathf.Abs(y + 0.5f - c) - (c - rad), 0f);
                float d = Mathf.Sqrt(dx * dx + dy * dy) - rad;
                px[y * N + x] = W(0.5f - d);
            }
        return Finish(t, px, 100f, new Vector4(28, 28, 28, 28));
    }

    static Sprite BuildHorseshoe()
    {
        const int N = 128;
        var t = NewTex(N, N);
        var px = new Color32[N * N];
        float cx = 64f, cy = 64f;
        Color red = new Color(0.9f, 0.18f, 0.2f, 1f);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float xf = x + 0.5f, yf = y + 0.5f;
                float dx = xf - cx, dy = yf - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float aRing = (yf < cy) ? Mathf.Clamp01(56f - d) * Mathf.Clamp01(d - 32f) : 0f;
                float aBar = 0f;
                if (yf >= cy && yf < 118f)
                {
                    float l = Mathf.Clamp01(Mathf.Min(xf - 8f, 32f - xf) + 0.5f);
                    float r = Mathf.Clamp01(Mathf.Min(xf - 96f, 120f - xf) + 0.5f);
                    aBar = Mathf.Max(l, r) * Mathf.Clamp01(118f - yf);
                }
                float a = Mathf.Max(aRing, aBar);
                Color col = yf >= 96f ? Color.white : red;
                px[y * N + x] = new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)Mathf.RoundToInt(a * 255f));
            }
        return Finish(t, px, N, Vector4.zero);
    }
}
