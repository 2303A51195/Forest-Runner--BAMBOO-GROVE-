using UnityEngine;

/// <summary>All persistent data (PlayerPrefs). Safe to call from anywhere.</summary>
public static class SaveData
{
    const string BestKey = "FR_Best", CoinsKey = "FR_Coins", LeavesKey = "FR_Leaves", SoundKey = "FR_Sound", MusicKey = "FR_Music";

    public static int Best { get { return PlayerPrefs.GetInt(BestKey, 0); } set { PlayerPrefs.SetInt(BestKey, value); } }

    // New players start with 100 coins so the Revive button can be tried right away.
    public static int Coins { get { return PlayerPrefs.GetInt(CoinsKey, 100); } set { PlayerPrefs.SetInt(CoinsKey, Mathf.Max(0, value)); } }

    public static int Leaves { get { return PlayerPrefs.GetInt(LeavesKey, 0); } set { PlayerPrefs.SetInt(LeavesKey, value); } }
    public static bool SoundOn { get { return PlayerPrefs.GetInt(SoundKey, 1) == 1; } set { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); } }
    public static bool MusicOn { get { return PlayerPrefs.GetInt(MusicKey, 1) == 1; } set { PlayerPrefs.SetInt(MusicKey, value ? 1 : 0); } }

    public static void Save() { PlayerPrefs.Save(); }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(BestKey);
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(LeavesKey);
        PlayerPrefs.Save();
    }
}
