using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DiscoverTunisia.EditorTools
{
    /// <summary>
    /// Configuration automatique du projet :
    ///  - matériaux URP placés dans Resources (garantis inclus dans les builds),
    ///  - les 4 scènes (chaque scène ne contient qu'un GameObject « Bootstrap »),
    ///  - la liste de build,
    ///  - les réglages joueur Android.
    /// S'exécute automatiquement après la première compilation, ou via le menu
    /// « DiscoverTunisia > Tout configurer ».
    /// </summary>
    public static class ProjectSetup
    {
        private const string SceneDir = "Assets/Scenes";
        private const string MatDir = "Assets/Resources/Materials";

        [InitializeOnLoadMethod]
        private static void AutoSetupOnLoad()
        {
            EditorApplication.delayCall += RunOnce;
        }

        private static void RunOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            // Les scènes peuvent déjà exître (fichiers livrés) : on crée quand même
            // les matériaux Resources et on applique les réglages joueur.
            CreateMaterials();
            if (!HasScenes()) CreateScenes();
            ApplyPlayerSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DiscoverTunisia] Matériaux et réglages vérifiés.");
        }

        private static bool HasScenes()
        {
            return File.Exists(SceneDir + "/01_Accueil.unity");
        }

        [MenuItem("DiscoverTunisia/Tout configurer")]
        public static void SetupAll()
        {
            try
            {
                CreateMaterials();
                CreateScenes();
                ApplyPlayerSettings();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[DiscoverTunisia] Configuration terminée : matériaux, 4 scènes, réglages Android.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[DiscoverTunisia] Échec de la configuration : " + e);
            }
        }

        // ------------------------------------------------------------------
        //  Matériaux (Resources => toujours inclus dans les builds)
        // ------------------------------------------------------------------
        private static void CreateMaterials()
        {
            Directory.CreateDirectory(MatDir);
            EnsureMat(MatDir + "/URP_Lit.mat", "Universal Render Pipeline/Lit");
            EnsureMat(MatDir + "/URP_Unlit.mat", "Universal Render Pipeline/Unlit");
        }

        private static void EnsureMat(string path, string shaderName)
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError("[DiscoverTunisia] Shader introuvable : " + shaderName);
                return;
            }
            AssetDatabase.CreateAsset(new Material(shader), path);
        }

        // ------------------------------------------------------------------
        //  Scènes
        // ------------------------------------------------------------------
        private static void CreateScenes()
        {
            Directory.CreateDirectory(SceneDir);
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            CreateScene(SceneDir + "/01_Accueil.unity", typeof(Screens.HomeBootstrap));
            CreateScene(SceneDir + "/02_Identification.unity", typeof(Screens.IdentificationBootstrap));
            CreateScene(SceneDir + "/03_Globe.unity", typeof(Screens.GlobeBootstrap));
            CreateScene(SceneDir + "/04_AR.unity", typeof(Screens.ARBootstrap));

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(SceneDir + "/01_Accueil.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/02_Identification.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/03_Globe.unity", true),
                new EditorBuildSettingsScene(SceneDir + "/04_AR.unity", true),
            };
        }

        private static void CreateScene(string path, System.Type bootstrapType)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Bootstrap");
            go.AddComponent(bootstrapType);
            EditorSceneManager.SaveScene(scene, path);
        }

        // ------------------------------------------------------------------
        //  Réglages joueur (Android, paysage)
        // ------------------------------------------------------------------
        private static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = "Projet Universitaire";
            PlayerSettings.productName = "Discover Tunisia";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.discovertunisia.app");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // Mono pour les builds de démo rapides ; passer en IL2CPP pour un
            // dépôt sur le Play Store (voir README.md).
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        }
    }
}
