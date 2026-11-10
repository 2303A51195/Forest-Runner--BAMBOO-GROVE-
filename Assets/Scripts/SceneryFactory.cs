using UnityEngine;
using static SpriteFactory;

/// <summary>Builds scenery objects out of coloured primitive sprites (all original procedural art).</summary>
public static class SceneryFactory
{
    public static GameObject Build(string kind, ForestTheme t)
    {
        var go = new GameObject(kind);
        var sc = go.AddComponent<Scenery>();
        var tr = go.transform;
        switch (kind)
        {
            case "tree": Tree(tr, t); break;
            case "bamboo": Bamboo(tr, t); break;
            case "mushroom": Mushroom(tr, t); break;
            case "hut": Hut(tr, t); break;
            case "lantern": Lantern(tr, t); break;
            case "bush": Bush(tr, t); break;
            case "flowers": Flowers(tr, t); break;
            case "rock": Rock(tr); break;
            case "tuft": Tuft(tr, t); break;
            case "plank": Plank(tr, t); sc.groundLayer = true; break;
            default: Bush(tr, t); break;
        }
        sc.CacheRenderers();
        return go;
    }

    static void Shadow(Transform tr, float w)
    {
        C(tr, new Color(0f, 0f, 0f, 0.2f), 0f, 0.05f, w, w * 0.22f, -2);
    }

    static void Tree(Transform tr, ForestTheme t)
    {
        Shadow(tr, 2.6f);
        R(tr, t.trunk, 0f, 1.4f, 0.55f, 2.8f, 0);
        R(tr, Darken(t.trunk, 0.25f), 0.14f, 1.4f, 0.14f, 2.6f, 1);
        T(tr, t.trunk, -0.45f, 0.2f, 0.6f, 0.5f, 0, 18f);
        T(tr, t.trunk, 0.45f, 0.2f, 0.6f, 0.5f, 0, -18f);
        C(tr, t.leafB, 0f, 3.7f, 3.3f, 3.0f, 2);
        C(tr, t.leafA, -1.0f, 3.15f, 2.2f, 2.0f, 3);
        C(tr, t.leafA, 1.0f, 3.15f, 2.2f, 2.0f, 3);
        C(tr, Lighten(t.leafA, 0.18f), -0.3f, 4.35f, 1.7f, 1.4f, 4);
        C(tr, Darken(t.leafB, 0.15f), 0.6f, 3.0f, 1.2f, 1.0f, 3);
    }

    static void Bamboo(Transform tr, ForestTheme t)
    {
        Shadow(tr, 1.8f);
        float[] xs = { -0.45f, 0.05f, 0.5f };
        float[] hs = { 5.4f, 6.8f, 5.0f };
        for (int i = 0; i < 3; i++)
        {
            float h = hs[i];
            R(tr, t.trunk, xs[i], h * 0.5f, 0.24f, h, i);
            R(tr, Lighten(t.trunk, 0.25f), xs[i] - 0.05f, h * 0.5f, 0.06f, h, i + 1);
            for (float y = 0.8f; y < h; y += 0.95f) R(tr, Darken(t.trunk, 0.3f), xs[i], y, 0.32f, 0.07f, i + 2);
            C(tr, t.leafA, xs[i] - 0.45f, h - 0.2f, 0.9f, 0.28f, i + 1, 25f);
            C(tr, t.leafB, xs[i] + 0.45f, h - 0.5f, 0.9f, 0.28f, i + 1, -25f);
            C(tr, t.leafA, xs[i] + 0.1f, h + 0.1f, 0.8f, 0.26f, i + 1, 80f);
        }
    }

    static void Mushroom(Transform tr, ForestTheme t)
    {
        Shadow(tr, 1.8f);
        G(tr, Alpha(t.accent, 0.45f), 0f, 0.9f, 3.6f, -1);
        R(tr, new Color(0.97f, 0.93f, 0.85f), 0f, 0.4f, 0.4f, 0.8f, 0);
        C(tr, t.leafA, 0f, 0.95f, 1.7f, 1.1f, 1);
        C(tr, Lighten(t.leafA, 0.3f), -0.3f, 1.15f, 0.5f, 0.3f, 2);
        C(tr, Color.white, 0.4f, 1.0f, 0.25f, 0.2f, 2);
        C(tr, Color.white, -0.5f, 0.8f, 0.2f, 0.16f, 2);
        C(tr, Color.white, 0.05f, 1.3f, 0.2f, 0.16f, 2);
    }

    static void Hut(Transform tr, ForestTheme t)
    {
        Shadow(tr, 3.2f);
        Color wood = Hex("#A87445");
        R(tr, wood, 0f, 0.8f, 2.1f, 1.6f, 0);
        for (float x = -0.8f; x <= 0.81f; x += 0.4f) R(tr, Darken(wood, 0.25f), x, 0.8f, 0.04f, 1.6f, 1);
        T(tr, Hex("#8C3B2E"), 0f, 2.2f, 2.9f, 1.3f, 2);
        T(tr, Darken(Hex("#8C3B2E"), 0.25f), 0.6f, 2.0f, 1.2f, 0.9f, 3);
        R(tr, Hex("#3E2A1A"), -0.4f, 0.5f, 0.55f, 1.0f, 3);
        R(tr, Hex("#FFE08A"), 0.55f, 0.95f, 0.45f, 0.45f, 3);
        R(tr, Hex("#6B4A2B"), 0.55f, 0.95f, 0.05f, 0.45f, 4);
        G(tr, new Color(1f, 0.85f, 0.4f, 0.4f), 0.55f, 0.95f, 1.2f, 2);
    }

    static void Lantern(Transform tr, ForestTheme t)
    {
        Shadow(tr, 0.8f);
        R(tr, Hex("#4A3220"), 0f, 1.2f, 0.14f, 2.4f, 0);
        R(tr, Hex("#4A3220"), 0.2f, 2.35f, 0.4f, 0.08f, 0);
        C(tr, Hex("#FFD36B"), 0.38f, 2.15f, 0.3f, 0.36f, 1);
        G(tr, new Color(1f, 0.85f, 0.4f, 0.55f), 0.38f, 2.15f, 2.0f, 2);
    }

    static void Bush(Transform tr, ForestTheme t)
    {
        Shadow(tr, 2.0f);
        C(tr, t.leafB, -0.55f, 0.55f, 1.3f, 1.0f, 0);
        C(tr, t.leafB, 0.6f, 0.5f, 1.2f, 0.9f, 0);
        C(tr, t.leafA, 0f, 0.8f, 1.5f, 1.2f, 1);
        C(tr, Lighten(t.leafA, 0.2f), -0.2f, 1.1f, 0.7f, 0.5f, 2);
        C(tr, t.accent, 0.4f, 0.9f, 0.18f, 0.18f, 3);
        C(tr, t.accent, -0.5f, 0.7f, 0.16f, 0.16f, 3);
    }

    static void Flowers(Transform tr, ForestTheme t)
    {
        Color[] heads = { Hex("#FF5A7A"), Hex("#FFE45C"), Color.white, Hex("#FF9CE0"), Hex("#8AD4FF") };
        for (int i = 0; i < 5; i++)
        {
            float x = -0.7f + i * 0.35f;
            float h = 0.45f + (i % 3) * 0.18f;
            R(tr, t.leafB, x, h * 0.5f, 0.06f, h, 0);
            C(tr, Hex("#FFD54A"), x, h + 0.02f, 0.2f, 0.2f, 2);
            for (int p = 0; p < 4; p++)
            {
                float a = p * 90f * Mathf.Deg2Rad;
                C(tr, heads[i], x + Mathf.Cos(a) * 0.12f, h + 0.02f + Mathf.Sin(a) * 0.12f, 0.17f, 0.17f, 1);
            }
        }
        T(tr, t.leafA, -0.8f, 0.2f, 0.3f, 0.4f, 0, 10f);
        T(tr, t.leafA, 0.8f, 0.2f, 0.3f, 0.4f, 0, -10f);
    }

    static void Rock(Transform tr)
    {
        Shadow(tr, 1.6f);
        C(tr, Hex("#8C8C94"), 0f, 0.4f, 1.3f, 0.85f, 0);
        C(tr, Hex("#A9A9B2"), -0.2f, 0.55f, 0.7f, 0.4f, 1);
        C(tr, Hex("#6F6F78"), 0.45f, 0.25f, 0.6f, 0.45f, 0);
        C(tr, Hex("#8FBF6A"), -0.35f, 0.15f, 0.5f, 0.2f, 2);
    }

    static void Tuft(Transform tr, ForestTheme t)
    {
        T(tr, t.leafA, -0.2f, 0.3f, 0.25f, 0.6f, 0, 12f);
        T(tr, t.leafB, 0.0f, 0.35f, 0.25f, 0.7f, 1);
        T(tr, t.leafA, 0.2f, 0.3f, 0.25f, 0.6f, 0, -12f);
    }

    static void Plank(Transform tr, ForestTheme t)
    {
        Color wood = Hex("#9A6A3C");
        R(tr, wood, 0f, 0f, 7.6f, 1.1f, 0);
        R(tr, Darken(wood, 0.3f), 0f, -0.5f, 7.6f, 0.08f, 1);
        R(tr, Lighten(wood, 0.15f), 0f, 0.25f, 7.4f, 0.1f, 1);
        R(tr, Darken(wood, 0.2f), -2.2f, 0f, 0.05f, 1.0f, 1);
        R(tr, Darken(wood, 0.2f), 1.5f, 0f, 0.05f, 1.0f, 1);
        // rail posts
        R(tr, Hex("#6B4A2B"), -3.65f, 0.55f, 0.3f, 1.3f, 2);
        R(tr, Hex("#6B4A2B"), 3.65f, 0.55f, 0.3f, 1.3f, 2);
        C(tr, Hex("#8B5E34"), -3.65f, 1.2f, 0.42f, 0.42f, 3);
        C(tr, Hex("#8B5E34"), 3.65f, 1.2f, 0.42f, 0.42f, 3);
        R(tr, Hex("#7A5230"), -3.65f, 1.0f, 0.12f, 1.9f, 2);
        R(tr, Hex("#7A5230"), 3.65f, 1.0f, 0.12f, 1.9f, 2);
    }
}
