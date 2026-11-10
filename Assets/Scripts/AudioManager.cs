using System;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx { Coin, Fruit, Leaf, Jump, Slide, Hit, PowerUp, Click, GameOver, Shield }

/// <summary>
/// Plays music and sound effects. All audio is synthesised in code at start-up,
/// so there are no audio files that can be missing. Every call is null-safe.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const int SR = 22050;
    AudioSource music, sfx;
    readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    System.Random rng = new System.Random(7);

    public static AudioManager Ensure()
    {
        if (Instance == null)
        {
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<AudioManager>();
        }
        return Instance;
    }

    public static void Play(Sfx s, float volume = 1f)
    {
        if (Instance == null) return;
        Instance.PlayInternal(s, volume);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0.32f;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        try { BuildClips(); } catch (Exception e) { Debug.LogWarning("Audio generation skipped: " + e.Message); }
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (music == null) return;
        bool musicWanted = SaveData.SoundOn && SaveData.MusicOn;
        if (musicWanted && music.clip != null)
        {
            if (!music.isPlaying) music.Play();
        }
        else if (music.isPlaying) music.Pause();
    }

    void PlayInternal(Sfx s, float volume)
    {
        if (!SaveData.SoundOn || sfx == null) return;
        AudioClip c;
        if (clips.TryGetValue(s, out c) && c != null) sfx.PlayOneShot(c, volume);
    }

    // ------------------------------------------------------------ synthesis
    static float Sine(float f, float t) { return Mathf.Sin(2f * Mathf.PI * f * t); }

    AudioClip Make(string name, float dur, Func<float, float> fn)
    {
        int n = Mathf.Max(1, (int)(SR * dur));
        var d = new float[n];
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(fn(i / (float)SR), -1f, 1f);
        var c = AudioClip.Create(name, n, 1, SR, false);
        c.SetData(d, 0);
        return c;
    }

    float Noise() { return (float)(rng.NextDouble() * 2.0 - 1.0); }

    void BuildClips()
    {
        clips[Sfx.Coin] = Make("coin", 0.22f, t =>
        {
            float f = t < 0.08f ? 988f : 1319f;
            float env = Mathf.Exp(-t * 14f);
            return (Sine(f, t) * 0.5f + Sine(f * 2f, t) * 0.15f) * env * 0.8f;
        });

        clips[Sfx.Fruit] = Make("fruit", 0.3f, t =>
        {
            float f = t < 0.1f ? 660f : (t < 0.2f ? 880f : 1100f);
            return Sine(f, t) * Mathf.Exp(-t * 8f) * 0.6f;
        });

        clips[Sfx.Leaf] = Make("leaf", 0.25f, t =>
            (Sine(1568f, t) * 0.4f + Sine(2349f, t) * 0.2f) * Mathf.Exp(-t * 12f));

        clips[Sfx.Jump] = Make("jump", 0.25f, t =>
        {
            float phase = 2f * Mathf.PI * (300f * t + 0.5f * (450f / 0.25f) * t * t);
            return Mathf.Sin(phase) * (1f - t / 0.25f) * 0.5f;
        });

        float lp = 0f;
        clips[Sfx.Slide] = Make("slide", 0.35f, t =>
        {
            lp += 0.22f * (Noise() - lp);
            return lp * 1.6f * Mathf.Exp(-t * 6f) * Mathf.Clamp01(t * 60f);
        });

        float lp2 = 0f;
        clips[Sfx.Hit] = Make("hit", 0.45f, t =>
        {
            lp2 += 0.35f * (Noise() - lp2);
            float low = Sine(110f - 60f * t, t) * Mathf.Exp(-t * 7f);
            return (lp2 * 1.2f * Mathf.Exp(-t * 9f) + low * 0.9f) * 0.9f;
        });

        float[] arp = { 523.25f, 659.25f, 783.99f, 1046.5f };
        clips[Sfx.PowerUp] = Make("powerup", 0.5f, t =>
        {
            int i = Mathf.Min(3, (int)(t / 0.09f));
            float lt = t - i * 0.09f;
            return (Sine(arp[i], t) * 0.5f + Sine(arp[i] * 2f, t) * 0.12f) * Mathf.Exp(-lt * 6f) * 0.9f;
        });

        clips[Sfx.Shield] = Make("shield", 0.4f, t =>
        {
            float f = 500f + 900f * t / 0.4f;
            float phase = 2f * Mathf.PI * (500f * t + 0.5f * (900f / 0.4f) * t * t);
            return (Mathf.Sin(phase) * 0.45f + Mathf.Sin(phase * 1.5f) * 0.15f) * (1f - t / 0.4f);
        });

        clips[Sfx.Click] = Make("click", 0.06f, t => Sine(820f, t) * Mathf.Exp(-t * 55f) * 0.6f);

        float[] over = { 440f, 392f, 330f, 262f };
        clips[Sfx.GameOver] = Make("gameover", 1.0f, t =>
        {
            int i = Mathf.Min(3, (int)(t / 0.25f));
            float lt = t - i * 0.25f;
            return (Sine(over[i], t) * 0.5f + Sine(over[i] * 0.5f, t) * 0.25f) * Mathf.Exp(-lt * 3.5f) * 0.9f;
        });

        BuildMusic();
    }

    void BuildMusic()
    {
        // 32 steps of a gentle pentatonic melody with a soft bass line.
        float[] scale = { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f, 587.33f, 659.25f };
        int[] melody =
        {
            0, 2, 4, 2,  3, 4, 5, 4,  2, 4, 3, 2,  0, 1, 2, -1,
            4, 5, 6, 5,  4, 3, 2, 3,  1, 2, 4, 3,  2, 1, 0, -1
        };
        float[] bass = { 130.81f, 110.00f, 98.00f, 130.81f };
        const float step = 0.375f;
        float total = step * melody.Length;

        var clip = Make("forest_music", total, t =>
        {
            int idx = Mathf.Min(melody.Length - 1, (int)(t / step));
            float lt = t - idx * step;
            float v = 0f;
            if (melody[idx] >= 0)
            {
                float f = scale[melody[idx]];
                float env = Mathf.Clamp01(lt / 0.012f) * Mathf.Exp(-lt * 2.6f);
                v += (Sine(f, lt) * 0.5f + Sine(f * 2f, lt) * 0.12f + Sine(f * 3f, lt) * 0.04f) * env;
            }
            int bi = Mathf.Min(3, idx / 8);
            float bt = t - bi * 8f * step;
            float benv = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * bt / (step * 4f));
            v += Sine(bass[bi], t) * 0.28f * benv;
            // little shimmer pad
            v += Sine(scale[(bi * 2) % 8] * 2f, t) * 0.04f;
            return v * 0.8f;
        });
        clips.Remove(Sfx.Click); // no-op safeguard to keep dictionary tidy
        clips[Sfx.Click] = Make("click", 0.06f, t => Sine(820f, t) * Mathf.Exp(-t * 55f) * 0.6f);
        music.clip = clip;
    }
}
