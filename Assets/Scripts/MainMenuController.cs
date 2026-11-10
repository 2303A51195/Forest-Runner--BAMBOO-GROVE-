using UnityEngine;
using UnityEngine.UI;

/// <summary>Main menu: title, Play, Settings, Sound toggle, best score, coin total and a running fox.</summary>
public class MainMenuController : MonoBehaviour
{
    FoxRig fox;
    float animT;
    GameObject settingsPanel;
    Button soundBtn, soundBtn2, musicBtn, resetBtn;
    Text bestText, coinText;
    bool resetArmed;

    static readonly Color Gold = new Color(1f, 0.86f, 0.25f);
    static readonly Color Green = new Color(0.25f, 0.75f, 0.3f);
    static readonly Color Orange = new Color(0.95f, 0.55f, 0.15f);
    static readonly Color Blue = new Color(0.25f, 0.55f, 0.9f);

    public void Init()
    {
        Time.timeScale = 1f;
        var foxParent = new GameObject("MenuFox").transform;
        foxParent.position = new Vector3(0f, LaneManager.GroundY, 0f);
        fox = FoxRig.Build(foxParent);
        SpriteFactory.AddOrder(foxParent, 1900);

        var ui = UIRoot.Create();
        var root = ui.root;
        Vector2 c = new Vector2(0.5f, 0.5f), tr = new Vector2(1f, 1f), bc = new Vector2(0.5f, 0f);

        UIKit.Label(root, "FOREST", 210, Gold, c, new Vector2(0f, 700f), new Vector2(1060f, 240f));
        UIKit.Label(root, "RUNNER", 190, new Color(0.5f, 1f, 0.4f), c, new Vector2(0f, 520f), new Vector2(1060f, 220f));
        bestText = UIKit.Label(root, "", 48, Color.white, c, new Vector2(0f, 360f), new Vector2(900f, 70f));

        var pill = UIKit.Img(root, "CoinPill", SpriteFactory.Rounded, new Color(0f, 0f, 0f, 0.45f), tr, new Vector2(-230f, -110f), new Vector2(380f, 100f), true);
        UIKit.Img(pill.transform, "CoinIcon", SpriteFactory.Circle, new Color(0.78f, 0.54f, 0f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(72f, 72f));
        UIKit.Img(pill.transform, "CoinIn", SpriteFactory.Circle, new Color(1f, 0.82f, 0.2f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(58f, 58f));
        coinText = UIKit.Label(pill.transform, "0", 58, Color.white, c, new Vector2(45f, 0f), new Vector2(240f, 90f));

        UIKit.MakeButton(root, "PLAY", c, new Vector2(0f, 170f), new Vector2(720f, 200f), Green, () => SceneLoader.Load(SceneLoader.GameScene), 100);
        UIKit.MakeButton(root, "SETTINGS", c, new Vector2(0f, -50f), new Vector2(620f, 150f), Orange, () => ShowSettings(true), 60);
        soundBtn = UIKit.MakeButton(root, " ", c, new Vector2(0f, -230f), new Vector2(620f, 150f), Blue, ToggleSound, 60);
        UIKit.Label(root, "ARROWS / A D = MOVE    SPACE / W = JUMP    S = SLIDE    ESC = PAUSE", 28, new Color(1f, 1f, 1f, 0.85f), bc, new Vector2(0f, 60f), new Vector2(1060f, 50f));

        BuildSettings(root);
        RefreshTexts();
    }

    void BuildSettings(RectTransform root)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);
        var bg = UIKit.NewRect(root, "SettingsPanel");
        UIKit.Stretch(bg);
        bg.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        var card = UIKit.Img(bg, "Card", SpriteFactory.Rounded, new Color(0.1f, 0.22f, 0.14f, 0.95f), c, Vector2.zero, new Vector2(880f, 1080f), true);
        UIKit.Label(card.transform, "SETTINGS", 100, Gold, c, new Vector2(0f, 440f), new Vector2(800f, 130f));
        soundBtn2 = UIKit.MakeButton(card.transform, " ", c, new Vector2(0f, 250f), new Vector2(640f, 140f), Blue, ToggleSound, 56);
        musicBtn = UIKit.MakeButton(card.transform, " ", c, new Vector2(0f, 80f), new Vector2(640f, 140f), Blue, ToggleMusic, 56);
        resetBtn = UIKit.MakeButton(card.transform, " ", c, new Vector2(0f, -90f), new Vector2(640f, 140f), new Color(0.8f, 0.3f, 0.3f), ResetProgress, 44);
        UIKit.Label(card.transform, "Swipe or use the keyboard.\nCollect coins, fruit and leaves.\nDodge, jump and slide to survive!", 38, Color.white, c, new Vector2(0f, -260f), new Vector2(800f, 190f));
        UIKit.MakeButton(card.transform, "BACK", c, new Vector2(0f, -440f), new Vector2(500f, 130f), Green, () => ShowSettings(false), 60);
        settingsPanel = bg.gameObject;
        settingsPanel.SetActive(false);
    }

    void ShowSettings(bool show)
    {
        resetArmed = false;
        settingsPanel.SetActive(show);
        RefreshTexts();
    }

    void ToggleSound()
    {
        SaveData.SoundOn = !SaveData.SoundOn;
        SaveData.Save();
        if (AudioManager.Instance != null) AudioManager.Instance.ApplySettings();
        RefreshTexts();
    }

    void ToggleMusic()
    {
        SaveData.MusicOn = !SaveData.MusicOn;
        SaveData.Save();
        if (AudioManager.Instance != null) AudioManager.Instance.ApplySettings();
        RefreshTexts();
    }

    void ResetProgress()
    {
        if (!resetArmed) { resetArmed = true; RefreshTexts(); return; }
        SaveData.ResetAll();
        resetArmed = false;
        RefreshTexts();
    }

    void RefreshTexts()
    {
        string snd = "SOUND: " + (SaveData.SoundOn ? "ON" : "OFF");
        UIKit.SetButtonText(soundBtn, snd);
        UIKit.SetButtonText(soundBtn2, snd);
        UIKit.SetButtonText(musicBtn, "MUSIC: " + (SaveData.MusicOn ? "ON" : "OFF"));
        UIKit.SetButtonText(resetBtn, resetArmed ? "TAP AGAIN TO CONFIRM" : "RESET PROGRESS");
        bestText.text = "BEST SCORE:  " + SaveData.Best;
        coinText.text = SaveData.Coins.ToString();
    }

    void Update()
    {
        animT += Time.deltaTime * 13f;
        if (fox != null) fox.Animate(animT, 0f, true, false, 0f);
    }
}
