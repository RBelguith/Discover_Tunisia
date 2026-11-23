using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiscoverTunisia.Core;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Globe
{
    /// <summary>
    /// F03/F04 — Globe 3D interactif : rotation à la souris/toucher (avec inertie),
    /// mise en évidence du gouvernorat sélectionné (halo pulsé + épingle + étiquette).
    /// </summary>
    public class GlobeController : MonoBehaviour
    {
        public float Radius = 3.0f;

        private class Marker
        {
            public GovernorateInfo gov;
            public Transform root;
            public Transform glow;
            public RectTransform label;
            public bool selected;
        }

        private readonly List<Marker> _markers = new List<Marker>();
        private Transform _globe;
        private float _yaw;
        private float _pitch;
        private float _velYaw;
        private float _velPitch;
        private float _lastInputTime;
        private Canvas _canvas;

        /// <summary>Construit le globe, l'atmosphère, les anneaux et les marqueurs.</summary>
        public void Build(GovernorateInfo selected, Canvas uiCanvas)
        {
            _canvas = uiCanvas;

            // --- sphère terrestre ---
            var earthGo = new GameObject("Earth");
            earthGo.transform.SetParent(transform, false);
            var mf = earthGo.AddComponent<MeshFilter>();
            mf.sharedMesh = SphereMesh.Build(Radius);
            var mr = earthGo.AddComponent<MeshRenderer>();
            var earthTex = Resources.Load<Texture2D>("Images/earth");
            if (earthTex == null) earthTex = MakeFallbackEarth();
            mr.sharedMaterial = MatLib.Lit(Color.white);
            if (mr.sharedMaterial.HasProperty("_BaseMap")) mr.sharedMaterial.SetTexture("_BaseMap", earthTex);
            if (mr.sharedMaterial.HasProperty("_Smoothness")) mr.sharedMaterial.SetFloat("_Smoothness", 0.12f);
            _globe = earthGo.transform;

            // --- atmosphère ---
            var atmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atmo.name = "Atmosphere";
            atmo.transform.SetParent(transform, false);
            atmo.transform.localScale = Vector3.one * (Radius * 1.045f);
            var atmoMat = MatLib.Transparent(new Color(0.30f, 0.70f, 1.00f, 0.20f));
            if (atmoMat.HasProperty("_Cull")) atmoMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Front);
            atmo.GetComponent<Renderer>().sharedMaterial = atmoMat;
            Destroy(atmo.GetComponent<Collider>());

            // --- anneaux orbitaux (style maquette) ---
            CreateOrbitRing("Ring1", 1.55f, new Color(0.45f, 0.85f, 1f, 0.55f), new Vector3(68f, 0f, 12f));
            CreateOrbitRing("Ring2", 1.78f, new Color(0.30f, 0.65f, 1f, 0.35f), new Vector3(74f, 0f, -18f));

            // --- marqueurs des gouvernorats ---
            foreach (var gov in ContentDatabase.All)
                _markers.Add(CreateMarker(gov, gov == selected));

            // --- étiquettes UI projetées ---
            foreach (var m in _markers)
                m.label = CreateLabel(m);

            // orientation initiale : gouvernorat sélectionné face à la caméra
            if (selected != null)
            {
                _pitch = selected.latitude;
                _yaw = -selected.longitude;
            }
            else { _pitch = 15f; _yaw = 0f; }
            _lastInputTime = Time.time;
            ApplyRotation();
        }

        // ------------------------------------------------------------------
        private Marker CreateMarker(GovernorateInfo gov, bool selected)
        {
            var root = new GameObject("Marker_" + gov.id).transform;
            root.SetParent(_globe, false);
            Vector3 normal = SphereMesh.PointOnSphere(1f, gov.latitude, gov.longitude);
            root.localPosition = normal * (Radius + 0.015f);
            root.localRotation = Quaternion.LookRotation(normal, Vector3.up);

            // halo (quad orienté tangent)
            var glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "Glow";
            glow.transform.SetParent(root, false);
            float glowSize = selected ? 1.5f : 1.0f;
            glow.transform.localScale = new Vector3(glowSize, glowSize, 1f);
            glow.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            var gmat = MatLib.Transparent(gov.accentColor * (selected ? 1f : 0.55f));
            if (gmat.HasProperty("_BaseMap")) gmat.SetTexture("_BaseMap", SpriteFactory.GlowTexture());
            if (gmat.HasProperty("_MainTex")) gmat.SetTexture("_MainTex", SpriteFactory.GlowTexture());
            glow.GetComponent<Renderer>().sharedMaterial = gmat;
            Destroy(glow.GetComponent<Collider>());

            // disque de repère
            var disc = GameObject.CreatePrimitive(PrimitiveType.Quad);
            disc.name = "Disc";
            disc.transform.SetParent(root, false);
            disc.transform.localScale = new Vector3(0.34f, 0.34f, 1f);
            disc.transform.localPosition = new Vector3(0f, 0f, 0.045f);
            var dmat = MatLib.Transparent(selected ? Color.white : gov.accentColor);
            if (dmat.HasProperty("_BaseMap")) dmat.SetTexture("_BaseMap", SpriteFactory.DiscTexture());
            if (dmat.HasProperty("_MainTex")) dmat.SetTexture("_MainTex", SpriteFactory.DiscTexture());
            disc.GetComponent<Renderer>().sharedMaterial = dmat;
            Destroy(disc.GetComponent<Collider>());

            // épingle (tige + tête) pour le gouvernorat sélectionné
            if (selected)
            {
                var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spike.name = "Spike";
                spike.transform.SetParent(root, false);
                spike.transform.localPosition = new Vector3(0f, 0f, 0.30f);
                spike.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                spike.transform.localScale = new Vector3(0.045f, 0.30f, 0.045f);
                spike.GetComponent<Renderer>().sharedMaterial = MatLib.Unlit(new Color(1f, 1f, 1f, 0.95f));
                Destroy(spike.GetComponent<Collider>());

                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head";
                head.transform.SetParent(root, false);
                head.transform.localPosition = new Vector3(0f, 0f, 0.66f);
                head.transform.localScale = Vector3.one * 0.30f;
                var hmat = MatLib.Unlit(gov.accentColor);
                head.GetComponent<Renderer>().sharedMaterial = hmat;
                Destroy(head.GetComponent<Collider>());
            }

            return new Marker { gov = gov, root = root, glow = glow.transform, selected = selected };
        }

        private RectTransform CreateLabel(Marker m)
        {
            var rt = UILib.Rect(_canvas != null ? _canvas.transform : transform, "Label_" + m.gov.id,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(320f, 60f));
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.text = "<b>" + m.gov.name + "</b>";
            txt.font = Theme.DefaultFont;
            txt.fontSize = m.selected ? 40 : 30;
            txt.color = m.selected ? Color.white : new Color(0.85f, 0.92f, 1f);
            txt.alignment = TextAnchor.LowerCenter;
            txt.supportRichText = true;
            txt.raycastTarget = false;
            return rt;
        }

        private void CreateOrbitRing(string name, float radius, Color color, Vector3 eulerAngles)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localScale = Vector3.one * Radius;
            go.transform.localEulerAngles = eulerAngles;
            var lr = go.AddComponent<LineRenderer>();
            lr.loop = true;
            lr.widthMultiplier = 0.012f;
            lr.positionCount = 128;
            var pts = new Vector3[128];
            for (int i = 0; i < 128; i++)
            {
                float a = i / 128f * Mathf.PI * 2f;
                pts[i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            }
            lr.SetPositions(pts);
            var mat = MatLib.Transparent(color);
            if (mat.HasProperty("_BaseMap")) { /* pas de texture : ligne unie */ }
            lr.sharedMaterial = mat;
            lr.textureMode = LineTextureMode.Stretch;
            lr.numCapVertices = 4;
        }

        private static Texture2D _fallbackEarth;
        private static Texture2D MakeFallbackEarth()
        {
            if (_fallbackEarth != null) return _fallbackEarth;
            var c = new SpriteFactory.Canvas2D(256, 128);
            c.Clear(new Color32(20, 60, 120, 255));
            // continents décoratifs approximatifs (si la texture n'est pas encore importée)
            c.FillPoly(new Vector2[] { new Vector2(120, 30), new Vector2(160, 40), new Vector2(150, 80), new Vector2(125, 75) }, new Color32(60, 120, 70, 255));
            c.FillPoly(new Vector2[] { new Vector2(130, 60), new Vector2(155, 70), new Vector2(145, 110), new Vector2(128, 100) }, new Color32(150, 130, 80, 255));
            _fallbackEarth = (Texture2D)c.ToSprite().texture;
            return _fallbackEarth;
        }

        // ------------------------------------------------------------------
        //  Rotation
        // ------------------------------------------------------------------
        private void Update()
        {
            bool input = false;

            if (InputHelper.PointerDown && !InputHelper.IsPointerOverUI())
                _dragging = true;
            if (InputHelper.PointerUp)
                _dragging = false;

            if (_dragging && InputHelper.PointerHeld)
            {
                Vector2 d = InputHelper.PointerDelta;
                _yaw += d.x * 0.28f;
                _pitch = Mathf.Clamp(_pitch - d.y * 0.28f, -75f, 75f);
                _velYaw = d.x * 0.28f;
                _velPitch = -d.y * 0.28f;
                _lastInputTime = Time.time;
                input = true;
            }

            if (!input)
            {
                // inertie
                _yaw += _velYaw;
                _pitch = Mathf.Clamp(_pitch + _velPitch, -75f, 75f);
                _velYaw *= 0.94f;
                _velPitch *= 0.94f;
                if (Mathf.Abs(_velYaw) < 0.02f) _velYaw = 0f;
                if (Mathf.Abs(_velPitch) < 0.02f) _velPitch = 0f;

                // rotation d'attente douce
                if (Time.time - _lastInputTime > 5f && _velYaw == 0f)
                    _yaw += 2.5f * Time.deltaTime;
            }

            ApplyRotation();
            UpdateMarkerPulse();
            UpdateLabels();
        }

        private bool _dragging;

        private void ApplyRotation()
        {
            // yaw autour de l'axe Y monde puis pitch autour de X monde
            transform.rotation = Quaternion.Euler(_pitch, 0f, 0f) * Quaternion.Euler(0f, _yaw, 0f);
        }

        private void UpdateMarkerPulse()
        {
            float t = Time.time;
            foreach (var m in _markers)
            {
                if (m.glow == null) continue;
                if (m.selected)
                {
                    float pulse = 1f + 0.22f * Mathf.Sin(t * 3.5f);
                    m.glow.localScale = Vector3.one * 1.5f * pulse;
                    var mat = m.glow.GetComponent<Renderer>().sharedMaterial;
                    if (mat != null && mat.HasProperty("_BaseColor"))
                    {
                        var c = m.gov.accentColor;
                        c.a = 0.75f + 0.25f * Mathf.Sin(t * 3.5f);
                        mat.SetColor("_BaseColor", c);
                    }
                }
            }
        }

        private void UpdateLabels()
        {
            if (_canvas == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            foreach (var m in _markers)
            {
                if (m.label == null) continue;
                Vector3 world = m.root.position;
                Vector3 outward = (world - transform.position).normalized;
                Vector3 toCam = (cam.transform.position - world).normalized;
                bool facing = Vector3.Dot(outward, toCam) > 0.12f;

                if (!facing)
                {
                    m.label.gameObject.SetActive(false);
                    continue;
                }

                Vector3 sp = cam.WorldToScreenPoint(world);
                if (sp.z < 0f) { m.label.gameObject.SetActive(false); continue; }

                m.label.gameObject.SetActive(true);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    (RectTransform)_canvas.transform, sp, null, out Vector2 local);
                // décalage au-dessus du marqueur (vers le haut de l'écran)
                float lift = m.selected ? 150f : 105f;
                m.label.anchoredPosition = local + new Vector2(0f, lift);
            }
        }

        /// <summary>Recentre le globe sur un gouvernorat.</summary>
        public void FocusOn(GovernorateInfo gov)
        {
            if (gov == null) return;
            _pitch = gov.latitude;
            _yaw = -gov.longitude;
            _velYaw = 0f;
            _velPitch = 0f;
            _lastInputTime = Time.time;
            ApplyRotation();
        }
    }
}
