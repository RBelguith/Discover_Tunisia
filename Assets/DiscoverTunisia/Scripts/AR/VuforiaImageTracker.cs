using System;
using System.Collections.Generic;
using DiscoverTunisia.Tracking;
using UnityEngine;
using Vuforia;

namespace DiscoverTunisia.AR
{
    /// <summary>Vuforia image tracking with runtime image targets made from textures, so no Target Manager database.</summary>
    public sealed class VuforiaImageTracker : ImageTracker
    {
        private const string LicenseResource = "VuforiaLicenseKey";

        [SerializeField, Tooltip("Seconds to wait for the camera before showing the failure message.")]
        private float startTimeout = 10f;

        private readonly List<(string id, Texture2D image, float width)> _pending = new List<(string, Texture2D, float)>();
        private readonly Dictionary<ObserverBehaviour, string> _targets = new Dictionary<ObserverBehaviour, string>();
        private bool _started;
        private bool _timedOut;
        private bool _subscribed;

        private void Awake()
        {
            if (CameraDisabled)
            {
                Fail("La caméra est désactivée pour cette session.");
                return;
            }
            var app = VuforiaApplication.Instance;
            app.OnVuforiaInitialized += OnInitialized;
            app.OnVuforiaStarted += OnStarted;
            app.OnVuforiaError += OnError;
            _subscribed = true;
            if (!LoadLicenseKey())
            {
                Fail("La clé de licence Vuforia est absente. Voir le README, section Vuforia.");
                return;
            }
            if (!app.IsInitialized) app.Initialize();
        }

        /// <summary>Puts the key on the config only for this session, so saving the config asset never writes it to disk.</summary>
        private static bool LoadLicenseKey()
        {
            var config = VuforiaConfiguration.Instance.Vuforia;
            if (!string.IsNullOrEmpty(config.LicenseKey)) return true;
            var key = Resources.Load<TextAsset>(LicenseResource);
            if (key == null || string.IsNullOrWhiteSpace(key.text)) return false;
            config.LicenseKey = key.text.Trim();
            Application.quitting += ClearLicenseKey;
            return true;
        }

        private static void ClearLicenseKey()
        {
            Application.quitting -= ClearLicenseKey;
            VuforiaConfiguration.Instance.Vuforia.LicenseKey = "";
        }

        private void Start() => Invoke(nameof(GiveUpIfStillStarting), startTimeout);

        /// <summary>A soft failure: OnStarted still recovers from it, for example after a slow camera permission prompt.</summary>
        private void GiveUpIfStillStarting()
        {
            if (State != TrackerState.Starting) return;
            _timedOut = true;
            SetState(TrackerState.Failed, "Vuforia n'a pas démarré. Dans l'éditeur, acceptez l'accord via Help > Vuforia Engine > Show Developer Agreement, puis vérifiez la webcam.");
        }

        private void Fail(string message)
        {
            _timedOut = false;
            SetState(TrackerState.Failed, message);
        }

        private void OnDestroy()
        {
            if (!_subscribed) return;
            var app = VuforiaApplication.Instance;
            app.OnVuforiaInitialized -= OnInitialized;
            app.OnVuforiaStarted -= OnStarted;
            app.OnVuforiaError -= OnError;
            foreach (var target in _targets.Keys)
                if (target != null) target.OnTargetStatusChanged -= OnStatusChanged;
        }

        public override void AddTarget(string id, Texture2D image, float widthMeters)
        {
            if (_started) Create(id, image, widthMeters);
            else _pending.Add((id, image, widthMeters));
        }

        private void OnInitialized(VuforiaInitError error)
        {
            if (error != VuforiaInitError.NONE) Fail(Describe(error));
        }

        private void OnStarted()
        {
            if (_started || VuforiaBehaviour.Instance == null) return;
            _started = true;
            foreach (var (id, image, width) in _pending) Create(id, image, width);
            _pending.Clear();
            if (State != TrackerState.Starting && !_timedOut) return;
            _timedOut = false;
            SetState(TrackerState.Running);
        }

        private void OnError(VuforiaEngineError error) =>
            Fail(error == VuforiaEngineError.CAMERA_DEVICE_LOST
                ? "La caméra a été coupée. Revenez en arrière puis relancez le scan."
                : "Vuforia a signalé une erreur : " + error + ".");

        private void Create(string id, Texture2D image, float widthMeters)
        {
            try
            {
                var target = VuforiaBehaviour.Instance.ObserverFactory.CreateImageTarget(image, widthMeters, id);
                target.OnTargetStatusChanged += OnStatusChanged;
                _targets[target] = id;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Fail("Impossible de préparer l'image \"" + id + "\" pour le suivi.");
            }
        }

        private void OnStatusChanged(ObserverBehaviour target, TargetStatus status)
        {
            if (!_targets.TryGetValue(target, out var id)) return;
            var quality = status.Status switch
            {
                Status.TRACKED => TrackingQuality.Tracked,
                Status.EXTENDED_TRACKED or Status.LIMITED => TrackingQuality.Limited,
                _ => TrackingQuality.None,
            };
            RaiseTarget(id, target.transform, quality);
        }

        private static string Describe(VuforiaInitError error) => error switch
        {
            VuforiaInitError.LICENSE_ERROR => "La clé de licence Vuforia est refusée. Vérifiez-la sur developer.vuforia.com.",
            VuforiaInitError.PERMISSION_ERROR => "L'accès à la caméra est refusé. Autorisez-le dans les réglages du système.",
            VuforiaInitError.DEVICE_NOT_SUPPORTED => "Cet appareil n'est pas pris en charge par Vuforia.",
            _ => "Vuforia n'a pas pu démarrer (" + error + "). Dans l'éditeur, acceptez l'accord via Help > Vuforia Engine.",
        };
    }
}
