using System;
using System.Collections.Generic;
using DiscoverTunisia.App;
using DiscoverTunisia.Content;
using DiscoverTunisia.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace DiscoverTunisia.Screens
{
    /// <summary>Main screen: the globe on the governorate, its avatar, the three sections and the monument stand.</summary>
    public sealed class GlobeScreen : ScreenController
    {
        private enum Section { Monuments, Food, Clothes }

        private readonly struct Item
        {
            public readonly string Title;
            public readonly string Description;
            public readonly Sprite Image;
            public readonly GameObject Model;

            public Item(string title, string description, Sprite image, GameObject model)
            {
                Title = title;
                Description = description;
                Image = image;
                Model = model;
            }
        }

        [SerializeField] private GlobeController globe;
        [SerializeField] private OutfitTint avatar;

        [Header("Monument stand")]
        [SerializeField] private GameObject stand;
        [SerializeField] private Transform standSpot;
        [SerializeField] private float standFootprint = 2.2f;
        [SerializeField] private float standSpinDegreesPerSecond = 22f;

        private GovernorateData _governorate;
        private Camera _camera;
        private VisualElement _menu;
        private VisualElement _about;
        private VisualElement _hint;
        private VisualElement _section;
        private VisualElement _modal;
        private VisualElement _callout;
        private Label _slideText;
        private VisualElement _dots;
        private string[] _slides;
        private int _slide;
        private List<Item> _items;
        private GameObject _model;

        protected override void Bind(VisualElement root)
        {
            _governorate = Governorate;
            _camera = Camera.main;

            OnClick("backButton", () => Navigator.Go(AppScreen.Identification));
            OnClick("homeButton", () => Navigator.Go(AppScreen.Home));
            OnClick("settingsButton", ShowSettings);
            OnClick("monumentsButton", () => Open(Section.Monuments));
            OnClick("foodButton", () => Open(Section.Food));
            OnClick("clothesButton", () => Open(Section.Clothes));
            OnClick("arButton", () => Navigator.Go(AppScreen.AR));
            OnClick("closeSectionButton", CloseSection);
            OnClick("prevSlideButton", () => ShowSlide(_slide - 1));
            OnClick("nextSlideButton", () => ShowSlide(_slide + 1));
            OnClick("moreButton", () => ShowModal("À propos de " + _governorate.DisplayName, _governorate.Description));

            _menu = root.Q("menu");
            _about = root.Q("about");
            _hint = root.Q("hint");
            _section = root.Q("section");
            _modal = root.Q("modal");
            _callout = root.Q("callout");
            _slideText = root.Q<Label>("slideText");
            _dots = root.Q("dots");

            root.Q<Label>("governorateName").text = _governorate.DisplayName;
            root.Q<Label>("governorateSubtitle").text = _governorate.Subtitle;
            root.Q("pinIcon").style.unityBackgroundImageTintColor = _governorate.AccentColor;
            root.Q<Label>("aboutTitle").text = "À propos de " + _governorate.DisplayName;

            BuildSlides();
            BuildCallout();

            globe.ShowMarkers(catalog.Governorates, _governorate);
            globe.FocusOn(_governorate, _camera);
            globe.IsPointerOverUI = IsPointerOverUI;
            if (avatar != null) avatar.Apply(_governorate.AccentColor);
            if (stand != null) stand.SetActive(false);
        }

        // About carousel

        private void BuildSlides()
        {
            var g = _governorate;
            _slides = new[]
            {
                g.ShortBlurb,
                $"{g.Subtitle}. Découvrez {Count(g.Monuments.Count, "monument")}, {Count(g.Foods.Count, "spécialité culinaire", "spécialités culinaires")} et {Count(g.Clothes.Count, "tenue traditionnelle", "tenues traditionnelles")}.",
                "Faites glisser le globe pour le faire tourner, puis ouvrez une rubrique depuis le menu à droite.",
            };
            _dots.Clear();
            foreach (var _ in _slides)
            {
                var dot = new VisualElement();
                dot.AddToClassList("dot");
                _dots.Add(dot);
            }
            ShowSlide(0);
        }

        private static string Count(int n, string singular, string plural = null) =>
            n + " " + (n > 1 ? plural ?? singular + "s" : singular);

        private void ShowSlide(int index)
        {
            _slide = (index % _slides.Length + _slides.Length) % _slides.Length;
            _slideText.text = _slides[_slide];
            for (int i = 0; i < _dots.childCount; i++)
                _dots[i].EnableInClassList("dot--active", i == _slide);
        }

        // Governorate labels next to the markers

        private void BuildCallout()
        {
            _callout.Clear();
            foreach (var g in catalog.Governorates)
            {
                var button = new Button(() => SwitchTo(g));
                button.AddToClassList("callout__item");
                button.EnableInClassList("callout__item--selected", g == _governorate);
                var dot = new VisualElement { pickingMode = PickingMode.Ignore };
                dot.AddToClassList("callout__dot");
                dot.style.backgroundColor = g.AccentColor;
                button.Add(dot);
                button.Add(new Label(g.DisplayName) { pickingMode = PickingMode.Ignore });
                _callout.Add(button);
            }
        }

        private void SwitchTo(GovernorateData g)
        {
            if (g == _governorate) return;
            AppSession.Select(g, SelectionSource.Manual);
            Navigator.Go(AppScreen.Globe);
        }

        private void LateUpdate()
        {
            if (_callout == null || _camera == null || globe.Markers.Count == 0) return;
            Vector3 anchor = Vector3.zero;
            foreach (var m in globe.Markers)
                if (m.Selected) anchor = m.View.transform.position;

            bool visible = globe.FacesCamera(anchor, _camera) && !_section.ClassListContains(VisibleSectionClass);
            _callout.EnableInClassList("hidden", !visible);
            if (!visible) return;
            var p = RuntimePanelUtils.CameraTransformWorldToPanel(_callout.panel, anchor, _camera);
            _callout.style.translate = new Translate(p.x, p.y);
        }

        // Sections

        private const string VisibleSectionClass = "section--open";

        private void Open(Section section)
        {
            var g = _governorate;
            _items = new List<Item>();
            string title;
            switch (section)
            {
                case Section.Monuments:
                    title = "Monuments";
                    foreach (var m in g.Monuments) _items.Add(new Item(m.Title, m.Description, m.Image, m.Model));
                    break;
                case Section.Food:
                    title = "Gastronomie";
                    foreach (var c in g.Foods) _items.Add(new Item(c.Title, c.Description, c.Image, null));
                    break;
                default:
                    title = "Vêtements traditionnels";
                    foreach (var c in g.Clothes) _items.Add(new Item(c.Title, c.Description, c.Image, null));
                    break;
            }

            _section.Q<Label>("sectionTitle").text = title;
            _section.Q<Label>("sectionEyebrow").text = g.DisplayName.ToUpperInvariant();
            var tabs = _section.Q("tabs");
            tabs.Clear();
            tabs.EnableInClassList("hidden", _items.Count < 2);
            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                var tab = new Button(() => Select(index)) { text = _items[i].Title };
                tab.AddToClassList("tab");
                tabs.Add(tab);
            }

            _section.AddToClassList(VisibleSectionClass);
            _menu.AddToClassList("hidden");
            _about.AddToClassList("hidden");
            _hint.AddToClassList("hidden");
            if (avatar != null) avatar.gameObject.SetActive(false);
            Select(0);
        }

        private void Select(int index)
        {
            if (_items.Count == 0) return;
            var item = _items[index];
            var tabs = _section.Q("tabs");
            for (int i = 0; i < tabs.childCount; i++)
                tabs[i].EnableInClassList("tab--selected", i == index);
            _section.Q("detailImage").style.backgroundImage = new StyleBackground(item.Image);
            _section.Q<Label>("detailName").text = item.Title;
            _section.Q<Label>("detailText").text = item.Description;
            _section.Q("modelHint").EnableInClassList("hidden", item.Model == null);
            ShowModel(item.Model);
        }

        private void CloseSection()
        {
            _section.RemoveFromClassList(VisibleSectionClass);
            _menu.RemoveFromClassList("hidden");
            _about.RemoveFromClassList("hidden");
            _hint.RemoveFromClassList("hidden");
            if (avatar != null) avatar.gameObject.SetActive(true);
            ShowModel(null);
        }

        private void ShowModel(GameObject prefab)
        {
            if (_model != null) Destroy(_model);
            _model = null;
            if (stand == null) return;
            stand.SetActive(prefab != null);
            if (prefab == null) return;
            _model = Instantiate(prefab);
            ModelFitter.Fit(_model.transform, ModelFitter.Measure(_model), standSpot, standFootprint);
        }

        // Modals

        private void ShowSettings()
        {
            string source = AppSession.Source == SelectionSource.Scan ? "scan de la carte d'identité" : "choix dans la liste";
            ShowModal("Réglages", $"Gouvernorat : {_governorate.DisplayName}\nChoisi par {source}.",
                ("Changer de gouvernorat", () => Navigator.Go(AppScreen.Identification), true),
                ("Retour à l'accueil", () => Navigator.Go(AppScreen.Home), false),
                ("À propos et crédits", ShowCredits, false));
        }

        private void ShowCredits()
        {
            ShowModal("À propos et crédits",
                "Discover Tunisia, projet universitaire de découverte du patrimoine tunisien en 3D et en réalité augmentée.\n\n" +
                "Textes rédigés d'après Wikipédia (CC BY-SA). Photos de Wikimedia Commons et de la NASA, sous licences libres. " +
                "Modèles 3D : maquettes modélisées dans Blender pour ce projet. Police Poppins (SIL Open Font License). Détails dans CREDITS.md.");
        }

        private void ShowModal(string title, string body, params (string label, Action action, bool primary)[] actions)
        {
            _modal.Q<Label>("modalTitle").text = title;
            _modal.Q<Label>("modalText").text = body;
            var list = _modal.Q("modalActions");
            list.Clear();
            foreach (var (label, action, primary) in actions)
                list.Add(ModalButton(label, action, primary));
            list.Add(ModalButton("Fermer", null, actions.Length == 0));
            _modal.RemoveFromClassList("hidden");
        }

        private Button ModalButton(string label, Action action, bool primary)
        {
            var button = new Button(() =>
            {
                CloseModal();
                action?.Invoke();
            });
            button.AddToClassList("btn");
            button.AddToClassList("modal__action");
            if (primary) button.AddToClassList("btn--primary");
            button.Add(new Label(label) { pickingMode = PickingMode.Ignore });
            return button;
        }

        private void CloseModal() => _modal.AddToClassList("hidden");

        protected override void OnBack()
        {
            if (!_modal.ClassListContains("hidden")) CloseModal();
            else if (_section.ClassListContains(VisibleSectionClass)) CloseSection();
            else Navigator.Go(AppScreen.Identification);
        }

        protected override void Update()
        {
            base.Update();
            if (_model != null) _model.transform.Rotate(0f, standSpinDegreesPerSecond * Time.deltaTime, 0f, Space.Self);
        }
    }
}
