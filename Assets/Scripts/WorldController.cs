using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns every gameplay entity (obstacles, coins, fruits, leaves, power-ups): moves them, lays them out in
/// perspective, checks collisions with the player and recycles them into the object pool.
/// Lane-based collision tests are used instead of physics so they are 100% deterministic.
/// </summary>
public class WorldController : MonoBehaviour
{
    public static WorldController Instance { get; private set; }

    readonly List<WorldEntity> entities = new List<WorldEntity>();
    PlayerController player;

    public void Init(PlayerController p)
    {
        Instance = this;
        player = p;
    }

    void Awake() { Instance = this; }

    public void Add(WorldEntity e)
    {
        entities.Add(e);
        e.ApplyLayout();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || player == null) return;
        float dt = Time.deltaTime;
        float speed = gm.Speed;
        bool checking = gm.State == GameState.Playing && !player.Dead;

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            if (e == null) { entities.RemoveAt(i); continue; }

            e.Tick(dt, speed);
            e.ApplyLayout();
            if (checking && !e.consumed) Check(e, gm);

            if (e.consumed || e.z < -6f)
            {
                entities.RemoveAt(i);
                ObjectPooler.Instance.Release(e.gameObject);
            }
        }
    }

    void Check(WorldEntity e, GameManager gm)
    {
        float dx = Mathf.Abs(e.worldX - player.X);

        if (e.kind == EntityKind.Obstacle)
        {
            var o = (Obstacle)e;
            if (o.ignored) return;
            if (Mathf.Abs(o.z) > o.halfDepth) return;
            if (dx > o.halfWidth) return;

            bool hit;
            switch (o.mode)
            {
                case ObstacleMode.Jump: hit = player.Height < o.clearHeight; break;
                case ObstacleMode.Slide: hit = !player.Sliding; break;
                default: hit = true; break;
            }
            if (hit) gm.OnObstacleHit(o);
        }
        else
        {
            if (Mathf.Abs(e.z) > 0.9f) return;
            if (dx > 1.3f) return;
            if (Mathf.Abs(e.elev - (player.Height + 0.9f)) > 1.4f) return;
            e.consumed = true;
            gm.OnCollect(e);
        }
    }

    /// <summary>Used by Revive: removes obstacles close to the player so they get a fair restart.</summary>
    public void ClearObstacles(float maxZ)
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            if (e == null) { entities.RemoveAt(i); continue; }
            if (e.kind == EntityKind.Obstacle && e.z < maxZ)
            {
                Fx.Burst(e.transform.position + Vector3.up * 0.5f, new Color(0.8f, 0.7f, 0.5f), 6, 3f, 0.2f);
                entities.RemoveAt(i);
                ObjectPooler.Instance.Release(e.gameObject);
            }
        }
    }
}
