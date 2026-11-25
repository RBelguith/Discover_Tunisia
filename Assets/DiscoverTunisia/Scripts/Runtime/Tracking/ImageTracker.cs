using System;
using UnityEngine;

namespace DiscoverTunisia.Tracking
{
    public enum TrackerState { Starting, Running, Failed, Off }

    public enum TrackingQuality { None, Limited, Tracked }

    /// <summary>Camera image tracking provided by an AR SDK. Screens fall back to a 3D preview when it is missing or fails.</summary>
    public abstract class ImageTracker : MonoBehaviour
    {
        /// <summary>Set before a tracking scene loads to keep the camera off. Trackers then fail at once and screens show their fallback.</summary>
        public static bool CameraDisabled { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => CameraDisabled = false;

        public TrackerState State { get; private set; } = TrackerState.Starting;

        /// <summary>Why tracking failed, written for the visitor.</summary>
        public string FailureMessage { get; private set; }

        public event Action<TrackerState> StateChanged;

        /// <summary>Raised with the target id, the transform that follows the image, and the tracking quality.</summary>
        public event Action<string, Transform, TrackingQuality> TargetChanged;

        /// <summary>Registers an image to track. Safe to call before the tracker is running.</summary>
        public abstract void AddTarget(string id, Texture2D image, float widthMeters);

        /// <summary>Turns the camera on. Also retries after a failure.</summary>
        public abstract void StartCamera();

        /// <summary>Turns the camera off and moves to Off. Registered targets are kept for the next start.</summary>
        public abstract void StopCamera();

        protected void SetState(TrackerState state, string failureMessage = null)
        {
            State = state;
            FailureMessage = failureMessage;
            StateChanged?.Invoke(state);
        }

        protected void RaiseTarget(string id, Transform anchor, TrackingQuality quality) => TargetChanged?.Invoke(id, anchor, quality);
    }
}
