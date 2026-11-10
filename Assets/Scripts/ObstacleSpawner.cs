using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns obstacle rows, coin trails/arcs, fruits, leaves and power-ups far down the path.
/// Rows always leave at least one way through, and difficulty (frequency, rows with 2-3 obstacles,
/// new obstacle types) increases gradually with distance.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    float acc, nextGap = 22f, nextPowerDist = 260f;
    bool powerPending;

    public void Init()
    {
        acc = 0f;
        nextGap = 22f;
        nextPowerDist = 260f;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.State != GameState.Playing) return;

        acc += gm.Speed * Time.deltaTime;
        if (acc >= nextGap)
        {
            acc -= nextGap;
            SpawnRow(gm);
            float sec = Mathf.Lerp(2.0f, 0.9f, gm.Difficulty);
            nextGap = Mathf.Max(14f, gm.Speed * sec * Random.Range(0.95f, 1.25f));
        }
        if (gm.Distance >= nextPowerDist)
        {
            powerPending = true;
            nextPowerDist = gm.Distance + Random.Range(380f, 600f);
        }
    }

    void SpawnRow(GameManager gm)
    {
        float d = gm.Difficulty;
        int n = 1;
        if (Random.value < 0.12f + 0.5f * d) n = 2;
        if (d > 0.5f && Random.value < 0.12f * d) n = 3;

        // pick which lanes are blocked
        var lanes = new List<int> { 0, 1, 2 };
        for (int i = lanes.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int tmp = lanes[i]; lanes[i] = lanes[j]; lanes[j] = tmp;
        }

        bool[] blocked = new bool[3];
        for (int i = 0; i < n; i++)
        {
            int lane = lanes[i];
            blocked[lane] = true;
            var type = PickType(n, d);
            SpawnObstacle(type, lane, LaneManager.SpawnZ);

            // coin arc over low obstacles
            var mode = Mode(type);
            if (mode == ObstacleMode.Jump && Random.value < 0.55f)
            {
                for (int k = 0; k < 5; k++)
                {
                    float u = (k - 2) / 2f;
                    float elev = 0.9f + 1.9f * (1f - u * u);
                    SpawnCollectible(EntityKind.Coin, 0, lane, LaneManager.SpawnZ + (k - 2) * 1.8f, elev);
                }
            }
        }

        // free lanes: coin trails / fruit / leaves / power-up
        var free = new List<int>();
        for (int l = 0; l < 3; l++) if (!blocked[l]) free.Add(l);
        if (free.Count == 0) return;
        int fl = free[Random.Range(0, free.Count)];
        float startZ = LaneManager.SpawnZ + 2f;

        if (powerPending)
        {
            powerPending = false;
            SpawnPowerUp((PowerType)Random.Range(0, 4), fl, startZ + 2f);
            return;
        }

        float r = Random.value;
        if (r < 0.6f)
        {
            int count = Random.Range(4, 6);
            for (int k = 0; k < count; k++) SpawnCollectible(EntityKind.Coin, 0, fl, startZ + k * 2.2f, 0.9f);
        }
        else if (r < 0.75f)
        {
            SpawnCollectible(EntityKind.Fruit, Random.Range(0, 3), fl, startZ + 2f, 0.9f);
        }
        else
        {
            for (int k = 0; k < 3; k++) SpawnCollectible(EntityKind.Leaf, 0, fl, startZ + k * 2.2f, 1.0f);
        }
    }

    static ObstacleMode Mode(ObstacleType t)
    {
        switch (t)
        {
            case ObstacleType.TreeTrunk: return ObstacleMode.Block;
            case ObstacleType.FallenTree:
            case ObstacleType.FlyingOwl: return ObstacleMode.Slide;
            default: return ObstacleMode.Jump;
        }
    }

    ObstacleType PickType(int rowSize, float d)
    {
        if (rowSize < 3 && Random.value < 0.16f) return ObstacleType.TreeTrunk;

        if (Random.value < 0.55f)
        {
            var jumpTypes = new List<ObstacleType> { ObstacleType.Stump, ObstacleType.Rock, ObstacleType.Thorns };
            if (d > 0.18f) jumpTypes.Add(ObstacleType.WildBoar);
            return jumpTypes[Random.Range(0, jumpTypes.Count)];
        }
        if (d > 0.08f && Random.value < 0.5f) return ObstacleType.FlyingOwl;
        return ObstacleType.FallenTree;
    }

    void SpawnObstacle(ObstacleType type, int lane, float z)
    {
        var go = ObjectPooler.Instance.Get("Obs_" + type, () => Obstacle.Build(type));
        var o = go.GetComponent<Obstacle>();
        o.ResetState();
        o.worldX = LaneManager.LaneToX(lane);
        o.z = z;
        WorldController.Instance.Add(o);
    }

    void SpawnCollectible(EntityKind kind, int variant, int lane, float z, float elev)
    {
        var go = ObjectPooler.Instance.Get("Col_" + kind + variant, () => Collectible.Build(kind, variant));
        var c = go.GetComponent<Collectible>();
        c.ResetState();
        c.worldX = LaneManager.LaneToX(lane);
        c.z = z;
        c.elev = elev;
        WorldController.Instance.Add(c);
    }

    void SpawnPowerUp(PowerType p, int lane, float z)
    {
        var go = ObjectPooler.Instance.Get("Pow_" + p, () => PowerUp.Build(p));
        var pu = go.GetComponent<PowerUp>();
        pu.ResetState();
        pu.worldX = LaneManager.LaneToX(lane);
        pu.z = z;
        pu.elev = 1.0f;
        WorldController.Instance.Add(pu);
    }
}
