using System;
using UnityEngine;
using UnityEngine.UI;
using DiscoverTunisia.Core;
using DiscoverTunisia.Globe;
using DiscoverTunisia.Screens;

namespace DiscoverTunisia.UI
{
    /// <summary>
    /// Interface de la scène principale (F06, F07, F08, F10 + panneau
    /// d'information de la maquette). Construite entièrement par code.
    /// </summary>
    public class GlobeUI
    {
        private Canvas _canvas;
        private GlobeWorld _world;
        private GovernorateInfo _gov;

        private GameObject _menuRoot;
        private GameObject _rubricRoot;
        private GameObject _infoRoot;
        private GameObject _modalRoot;

        // carrousel "À propos"
        private string[] _slides;
        private int _slide;
        private Text _slideText;
        private Image[] _dotImages;

        // rubrique ouverte
        private string _rubric;
        private GameObject _listViewport;
        private RectTransform _listContent;
        private RectTransform _detailRoot;
        private Image _detailImage;
        private Text _detailName;
        private Text _detailDesc;
        private Text _detailHint;
        private int _monIndex = -1;

        public void Build(Canvas canvas, GlobeWorld world, GovernorateInfo gov)
        {
            _canvas = canvas;
            _world = world;
            _gov = gov;

            BuildTopBar();
            BuildMenu();
            BuildInfoPanel();
            BuildHint();
            BuildRubricShell();
        }

        // ==================================================================
        //  Barre supérieure (F10 : retour / accueil / réglages)
        // ==================================================================
        private void BuildTopBar()
        {
            var root = _canvas.transform;

            // retour (changer de gouvernorat)
            UILib.MakeButton(root, "Back", "",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(40f, -40f), new Vector2(84f, 84f),
                () => Nav.Identification(),
                new UILib.ButtonStyle { icon = "back", iconSize = 42, background = Theme.Panel });

            // pastille de localisation (centre haut)
            var pill = UILib.Panel(root, "LocationPill",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(640f, 116f), SpriteFactory.RoundRect(58), Theme.Panel);
            var pillGlow = UILib.AddGlow(pill.transform, "Glow", new Color(_gov.accentColor.r, _gov.accentColor.g, _gov.accentColor.b, 0.35f), 780f);
            pillGlow.transform.SetAsFirstSibling();

            var pin = UILib.Rect(pill.transform, "Pin",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(34f, 0f), new Vector2(52f, 52f));
            var pinImg = pin.gameObject.AddComponent<Image>();
            pinImg.sprite = SpriteFactory.Icon("pin");
            pinImg.color = Theme.Gold;
            pinImg.preserveAspect = true;
            pinImg.raycastTarget = false;

            var txt = UILib.Rect(pill.transform, "Txt",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            txt.offsetMin = new Vector2(100f, 4f);
            txt.offsetMax = new Vector2(-90f, -4f);
            var label = txt.gameObject.AddComponent<Text>();
            label.text = "<b><size=46>" + _gov.name + "</size></b>\n<size=26><color=#B3CBE0>" + _gov.subtitle + "</color></size>";
            label.font = Theme.DefaultFont;
            label.fontSize = 30;
            label.color = Theme.TextPrimary;
            label.alignment = TextAnchor.MiddleCenter;
            label.supportRichText = true;
            label.raycastTarget = false;

            // réglages + accueil (haut droite)
            UILib.MakeButton(root, "Settings", "",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-140f, -40f), new Vector2(84f, 84f),
                ShowSettings,
                new UILib.ButtonStyle { icon = "gear", iconSize = 44, background = Theme.Panel });

            UILib.MakeButton(root, "Home", "",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-40f, -40f), new Vector2(84f, 84f),
                () => Nav.Home(),
                new UILib.ButtonStyle { icon = "home", iconSize = 44, background = Theme.Panel });
        }

        // ==================================================================
        //  Menu principal (droite) — F06/F07/F08/F09
        // ==================================================================
        private void BuildMenu()
        {
            _menuRoot = new GameObject("Menu", typeof(RectTransform));
            ((RectTransform)_menuRoot.transform).SetParent(_canvas.transform, false);
            var rt = (RectTransform)_menuRoot.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-36f, 60f);
            rt.sizeDelta = new Vector2(500f, 640f);

            string[] labels = { "Monuments", "Gastronomie", "Vêtements\ntraditionnels", "Voir en AR" };
            string[] icons = { "monuments", "food", "clothing", "ar" };
            Action[] actions =
            {
                () => OpenRubric("monuments"),
                () => OpenRubric("food"),
                () => OpenRubric("clothes"),
                () => Nav.AR(),
            };
            Color[] tints = { Theme.Accent, Theme.Gold, new Color(1f, 0.5f, 0.55f), Theme.Accent };

            for (int i = 0; i < labels.Length; i++)
            {
                int idx = i;
                float y = 260f - i * 172f;
                var btn = UILib.MakeButton(_menuRoot.transform, "Menu_" + i, labels[i],
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0f, y), new Vector2(500f, 150f),
                    () => actions[idx](),
                    new UILib.ButtonStyle
                    {
                        background = Theme.Panel,
                        textColor = Theme.TextPrimary,
                        fontSize = 40,
                        fontStyle = FontStyle.Bold,
                        icon = icons[i],
                        iconTint = tints[i],
                        iconSize = 64,
                    });
                var glow = UILib.AddGlow(btn.transform, "Glow", new Color(tints[i].r, tints[i].g, tints[i].b, 0.30f), 640f);
                glow.transform.SetAsFirstSibling();
            }
        }

        // ==================================================================
        //  Panneau d'information (carrousel) — zone centrale basse
        // ==================================================================
        private void BuildInfoPanel()
        {
            var root = _canvas.transform;
            _infoRoot = new GameObject("InfoPanel", typeof(RectTransform));
            ((RectTransform)_infoRoot.transform).SetParent(root, false);
            var rt = (RectTransform)_infoRoot.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 26f);
            rt.sizeDelta = new Vector2(840f, 300f);

            var bg = UILib.Panel(_infoRoot.transform, "Bg", SpriteFactory.RoundRect(30), Theme.Panel);
            bg.raycastTarget = false;

            // en-tête
            var infoIcon = UILib.Rect(_infoRoot.transform, "InfoIcon",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-330f, -38f), new Vector2(44f, 44f));
            var ii = infoIcon.gameObject.AddComponent<Image>();
            ii.sprite = SpriteFactory.Icon("info");
            ii.color = Theme.Accent;
            ii.preserveAspect = true;
            ii.raycastTarget = false;

            UILib.Label(_infoRoot.transform, "Header", "<b>À propos de " + _gov.name + "</b>", 32,
                Theme.TextPrimary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -38f), new Vector2(600f, 50f));

            // texte du carrousel
            var bodyRt = UILib.Rect(_infoRoot.transform, "Body",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            bodyRt.offsetMin = new Vector2(100f, 120f);
            bodyRt.offsetMax = new Vector2(-100f, -84f);
            _slideText = bodyRt.gameObject.AddComponent<Text>();
            _slideText.font = Theme.DefaultFont;
            _slideText.fontSize = 28;
            _slideText.color = Theme.TextSecondary;
            _slideText.alignment = TextAnchor.MiddleCenter;
            _slideText.supportRichText = true;
            _slideText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _slideText.verticalOverflow = VerticalWrapMode.Overflow;
            _slideText.raycastTarget = false;

            _slides = new string[]
            {
                _gov.shortBlurb,
                "<b>" + _gov.subtitle + "</b>\nDécouvrez " + _gov.monuments.Length +
                    " monuments, " + _gov.foods.Length + " spécialité culinaire et " +
                    _gov.clothes.Length + " tenue traditionnelle de la région.",
                "Faites glisser le globe pour le faire tourner, puis ouvrez les rubriques " +
                    "Monuments, Gastronomie ou Vêtements traditionnels depuis le menu.",
            };

            // flèches
            UILib.MakeButton(_infoRoot.transform, "Prev", "",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(28f, 0f), new Vector2(64f, 64f),
                () => ShowSlide(_slide - 1),
                new UILib.ButtonStyle { icon = "back", iconSize = 36, background = Theme.PanelSoft });
            UILib.MakeButton(_infoRoot.transform, "Next", "",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-28f, 0f), new Vector2(64f, 64f),
                () => ShowSlide(_slide + 1),
                new UILib.ButtonStyle { icon = "next", iconSize = 36, background = Theme.PanelSoft });

            // pastilles
            _dotImages = new Image[_slides.Length];
            for (int i = 0; i < _slides.Length; i++)
            {
                var dot = UILib.Panel(_infoRoot.transform, "Dot" + i,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2((i - (_slides.Length - 1) * 0.5f) * 44f, 96f),
                    new Vector2(22f, 22f), SpriteFactory.Disc(), Theme.PanelSoft);
                dot.raycastTarget = false;
                _dotImages[i] = dot;
            }

            // bouton "En savoir plus"
            UILib.MakeButton(_infoRoot.transform, "More", "En savoir plus  ›",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 42f), new Vector2(340f, 76f),
                ShowAbout,
                new UILib.ButtonStyle
                {
                    background = Theme.PanelSoft,
                    textColor = Theme.Accent,
                    fontSize = 30,
                    fontStyle = FontStyle.Bold,
                });

            ShowSlide(0);
        }

        private void ShowSlide(int index)
        {
            int n = _slides.Length;
            _slide = ((index % n) + n) % n;
            _slideText.text = _slides[_slide];
            for (int i = 0; i < _dotImages.Length; i++)
                _dotImages[i].color = i == _slide ? Theme.Accent : Theme.PanelSoft;
        }

        private void ShowAbout()
        {
            ShowTextModal("À propos de " + _gov.name, _gov.description, "Fermer", null);
        }

        // ==================================================================
        //  Indication de rotation (bas gauche)
        // ==================================================================
        private void BuildHint()
        {
            var root = _canvas.transform;
            var hint = UILib.Panel(root, "Hint",
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(40f, 34f), new Vector2(440f, 96f), SpriteFactory.RoundRect(48), Theme.PanelSoft);
            hint.raycastTarget = false;

            var icon = UILib.Rect(hint.transform, "Icon",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, 0f), new Vector2(56f, 56f));
            var iconImg = icon.gameObject.AddComponent<Image>();
            iconImg.sprite = SpriteFactory.Icon("globe");
            iconImg.color = Theme.Accent;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var txt = UILib.Rect(hint.transform, "Txt",
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            txt.offsetMin = new Vector2(100f, 6f);
            txt.offsetMax = new Vector2(-16f, -6f);
            var label = txt.gameObject.AddComponent<Text>();
            label.text = "<size=26>Faites glisser pour\ntourner le globe</size>";
            label.font = Theme.DefaultFont;
            label.fontSize = 26;
            label.color = Theme.TextSecondary;
            label.alignment = TextAnchor.MiddleLeft;
            label.supportRichText = true;
            label.raycastTarget = false;
        }

        // ==================================================================
        //  Rubriques (F06 / F07 / F08)
        // ==================================================================
        private void BuildRubricShell()
        {
            var root = _canvas.transform;
            _rubricRoot = new GameObject("RubricPanel", typeof(RectTransform));
            ((RectTransform)_rubricRoot.transform).SetParent(root, false);
            var rt = (RectTransform)_rubricRoot.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(34f, 0f);
            rt.sizeDelta = new Vector2(760f, 960f);
            _rubricRoot.SetActive(false);

            var bg = UILib.Panel(_rubricRoot.transform, "Bg", SpriteFactory.RoundRect(34), Theme.Panel);

            // titre + fermeture
            _rubricTitle = UILib.Label(_rubricRoot.transform, "Title", "", 44, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(-60f, -46f), new Vector2(520f, 64f), TextAnchor.MiddleCenter, FontStyle.Bold);

            UILib.MakeButton(_rubricRoot.transform, "Close", "Fermer",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -26f), new Vector2(170f, 66f),
                CloseRubric,
                new UILib.ButtonStyle { background = Theme.PanelSoft, textColor = Theme.TextSecondary, fontSize = 28 });

            // --- liste (monuments) ---
            _listViewport = new GameObject("Viewport", typeof(RectTransform));
            ((RectTransform)_listViewport.transform).SetParent(_rubricRoot.transform, false);
            var vp = (RectTransform)_listViewport.transform;
            vp.anchorMin = new Vector2(0.5f, 1f);
            vp.anchorMax = new Vector2(0.5f, 1f);
            vp.pivot = new Vector2(0.5f, 1f);
            vp.anchoredPosition = new Vector2(0f, -90f);
            vp.sizeDelta = new Vector2(700f, 260f);
            _listViewport.AddComponent<RectMask2D>();

            _listContent = UILib.Rect(_listViewport.transform, "Content",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(700f, 0f));
            var vlg = _listContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            var fitter = _listContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = _listViewport.AddComponent<ScrollRect>();
            scroll.content = _listContent;
            scroll.viewport = vp;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 45f;

            // --- zone de détail ---
            _detailRoot = UILib.Rect(_rubricRoot.transform, "Detail",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 300f), new Vector2(700f, 540f));
            var dvg = _detailRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            dvg.padding = new RectOffset(10, 10, 10, 10);
            dvg.spacing = 10f;
            dvg.childAlignment = TextAnchor.UpperCenter;
            dvg.childControlWidth = true;
            dvg.childControlHeight = true;
            dvg.childForceExpandWidth = true;
            dvg.childForceExpandHeight = false;

            // image
            var imgRt = UILib.Rect(_detailRoot, "Image", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 300f));
            _detailImage = imgRt.gameObject.AddComponent<Image>();
            _detailImage.preserveAspect = true;
            _detailImage.color = Color.white;
            var leImg = imgRt.gameObject.AddComponent<LayoutElement>();
            leImg.preferredHeight = 300f;
            leImg.minHeight = 300f;

            // nom
            var nameRt = UILib.Rect(_detailRoot, "Name", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 54f));
            _detailName = nameRt.gameObject.AddComponent<Text>();
            _detailName.font = Theme.DefaultFont;
            _detailName.fontSize = 36;
            _detailName.fontStyle = FontStyle.Bold;
            _detailName.color = Theme.TextPrimary;
            _detailName.alignment = TextAnchor.MiddleCenter;
            _detailName.supportRichText = true;
            _detailName.raycastTarget = false;
            var leName = nameRt.gameObject.AddComponent<LayoutElement>();
            leName.preferredHeight = 54f;

            // description
            var descRt = UILib.Rect(_detailRoot, "Desc", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 170f));
            _detailDesc = descRt.gameObject.AddComponent<Text>();
            _detailDesc.font = Theme.DefaultFont;
            _detailDesc.fontSize = 26;
            _detailDesc.color = Theme.TextSecondary;
            _detailDesc.alignment = TextAnchor.UpperCenter;
            _detailDesc.supportRichText = true;
            _detailDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailDesc.verticalOverflow = VerticalWrapMode.Overflow;
            _detailDesc.raycastTarget = false;
            descRt.gameObject.AddComponent<RectMask2D>();
            var leDesc = descRt.gameObject.AddComponent<LayoutElement>();
            leDesc.preferredHeight = 170f;
            leDesc.minHeight = 170f;

            // indice 3D
            var hintRt = UILib.Rect(_detailRoot, "Hint3D", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 44f));
            _detailHint = hintRt.gameObject.AddComponent<Text>();
            _detailHint.text = "<color=#4FC3F7>▶ Modèle 3D en rotation affiché à droite</color>";
            _detailHint.font = Theme.DefaultFont;
            _detailHint.fontSize = 26;
            _detailHint.alignment = TextAnchor.MiddleCenter;
            _detailHint.supportRichText = true;
            _detailHint.raycastTarget = false;
            var leHint = hintRt.gameObject.AddComponent<LayoutElement>();
            leHint.preferredHeight = 44f;
        }

        private Text _rubricTitle;

        private void OpenRubric(string rubric)
        {
            _rubric = rubric;
            _rubricRoot.SetActive(true);
            _menuRoot.SetActive(false);
            _infoRoot.SetActive(false);

            switch (rubric)
            {
                case "monuments":
                    _rubricTitle.text = "<b>Monuments — " + _gov.name + "</b>";
                    _listViewport.SetActive(true);
                    LayoutForDetail(270f, 570f, 150f, true);
                    BuildMonumentList();
                    SelectMonument(0);
                    break;
                case "food":
                    _rubricTitle.text = "<b>Gastronomie — " + _gov.name + "</b>";
                    _listViewport.SetActive(false);
                    LayoutForDetail(430f, 840f, 260f, false);
                    ShowSingle(_gov.foods.Length > 0 ? _gov.foods[0].name : "—",
                        _gov.foods.Length > 0 ? _gov.foods[0].description : "",
                        _gov.foods.Length > 0 ? _gov.foods[0].imageKey : null);
                    break;
                case "clothes":
                    _rubricTitle.text = "<b>Vêtements traditionnels — " + _gov.name + "</b>";
                    _listViewport.SetActive(false);
                    LayoutForDetail(430f, 840f, 260f, false);
                    ShowSingle(_gov.clothes.Length > 0 ? _gov.clothes[0].name : "—",
                        _gov.clothes.Length > 0 ? _gov.clothes[0].description : "",
                        _gov.clothes.Length > 0 ? _gov.clothes[0].imageKey : null);
                    break;
            }
        }

        /// <summary>Ajuste la géométrie de la zone de détail selon le mode.</summary>
        private void LayoutForDetail(float imageH, float totalH, float descH, bool showList)
        {
            // ancrage en bas : position calculée pour tenir dans le panneau (hauteur 960)
            _detailRoot.sizeDelta = new Vector2(700f, totalH);
            float topEdge = showList ? 120f : 390f;          // limite haute disponible
            float center = topEdge - totalH * 0.5f;
            _detailRoot.anchoredPosition = new Vector2(0f, center + 480f);

            var leImg = _detailImage.GetComponent<LayoutElement>();
            leImg.preferredHeight = imageH;
            leImg.minHeight = imageH;
            var leDesc = _detailDesc.GetComponent<LayoutElement>();
            leDesc.preferredHeight = descH;
            leDesc.minHeight = descH;
            _detailDesc.fontSize = showList ? 26 : 27;
            _detailHint.gameObject.SetActive(showList);
        }

        private void CloseRubric()
        {
            _rubricRoot.SetActive(false);
            _menuRoot.SetActive(true);
            _infoRoot.SetActive(true);
            _world.HideStage();
            _rubric = null;
        }

        // ------------------------------------------------------------------
        //  Liste des monuments + détail (F06)
        // ------------------------------------------------------------------
        private void BuildMonumentList()
        {
            foreach (Transform child in _listContent)
                UnityEngine.Object.Destroy(child.gameObject);

            for (int i = 0; i < _gov.monuments.Length; i++)
            {
                int idx = i;
                var mon = _gov.monuments[i];

                var card = UILib.Panel(_listContent, "Card_" + i, SpriteFactory.RoundRect(20), Theme.PanelSoft);
                var le = card.gameObject.AddComponent<LayoutElement>();
                le.preferredHeight = 132f;
                le.minHeight = 132f;

                // vignette
                var thumb = UILib.Rect(card.transform, "Thumb",
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(14f, 0f), new Vector2(170f, 104f));
                var thImg = thumb.gameObject.AddComponent<Image>();
                thImg.sprite = ImageLib.Load(mon.imageKey, 340, 208);
                thImg.preserveAspect = true;
                thImg.color = Color.white;
                thImg.raycastTarget = false;
                thumb.gameObject.AddComponent<RectMask2D>();

                // texte
                var nameRt = UILib.Rect(card.transform, "Name",
                    new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                nameRt.offsetMin = new Vector2(204f, 8f);
                nameRt.offsetMax = new Vector2(-64f, -8f);
                var nameTxt = nameRt.gameObject.AddComponent<Text>();
                nameTxt.text = "<b>" + mon.name + "</b>";
                nameTxt.font = Theme.DefaultFont;
                nameTxt.fontSize = 30;
                nameTxt.color = Theme.TextPrimary;
                nameTxt.alignment = TextAnchor.MiddleLeft;
                nameTxt.supportRichText = true;
                nameTxt.raycastTarget = false;

                // chevron
                var chev = UILib.Rect(card.transform, "Chev",
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(-22f, 0f), new Vector2(34f, 34f));
                var chevImg = chev.gameObject.AddComponent<Image>();
                chevImg.sprite = SpriteFactory.Icon("next");
                chevImg.color = Theme.Accent;
                chevImg.preserveAspect = true;
                chevImg.raycastTarget = false;

                // interaction : l'image de la carte sert de cible
                card.raycastTarget = true;
                var btn = card.gameObject.AddComponent<Button>();
                btn.targetGraphic = card;
                int captured = idx;
                btn.onClick.AddListener(() => SelectMonument(captured));
            }
        }

        private void SelectMonument(int index)
        {
            if (index < 0 || index >= _gov.monuments.Length) return;
            _monIndex = index;
            var mon = _gov.monuments[index];
            _detailImage.sprite = ImageLib.Load(mon.imageKey, 680, 300);
            _detailName.text = "<b>" + mon.name + "</b>";
            _detailDesc.text = mon.description;
            _world.ShowMonument(mon, _gov);
        }

        private void ShowSingle(string name, string description, string imageKey)
        {
            _detailImage.sprite = ImageLib.Load(imageKey, 680, 430);
            _detailName.text = "<b>" + name + "</b>";
            _detailDesc.text = description;
            _world.HideStage();
        }

        // ==================================================================
        //  Modales (réglages, à propos, crédits)
        // ==================================================================
        private void ShowTextModal(string title, string body, string primaryLabel, Action primary,
            string secondLabel = null, Action second = null)
        {
            string[] labels = new string[2];
            Action[] actions = new Action[2];
            int n = 0;
            if (!string.IsNullOrEmpty(primaryLabel)) { labels[n] = primaryLabel; actions[n] = primary; n++; }
            if (!string.IsNullOrEmpty(secondLabel)) { labels[n] = secondLabel; actions[n] = second; n++; }
            if (n == 0) { labels[n] = "Fermer"; actions[n] = null; n++; }
            ShowModal(title, body, n, labels, actions);
        }

        private void ShowSettings()
        {
            string[] labels =
            {
                "Changer de gouvernorat",
                "Retour à l'accueil",
                "À propos & crédits",
                "Fermer",
            };
            Action[] actions =
            {
                () => { CloseModal(); Nav.Identification(); },
                () => { CloseModal(); Nav.Home(); },
                ShowCredits,
                null,
            };
            ShowModal("Réglages", "Gouvernorat sélectionné : <b>" + _gov.name + "</b>\n" +
                "Source : " + (AppState.SelectionSource == "scan" ? "scan de carte (simulé)" : "sélection manuelle"),
                4, labels, actions);
        }

        private void ShowCredits()
        {
            CloseModal();
            ShowTextModal("À propos & crédits",
                "<b>Discover Tunisia</b> — application interactive de découverte du patrimoine " +
                "tunisien (projet académique de réalité augmentée).\n\n" +
                "<b>Gouvernorats :</b> Nabeul, Bizerte, Kairouan.\n" +
                "<b>Textes :</b> rédigés d'après Wikipédia (licence CC BY-SA).\n" +
                "<b>Images :</b> Wikimedia Commons et NASA (licences libres) — voir CREDITS.md.\n" +
                "<b>Modèles 3D :</b> maquettes procédurales créées pour ce prototype.",
                "Fermer", null);
        }

        private void ShowModal(string title, string body, int buttonCount, string[] labels, Action[] actions)
        {
            CloseModal();

            var root = _canvas.transform;
            _modalRoot = new GameObject("Modal", typeof(RectTransform));
            ((RectTransform)_modalRoot.transform).SetParent(root, false);
            var mrt = (RectTransform)_modalRoot.transform;
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.offsetMin = Vector2.zero;
            mrt.offsetMax = Vector2.zero;

            var veil = UILib.Panel(_modalRoot.transform, "Veil", SpriteFactory.RoundRect(2),
                new Color(0.01f, 0.03f, 0.07f, 0.85f));
            veil.raycastTarget = true;

            float boxH = 300f + Mathf.Max(1, buttonCount) * 100f;
            var box = UILib.Panel(_modalRoot.transform, "Box",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1100f, boxH), SpriteFactory.RoundRect(34), Theme.Panel);

            UILib.Label(box.transform, "Title", "<b>" + title + "</b>", 44, Theme.TextPrimary,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -50f), new Vector2(980f, 64f));

            var bodyRt = UILib.Rect(box.transform, "Body", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            bodyRt.offsetMin = new Vector2(70f, 40f + buttonCount * 100f);
            bodyRt.offsetMax = new Vector2(-70f, -96f);
            bodyRt.gameObject.AddComponent<RectMask2D>();
            var bodyTxt = bodyRt.gameObject.AddComponent<Text>();
            bodyTxt.text = body;
            bodyTxt.font = Theme.DefaultFont;
            bodyTxt.fontSize = 30;
            bodyTxt.color = Theme.TextSecondary;
            bodyTxt.alignment = TextAnchor.UpperCenter;
            bodyTxt.supportRichText = true;
            bodyTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyTxt.verticalOverflow = VerticalWrapMode.Overflow;
            bodyTxt.raycastTarget = false;

            for (int i = 0; i < buttonCount; i++)
            {
                int idx = i;
                string lbl = labels[i];
                Action act = actions[i];
                UILib.MakeButton(box.transform, "Btn" + i, lbl,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(0f, 40f + i * 100f), new Vector2(700f, 84f),
                    () => { CloseModal(); act?.Invoke(); },
                    new UILib.ButtonStyle
                    {
                        background = i == 0 ? Theme.Accent : Theme.PanelSoft,
                        textColor = i == 0 ? new Color(0.02f, 0.08f, 0.16f) : Theme.TextPrimary,
                        fontSize = 32,
                        fontStyle = FontStyle.Bold,
                    });
            }
        }

        private void CloseModal()
        {
            if (_modalRoot != null)
            {
                UnityEngine.Object.Destroy(_modalRoot);
                _modalRoot = null;
            }
        }
    }
}
