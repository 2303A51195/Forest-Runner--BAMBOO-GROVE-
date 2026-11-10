using UnityEngine;

public enum EntityKind { Scenery, Obstacle, Coin, Fruit, Leaf, PowerUp }

/// <summary>
/// Base class for everything that lives in the pseudo-3D world (obstacles, pickups, scenery).
/// It has a depth (z) and a sideways position (worldX). Layout converts these to a screen position,
/// perspective scale and sorting order.
/// </summary>
public class WorldEntity : MonoBehaviour
{
    public EntityKind kind;
    public float z, worldX, elev, baseScale = 1f;
    public bool consumed, groundLayer, flipped;

    SpriteRenderer[] srs;
    int[] orders;

    public void CacheRenderers()
    {
        srs = GetComponentsInChildren<SpriteRenderer>(true);
        orders = new int[srs.Length];
        for (int i = 0; i < srs.Length; i++) orders[i] = srs[i].sortingOrder;
    }

    public virtual void Tick(float dt, float speed)
    {
        z -= speed * dt;
    }

    public void ApplyLayout()
    {
        if (srs == null) CacheRenderers();
        float f = LaneManager.F(z);
        transform.position = new Vector3(worldX * f, LaneManager.ScreenY(z) + elev * f, 0f);
        float s = f * baseScale;
        transform.localScale = new Vector3(flipped ? -s : s, s, 1f);
        int o = groundLayer ? -310 + (int)((100f - z) * 0.5f) : 100 + (int)((90f - z) * 20f);
        for (int i = 0; i < srs.Length; i++) srs[i].sortingOrder = o + orders[i];
    }
}
