using UnityEngine;

/// <summary>Particle effects (coin sparkles, power-up rings, dust) using pooled sprites.</summary>
public static class Fx
{
    public static void Burst(Vector3 pos, Color c, int count, float speed = 4f, float size = 0.2f, float life = 0.55f, float gravity = 5f)
    {
        if (ObjectPooler.Instance == null) return;
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized * Random.Range(0.35f, 1f) * speed;
            var p = Get();
            p.Setup(SpriteFactory.Circle, pos, new Vector3(dir.x, dir.y, 0f), life * Random.Range(0.7f, 1.2f), c,
                    size * Random.Range(0.6f, 1.3f), gravity, 0f, 0f);
        }
    }

    public static void Ring(Vector3 pos, Color c, float size, float life, float endScale)
    {
        if (ObjectPooler.Instance == null) return;
        var p = Get();
        p.Setup(SpriteFactory.Ring, pos, Vector3.zero, life, c, size, 0f, endScale, 0f);
    }

    public static void Dust(Vector3 pos, Color c)
    {
        if (ObjectPooler.Instance == null) return;
        var p = Get();
        p.Setup(SpriteFactory.Circle, pos + new Vector3(Random.Range(-0.3f, 0.3f), 0f, 0f),
                new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(0.4f, 1.2f), 0f), 0.4f, c, 0.3f, 0f, 1.8f, 0f);
    }

    static FxParticle Get()
    {
        var go = ObjectPooler.Instance.Get("FxParticle", Build);
        return go.GetComponent<FxParticle>();
    }

    static GameObject Build()
    {
        var go = new GameObject("FxParticle");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1950;
        go.AddComponent<FxParticle>();
        return go;
    }
}
