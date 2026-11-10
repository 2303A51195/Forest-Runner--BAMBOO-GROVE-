#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Runs automatically when the project opens: creates MainMenu.unity and GameScene.unity (with camera and
/// SceneRoot), registers them in Build Settings, sets portrait orientation, and makes the Play button
/// always start from the Main Menu. Also available as the menu "Forest Runner > Setup Project".
/// </summary>
[InitializeOnLoad]
public static class ForestRunnerSetup
{
    const string MenuPath = "Assets/Scenes/MainMenu.unity";
    const string GamePath = "Assets/Scenes/GameScene.unity";

    static ForestRunnerSetup()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    static void AutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        bool missing = !File.Exists(MenuPath) || !File.Exists(GamePath);
        if (missing) CreateScenes();
        ApplySettings();
        if (missing) EditorSceneManager.OpenScene(MenuPath);
    }

    [MenuItem("Forest Runner/Setup Project (rebuild scenes + settings)")]
    static void Manual()
    {
        CreateScenes();
        ApplySettings();
        EditorSceneManager.OpenScene(MenuPath);
        Debug.Log("Forest Runner: scenes and settings are ready. Press Play!");
    }

    static void CreateScenes()
    {
        Directory.CreateDirectory("Assets/Scenes");
        MakeScene(MenuPath);
        MakeScene(GamePath);
        AssetDatabase.Refresh();
    }

    static void MakeScene(string path)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.36f, 0.72f, 0.91f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();
        new GameObject("SceneRoot").AddComponent<SceneRoot>();
        EditorSceneManager.SaveScene(scene, path);
    }

    static void ApplySettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuPath, true),
            new EditorBuildSettingsScene(GamePath, true)
        };

        PlayerSettings.productName = "Forest Runner";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

        var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath);
        if (menu != null) EditorSceneManager.playModeStartScene = menu;
    }
}
#endif
