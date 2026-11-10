using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Builds a whole scene at start-up: camera, environment, player, spawners, UI.
/// The scene called "GameScene" becomes the game; every other scene becomes the main menu.
/// The editor setup script puts one of these in each scene, and a fallback creates one automatically,
/// so nothing ever has to be wired by hand.
/// </summary>
public class SceneRoot : MonoBehaviour
{
    bool built;

    void Awake() { Build(); }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Hook()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.sceneLoaded += OnLoaded;
    }

    static void OnLoaded(Scene s, LoadSceneMode m)
    {
        if (Object.FindFirstObjectByType<SceneRoot>() == null)
            new GameObject("SceneRoot").AddComponent<SceneRoot>();
    }

    void Build()
    {
        if (built) return;
        built = true;
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
        bool game = SceneManager.GetActiveScene().name == SceneLoader.GameScene;

        CameraController.Create();
        ObjectPooler.Ensure();
        AudioManager.Ensure();

        var env = new GameObject("Environment").AddComponent<EnvironmentController>();
        env.cycleThemesInMenu = !game;
        env.Init();

        if (game) BuildGame(); else BuildMenu();
    }

    void BuildMenu()
    {
        new GameObject("MainMenu").AddComponent<MainMenuController>().Init();
    }

    void BuildGame()
    {
        var player = new GameObject("Player").AddComponent<PlayerController>();
        player.Init();

        var world = new GameObject("World").AddComponent<WorldController>();
        world.Init(player);

        var spawner = new GameObject("ObstacleSpawner").AddComponent<ObstacleSpawner>();
        spawner.Init();

        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        var ui = new GameObject("UIManager").AddComponent<UIManager>();
        gm.Init(player, ui);
        ui.Init(gm);
    }
}
