using Cipher.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Cipher.EditorTools
{
    public static class Mvp1SceneMenu
    {
        const string ScenePath = "Assets/Scenes/MVP1_Blacksite.unity";

        [MenuItem("CIPHER/Create MVP1 Scene")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("Mvp1Bootstrap");
            bootstrap.AddComponent<Mvp1Bootstrap>();

            // Placeholder camera so the empty scene is not pitch-black before Awake rebuilds player cam.
            var camGo = new GameObject("BootstrapCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 12f, -18f);
            camGo.transform.rotation = Quaternion.Euler(25f, 0f, 0f);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.Refresh();
            Debug.Log("[CIPHER] MVP1 scene saved at " + ScenePath);
        }
    }
}
