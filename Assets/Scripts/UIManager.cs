using UnityEngine;
using UnityEngine.UI;

/// <summary>In-game interface: HUD, power-up timers, banners, pause menu and game-over screen.</summary>
public class UIManager : MonoBehaviour
{
    class Slot { public RectTransform rt; public Image fill; public PowerType type; }

    GameManager gm;
    UIRoot uiRoot;
    Text coinText, scoreText, leafText, distText, bannerText, hintText;
    Text goScore, goBest, goCoins, goExtra;
    Button reviveBtn;
    GameObject pausePanel, overPanel;
    readonly Slot[] slots = new Slot[4];
    float bumpCoin, bumpScore, bumpLeaf, bannerT, hintT = 8f;
    int lastCoins = -1, lastScore = -1, lastLeaves = -1, lastDist = -1;

    static readonly Color Gold = new Color(1f, 0.86f, 0.25f);
    static readonly Color Green = new Color(0.25f, 0.75f, 0.3f);
    static readonly Color Orange = new Color(0.95f, 0.55f, 0.15f);
    static readonly Color Blue = new Color(0.25f, 0.55f, 0.9f);
    static readonly Color Dim = new Color(0f, 0f, 0f, 0.45f);

    public void Init(GameManager g)
    {
        gm = g;
        uiRoot = UIRoot.Create();
        var root = uiRoot.root;
        BuildHud(root);
        BuildPause(root);
        BuildGameOver(root);
    }

    // ------------------------------------------------------------------ build
    void BuildHud(RectTransform root)
    {
        Vector2 tl = new Vector2(0f, 1f), tc = new Vector2(0.5f, 1f), tr = new Vector2(1f, 1f), c = new Vector2(0.5f, 0.5f), bc = new Vector2(0.5f, 0f);

        // coin pill
        var pill = UIKit.Img(root, "CoinPill", SpriteFactory.Rounded, Dim, tl, new Vector2(235f, -110f), new Vector2(380f, 100f), true);
        UIKit.Img(pill.transform, "CoinIcon", SpriteFactory.Circle, new Color(0.78f, 0.54f, 0f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(72f, 72f));
        UIKit.Img(pill.transform, "CoinIn", SpriteFactory.Circle, new Color(1f, 0.82f, 0.2f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(58f, 58f));
        UIKit.Img(pill.transform, "CoinShine", SpriteFactory.Circle, new Color(1f, 1f, 1f, 0.7f), new Vector2(0f, 0.5f), new Vector2(46f, 12f), new Vector2(14f, 14f));
        coinText = UIKit.Label(pill.transform, "0", 58, Color.white, c, new Vector2(45f, 0f), new Vector2(240f, 90f));

        // leaf pill
        var lp = UIKit.Img(root, "LeafPill", SpriteFactory.Rounded, Dim, tl, new Vector2(205f, -225f), new Vector2(320f, 88f), true);
        var leaf = UIKit.Img(lp.transform, "LeafIcon", SpriteFactory.Circle, new Color(0.3f, 0.85f, 0.4f), new Vector2(0f, 0.5f), new Vector2(55f, 0f), new Vector2(34f, 64f));
        leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 35f);
        leafText = UIKit.Label(lp.transform, "0", 50, Color.white, c, new Vector2(40f, 0f), new Vector2(200f, 80f));

        // score
        UIKit.Label(root, "SCORE", 34, new Color(1f, 1f, 1f, 0.85f), tc, new Vector2(0f, -70f), new Vector2(400f, 50f));
        scoreText = UIKit.Label(root, "0", 112, Color.white, tc, new Vector2(0f, -145f), new Vector2(520f, 130f));
        distText = UIKit.Label(root, "0 m", 40, new Color(1f, 1f, 1f, 0.8f), tc, new Vector2(0f, -232f), new Vector2(400f, 50f));

        // pause button
        var pb = UIKit.MakeButton(root, "", tr, new Vector2(-100f, -115f), new Vector2(120f, 120f), new Color(0.15f, 0.2f, 0.25f, 0.8f), OnPauseClicked);
        UIKit.Img(pb.transform, "BarL", SpriteFactory.Square, Color.white, c, new Vector2(-17f, 0f), new Vector2(18f, 56f));
        UIKit.Img(pb.transform, "BarR", SpriteFactory.Square, Color.white, c, new Vector2(17f, 0f), new Vector2(18f, 56f));

        // power-up slots
        string[] names = { "MAG", "SHLD", "BOOST", "x2" };
        for (int i = 0; i < 4; i++)
        {
            var s = new Slot();
            s.type = (PowerType)i;
            Color col = PowerUp.ColorOf(s.type);
            var bg = UIKit.Img(root, "Slot" + i, SpriteFactory.Circle, new Color(0f, 0f, 0f, 0.55f), tc, new Vector2(0f, -360f), new Vector2(134f, 134f));
            s.rt = bg.rectTransform;
            s.fill = UIKit.Img(bg.transform, "Fill", SpriteFactory.Circle, col, c, Vector2.zero, new Vector2(134f, 134f));
            s.fill.type = Image.Type.Filled;
            s.fill.fillMethod = Image.FillMethod.Radial360;
            s.fill.fillOrigin = (int)Image.Origin360.Top;
            s.fill.fillClockwise = false;
            UIKit.Img(bg.transform, "Inner", SpriteFactory.Circle, new Color(0.1f, 0.12f, 0.16f, 0.95f), c, Vector2.zero, new Vector2(102f, 102f));
            UIKit.Label(bg.transform, names[i], 30, Color.white, c, Vector2.zero, new Vector2(100f, 50f));
            s.rt.gameObject.SetActive(false);
            slots[i] = s;
        }

        bannerText = UIKit.Label(root, "", 84, Gold, c, new Vector2(0f, 420f), new Vector2(1000f, 140f));
        hintText = UIKit.Label(root, "SWIPE  or  ARROW KEYS / WASD\nSPACE = JUMP     S / DOWN = SLIDE", 38, Color.white, bc, new Vector2(0f, 190f), new Vector2(1000f, 120f));
    }

    void BuildPause(RectTransform root)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);
        var bg = UIKit.NewRect(root, "PausePanel");
        UIKit.Stretch(bg);
        var im = bg.gameObject.AddComponent<Image>();
        im.color = new Color(0f, 0f, 0f, 0.65f);
        UIKit.Label(bg, "PAUSED", 140, Gold, c, new Vector2(0f, 400f), new Vector2(900f, 180f));
        UIKit.MakeButton(bg, "RESUME", c, new Vector2(0f, 120f), new Vector2(660f, 170f), Green, () => gm.Resume(), 70);
        UIKit.MakeButton(bg, "RESTART", c, new Vector2(0f, -100f), new Vector2(660f, 170f), Orange, () => gm.Restart(), 70);
        UIKit.MakeButton(bg, "HOME", c, new Vector2(0f, -320f), new Vector2(660f, 170f), Blue, () => gm.Home(), 70);
        pausePanel = bg.gameObject;
        pausePanel.SetActive(false);
    }

    void BuildGameOver(RectTransform root)
    {
        Vector2 c = new Vector2(0.5f, 0.5f);
        var bg = UIKit.NewRect(root, "GameOverPanel");
        UIKit.Stretch(bg);
        var im = bg.gameObject.AddComponent<Image>();
        im.color = new Color(0f, 0f, 0f, 0.72f);

        UIKit.Label(bg, "GAME OVER", 150, new Color(1f, 0.38f, 0.3f), c, new Vector2(0f, 650f), new Vector2(1000f, 190f));
        var card = UIKit.Img(bg, "Card", SpriteFactory.Rounded, new Color(0.1f, 0.22f, 0.14f, 0.92f), c, new Vector2(0f, 190f), new Vector2(860f, 760f), true);
        UIKit.Label(card.transform, "SCORE", 46, new Color(1f, 1f, 1f, 0.8f), c, new Vector2(0f, 290f), new Vector2(600f, 60f));
        goScore = UIKit.Label(card.transform, "0", 170, Color.white, c, new Vector2(0f, 180f), new Vector2(760f, 200f));
        goBest = UIKit.Label(card.transform, "BEST 0", 60, Gold, c, new Vector2(0f, 50f), new Vector2(760f, 80f));
        goCoins = UIKit.Label(card.transform, "COINS  +0", 64, new Color(1f, 0.85f, 0.3f), c, new Vector2(0f, -60f), new Vector2(760f, 90f));
        goExtra = UIKit.Label(card.transform, "LEAVES 0     FRUITS 0", 46, new Color(0.7f, 1f, 0.7f), c, new Vector2(0f, -170f), new Vector2(760f, 70f));

        reviveBtn = UIKit.MakeButton(bg, "REVIVE", c, new Vector2(0f, -330f), new Vector2(780f, 160f), new Color(0.95f, 0.7f, 0.1f), () => gm.Revive(), 62);
        UIKit.MakeButton(bg, "RETRY", c, new Vector2(-205f, -530f), new Vector2(370f, 150f), Green, () => gm.Restart(), 62);
        UIKit.MakeButton(bg, "HOME", c, new Vector2(205f, -530f), new Vector2(370f, 150f), Blue, () => gm.Home(), 62);

        overPanel = bg.gameObject;
        overPanel.SetActive(false);
    }

    void OnPauseClicked()
    {
        if (gm != null) gm.Pause();
    }

    // ------------------------------------------------------------------ public API
    public void BumpCoins() { bumpCoin = 1f; }
    public void BumpScore() { bumpScore = 1f; }
    public void BumpLeaves() { bumpLeaf = 1f; }

    public void ShowBanner(string text)
    {
        bannerText.text = text;
        bannerT = 2.4f;
    }

    public void ShowPause(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }

    public void ShowGameOver(int score, int best, bool newBest, int coins, int leaves, int fruits)
    {
        goScore.text = score.ToString();
        goBest.text = newBest ? "NEW BEST!" : "BEST  " + best;
        goBest.color = newBest ? new Color(1f, 0.95f, 0.4f) : new Color(1f, 0.86f, 0.25f);
        goCoins.text = "COINS  +" + coins;
        goExtra.text = "LEAVES " + leaves + "     FRUITS " + fruits;

        if (gm.revivedThisRun) { UIKit.SetButtonText(reviveBtn, "ALREADY REVIVED"); reviveBtn.interactable = false; }
        else if (SaveData.Coins < GameManager.ReviveCost) { UIKit.SetButtonText(reviveBtn, "REVIVE: NEED " + GameManager.ReviveCost + " COINS"); reviveBtn.interactable = false; }
        else { UIKit.SetButtonText(reviveBtn, "REVIVE  (-" + GameManager.ReviveCost + " COINS)"); reviveBtn.interactable = true; }

        overPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        if (overPanel != null) overPanel.SetActive(false);
    }

    // ------------------------------------------------------------------ per frame
    void Update()
    {
        if (gm == null) return;
        float dt = Time.unscaledDeltaTime;

        if (lastCoins != gm.RunCoins) { lastCoins = gm.RunCoins; coinText.text = lastCoins.ToString(); }
        if (lastScore != gm.Score) { lastScore = gm.Score; scoreText.text = lastScore.ToString(); }
        if (lastLeaves != gm.RunLeaves) { lastLeaves = gm.RunLeaves; leafText.text = lastLeaves.ToString(); }
        int dist = Mathf.FloorToInt(gm.Distance);
        if (lastDist != dist) { lastDist = dist; distText.text = dist + " m"; }

        bumpCoin = Mathf.Max(0f, bumpCoin - dt * 4f);
        bumpScore = Mathf.Max(0f, bumpScore - dt * 4f);
        bumpLeaf = Mathf.Max(0f, bumpLeaf - dt * 4f);
        coinText.rectTransform.localScale = Vector3.one * (1f + 0.35f * bumpCoin);
        leafText.rectTransform.localScale = Vector3.one * (1f + 0.35f * bumpLeaf);
        float multPulse = gm.multiplierTime > 0f ? 0.08f * Mathf.Sin(Time.unscaledTime * 8f) : 0f;
        scoreText.rectTransform.localScale = Vector3.one * (1f + 0.3f * bumpScore + multPulse);
        scoreText.color = gm.multiplierTime > 0f ? new Color(0.85f, 0.5f, 1f) : Color.white;

        // power-up indicators
        float[] ratios =
        {
            gm.magnetTime / GameManager.MagnetDur, gm.shieldTime / GameManager.ShieldDur,
            gm.speedTime / GameManager.SpeedDur, gm.multiplierTime / GameManager.MultDur
        };
        int active = 0;
        for (int i = 0; i < 4; i++) if (ratios[i] > 0f) active++;
        int idx = 0;
        for (int i = 0; i < 4; i++)
        {
            bool on = ratios[i] > 0f;
            if (slots[i].rt.gameObject.activeSelf != on) slots[i].rt.gameObject.SetActive(on);
            if (!on) continue;
            slots[i].fill.fillAmount = Mathf.Clamp01(ratios[i]);
            slots[i].rt.anchoredPosition = new Vector2((idx - (active - 1) * 0.5f) * 155f, -360f);
            idx++;
        }

        // banner + hint
        if (bannerT > 0f)
        {
            bannerT -= dt;
            float a = Mathf.Clamp01(Mathf.Min(bannerT, 2.4f - bannerT) * 3f);
            var col = bannerText.color; col.a = a; bannerText.color = col;
            bannerText.rectTransform.localScale = Vector3.one * (0.85f + 0.15f * a);
        }
        else if (bannerText.color.a > 0f)
        {
            var col = bannerText.color; col.a = 0f; bannerText.color = col;
        }
        if (hintT > 0f)
        {
            hintT -= dt;
            var col = hintText.color; col.a = Mathf.Clamp01(hintT); hintText.color = col;
        }
    }
}
