using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Playing, Paused, Dead, GameOver }

/// <summary>
/// Game rules: speed/difficulty curve, score, collectibles, power-up timers, pause/resume/restart/home,
/// game over and revive. Exactly one exists per game scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const float BaseSpeed = 13f, MaxSpeed = 27f;
    public const float MagnetDur = 9f, ShieldDur = 15f, SpeedDur = 5f, MultDur = 10f;
    public const int ReviveCost = 50;

    public GameState State = GameState.Playing;
    public float Speed;
    public float Distance;
    public int RunCoins, RunLeaves, RunFruits;
    public float magnetTime, shieldTime, speedTime, multiplierTime;
    public bool revivedThisRun;

    float scoreF;
    PlayerController player;
    UIManager ui;
    int lastTheme;

    public int Score { get { return Mathf.FloorToInt(scoreF); } }
    public float Difficulty { get { return Mathf.Clamp01(Distance / 3500f); } }
    public int ThemeIndex { get { return Mathf.FloorToInt(Distance / 600f) % ForestTheme.All.Length; } }
    public bool CanRevive { get { return !revivedThisRun && SaveData.Coins >= ReviveCost; } }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Time.timeScale = 1f;
    }

    public void Init(PlayerController p, UIManager u)
    {
        Instance = this;
        player = p;
        ui = u;
        Time.timeScale = 1f;
        State = GameState.Playing;
        Speed = 0f;
        lastTheme = 0;
    }

    void OnDestroy()
    {
        if (Instance == this) Time.timeScale = 1f;
    }

    void Update()
    {
        GameInput.Poll();
        if (GameInput.Pause)
        {
            if (State == GameState.Playing) Pause();
            else if (State == GameState.Paused) Resume();
        }

        float dt = Time.deltaTime;
        if (State == GameState.Playing)
        {
            float target = Mathf.Min(MaxSpeed, BaseSpeed + Distance * 0.0035f);
            if (speedTime > 0f) target *= 1.55f;
            Speed = Mathf.MoveTowards(Speed, target, (speedTime > 0f ? 22f : 8f) * dt);

            Distance += Speed * dt;
            scoreF += Speed * dt * 0.7f * (multiplierTime > 0f ? 2f : 1f);

            magnetTime = Mathf.Max(0f, magnetTime - dt);
            shieldTime = Mathf.Max(0f, shieldTime - dt);
            multiplierTime = Mathf.Max(0f, multiplierTime - dt);
            if (speedTime > 0f)
            {
                speedTime -= dt;
                if (speedTime <= 0f) { speedTime = 0f; if (player != null) player.GrantInvuln(1.2f); }
            }

            if (ThemeIndex != lastTheme)
            {
                lastTheme = ThemeIndex;
                if (ui != null) ui.ShowBanner(ForestTheme.All[lastTheme].name);
            }
        }
        else if (State == GameState.Dead)
        {
            Speed = Mathf.MoveTowards(Speed, 0f, 40f * dt);
        }
    }

    void OnApplicationPause(bool paused)
    {
        if (paused && State == GameState.Playing) Pause();
    }

    // ------------------------------------------------------------------ pickups
    void AddScore(int v) { scoreF += v * (multiplierTime > 0f ? 2f : 1f); }

    public void OnCollect(WorldEntity e)
    {
        Vector3 pos = e.transform.position;
        switch (e.kind)
        {
            case EntityKind.Coin:
                RunCoins++;
                SaveData.Coins = SaveData.Coins + 1;
                AddScore(10);
                AudioManager.Play(Sfx.Coin, 0.7f);
                Fx.Burst(pos, new Color(1f, 0.82f, 0.2f), 6, 3.5f, 0.16f, 0.45f, 2f);
                if (ui != null) ui.BumpCoins();
                break;
            case EntityKind.Fruit:
                RunFruits++;
                AddScore(100);
                AudioManager.Play(Sfx.Fruit);
                Fx.Burst(pos, new Color(1f, 0.35f, 0.3f), 12, 4.5f, 0.2f, 0.6f, 3f);
                Fx.Ring(pos, new Color(1f, 0.9f, 0.5f, 0.9f), 0.8f, 0.4f, 3f);
                if (ui != null) ui.BumpScore();
                break;
            case EntityKind.Leaf:
                RunLeaves++;
                SaveData.Leaves = SaveData.Leaves + 1;
                AddScore(25);
                AudioManager.Play(Sfx.Leaf);
                Fx.Burst(pos, new Color(0.4f, 0.95f, 0.45f), 8, 3.5f, 0.17f, 0.55f, 1f);
                if (ui != null) ui.BumpLeaves();
                break;
            case EntityKind.PowerUp:
                Activate(((PowerUp)e).power, pos);
                break;
        }
    }

    void Activate(PowerType p, Vector3 pos)
    {
        Color c = PowerUp.ColorOf(p);
        switch (p)
        {
            case PowerType.Magnet: magnetTime = MagnetDur; AudioManager.Play(Sfx.PowerUp); break;
            case PowerType.Shield: shieldTime = ShieldDur; AudioManager.Play(Sfx.Shield); break;
            case PowerType.Speed: speedTime = SpeedDur; AudioManager.Play(Sfx.PowerUp); break;
            case PowerType.Multiplier: multiplierTime = MultDur; AudioManager.Play(Sfx.PowerUp); break;
        }
        Fx.Burst(pos, c, 18, 6f, 0.24f, 0.7f, 1f);
        Fx.Ring(pos, new Color(c.r, c.g, c.b, 0.9f), 1f, 0.6f, 5f);
        if (player != null) Fx.Ring(player.transform.position + Vector3.up, new Color(c.r, c.g, c.b, 0.8f), 1f, 0.7f, 4f);
        if (ui != null) ui.ShowBanner(PowerUp.NameOf(p) + "!");
    }

    // ------------------------------------------------------------------ collisions
    public void OnObstacleHit(Obstacle o)
    {
        if (State != GameState.Playing || player == null) return;

        if (player.Invulnerable) { o.ignored = true; return; }

        if (speedTime > 0f)
        {
            Smash(o);
            return;
        }
        if (shieldTime > 0f)
        {
            shieldTime = 0f;
            player.GrantInvuln(1.5f);
            Smash(o);
            Fx.Ring(player.transform.position + Vector3.up, new Color(0.5f, 0.9f, 1f, 0.9f), 1.5f, 0.5f, 3f);
            return;
        }
        Die();
    }

    void Smash(Obstacle o)
    {
        o.consumed = true;
        Fx.Burst(o.transform.position + Vector3.up * 0.8f, new Color(0.7f, 0.5f, 0.3f), 14, 6f, 0.3f, 0.6f, 6f);
        AudioManager.Play(Sfx.Hit, 0.8f);
        CameraController.Shake(0.25f, 0.25f);
    }

    void Die()
    {
        State = GameState.Dead;
        player.Die();
        AudioManager.Play(Sfx.Hit);
        CameraController.Shake(0.5f, 0.5f);
        magnetTime = shieldTime = speedTime = 0f;
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.1f);
        if (State != GameState.Dead) yield break;
        State = GameState.GameOver;
        Speed = 0f;
        bool newBest = Score > SaveData.Best;
        if (newBest) SaveData.Best = Score;
        SaveData.Save();
        AudioManager.Play(Sfx.GameOver);
        if (ui != null) ui.ShowGameOver(Score, SaveData.Best, newBest, RunCoins, RunLeaves, RunFruits);
    }

    // ------------------------------------------------------------------ buttons
    public void Pause()
    {
        if (State != GameState.Playing) return;
        State = GameState.Paused;
        Time.timeScale = 0f;
        SaveData.Save();
        if (ui != null) ui.ShowPause(true);
    }

    public void Resume()
    {
        if (State != GameState.Paused) return;
        State = GameState.Playing;
        Time.timeScale = 1f;
        if (ui != null) ui.ShowPause(false);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SaveData.Save();
        SceneLoader.Load(SceneLoader.GameScene);
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SaveData.Save();
        SceneLoader.Load(SceneLoader.MenuScene);
    }

    public void Revive()
    {
        if (State != GameState.GameOver || !CanRevive) return;
        SaveData.Coins = SaveData.Coins - ReviveCost;
        revivedThisRun = true;
        SaveData.Save();
        if (WorldController.Instance != null) WorldController.Instance.ClearObstacles(45f);
        player.Revive();
        Speed = BaseSpeed * 0.6f;
        State = GameState.Playing;
        AudioManager.Play(Sfx.PowerUp);
        if (ui != null) ui.HideGameOver();
    }
}
