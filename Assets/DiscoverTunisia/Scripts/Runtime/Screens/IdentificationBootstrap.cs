using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DiscoverTunisia.Core;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Screens
{
    /// <summary>
    /// F02 — Identification de la région.
    /// Deux parcours : scan de carte d'identité (simulé dans le prototype,
    /// aucune image ni donnée conservée) et sélection manuelle (secours obligatoire).
    /// </summary>
    public class IdentificationBootstrap : SceneBootstrap
    {
        private RectTransform _scanOverlay;
        private Image _scanLine;
        private Image _scanCard;
        private GameObject _scanResultPanel;
        private string _detectedId;

        protected override void Awake()
        {
            base.Awake();
            BuildBackground();
            BuildHeader();
            BuildScanCard();
            BuildManualSelection();
        }

        private void BuildBackground()
        {
            var bg = UILib.Panel(Canvas.transform, "Bg", SpriteFactory.VerticalGradient(Theme.BackgroundTop, Theme.BackgroundBottom));
            bg.raycastTarget = false;
            var halo = UILib.AddGlow(Canvas.transform, "BgHalo", new Color(0.15f, 0.35f, 0.65f, 0.35f), 1600f, new Vector2(0f, 150f));
            halo.raycastTarget = false;
        }

        private void BuildHeader()
        {
            var root = Canvas.transform;

            var back = UILib.MakeButton(root, "Back", "",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -40f), new Vector2(84f, 84f),
                () => Nav.Home(),
                new UILib.ButtonStyle { icon = "back", iconSize = 44, background = Theme.Panel });
            back.image.color = Theme.Panel;

            UILib.Label(root, "Title", "Identification de la région", 52, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -66f), new Vector2(1400f, 80f), TextAnchor.MiddleCenter, FontStyle.Bold);

            UILib.Label(root, "Step", "Étape 1 / 2", 26, Theme.Accent,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -128f), new Vector2(600f, 40f));
        }

        // ------------------------------------------------------------------
        //  F02a — Scan de carte d'identité (simulé)
        // ------------------------------------------------------------------
        private void BuildScanCard()
        {
            var root = Canvas.transform;

            var card = UILib.Panel(root, "ScanCard",
                new Vector2(0.25f, 0.5f), new Vector2(0.25f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -70f), new Vector2(760f, 640f), SpriteFactory.RoundRect(32), Theme.Panel);

            UILib.Label(card.transform, "Head", "<b>Scanner ma carte d'identité</b>", 40, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -46f), new Vector2(700f, 60f));

            // Cadre "carte" fictive dessiné par code
            var frame = UILib.Panel(card.transform, "IdFrame",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(560f, 330f), SpriteFactory.RoundRect(24),
                new Color(0.88f, 0.91f, 0.95f, 1f));
            // photo + lignes de texte (fictives)
            UILib.Panel(frame.transform, "Photo",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(36f, 0f), new Vector2(140f, 180f), SpriteFactory.RoundRect(12),
                new Color(0.65f, 0.72f, 0.82f, 1f));
            for (int i = 0; i < 4; i++)
            {
                float w = i == 0 ? 300f : (i % 2 == 0 ? 330f : 250f);
                UILib.Panel(frame.transform, "Line" + i,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(210f, 105f - i * 52f), new Vector2(w, i == 0 ? 30f : 20f),
                    SpriteFactory.RoundRect(10), new Color(0.45f, 0.55f, 0.68f, 1f));
            }
            // bande dorée de la carte
            UILib.Panel(frame.transform, "GoldBand",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 24f), new Vector2(560f, 44f), null, new Color(0.93f, 0.78f, 0.35f, 1f));

            // Ligne de balayage animée
            _scanLine = UILib.Panel(frame.transform, "ScanLine",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 140f), new Vector2(540f, 6f), null, Theme.Accent);
            _scanLine.gameObject.SetActive(false);

            _scanCard = frame;

            // Bouton de lancement
            UILib.MakeButton(card.transform, "ScanButton", "Lancer le scan",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 74f), new Vector2(480f, 92f),
                () => StartCoroutine(ScanRoutine()),
                new UILib.ButtonStyle
                {
                    background = Theme.Accent,
                    textColor = new Color(0.02f, 0.08f, 0.16f),
                    fontSize = 36,
                    fontStyle = FontStyle.Bold,
                    icon = "scan",
                    iconTint = new Color(0.02f, 0.08f, 0.16f),
                    iconSize = 42,
                });

            UILib.Label(card.transform, "Privacy",
                "Prototype : lecture simulée (données fictives).\nAucune image ni donnée personnelle n'est conservée.",
                24, Theme.TextSecondary, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(700f, 76f));
        }

        private IEnumerator ScanRoutine()
        {
            // 1. Animation de balayage
            _scanLine.gameObject.SetActive(true);
            float t = 0f;
            while (t < 2.2f)
            {
                t += Time.deltaTime;
                float k = Mathf.PingPong(t / 2.2f, 1f);
                var rt = (RectTransform)_scanLine.transform;
                rt.anchoredPosition = new Vector2(0f, Mathf.Lerp(-150f, 150f, k));
                yield return null;
            }
            _scanLine.gameObject.SetActive(false);

            // 2. Résultat simulé : gouvernorat tiré au sort parmi les trois
            var all = ContentDatabase.All;
            _detectedId = all[Random.Range(0, all.Length)].id;
            var info = ContentDatabase.GetById(_detectedId);

            ShowScanResult(info);
        }

        private void ShowScanResult(GovernorateInfo info)
        {
            if (_scanResultPanel != null) return;

            var root = Canvas.transform;
            // Voile sombre
            var veil = UILib.Panel(root, "ScanVeil", SpriteFactory.RoundRect(2), new Color(0.01f, 0.03f, 0.07f, 0.86f));
            veil.raycastTarget = true;

            var box = UILib.Panel(root, "ScanResult",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(880f, 560f), SpriteFactory.RoundRect(32), Theme.Panel);

            // pastille verte "check"
            var check = UILib.Panel(box.transform, "Check",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -80f), new Vector2(110f, 110f), SpriteFactory.Disc(), Theme.Success);
            UILib.Label(check.transform, "Mark", "✓", 64, Color.white);

            UILib.Label(box.transform, "Ok", "<b>Carte d'identité scannée !</b>", 44, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -160f), new Vector2(800f, 60f));

            UILib.Label(box.transform, "Detected",
                "Gouvernorat détecté :\n<color=#FFC94D><b>" + info.name + "</b></color>  —  " + info.subtitle,
                36, Theme.TextSecondary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -260f), new Vector2(800f, 130f));

            UILib.Label(box.transform, "Note",
                "Lecture simulée : la sélection manuelle reste disponible à tout moment.",
                24, new Color(0.55f, 0.66f, 0.78f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -350f), new Vector2(800f, 50f));

            UILib.MakeButton(box.transform, "Continue", "Continuer vers le globe",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 70f), new Vector2(520f, 92f),
                () => { AppState.Select(_detectedId, "scan"); Nav.Globe(); },
                new UILib.ButtonStyle
                {
                    background = Theme.Accent,
                    textColor = new Color(0.02f, 0.08f, 0.16f),
                    fontSize = 36,
                    fontStyle = FontStyle.Bold,
                });

            UILib.MakeButton(box.transform, "Retry", "Réessayer",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 70f), new Vector2(520f, 92f),
                () => { Destroy(veil.gameObject); Destroy(box.gameObject); _scanResultPanel = null; },
                new UILib.ButtonStyle { background = Theme.PanelSoft, fontSize = 30 });
            // décale "Réessayer" à droite de "Continuer"
            ((RectTransform)box.transform.Find("Retry")).anchoredPosition = new Vector2(330f, 70f);
            ((RectTransform)box.transform.Find("Continue")).anchoredPosition = new Vector2(-130f, 70f);
            ((RectTransform)box.transform.Find("Continue")).sizeDelta = new Vector2(380f, 92f);
            ((RectTransform)box.transform.Find("Retry")).sizeDelta = new Vector2(240f, 92f);

            _scanResultPanel = box.gameObject;
        }

        // ------------------------------------------------------------------
        //  F02b — Sélection manuelle (solution de secours)
        // ------------------------------------------------------------------
        private void BuildManualSelection()
        {
            var root = Canvas.transform;

            var card = UILib.Panel(root, "ManualCard",
                new Vector2(0.75f, 0.5f), new Vector2(0.75f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -70f), new Vector2(760f, 640f), SpriteFactory.RoundRect(32), Theme.Panel);

            UILib.Label(card.transform, "Head", "<b>Ou sélectionner manuellement</b>", 40, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -46f), new Vector2(700f, 60f));

            UILib.Label(card.transform, "Sub", "Solution de secours si la lecture échoue", 26, Theme.TextSecondary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -96f), new Vector2(700f, 40f));

            var all = ContentDatabase.All;
            for (int i = 0; i < all.Length; i++)
            {
                var g = all[i];
                int index = i;
                float y = -60f - i * 150f;

                var btn = UILib.MakeButton(card.transform, "Gov_" + g.id, "",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(0f, y), new Vector2(660f, 128f),
                    () => { AppState.Select(all[index].id, "manuel"); Nav.Globe(); },
                    new UILib.ButtonStyle { background = Theme.PanelSoft, fontSize = 34 });

                // pastille couleur du gouvernorat
                var dot = UILib.Panel(btn.transform, "Dot",
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(26f, 0f), new Vector2(58f, 58f), SpriteFactory.Disc(), g.accentColor);
                dot.raycastTarget = false;

                var txt = UILib.Rect(btn.transform, "Txt",
                    new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, 0f), Vector2.zero);
                txt.offsetMin = new Vector2(110f, 0f);
                txt.offsetMax = new Vector2(-20f, 0f);
                var label = txt.gameObject.AddComponent<Text>();
                label.text = "<b>" + g.name + "</b>\n<size=24><color=#B3CBE0>" + g.subtitle + "</color></size>";
                label.font = Theme.DefaultFont;
                label.fontSize = 34;
                label.color = Theme.TextPrimary;
                label.alignment = TextAnchor.MiddleLeft;
                label.supportRichText = true;
                label.raycastTarget = false;

                var arrow = UILib.Rect(btn.transform, "Arrow",
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(-24f, 0f), new Vector2(36f, 36f));
                var arrowImg = arrow.gameObject.AddComponent<Image>();
                arrowImg.sprite = SpriteFactory.Icon("next");
                arrowImg.color = g.accentColor;
                arrowImg.preserveAspect = true;
                arrowImg.raycastTarget = false;
            }

            UILib.Label(card.transform, "Footer",
                "Aucune donnée personnelle n'est enregistrée.",
                24, new Color(0.55f, 0.66f, 0.78f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(700f, 40f));
        }
    }
}
