using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PocketPlanetProjectSetup
{
    const string ScenePath = "Assets/PocketPlanetFoundry/Scenes/Foundry.unity";
    static PocketPlanetProjectSetup()
    {
        EditorApplication.delayCall += EnsureProjectSetup;
    }
    static void EnsureProjectSetup()
    {
        PlayerSettings.productName = "Pocket Planet Foundry";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        if (!File.Exists(ScenePath))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(x => x.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        AssetDatabase.SaveAssets();
    }
}
