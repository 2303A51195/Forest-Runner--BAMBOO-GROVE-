using UnityEngine;

/// <summary>Colour palettes for the four forest themes.</summary>
public class ForestTheme
{
    public string name;
    public Color skyTop, skyHorizon, sun, grass, verge, path, stripe, leafA, leafB, trunk, accent, ambient, hillFar, hillNear;

    static Color H(string hex) { Color c; ColorUtility.TryParseHtmlString(hex, out c); return c; }

    public static readonly ForestTheme[] All = new ForestTheme[]
    {
        new ForestTheme { name = "ENCHANTED FOREST",
            skyTop = H("#5BB8E8"), skyHorizon = H("#CFF3E4"), sun = H("#FFF2A8"), grass = H("#3F9E4A"), verge = H("#2F8040"),
            path = H("#B98E5A"), stripe = H("#8A6338"), leafA = H("#2F9A45"), leafB = H("#1F7A38"), trunk = H("#6B4A2B"),
            accent = H("#FF6FA8"), ambient = H("#FFF59D"), hillFar = H("#9AD8C2"), hillNear = H("#5FB58C") },
        new ForestTheme { name = "BAMBOO GROVE",
            skyTop = H("#8FD6C8"), skyHorizon = H("#F4FCD8"), sun = H("#FFFBD0"), grass = H("#86C24F"), verge = H("#6BA83C"),
            path = H("#DCC88E"), stripe = H("#B09A5E"), leafA = H("#7FC450"), leafB = H("#5BA33A"), trunk = H("#9BCB5E"),
            accent = H("#FFE45C"), ambient = H("#C6F27A"), hillFar = H("#B5E3B0"), hillNear = H("#86C987") },
        new ForestTheme { name = "MAGIC WOODLAND",
            skyTop = H("#5A3F9E"), skyHorizon = H("#F3B8E6"), sun = H("#FFD6F4"), grass = H("#4DA38A"), verge = H("#3A8573"),
            path = H("#C8A2E0"), stripe = H("#9A74BE"), leafA = H("#C157B5"), leafB = H("#8A56D8"), trunk = H("#4A2D5E"),
            accent = H("#7FFFF0"), ambient = H("#FFB8F2"), hillFar = H("#A98BD6"), hillNear = H("#7D68C0") },
        new ForestTheme { name = "TREE VILLAGE",
            skyTop = H("#FF9F5A"), skyHorizon = H("#FFE7A8"), sun = H("#FFD27A"), grass = H("#75AE3C"), verge = H("#5E9430"),
            path = H("#CFA062"), stripe = H("#A57A3F"), leafA = H("#4F9C36"), leafB = H("#3A7D2B"), trunk = H("#774A2A"),
            accent = H("#FFC857"), ambient = H("#FFD36B"), hillFar = H("#E8B77A"), hillNear = H("#B0A05A") },
    };

    public static ForestTheme Lerp(ForestTheme a, ForestTheme b, float t)
    {
        var r = new ForestTheme();
        r.name = t < 0.5f ? a.name : b.name;
        r.skyTop = Color.Lerp(a.skyTop, b.skyTop, t);
        r.skyHorizon = Color.Lerp(a.skyHorizon, b.skyHorizon, t);
        r.sun = Color.Lerp(a.sun, b.sun, t);
        r.grass = Color.Lerp(a.grass, b.grass, t);
        r.verge = Color.Lerp(a.verge, b.verge, t);
        r.path = Color.Lerp(a.path, b.path, t);
        r.stripe = Color.Lerp(a.stripe, b.stripe, t);
        r.leafA = Color.Lerp(a.leafA, b.leafA, t);
        r.leafB = Color.Lerp(a.leafB, b.leafB, t);
        r.trunk = Color.Lerp(a.trunk, b.trunk, t);
        r.accent = Color.Lerp(a.accent, b.accent, t);
        r.ambient = Color.Lerp(a.ambient, b.ambient, t);
        r.hillFar = Color.Lerp(a.hillFar, b.hillFar, t);
        r.hillNear = Color.Lerp(a.hillNear, b.hillNear, t);
        return r;
    }
}
