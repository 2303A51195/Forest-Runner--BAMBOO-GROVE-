using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Loads scenes safely (prints a clear message if a scene is missing from Build Settings).</summary>
public static class SceneLoader
{
    public const string MenuScene = "MainMenu";
    public const string GameScene = "GameScene";

    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogError("Scene '" + sceneName + "' is not in Build Settings. Use the menu: Forest Runner > Setup Project.");
    }
}
