using UnityEngine;
using DiscoverTunisia.Core;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Globe
{
    /// <summary>
    /// Monde 3D de la scène principale : fond étoilé, globe (F03/F04),
    /// avatar (F05) et scène d'exposition des monuments (F06).
    /// </summary>
    public class GlobeWorld : MonoBehaviour
    {
        public GlobeController Globe { get; private set; }

        private GameObject _backdrop;
        private GameObject _avatar;
        private GameObject _stage;          // socle d'exposition des monuments
        private GameObject _stageModel;
        private Transform _stageSpin;

        public void Build(GovernorateInfo selected, Canvas uiCanvas)
        {
            // --- fond étoilé (quad derrière le globe) ---
            _backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backdrop.name = "Backdrop";
            _backdrop.transform.SetParent(transform, false);
            _backdrop.transform.localPosition = new Vector3(0f, 0f, 16f);
            _backdrop.transform.localScale = new Vector3(64f, 32f, 1f);
            var bmat = MatLib.Unlit(Color.white);
            var starTex = Backgrounds.Starfield().texture;
            if (bmat.HasProperty("_BaseMap")) bmat.SetTexture("_BaseMap", starTex);
            if (bmat.HasProperty("_MainTex")) bmat.SetTexture("_MainTex", starTex);
            _backdrop.GetComponent<Renderer>().sharedMaterial = bmat;
            Destroy(_backdrop.GetComponent<Collider>());

            // --- globe (F03/F04) ---
            var globeGo = new GameObject("Globe");
            globeGo.transform.SetParent(transform, false);
            globeGo.transform.localPosition = new Vector3(0.9f, 0.25f, 0f);
            Globe = globeGo.AddComponent<GlobeController>();
            Globe.Build(selected, uiCanvas);

            // --- avatar (F05) ---
            if (selected != null)
            {
                _avatar = AvatarBuilder.Create(selected, transform);
                _avatar.transform.localPosition = new Vector3(-4.9f, -1.75f, 1.4f);
                _avatar.transform.localScale = Vector3.one * 1.9f;
                _avatar.transform.localRotation = Quaternion.Euler(0f, 14f, 0f);
            }

            // --- scène d'exposition des monuments (F06) ---
            _stage = new GameObject("MonumentStage").transform.gameObject;
            _stage.transform.SetParent(transform, false);
            _stage.transform.localPosition = new Vector3(4.6f, -1.9f, 1.6f);

            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Socle";
            disc.transform.SetParent(_stage.transform, false);
            disc.transform.localScale = new Vector3(2.4f, 0.05f, 2.4f);
            disc.GetComponent<Renderer>().sharedMaterial = MatLib.Lit(new Color(0.10f, 0.20f, 0.34f));
            Destroy(disc.GetComponent<Collider>());

            var discGlow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            discGlow.name = "Halo";
            discGlow.transform.SetParent(_stage.transform, false);
            discGlow.transform.localPosition = new Vector3(0f, -0.06f, 0f);
            discGlow.transform.localScale = new Vector3(3.4f, 0.02f, 3.4f);
            var gmat = MatLib.Transparent(new Color(0.31f, 0.76f, 0.97f, 0.35f));
            discGlow.GetComponent<Renderer>().sharedMaterial = gmat;
            Destroy(discGlow.GetComponent<Collider>());

            _stage.SetActive(false);
        }

        /// <summary>Affiche le modèle 3D du monument sélectionné sur la scène.</summary>
        public void ShowMonument(MonumentInfo monument, GovernorateInfo gov)
        {
            ClearStage();
            _stage.SetActive(true);
            _stageModel = MonumentBuilder.Build(monument.style, gov, _stage.transform);
            _stageModel.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            _stageModel.transform.localScale = Vector3.one * 0.85f;
            _stageSpin = _stageModel.transform;
        }

        public void HideStage()
        {
            ClearStage();
            if (_stage != null) _stage.SetActive(false);
        }

        private void ClearStage()
        {
            if (_stageModel != null)
            {
                Destroy(_stageModel);
                _stageModel = null;
                _stageSpin = null;
            }
        }

        public void SetAvatarActive(bool active)
        {
            if (_avatar != null) _avatar.SetActive(active);
        }

        private void Update()
        {
            if (_stageSpin != null)
                _stageSpin.Rotate(0f, 22f * Time.deltaTime, 0f, Space.World);
        }
    }
}
