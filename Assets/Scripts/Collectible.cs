using UnityEngine;
using static SpriteFactory;

/// <summary>Coins, fruits and leaves. Coins can be pulled toward the player by the magnet power-up.</summary>
public class Collectible : WorldEntity
{
    public bool attracted;
    Transform visual;
    float spin;

    public static GameObject Build(EntityKind k, int variant)
    {
        var go = new GameObject("Collectible_" + k + variant);
        var c = go.AddComponent<Collectible>();
        c.kind = k;
        var tr = go.transform;
        c.visual = new GameObject("Visual").transform;
        c.visual.SetParent(tr, false);
        var v = c.visual;
        G(tr, new Color(1f, 0.9f, 0.4f, 0.35f), 0f, 0f, 1.5f, -1);

        switch (k)
        {
            case EntityKind.Coin:
                C(v, Hex("#C98A00"), 0f, 0f, 0.72f, 0.72f, 0);
                C(v, Hex("#FFC933"), 0f, 0f, 0.62f, 0.62f, 1);
                C(v, Hex("#FFE27A"), 0f, 0f, 0.42f, 0.42f, 2);
                R(v, Hex("#E0A200"), 0f, 0f, 0.1f, 0.3f, 3);
                C(v, new Color(1f, 1f, 1f, 0.8f), -0.15f, 0.15f, 0.12f, 0.12f, 3);
                break;
            case EntityKind.Fruit:
                {
                    Color body = variant == 0 ? Hex("#E53935") : (variant == 1 ? Hex("#FB8C00") : Hex("#7B1FA2"));
                    C(v, Darken(body, 0.2f), 0f, -0.02f, 0.82f, 0.78f, 0);
                    C(v, body, 0f, 0f, 0.76f, 0.72f, 1);
                    C(v, Lighten(body, 0.55f), -0.18f, 0.16f, 0.2f, 0.14f, 2);
                    R(v, Hex("#5D4037"), 0f, 0.42f, 0.07f, 0.22f, 2);
                    C(v, Hex("#4CAF50"), 0.14f, 0.45f, 0.26f, 0.13f, 3, 25f);
                }
                break;
            case EntityKind.Leaf:
                C(v, Hex("#2E9E4A"), 0f, 0f, 0.5f, 0.9f, 0, 35f);
                C(v, Hex("#5BD16A"), -0.03f, 0.03f, 0.36f, 0.72f, 1, 35f);
                R(v, Hex("#1F7A38"), 0f, 0f, 0.05f, 0.8f, 2, 35f);
                G(tr, new Color(0.4f, 1f, 0.5f, 0.4f), 0f, 0f, 1.5f, -1);
                break;
        }
        c.CacheRenderers();
        return go;
    }

    public void ResetState()
    {
        consumed = false;
        attracted = false;
        spin = Random.Range(0f, 6f);
        baseScale = 1f;
        flipped = false;
    }

    public override void Tick(float dt, float speed)
    {
        z -= speed * dt;
        spin += dt;
        if (visual == null) return;

        if (kind == EntityKind.Coin)
        {
            float sx = Mathf.Max(0.25f, Mathf.Abs(Mathf.Cos(spin * 5f)));
            visual.localScale = new Vector3(sx, 1f, 1f);
        }
        else if (kind == EntityKind.Fruit)
        {
            visual.localPosition = new Vector3(0f, Mathf.Sin(spin * 4f) * 0.08f, 0f);
        }
        else
        {
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(spin * 3f) * 18f);
            visual.localPosition = new Vector3(0f, Mathf.Sin(spin * 3.5f) * 0.1f, 0f);
        }

        // magnet
        var gm = GameManager.Instance;
        var p = PlayerController.Instance;
        if (kind == EntityKind.Coin && gm != null && p != null && gm.magnetTime > 0f && z < 16f && z > -1f)
        {
            if (Mathf.Abs(worldX - p.X) < 5f) attracted = true;
        }
        if (attracted && p != null)
        {
            worldX = Mathf.MoveTowards(worldX, p.X, 22f * dt);
            elev = Mathf.MoveTowards(elev, p.Height + 0.9f, 14f * dt);
            z -= 14f * dt;
        }
    }
}
