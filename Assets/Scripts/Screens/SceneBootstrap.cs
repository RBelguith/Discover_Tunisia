using UnityEngine;
using UnityEngine.SceneManagement;
using DiscoverTunisia.Core;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Screens
{
    /// <summary>
    /// Base commune des scènes : crée caméra, éclairage, EventSystem et canvas
    /// entièrement par code (les scènes .unity ne contiennent que ce bootstrap).
    /// </summary>
    public abstract class SceneBootstrap : MonoBehaviour
    {
        protected Canvas Canvas { get; private set; }
        protected Camera Cam { get; private set; }

        /// <summary>Les scènes AR laissent la caméra Vuforia intacte (false).</summary>
        protected virtual bool CreateCameraIfMissing => true;

        protected virtual void Awake()
        {
            Application.targetFrameRate = 60;
            SetupCamera();
            SetupLight();
            UILib.EnsureEventSystem();
            Canvas = UILib.CreateCanvas("Canvas");
        }

        private void SetupCamera()
        {
            Cam = Camera.main;
            if (Cam == null)
            {
                if (!CreateCameraIfMissing) return;
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                Cam = go.AddComponent<Camera>();
                if (Object.FindFirstObjectByType<AudioListener>() == null)
                    go.AddComponent<AudioListener>();
            }
            else if (Cam.gameObject.name == "ARCamera")
            {
                return; // caméra Vuforia : configuration laissée à Vuforia
            }
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = Theme.BackgroundBottom;
            Cam.fieldOfView = 45f;
            Cam.nearClipPlane = 0.1f;
            Cam.transform.position = new Vector3(0f, 0f, -10f);
            Cam.transform.rotation = Quaternion.identity;
        }

        private void SetupLight()
        {
            if (Object.FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.97f, 0.92f);
            go.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        protected static void GoTo(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>Navigation utilitaire (F10).</summary>
    public static class Nav
    {
        public static void Home() => SceneManager.LoadScene(SceneNames.Accueil);
        public static void Identification() => SceneManager.LoadScene(SceneNames.Identification);
        public static void Globe() => SceneManager.LoadScene(SceneNames.Globe);
        public static void AR() => SceneManager.LoadScene(SceneNames.AR);
    }
}
