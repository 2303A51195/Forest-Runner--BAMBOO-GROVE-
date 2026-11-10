using System.Collections.Generic;
using UnityEngine;
using static SpriteFactory;

/// <summary>
/// The animated forest: sky, sun, hills, ground and path (meshes), scrolling path markings, scenery on both
/// sides, bridges, light shafts, clouds and drifting particles. Four colour themes cross-fade.
/// It is used both in the Main Menu (as a backdrop) and in the game.
/// </summary>
public class EnvironmentController : MonoBehaviour
{
    public float menuSpeed = 11f;
    public bool cycleThemesInMenu;

    class Stripe { public Transform root; public SpriteRenderer band, dashL, dashR; public float z; }
    class Mote { public Transform t; public SpriteRenderer sr; public Vector2 pos; public float vy, sway, phase, size, rot; }

    Mesh skyM, hillFarM, hillNearM, grassM, vergeM, pathM;
    Transform hillFarT, hillNearT;
    ForestTheme fromTheme, to, cur;
    float blend = 1f;
    int themeIndex = -1;

    SpriteRenderer sunGlow, sunDisk, fog;
    SpriteRenderer[] rays = new SpriteRenderer[4];
    readonly List<SpriteRenderer> clouds = new List<SpriteRenderer>();
    readonly List<Stripe> stripes = new List<Stripe>();
    readonly List<Mote> motes = new List<Mote>();
    readonly List<WorldEntity> scenery = new List<WorldEntity>();

    float sceneryAcc, plankAcc, traveled, nextBridge, menuT, time;
    int bridgeLeft;
    bool leftSide;

    const float Far = 14f;

    public void Init()
    {
        fromTheme = to = cur = ForestTheme.All[0];
        BuildMeshes();
        BuildSun();
        BuildClouds();
        BuildStripes();
        BuildMotes();

        fog = R(transform, Color.white, 0f, LaneManager.HorizonY - 1.0f, 960f, 2f, 1000); // gradient sprite is 1/32 unit wide
        fog.sprite = GradientV;

        for (int i = 0; i < 4; i++)
        {
            rays[i] = R(transform, new Color(1f, 1f, 0.8f, 0.1f), -3f + i * 2.4f, 3f, 0.9f + i * 0.2f, 18f, 1200, -22f);
        }

        nextBridge = Random.Range(350f, 600f);
        SetTheme(0, true);

        // pre-populate scenery so the forest is not empty at start
        for (float z = 4f; z < LaneManager.SpawnZ; z += 3.2f) SpawnScenery(z);
    }

    // ------------------------------------------------------------------ build
    void BuildMeshes()
    {
        skyM = NewMesh("Sky", transform, -400);
        hillFarM = NewMesh("HillsFar", transform, -395);
        hillNearM = NewMesh("HillsNear", transform, -390);
        grassM = NewMesh("Grass", transform, -380);
        vergeM = NewMesh("Verge", transform, -370);
        pathM = NewMesh("Path", transform, -360);

        float hy = LaneManager.HorizonY;
        // sky
        skyM.vertices = new Vector3[] { new Vector3(-Far, hy - 0.3f), new Vector3(Far, hy - 0.3f), new Vector3(Far, 13f), new Vector3(-Far, 13f) };
        skyM.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        // grass
        grassM.vertices = new Vector3[] { new Vector3(-Far, -17f), new Vector3(Far, -17f), new Vector3(Far, hy), new Vector3(-Far, hy) };
        grassM.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        // verge + path: converge to the vanishing point on the horizon
        float fBottom = (hy + 17f) / (hy - LaneManager.GroundY);
        float vw = (LaneManager.PathHalfWidth + 0.8f) * fBottom;
        float pw = LaneManager.PathHalfWidth * fBottom;
        vergeM.vertices = new Vector3[] { new Vector3(-vw, -17f), new Vector3(vw, -17f), new Vector3(0.14f, hy), new Vector3(-0.14f, hy) };
        vergeM.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        pathM.vertices = new Vector3[] { new Vector3(-pw, -17f), new Vector3(pw, -17f), new Vector3(0.1f, hy), new Vector3(-0.1f, hy) };
        pathM.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        BuildHills(hillFarM, 0.15f, 1.7f, 0.55f, 0.7f);
        BuildHills(hillNearM, 0f, 1.0f, 0.8f, 2.1f);
        hillFarT = FindMeshTransform(hillFarM);
        hillNearT = FindMeshTransform(hillNearM);
    }

    Transform FindMeshTransform(Mesh m)
    {
        foreach (Transform c in transform) { var mf = c.GetComponent<MeshFilter>(); if (mf != null && mf.sharedMesh == m) return c; }
        return transform;
    }

    void BuildHills(Mesh m, float baseOffset, float amp, float freq, float phase)
    {
        const int N = 40;
        var v = new Vector3[N * 2];
        var tri = new List<int>();
        float baseY = LaneManager.HorizonY + baseOffset;
        for (int i = 0; i < N; i++)
        {
            float x = Mathf.Lerp(-Far, Far, i / (float)(N - 1));
            float h = amp * (0.6f * Mathf.Sin(x * freq + phase) + 0.4f * Mathf.Sin(x * freq * 2.3f + phase * 1.7f) + 1f) * 0.5f + 0.2f;
            v[i * 2] = new Vector3(x, LaneManager.HorizonY - 0.3f, 0f);
            v[i * 2 + 1] = new Vector3(x, baseY + h, 0f);
            if (i < N - 1)
            {
                int a = i * 2;
                tri.Add(a); tri.Add(a + 1); tri.Add(a + 2);
                tri.Add(a + 1); tri.Add(a + 3); tri.Add(a + 2);
            }
        }
        m.vertices = v;
        m.triangles = tri.ToArray();
    }

    void BuildSun()
    {
        var sun = new GameObject("Sun").transform;
        sun.SetParent(transform, false);
        sun.localPosition = new Vector3(2.0f, 5.9f, 0f);
        sunGlow = G(sun, new Color(1f, 0.95f, 0.6f, 0.55f), 0f, 0f, 8f, -399);
        sunDisk = C(sun, Color.white, 0f, 0f, 1.7f, 1.7f, -398);
    }

    void BuildClouds()
    {
        for (int i = 0; i < 4; i++)
        {
            var cl = new GameObject("Cloud").transform;
            cl.SetParent(transform, false);
            cl.localPosition = new Vector3(-6f + i * 4f, 4.8f + (i % 2) * 1.6f, 0f);
            float s = 0.8f + (i % 3) * 0.3f;
            var a = C(cl, new Color(1f, 1f, 1f, 0.7f), 0f, 0f, 2.2f * s, 0.9f * s, -392);
            C(cl, new Color(1f, 1f, 1f, 0.7f), 0.8f * s, 0.15f * s, 1.5f * s, 0.8f * s, -392);
            C(cl, new Color(1f, 1f, 1f, 0.7f), -0.8f * s, 0.1f * s, 1.4f * s, 0.7f * s, -392);
            clouds.Add(a);
        }
    }

    void BuildStripes()
    {
        const int N = 18;
        const float spacing = 5f;
        for (int i = 0; i < N; i++)
        {
            var s = new Stripe();
            var go = new GameObject("Stripe");
            go.transform.SetParent(transform, false);
            s.root = go.transform;
            s.band = R(s.root, new Color(0f, 0f, 0f, 0.1f), 0f, 0f, 7.6f, 0.5f, -330);
            s.dashL = R(s.root, new Color(1f, 1f, 1f, 0.3f), -1.25f, 0f, 0.16f, 1.6f, -329);
            s.dashR = R(s.root, new Color(1f, 1f, 1f, 0.3f), 1.25f, 0f, 0.16f, 1.6f, -329);
            s.z = i * spacing;
            stripes.Add(s);
        }
    }

    void BuildMotes()
    {
        for (int i = 0; i < 26; i++)
        {
            var m = new Mote();
            var go = new GameObject("Mote");
            go.transform.SetParent(transform, false);
            m.t = go.transform;
            m.sr = go.AddComponent<SpriteRenderer>();
            m.sr.sprite = Circle;
            m.sr.sortingOrder = 1800;
            m.pos = new Vector2(Random.Range(-5f, 5f), Random.Range(-8f, 8f));
            m.vy = Random.Range(0.3f, 1f);
            m.sway = Random.Range(0.5f, 1.6f);
            m.phase = Random.Range(0f, 6.28f);
            m.size = Random.Range(0.12f, 0.26f);
            m.rot = Random.Range(-80f, 80f);
            motes.Add(m);
        }
    }

    // ------------------------------------------------------------------ theme
    public void SetTheme(int idx, bool instant)
    {
        idx = Mathf.Clamp(idx, 0, ForestTheme.All.Length - 1);
        if (idx == themeIndex) return;
        fromTheme = cur;
        to = ForestTheme.All[idx];
        themeIndex = idx;
        blend = instant ? 1f : 0f;
        if (instant) cur = to;
        ApplyColors();
    }

    void ApplyColors()
    {
        var c = cur;
        Color far = Color.Lerp(c.grass, c.skyHorizon, 0.65f);
        SetMeshColors(skyM, c.skyHorizon, c.skyHorizon, c.skyTop, c.skyTop);
        SetMeshColors(grassM, c.grass, c.grass, far, far);
        SetMeshColors(vergeM, c.verge, c.verge, Color.Lerp(c.verge, c.skyHorizon, 0.65f), Color.Lerp(c.verge, c.skyHorizon, 0.65f));
        Color pf = Color.Lerp(c.path, c.skyHorizon, 0.7f);
        SetMeshColors(pathM, c.path, c.path, pf, pf);
        Color hf = c.hillFar, hn = c.hillNear;
        SetMeshColors(hillFarM, hf);
        SetMeshColors(hillNearM, hn);

        if (sunDisk != null) sunDisk.color = c.sun;
        if (sunGlow != null) sunGlow.color = new Color(c.sun.r, c.sun.g, c.sun.b, 0.5f);
        if (fog != null) fog.color = c.skyHorizon;
        foreach (var s in stripes)
        {
            s.band.color = new Color(c.stripe.r, c.stripe.g, c.stripe.b, 0.28f);
        }
        foreach (var m in motes) m.sr.color = new Color(c.ambient.r, c.ambient.g, c.ambient.b, 0.85f);
        if (CameraController.Instance != null) CameraController.Instance.SetBackground(c.skyTop);
    }

    static void SetMeshColors(Mesh m, Color a, Color b, Color c, Color d)
    {
        m.colors = new Color[] { a, b, c, d };
    }

    static void SetMeshColors(Mesh m, Color all)
    {
        var cols = new Color[m.vertexCount];
        for (int i = 0; i < cols.Length; i++) cols[i] = all;
        m.colors = cols;
    }

    // ------------------------------------------------------------------ update
    void Update()
    {
        float dt = Time.deltaTime;
        time += dt;
        var gm = GameManager.Instance;
        float speed = gm != null ? gm.Speed : menuSpeed;

        if (gm != null) SetTheme(gm.ThemeIndex, false);
        else if (cycleThemesInMenu)
        {
            menuT += dt;
            if (menuT > 9f) { menuT = 0f; SetTheme((themeIndex + 1) % ForestTheme.All.Length, false); }
        }

        if (blend < 1f)
        {
            blend = Mathf.Min(1f, blend + dt / 2.2f);
            cur = ForestTheme.Lerp(fromTheme, to, blend);
            ApplyColors();
        }

        traveled += speed * dt;
        UpdateStripes(dt, speed);
        UpdateScenery(dt, speed);
        UpdateMotes(dt);
        UpdateSky(dt);

        float px = PlayerController.Instance != null ? PlayerController.Instance.X : 0f;
        if (hillFarT != null) hillFarT.localPosition = new Vector3(-px * 0.05f, 0f, 0f);
        if (hillNearT != null) hillNearT.localPosition = new Vector3(-px * 0.1f, 0f, 0f);
    }

    void UpdateSky(float dt)
    {
        for (int i = 0; i < rays.Length; i++)
        {
            float a = 0.07f + 0.05f * Mathf.Sin(time * 0.6f + i * 1.7f);
            rays[i].color = new Color(1f, 0.97f, 0.8f, a);
        }
        foreach (var c in clouds)
        {
            var p = c.transform.parent.localPosition;
            p.x += 0.25f * dt;
            if (p.x > 8f) p.x = -8f;
            c.transform.parent.localPosition = p;
        }
        if (sunGlow != null)
        {
            float s = 8f + Mathf.Sin(time * 0.8f) * 0.4f;
            sunGlow.transform.localScale = new Vector3(s, s, 1f);
        }
    }

    void UpdateStripes(float dt, float speed)
    {
        float span = stripes.Count * 5f;
        foreach (var s in stripes)
        {
            s.z -= speed * dt;
            if (s.z < -4f) s.z += span;
            float f = LaneManager.F(s.z);
            s.root.localPosition = new Vector3(0f, LaneManager.ScreenY(s.z), 0f);
            s.root.localScale = new Vector3(f, f, 1f);
        }
    }

    void UpdateScenery(float dt, float speed)
    {
        sceneryAcc += speed * dt;
        while (sceneryAcc >= 2.2f)
        {
            sceneryAcc -= 2.2f;
            SpawnScenery(LaneManager.SpawnZ + Random.Range(0f, 1.5f));
        }

        // bridges
        if (bridgeLeft <= 0 && traveled > nextBridge)
        {
            bridgeLeft = 14;
            plankAcc = 0f;
            nextBridge = traveled + Random.Range(600f, 1000f);
        }
        if (bridgeLeft > 0)
        {
            plankAcc += speed * dt;
            while (plankAcc >= 1.6f && bridgeLeft > 0)
            {
                plankAcc -= 1.6f;
                bridgeLeft--;
                SpawnPiece("plank", LaneManager.SpawnZ, 0f, 1f, false);
            }
        }

        for (int i = scenery.Count - 1; i >= 0; i--)
        {
            var e = scenery[i];
            if (e == null) { scenery.RemoveAt(i); continue; }
            e.Tick(dt, speed);
            if (e.z < -6f)
            {
                scenery.RemoveAt(i);
                ObjectPooler.Instance.Release(e.gameObject);
                continue;
            }
            e.ApplyLayout();
        }
    }

    void SpawnScenery(float z)
    {
        int side = leftSide ? -1 : 1;
        leftSide = !leftSide;
        int ti = Mathf.Max(0, themeIndex);
        float r = Random.value;
        string kind; float ax; float sc = 1f;

        if (r < 0.32f)
        {
            switch (ti)
            {
                case 1: kind = "bamboo"; break;
                case 2: kind = Random.value < 0.4f ? "mushroom" : "tree"; if (kind == "mushroom") sc = Random.Range(1.4f, 2.4f); break;
                case 3: kind = Random.value < 0.35f ? "hut" : "tree"; break;
                default: kind = "tree"; break;
            }
            ax = kind == "hut" ? Random.Range(6.5f, 8.5f) : Random.Range(6.2f, 10.5f);
        }
        else if (r < 0.50f) { kind = "bush"; ax = Random.Range(4.7f, 7f); }
        else if (r < 0.72f) { kind = "flowers"; ax = Random.Range(4.5f, 6.3f); }
        else if (r < 0.82f) { kind = "rock"; ax = Random.Range(4.5f, 6f); }
        else if (r < 0.90f) { kind = "tuft"; ax = Random.Range(4.4f, 5.8f); }
        else
        {
            switch (ti)
            {
                case 1: kind = "bamboo"; sc = 0.8f; break;
                case 2: kind = "mushroom"; sc = Random.Range(0.8f, 1.2f); break;
                case 3: kind = "lantern"; break;
                default: kind = "bush"; break;
            }
            ax = Random.Range(4.6f, 6.5f);
        }
        SpawnPiece(kind, z, side * ax, sc * Random.Range(0.85f, 1.2f), Random.value < 0.5f);
    }

    void SpawnPiece(string kind, float z, float x, float scale, bool flip)
    {
        int ti = Mathf.Max(0, themeIndex);
        string key = kind + "_" + ti;
        var go = ObjectPooler.Instance.Get(key, () => SceneryFactory.Build(kind, ForestTheme.All[ti]));
        var e = go.GetComponent<WorldEntity>();
        e.z = z; e.worldX = x; e.elev = 0f; e.baseScale = scale; e.flipped = flip; e.consumed = false;
        scenery.Add(e);
        e.ApplyLayout();
    }

    void UpdateMotes(float dt)
    {
        int ti = Mathf.Max(0, themeIndex);
        bool twinkle = ti == 0 || ti == 2;
        float dir = ti == 1 ? -1f : (ti == 3 ? -0.35f : (ti == 0 ? 0.2f : 0.5f));
        foreach (var m in motes)
        {
            m.pos.y += dir * m.vy * dt;
            m.pos.x += Mathf.Sin(time * m.sway + m.phase) * 0.5f * dt;
            if (m.pos.y > 8.5f) m.pos.y = -8.5f;
            if (m.pos.y < -8.5f) m.pos.y = 8.5f;
            if (m.pos.x > 5.5f) m.pos.x = -5.5f;
            if (m.pos.x < -5.5f) m.pos.x = 5.5f;
            m.t.localPosition = new Vector3(m.pos.x, m.pos.y, 0f);
            float sx = m.size, sy = m.size;
            if (ti == 1) { sx = m.size * 1.8f; sy = m.size * 0.7f; }
            m.t.localScale = new Vector3(sx, sy, 1f);
            m.t.localRotation = Quaternion.Euler(0f, 0f, ti == 1 ? Mathf.Sin(time * m.sway + m.phase) * 60f : 0f);
            if (twinkle)
            {
                Color c = m.sr.color;
                c.a = 0.35f + 0.55f * Mathf.Abs(Mathf.Sin(time * m.sway + m.phase));
                m.sr.color = c;
            }
        }
    }
}
