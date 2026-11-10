using UnityEngine;
using static SpriteFactory;

public enum PowerType { Magnet, Shield, Speed, Multiplier }

/// <summary>A collectible power-up (Magnet, Shield, Speed Boost, Score Multiplier) with a glowing badge icon.</summary>
public class PowerUp : WorldEntity
{
    public PowerType power;
    Transform badge;
    SpriteRenderer pulse;
    float t;

    public static Color ColorOf(PowerType p)
    {
        switch (p)
        {
            case PowerType.Magnet: return Hex("#E53935");
            case PowerType.Shield: return Hex("#29B6F6");
            case PowerType.Speed: return Hex("#FFB300");
            default: return Hex("#AB47BC");
        }
    }

    public static string NameOf(PowerType p)
    {
        switch (p)
        {
            case PowerType.Magnet: return "MAGNET";
            case PowerType.Shield: return "SHIELD";
            case PowerType.Speed: return "BOOST";
            default: return "x2";
        }
    }

    public static GameObject Build(PowerType p)
    {
        var go = new GameObject("PowerUp_" + p);
        var pu = go.AddComponent<PowerUp>();
        pu.kind = EntityKind.PowerUp;
        pu.power = p;
        var tr = go.transform;
        Color col = ColorOf(p);
        G(tr, Alpha(col, 0.55f), 0f, 0f, 3.0f, -2);
        pu.badge = new GameObject("Badge").transform;
        pu.badge.SetParent(tr, false);
        var b = pu.badge;
        C(b, Color.white, 0f, 0f, 1.35f, 1.35f, 0);
        C(b, col, 0f, 0f, 1.2f, 1.2f, 1);
        C(b, Lighten(col, 0.4f), 0f, 0.2f, 0.9f, 0.6f, 2).color = new Color(1f, 1f, 1f, 0.25f);
        pu.pulse = Part(tr, Ring, Alpha(Color.white, 0.8f), 0f, 0f, 1.5f, 1.5f, 0f, 5, "pulse");

        switch (p)
        {
            case PowerType.Magnet:
                Part(b, Horseshoe, Color.white, 0f, -0.02f, 0.78f, 0.78f, 0f, 4, "magnet");
                break;
            case PowerType.Shield:
                R(b, Color.white, 0f, 0.14f, 0.7f, 0.5f, 4);
                T(b, Color.white, 0f, -0.22f, 0.7f, 0.5f, 4, 180f);
                R(b, Hex("#0288D1"), 0f, 0.14f, 0.5f, 0.34f, 5);
                T(b, Hex("#0288D1"), 0f, -0.2f, 0.5f, 0.38f, 5, 180f);
                R(b, Color.white, 0f, 0.05f, 0.08f, 0.6f, 6);
                break;
            case PowerType.Speed:
                T(b, Color.white, -0.04f, 0.2f, 0.52f, 0.7f, 4, 14f);
                T(b, Color.white, 0.06f, -0.2f, 0.52f, 0.7f, 4, 194f);
                break;
            default:
                // "x2" drawn with rectangles
                R(b, Color.white, -0.28f, 0f, 0.07f, 0.5f, 4, 40f);
                R(b, Color.white, -0.28f, 0f, 0.07f, 0.5f, 4, -40f);
                R(b, Color.white, 0.2f, 0.28f, 0.34f, 0.08f, 4);
                R(b, Color.white, 0.2f, 0.0f, 0.34f, 0.08f, 4);
                R(b, Color.white, 0.2f, -0.28f, 0.34f, 0.08f, 4);
                R(b, Color.white, 0.34f, 0.14f, 0.08f, 0.3f, 4);
                R(b, Color.white, 0.06f, -0.14f, 0.08f, 0.3f, 4);
                break;
        }
        pu.CacheRenderers();
        return go;
    }

    public void ResetState()
    {
        consumed = false;
        baseScale = 1.15f;
        flipped = false;
        t = Random.Range(0f, 6f);
    }

    public override void Tick(float dt, float speed)
    {
        z -= speed * dt;
        t += dt;
        if (badge != null)
        {
            badge.localPosition = new Vector3(0f, Mathf.Sin(t * 4f) * 0.1f, 0f);
            float s = 1f + Mathf.Sin(t * 6f) * 0.06f;
            badge.localScale = new Vector3(s, s, 1f);
        }
        if (pulse != null)
        {
            float k = (t * 1.2f) % 1f;
            float sc = Mathf.Lerp(1.3f, 2.4f, k);
            pulse.transform.localScale = new Vector3(sc, sc, 1f);
            pulse.color = new Color(1f, 1f, 1f, 0.7f * (1f - k));
        }
    }
}
