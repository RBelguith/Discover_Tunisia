using System;
using System.Collections.Generic;
using DiscoverTunisia.Content;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DiscoverTunisia.World
{
    /// <summary>Rotates the globe by drag with inertia, idles slowly, and places a marker per governorate.</summary>
    public sealed class GlobeController : MonoBehaviour
    {
        [SerializeField] private Transform spin;
        [SerializeField] private MarkerView markerPrefab;
        [SerializeField] private float radius = 3f;

        [Header("Interaction")]
        [SerializeField] private bool interactive = true;
        [SerializeField] private float degreesPerPixel = 0.28f;
        [SerializeField, Range(0.8f, 0.99f)] private float inertiaDamping = 0.94f;
        [SerializeField] private float idleDelay = 5f;
        [SerializeField] private float idleDegreesPerSecond = 2.5f;
        [SerializeField] private float maxPitch = 75f;

        private readonly List<Marker> _markers = new List<Marker>();
        private float _pitch;
        private float _yaw;
        private Vector2 _velocity;
        private float _lastInput = float.NegativeInfinity;
        private bool _dragging;

        /// <summary>Lets the screen veto a drag that starts on its UI.</summary>
        public Func<Vector2, bool> IsPointerOverUI { get; set; }

        public IReadOnlyList<Marker> Markers => _markers;

        public readonly struct Marker
        {
            public readonly GovernorateData Governorate;
            public readonly MarkerView View;
            public readonly bool Selected;

            public Marker(GovernorateData governorate, MarkerView view, bool selected)
            {
                Governorate = governorate;
                View = view;
                Selected = selected;
            }
        }

        public bool Interactive
        {
            get => interactive;
            set { interactive = value; _dragging = false; }
        }

        public void ShowMarkers(IEnumerable<GovernorateData> governorates, GovernorateData selected)
        {
            foreach (var m in _markers) Destroy(m.View.gameObject);
            _markers.Clear();
            foreach (var g in governorates)
            {
                var view = Instantiate(markerPrefab, spin);
                var normal = GeoMath.PointOnSphere(1f, g.Latitude, g.Longitude);
                view.transform.localPosition = normal * (radius + 0.015f);
                view.transform.localRotation = Quaternion.LookRotation(normal, Vector3.up);
                view.name = "Marker_" + g.Id;
                view.Setup(g.AccentColor, g == selected);
                _markers.Add(new Marker(g, view, g == selected));
            }
        }

        /// <summary>Turns the globe so the governorate faces the camera, wherever the globe sits on screen.</summary>
        public void FocusOn(GovernorateData governorate, Camera cam)
        {
            var toCamera = (cam.transform.position - spin.position).normalized;
            if (spin.parent != null) toCamera = spin.parent.InverseTransformDirection(toCamera);
            var angles = GeoMath.AnglesToward(governorate.Latitude, governorate.Longitude, toCamera);
            _pitch = Mathf.Clamp(angles.x, -maxPitch, maxPitch);
            _yaw = angles.y;
            _velocity = Vector2.zero;
            _lastInput = Time.time;
            Apply();
        }

        /// <summary>True when a point on the globe faces the camera rather than the far side.</summary>
        public bool FacesCamera(Vector3 worldPoint, Camera cam, float threshold = 0.12f)
        {
            var outward = (worldPoint - spin.position).normalized;
            var toCamera = (cam.transform.position - worldPoint).normalized;
            return Vector3.Dot(outward, toCamera) > threshold;
        }

        private void Update()
        {
            var delta = Vector2.zero;
            bool input = interactive && ReadDrag(out delta);
            if (input)
            {
                _velocity = new Vector2(delta.y, -delta.x) * degreesPerPixel;
                _lastInput = Time.time;
            }
            else
            {
                _velocity *= Mathf.Pow(inertiaDamping, Time.deltaTime * 60f);
                if (_velocity.sqrMagnitude < 0.0004f) _velocity = Vector2.zero;
                if (Time.time - _lastInput > idleDelay && _velocity == Vector2.zero)
                    _yaw -= idleDegreesPerSecond * Time.deltaTime;
            }
            _pitch = Mathf.Clamp(_pitch + _velocity.x, -maxPitch, maxPitch);
            _yaw += _velocity.y;
            Apply();
        }

        private bool ReadDrag(out Vector2 delta)
        {
            delta = Vector2.zero;
            var pointer = Pointer.current;
            if (pointer == null) return false;
            if (pointer.press.wasPressedThisFrame)
                _dragging = IsPointerOverUI == null || !IsPointerOverUI(pointer.position.ReadValue());
            if (pointer.press.wasReleasedThisFrame || !pointer.press.isPressed) _dragging = false;
            if (!_dragging) return false;
            delta = pointer.delta.ReadValue();
            return true;
        }

        private void Apply() => spin.localRotation = Quaternion.Euler(_pitch, 0f, 0f) * Quaternion.Euler(0f, _yaw, 0f);
    }
}
