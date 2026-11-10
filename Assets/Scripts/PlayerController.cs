using UnityEngine;
using static SpriteFactory;

/// <summary>
/// The player: automatic running, 3-lane movement, jump and slide, power-up visuals and death/revive.
/// Input comes from GameInput (keyboard + swipes).
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public int TargetLane = 1;
    public float LaneF = 1f;
    public float Height;
    public bool Sliding, Dead;
    public float X { get { return LaneManager.LaneToX(LaneF); } }
    public bool Invulnerable { get { return invuln > 0f; } }

    const float Gravity = 34f, JumpV = 12.5f, SlideDur = 0.75f, LaneSpeed = 11f;

    FoxRig rig;
    Transform shieldT, boostT, magnetT;
    SpriteRenderer shieldFill, shieldRing, magnetRing;
    SpriteRenderer[] flames;
    float vy, slideT, invuln, animT, deadT, dustT, trailT, magT;
    bool grounded = true, slideQueued;
    float deathDir = 1f;

    void Awake() { Instance = this; }

    public void Init()
    {
        Instance = this;
        rig = FoxRig.Build(transform);

        // shield bubble
        shieldT = new GameObject("Shield").transform;
        shieldT.SetParent(rig.root, false);
        shieldFill = C(shieldT, new Color(0.4f, 0.85f, 1f, 0.2f), 0f, 1.15f, 2.6f, 2.8f, 12);
        shieldRing = Part(shieldT, Ring, new Color(0.6f, 0.95f, 1f, 0.9f), 0f, 1.15f, 2.6f, 2.8f, 0f, 13, "ring");
        shieldT.gameObject.SetActive(false);

        // boost flames
        boostT = new GameObject("Boost").transform;
        boostT.SetParent(rig.root, false);
        flames = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
            flames[i] = T(boostT, new Color(1f, 0.6f, 0.1f, 0.9f), -0.45f + i * 0.45f, -0.35f, 0.4f, 0.9f, -3, 180f);
        boostT.gameObject.SetActive(false);

        // magnet aura
        magnetT = new GameObject("Magnet").transform;
        magnetT.SetParent(rig.root, false);
        magnetRing = Part(magnetT, Ring, new Color(1f, 0.3f, 0.3f, 0.5f), 0f, 1.1f, 3.2f, 3.2f, 0f, -4, "ring");
        magnetT.gameObject.SetActive(false);

        AddOrder(transform, 1900);
        Place();
    }

    void Place()
    {
        transform.position = new Vector3(X, LaneManager.GroundY, 0f);
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || rig == null) return;
        float dt = Time.deltaTime;

        if (gm.State == GameState.Playing && !Dead)
        {
            GameInput.Poll();
            HandleInput();
            Simulate(dt);
            if (grounded && !Sliding)
            {
                dustT -= dt;
                if (dustT <= 0f) { dustT = 0.12f; Fx.Dust(transform.position + new Vector3(0f, 0.1f, 0f), new Color(0.85f, 0.75f, 0.55f, 0.5f)); }
            }
        }
        else if (Dead)
        {
            deadT += dt;
        }

        if (invuln > 0f) invuln -= dt;
        animT += dt * (6f + gm.Speed * 0.5f);
        Place();
        UpdateVisuals(gm, dt);
    }

    void HandleInput()
    {
        if (GameInput.Left && TargetLane > 0) TargetLane--;
        if (GameInput.Right && TargetLane < LaneManager.LaneCount - 1) TargetLane++;

        if (GameInput.Jump)
        {
            if (grounded)
            {
                vy = JumpV;
                grounded = false;
                Sliding = false;
                slideT = 0f;
                slideQueued = false;
                AudioManager.Play(Sfx.Jump);
                Fx.Burst(transform.position + new Vector3(0f, 0.1f, 0f), new Color(0.9f, 0.85f, 0.6f, 0.8f), 5, 2.5f, 0.2f, 0.4f, 0f);
            }
        }
        if (GameInput.Slide)
        {
            if (grounded) StartSlide();
            else { vy = -24f; slideQueued = true; }
        }
    }

    void StartSlide()
    {
        slideT = SlideDur;
        Sliding = true;
        AudioManager.Play(Sfx.Slide);
    }

    void Simulate(float dt)
    {
        LaneF = Mathf.MoveTowards(LaneF, TargetLane, LaneSpeed * dt);

        if (!grounded)
        {
            vy -= Gravity * dt;
            Height += vy * dt;
            if (Height <= 0f)
            {
                Height = 0f;
                vy = 0f;
                grounded = true;
                Fx.Burst(transform.position + new Vector3(0f, 0.1f, 0f), new Color(0.9f, 0.85f, 0.6f, 0.8f), 4, 2f, 0.18f, 0.35f, 0f);
                if (slideQueued) { slideQueued = false; StartSlide(); }
            }
        }
        if (slideT > 0f) slideT -= dt;
        Sliding = grounded && slideT > 0f;
    }

    void UpdateVisuals(GameManager gm, float dt)
    {
        float lean = -(TargetLane - LaneF) * 14f;
        float h = Height;
        bool g = grounded, s = Sliding;

        if (Dead)
        {
            float k = Mathf.Min(deadT * 4f, 1f);
            lean = 85f * k * deathDir;
            h = Mathf.Abs(Mathf.Sin(Mathf.Min(deadT, 0.6f) * 5f)) * 0.9f * (1f - Mathf.Min(deadT, 1f) * 0.7f);
            g = false; s = false;
        }
        rig.Animate(animT, h, g, s, lean);

        // blink while invulnerable
        bool visible = !(invuln > 0f && !Dead && Mathf.Sin(Time.time * 40f) > 0f);
        rig.SetVisible(visible);

        // shield
        bool shield = gm.shieldTime > 0f && !Dead;
        shieldT.gameObject.SetActive(shield);
        if (shield)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.05f;
            shieldT.localPosition = new Vector3(0f, h, 0f);
            shieldT.localScale = new Vector3(pulse, pulse, 1f);
            bool flicker = gm.shieldTime < 2f && Mathf.Sin(Time.time * 25f) > 0f;
            shieldRing.enabled = !flicker;
            shieldFill.enabled = !flicker;
        }

        // boost flames + trail
        bool boost = gm.speedTime > 0f && !Dead;
        boostT.gameObject.SetActive(boost);
        if (boost)
        {
            boostT.localPosition = new Vector3(0f, h, 0f);
            for (int i = 0; i < flames.Length; i++)
            {
                float fl = 0.8f + Mathf.Sin(Time.time * 40f + i * 2f) * 0.35f;
                flames[i].transform.localScale = new Vector3(0.4f, 0.9f * fl, 1f);
            }
            trailT -= dt;
            if (trailT <= 0f)
            {
                trailT = 0.04f;
                Fx.Burst(transform.position + new Vector3(0f, h + 0.3f, 0f), new Color(1f, 0.7f, 0.2f, 0.9f), 1, 1.2f, 0.3f, 0.35f, 0f);
            }
        }

        // magnet aura
        bool mag = gm.magnetTime > 0f && !Dead;
        magnetT.gameObject.SetActive(mag);
        if (mag)
        {
            magnetT.localPosition = new Vector3(0f, h, 0f);
            magT = (magT + dt * 1.3f) % 1f;
            float sc = Mathf.Lerp(0.6f, 1.4f, magT);
            magnetT.localScale = new Vector3(sc, sc, 1f);
            magnetRing.color = new Color(1f, 0.3f, 0.3f, 0.6f * (1f - magT));
        }
    }

    public void GrantInvuln(float seconds) { invuln = Mathf.Max(invuln, seconds); }

    public void Die()
    {
        if (Dead) return;
        Dead = true;
        deadT = 0f;
        deathDir = Random.value < 0.5f ? -1f : 1f;
        Fx.Burst(transform.position + new Vector3(0f, 1.2f, 0f), new Color(1f, 0.9f, 0.3f), 14, 6f, 0.26f, 0.7f, 6f);
    }

    public void Revive()
    {
        Dead = false;
        deadT = 0f;
        Height = 0f; vy = 0f; grounded = true; Sliding = false; slideT = 0f; slideQueued = false;
        TargetLane = Mathf.Clamp(Mathf.RoundToInt(LaneF), 0, 2);
        LaneF = TargetLane;
        invuln = 3f;
        Fx.Ring(transform.position + new Vector3(0f, 1f, 0f), new Color(1f, 1f, 0.6f, 0.9f), 1f, 0.6f, 4f);
    }
}
