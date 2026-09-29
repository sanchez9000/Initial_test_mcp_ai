using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds Splash, Landing, and two simple mini-game scenes with UI only.
/// Menu: Tools / Arcade Sample / Setup Scenes
/// </summary>
public static class SetupArcadeScenes
{
    const string ScenesFolder = "Assets/Scenes";
    const string TriggerPath = "Assets/Editor/RUN_SETUP.txt";

    [InitializeOnLoadMethod]
    static void AutoSetupFromTrigger()
    {
        if (!File.Exists(TriggerPath)) return;
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(TriggerPath)) return;
            File.Delete(TriggerPath);
            if (File.Exists(TriggerPath + ".meta"))
                File.Delete(TriggerPath + ".meta");
            SetupAll();
        };
    }

    [MenuItem("Tools/Arcade Sample/Setup Scenes")]
    public static void SetupAll()
    {
        // Disabled: this used to overwrite Build Settings and break SampleScene/ArcadeApp.
        // The live arcade (splash/menu/slots/coin race) runs from SampleScene via ArcadeApp.
        bool proceed = EditorUtility.DisplayDialog(
            "Setup Scenes disabled",
            "This menu used to replace Build Settings with Splash/Landing scenes and broke Play Mode.\n\n" +
            "The working game is SampleScene + ArcadeApp.\n\n" +
            "Click OK only if you still want to regenerate the old multi-scene files WITHOUT changing Build Settings.",
            "Regenerate scene files only",
            "Cancel");
        if (!proceed) return;

        Directory.CreateDirectory(ScenesFolder);

        CreateSplashScene();
        CreateLandingScene();
        CreateClickSquareScene();
        CreateCatchCircleScene();
        // Intentionally does NOT call ConfigureBuildSettings() anymore.

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Debug.Log("Old multi-scene files regenerated. Build Settings left on SampleScene. Open SampleScene and press Play.");
    }

    [MenuItem("Tools/Arcade Sample/Restore SampleScene Build Settings")]
    public static void RestoreSampleSceneBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
        };
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Debug.Log("Build Settings restored to SampleScene only.");
    }

    static void CreateSplashScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();
        var root = new GameObject("SplashRoot");
        var controller = root.AddComponent<SplashController>();
        SplashController.BuildUi(canvas.transform);
        CreateEventSystem();
        EnsureCamera();
        EditorSceneManager.SaveScene(scene, ScenesFolder + "/Splash.unity");
    }

    static void CreateLandingScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();
        var root = new GameObject("LandingRoot");
        var controller = root.AddComponent<LandingMenuController>();
        LandingMenuController.BuildUi(canvas.transform, controller);
        CreateEventSystem();
        EnsureCamera();
        EditorSceneManager.SaveScene(scene, ScenesFolder + "/Landing.unity");
    }

    static void CreateClickSquareScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();
        var root = new GameObject("ClickSquareRoot");
        var game = root.AddComponent<ClickSquareGame>();
        ClickSquareGame.BuildUi(canvas.transform, game);
        CreateEventSystem();
        EnsureCamera();
        EditorSceneManager.SaveScene(scene, ScenesFolder + "/GameClickSquare.unity");
    }

    static void CreateCatchCircleScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();
        var root = new GameObject("CatchCircleRoot");
        var game = root.AddComponent<CatchCircleGame>();
        CatchCircleGame.BuildUi(canvas.transform, game);
        CreateEventSystem();
        EnsureCamera();
        EditorSceneManager.SaveScene(scene, ScenesFolder + "/GameCatchCircle.unity");
    }

    static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static void CreateEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
    }

    static void EnsureCamera()
    {
        if (Camera.main != null) return;
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
        cam.orthographic = true;
        camGo.AddComponent<AudioListener>();
    }

    static void ConfigureBuildSettings()
    {
        var scenes = new[]
        {
            ScenesFolder + "/Splash.unity",
            ScenesFolder + "/Landing.unity",
            ScenesFolder + "/GameClickSquare.unity",
            ScenesFolder + "/GameCatchCircle.unity",
        };

        var list = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            list[i] = new EditorBuildSettingsScene(scenes[i], true);
        EditorBuildSettings.scenes = list;
    }
}


