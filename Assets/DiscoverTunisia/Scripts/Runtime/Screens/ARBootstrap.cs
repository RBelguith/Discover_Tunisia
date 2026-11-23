using UnityEngine;
using UnityEngine.UI;
using DiscoverTunisia.Core;
using DiscoverTunisia.Globe;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Screens
{
    /// <summary>
    /// F09 — Mode réalité augmentée (Vuforia).
    ///
    /// Le script ne dépend d'aucune API Vuforia : si la scène contient un
    /// « ARCamera » et un « ImageTarget » (ajoutés par l'utilisateur après
    /// installation du package), le modèle 3D est automatiquement parenté
    /// sous la cible. Sinon, un mode aperçu avec instructions s'affiche.
    /// </summary>
    public class ARBootstrap : SceneBootstrap
    {
        protected override bool CreateCameraIfMissing => false;

        private GameObject _model;
        private Transform _spin;
        private bool _onTarget;

        protected override void Awake()
        {
            base.Awake();

            var gov = AppState.Selected;
            if (gov == null)
            {
                gov = ContentDatabase.All[0];
                AppState.Select(gov.id, "manuel");
            }

            var arCam = FindByHint("ARCamera", "Vuforia");
            var target = FindByHint("ImageTarget", "ImageTarget");
            bool vuforia = arCam != null;
            _onTarget = vuforia && target != null;

            if (!vuforia)
            {
                BuildPreviewEnvironment();
                if (Camera.main == null) CreatePreviewCamera();
            }

            BuildModel(gov, target);
            BuildUI(gov, vuforia, target != null);
        }

        // ------------------------------------------------------------------
        private void BuildPreviewEnvironment()
        {
            // fond étoilé (pas de flux caméra en mode aperçu)
            var bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "Backdrop";
            bg.transform.position = new Vector3(0f, 0f, 8f);
            bg.transform.localScale = new Vector3(56f, 28f, 1f);
            var mat = MatLib.Unlit(Color.white);
            var tex = Backgrounds.Starfield().texture;
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            bg.GetComponent<Renderer>().sharedMaterial = mat;
            Destroy(bg.GetComponent<Collider>());
        }

        private void CreatePreviewCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var previewCam = go.AddComponent<Camera>();
            if (Object.FindFirstObjectByType<AudioListener>() == null)
                go.AddComponent<AudioListener>();
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            previewCam.backgroundColor = Theme.BackgroundBottom;
            previewCam.fieldOfView = 45f;
            previewCam.transform.position = new Vector3(0f, 0f, -7.5f);
        }

        private void BuildModel(GovernorateInfo gov, GameObject target)
        {
            var mon = gov.monuments.Length > 0 ? gov.monuments[0] : null;
            if (mon == null) return;

            if (target != null)
            {
                // sur la cible Vuforia
                _model = MonumentBuilder.Build(mon.style, gov, target.transform);
                _model.transform.localPosition = new Vector3(0f, 0.12f, 0f);
                _model.transform.localRotation = Quaternion.identity;
                _model.transform.localScale = Vector3.one * 0.35f;
                _spin = _model.transform;
            }
            else
            {
                // aperçu à côté du panneau d'instructions
                _model = MonumentBuilder.Build(mon.style, gov, null);
                _model.transform.position = new Vector3(2.6f, -1.5f, 0f);
                _model.transform.localScale = Vector3.one * 1.1f;
                _spin = _model.transform;

                // socle de présentation
                var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                disc.name = "Socle";
                disc.transform.position = new Vector3(2.6f, -1.65f, 0f);
                disc.transform.localScale = new Vector3(2.6f, 0.05f, 2.6f);
                disc.GetComponent<Renderer>().sharedMaterial = MatLib.Lit(new Color(0.10f, 0.20f, 0.34f));
                Destroy(disc.GetComponent<Collider>());
            }
        }

        // ------------------------------------------------------------------
        /// <summary>
        /// Recherche un objet AR par nom ou par type de composant (Vuforia
        /// n'étant pas encore installé, on ne peut pas typer la recherche).
        /// </summary>
        private static GameObject FindByHint(string nameHint, string typeHint)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.IndexOf(nameHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        return t.gameObject;
                    foreach (var c in t.GetComponents<Component>())
                    {
                        if (c != null && c.GetType().Name.IndexOf(typeHint, System.StringComparison.OrdinalIgnoreCase) >= 0)
                            return t.gameObject;
                    }
                }
            }
            return null;
        }

        private void BuildUI(GovernorateInfo gov, bool vuforia, bool hasTarget)
        {
            var root = Canvas.transform;

            // fond d'interface translucide (seulement en mode aperçu)
            if (!vuforia)
            {
                var bg = UILib.Panel(root, "Bg", SpriteFactory.VerticalGradient(
                    new Color(0.03f, 0.07f, 0.17f, 0.94f), new Color(0.01f, 0.03f, 0.09f, 0.94f)));
                bg.raycastTarget = true;
            }

            // barre supérieure
            UILib.MakeButton(root, "Back", "",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -40f), new Vector2(84f, 84f),
                () => Nav.Globe(),
                new UILib.ButtonStyle { icon = "back", iconSize = 42, background = Theme.Panel });

            UILib.Label(root, "Title", "<b>Mode réalité augmentée</b> — " + gov.name, 46,
                Theme.TextPrimary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -60f), new Vector2(1300f, 70f));

            if (vuforia && hasTarget)
            {
                // --- expérience AR active ---
                var pill = UILib.Panel(root, "ARPill",
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 40f), new Vector2(1200f, 150f), SpriteFactory.RoundRect(30), Theme.Panel);
                UILib.Label(pill.transform, "Txt",
                    "<b>Pointez la caméra vers la cible imprimée.</b>\n" +
                    "Le modèle de " + gov.name + " s'affiche sur la cible. Reculez ou approchez " +
                    "l'appareil pour cadrer.",
                    30, Theme.TextSecondary, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 130f));
            }
            else if (vuforia && !hasTarget)
            {
                Warning(root, "ARCamera détectée, mais aucune cible.",
                    "Ajoutez une cible : GameObject > Vuforia Engine > Image Target,\n" +
                    "puis choisissez votre base de données (créée depuis CIBLE_AR.png).");
            }
            else
            {
                // --- mode aperçu + instructions d'installation ---
                var panel = UILib.Panel(root, "Install",
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(40f, -40f), new Vector2(940f, 760f), SpriteFactory.RoundRect(34), Theme.Panel);

                UILib.Label(panel.transform, "Head", "<b>Activer la réalité augmentée (Vuforia)</b>",
                    40, Theme.TextPrimary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, -50f), new Vector2(880f, 60f));

                string steps =
                    "<b>1.</b>  Package Manager > <b>My Assets</b> > « Vuforia Engine » > Download puis Import.\n\n" +
                    "<b>2.</b>  Créez une licence sur <b>developer.vuforia.com</b> et collez la clé dans " +
                    "<b>Vuforia Configuration</b> (Assets/Resources).\n\n" +
                    "<b>3.</b>  GameObject > Vuforia Engine > <b>AR Camera</b> (remplace la caméra principale).\n\n" +
                    "<b>4.</b>  Créez une base de données sur le <b>Target Manager</b> à partir de " +
                    "<b>CIBLE_AR.png</b> (fourni), importez-la, puis GameObject > Vuforia Engine > <b>Image Target</b>.\n\n" +
                    "<color=#4FC3F7>Le modèle 3D s'accrochera automatiquement à la cible.</color>";

                var bodyRt = UILib.Rect(panel.transform, "Steps",
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                bodyRt.offsetMin = new Vector2(40f, 40f);
                bodyRt.offsetMax = new Vector2(-40f, -100f);
                bodyRt.gameObject.AddComponent<RectMask2D>();
                var txt = bodyRt.gameObject.AddComponent<Text>();
                txt.text = steps;
                txt.font = Theme.DefaultFont;
                txt.fontSize = 27;
                txt.color = Theme.TextSecondary;
                txt.alignment = TextAnchor.UpperLeft;
                txt.supportRichText = true;
                txt.horizontalOverflow = HorizontalWrapMode.Wrap;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                txt.raycastTarget = false;
            }

            // indication aperçu
            if (!vuforia)
            {
                UILib.Label(root, "PreviewTag",
                    "<i>Aperçu du modèle (activez Vuforia pour l'AR)</i>", 26,
                    new Color(0.55f, 0.66f, 0.78f), new Vector2(0.75f, 0f), new Vector2(0.75f, 0f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(700f, 40f));
            }
        }

        private void Warning(Transform root, string title, string body)
        {
            var panel = UILib.Panel(root, "Warn",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 40f), new Vector2(1200f, 170f), SpriteFactory.RoundRect(30),
                new Color(0.35f, 0.20f, 0.08f, 0.95f));
            UILib.Label(panel.transform, "Txt", "<b>" + title + "</b>\n" + body, 30,
                new Color(1f, 0.85f, 0.6f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1140f, 150f));
        }

        private void Update()
        {
            if (_spin != null && !_onTarget)
                _spin.Rotate(0f, 25f * Time.deltaTime, 0f, Space.World);
        }
    }
}
