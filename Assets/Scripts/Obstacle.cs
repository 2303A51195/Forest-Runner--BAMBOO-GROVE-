using UnityEngine;
using static SpriteFactory;

public enum ObstacleType { TreeTrunk, Stump, FallenTree, Rock, Thorns, WildBoar, FlyingOwl }
public enum ObstacleMode { Block, Jump, Slide }

/// <summary>
/// An obstacle in a lane. Mode tells how to survive it:
/// Block = change lane, Jump = jump over it, Slide = slide under it.
/// </summary>
public class Obstacle : WorldEntity
{
    public ObstacleType type;
    public ObstacleMode mode;
    public float clearHeight;     // for Jump mode: player's height must exceed this
    public float halfWidth = 1.05f;
    public float halfDepth = 0.9f;
    public float extraSpeed;      // the boar charges at you
    public bool ignored;          // already passed through (player was invulnerable)

    Transform legA, legB, wingL, wingR, body;
    float anim, baseElev;

    public static GameObject Build(ObstacleType type)
    {
        var go = new GameObject("Obstacle_" + type);
        var o = go.AddComponent<Obstacle>();
        o.kind = EntityKind.Obstacle;
        o.type = type;
        var tr = go.transform;
        Color wood = Hex("#7A5230");

        switch (type)
        {
            case ObstacleType.TreeTrunk:
                o.mode = ObstacleMode.Block; o.halfWidth = 1.0f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 2.2f, 0.5f, -2);
                R(tr, wood, 0f, 2.1f, 1.5f, 4.2f, 0);
                R(tr, Darken(wood, 0.3f), 0.45f, 2.1f, 0.3f, 4.0f, 1);
                R(tr, Lighten(wood, 0.15f), -0.45f, 2.1f, 0.15f, 3.9f, 1);
                for (int i = 0; i < 4; i++) R(tr, Darken(wood, 0.35f), -0.1f + (i % 2) * 0.3f, 0.7f + i * 0.9f, 0.5f, 0.06f, 2);
                T(tr, wood, -0.85f, 0.3f, 0.9f, 0.8f, 0, 20f);
                T(tr, wood, 0.85f, 0.3f, 0.9f, 0.8f, 0, -20f);
                C(tr, Hex("#2F8A3E"), 0f, 4.4f, 2.4f, 1.3f, 2);
                C(tr, Hex("#3FAE52"), -0.4f, 4.7f, 1.4f, 0.8f, 3);
                C(tr, Hex("#1F6E32"), 0.7f, 4.3f, 1.2f, 0.8f, 2);
                break;

            case ObstacleType.Stump:
                o.mode = ObstacleMode.Jump; o.clearHeight = 1.0f; o.halfWidth = 0.95f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 1.9f, 0.45f, -2);
                R(tr, wood, 0f, 0.5f, 1.3f, 1.0f, 0);
                R(tr, Darken(wood, 0.3f), 0.4f, 0.5f, 0.3f, 0.95f, 1);
                T(tr, wood, -0.7f, 0.2f, 0.7f, 0.5f, 0, 25f);
                T(tr, wood, 0.7f, 0.2f, 0.7f, 0.5f, 0, -25f);
                C(tr, Hex("#E3B97A"), 0f, 1.0f, 1.4f, 0.5f, 2);
                C(tr, Hex("#C99A5B"), 0f, 1.0f, 0.9f, 0.3f, 3);
                C(tr, Hex("#A87A41"), 0f, 1.0f, 0.4f, 0.14f, 4);
                C(tr, Hex("#6FBF4A"), -0.5f, 0.95f, 0.4f, 0.2f, 4);
                break;

            case ObstacleType.Rock:
                o.mode = ObstacleMode.Jump; o.clearHeight = 1.1f; o.halfWidth = 1.0f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 2.0f, 0.45f, -2);
                C(tr, Hex("#7C7C86"), 0f, 0.55f, 1.8f, 1.2f, 0);
                C(tr, Hex("#9A9AA5"), -0.25f, 0.8f, 1.0f, 0.6f, 1);
                C(tr, Hex("#B9B9C4"), -0.4f, 0.95f, 0.4f, 0.22f, 2);
                C(tr, Hex("#62626C"), 0.65f, 0.3f, 0.8f, 0.6f, 0);
                C(tr, Hex("#6FBF4A"), -0.6f, 0.2f, 0.6f, 0.25f, 2);
                break;

            case ObstacleType.Thorns:
                o.mode = ObstacleMode.Jump; o.clearHeight = 0.8f; o.halfWidth = 1.15f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 2.3f, 0.4f, -2);
                C(tr, Hex("#3C2A55"), 0f, 0.25f, 2.1f, 0.5f, 0);
                for (int i = 0; i < 7; i++)
                {
                    float x = -0.95f + i * 0.32f;
                    float h = 0.9f + ((i * 37) % 5) * 0.1f;
                    float rot = (i - 3) * 9f;
                    T(tr, Hex("#5B2A86"), x, h * 0.5f, 0.3f, h, 1, rot);
                    T(tr, Hex("#E34B6B"), x + Mathf.Sin(rot * Mathf.Deg2Rad) * -h * 0.35f, h * 0.88f, 0.14f, 0.28f, 2, rot);
                }
                break;

            case ObstacleType.WildBoar:
                o.mode = ObstacleMode.Jump; o.clearHeight = 1.15f; o.halfWidth = 1.0f; o.extraSpeed = 6f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 2.2f, 0.45f, -2);
                o.legA = R(tr, Hex("#3D2A1E"), -0.45f, 0.3f, 0.3f, 0.6f, 0).transform;
                o.legB = R(tr, Hex("#3D2A1E"), 0.45f, 0.3f, 0.3f, 0.6f, 0).transform;
                o.body = new GameObject("Body").transform;
                o.body.SetParent(tr, false);
                C(o.body, Hex("#6B4630"), 0f, 0.95f, 1.9f, 1.4f, 1);
                C(o.body, Hex("#85583B"), -0.1f, 1.15f, 1.1f, 0.6f, 2);
                for (int i = 0; i < 5; i++) T(o.body, Hex("#3D2A1E"), -0.6f + i * 0.3f, 1.65f, 0.28f, 0.4f, 1, (i - 2) * 8f);
                T(o.body, Hex("#4F3322"), -0.55f, 1.65f, 0.4f, 0.5f, 3, 10f);
                T(o.body, Hex("#4F3322"), 0.55f, 1.65f, 0.4f, 0.5f, 3, -10f);
                C(o.body, Hex("#F4A9B4"), 0f, 0.75f, 0.85f, 0.6f, 4);
                C(o.body, Hex("#C9707F"), -0.15f, 0.75f, 0.14f, 0.2f, 5);
                C(o.body, Hex("#C9707F"), 0.15f, 0.75f, 0.14f, 0.2f, 5);
                T(o.body, Color.white, -0.55f, 0.65f, 0.2f, 0.5f, 5, 25f);
                T(o.body, Color.white, 0.55f, 0.65f, 0.2f, 0.5f, 5, -25f);
                C(o.body, Color.white, -0.35f, 1.2f, 0.3f, 0.3f, 5);
                C(o.body, Color.white, 0.35f, 1.2f, 0.3f, 0.3f, 5);
                C(o.body, new Color(0.7f, 0.05f, 0.05f), -0.35f, 1.2f, 0.14f, 0.14f, 6);
                C(o.body, new Color(0.7f, 0.05f, 0.05f), 0.35f, 1.2f, 0.14f, 0.14f, 6);
                break;

            case ObstacleType.FallenTree:
                o.mode = ObstacleMode.Slide; o.halfWidth = 1.2f;
                o.elev = 0f;
                C(tr, new Color(0, 0, 0, 0.25f), 0f, 0.05f, 2.8f, 0.45f, -2);
                R(tr, Hex("#5E4026"), -1.05f, 0.7f, 0.28f, 1.4f, 0, 12f);
                R(tr, Hex("#5E4026"), 1.05f, 0.7f, 0.28f, 1.4f, 0, -12f);
                R(tr, wood, 0f, 1.65f, 2.9f, 0.7f, 1);
                R(tr, Darken(wood, 0.3f), 0f, 1.4f, 2.9f, 0.14f, 2);
                R(tr, Lighten(wood, 0.2f), 0f, 1.9f, 2.8f, 0.08f, 2);
                C(tr, Hex("#C99A5B"), -1.45f, 1.65f, 0.55f, 0.7f, 3);
                C(tr, Hex("#A87A41"), -1.45f, 1.65f, 0.3f, 0.4f, 4);
                C(tr, Hex("#4FA03F"), 0.3f, 1.95f, 1.0f, 0.3f, 3);
                C(tr, Hex("#E34B6B"), 0.6f, 2.05f, 0.2f, 0.2f, 4);
                break;

            case ObstacleType.FlyingOwl:
                o.mode = ObstacleMode.Slide; o.halfWidth = 1.0f; o.baseElev = 1.15f; o.elev = 1.15f;
                C(tr, new Color(0, 0, 0, 0.18f), 0f, -o.baseElev + 0.05f, 1.4f, 0.35f, -2); // ground shadow
                o.body = new GameObject("Body").transform;
                o.body.SetParent(tr, false);
                o.wingL = C(o.body, Hex("#7B5A3C"), -0.85f, 0.7f, 1.2f, 0.45f, 0, 20f).transform;
                o.wingR = C(o.body, Hex("#7B5A3C"), 0.85f, 0.7f, 1.2f, 0.45f, 0, -20f).transform;
                C(o.body, Hex("#9C7650"), 0f, 0.7f, 1.1f, 1.2f, 1);
                C(o.body, Hex("#E8D3B0"), 0f, 0.5f, 0.7f, 0.7f, 2);
                C(o.body, Hex("#9C7650"), 0f, 1.35f, 1.1f, 0.9f, 2);
                T(o.body, Hex("#6B4A2B"), -0.38f, 1.9f, 0.3f, 0.4f, 2, 12f);
                T(o.body, Hex("#6B4A2B"), 0.38f, 1.9f, 0.3f, 0.4f, 2, -12f);
                C(o.body, Color.white, -0.25f, 1.4f, 0.42f, 0.42f, 3);
                C(o.body, Color.white, 0.25f, 1.4f, 0.42f, 0.42f, 3);
                C(o.body, Hex("#FFC400"), -0.25f, 1.4f, 0.28f, 0.28f, 4);
                C(o.body, Hex("#FFC400"), 0.25f, 1.4f, 0.28f, 0.28f, 4);
                C(o.body, Color.black, -0.25f, 1.4f, 0.14f, 0.14f, 5);
                C(o.body, Color.black, 0.25f, 1.4f, 0.14f, 0.14f, 5);
                T(o.body, Hex("#FF8A00"), 0f, 1.2f, 0.2f, 0.28f, 5, 180f);
                break;
        }
        o.CacheRenderers();
        return go;
    }

    public void ResetState()
    {
        consumed = false;
        ignored = false;
        anim = Random.Range(0f, 6f);
        flipped = false;
        baseScale = 1f;
        if (type == ObstacleType.FlyingOwl) elev = baseElev;
    }

    public override void Tick(float dt, float speed)
    {
        z -= (speed + extraSpeed) * dt;
        anim += dt;
        if (type == ObstacleType.WildBoar && legA != null)
        {
            float s = Mathf.Sin(anim * 18f);
            legA.localPosition = new Vector3(-0.45f, 0.3f + Mathf.Max(0f, s) * 0.18f, 0f);
            legB.localPosition = new Vector3(0.45f, 0.3f + Mathf.Max(0f, -s) * 0.18f, 0f);
            body.localPosition = new Vector3(0f, Mathf.Abs(s) * 0.06f, 0f);
        }
        else if (type == ObstacleType.FlyingOwl && body != null)
        {
            float flap = Mathf.Sin(anim * 14f);
            wingL.localRotation = Quaternion.Euler(0f, 0f, 20f + flap * 28f);
            wingR.localRotation = Quaternion.Euler(0f, 0f, -20f - flap * 28f);
            elev = baseElev + Mathf.Sin(anim * 3f) * 0.12f;
        }
    }
}
