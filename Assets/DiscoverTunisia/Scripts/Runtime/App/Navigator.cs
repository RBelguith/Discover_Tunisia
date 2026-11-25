using UnityEngine.SceneManagement;

namespace DiscoverTunisia.App
{
    public enum AppScreen { Home, Identification, Scan, Globe, AR }

    /// <summary>Loads one screen scene at a time and ignores taps while a load is running.</summary>
    public static class Navigator
    {
        private static readonly string[] Scenes = { "01_Accueil", "02_Identification", "03_Scan", "04_Globe", "05_AR" };
        private static bool _loading;

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => _loading = false;

        public static string SceneName(AppScreen screen) => Scenes[(int)screen];

        public static void Go(AppScreen screen)
        {
            if (_loading) return;
            _loading = true;
            var op = SceneManager.LoadSceneAsync(SceneName(screen));
            if (op == null) { _loading = false; return; }
            op.completed += _ => _loading = false;
        }
    }
}
