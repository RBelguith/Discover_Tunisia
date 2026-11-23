using System;
using DiscoverTunisia.Content;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DiscoverTunisia.App
{
    /// <summary>Base for a screen: binds its UIDocument, fades it in and maps Escape / Android back to OnBack.</summary>
    [RequireComponent(typeof(UIDocument))]
    public abstract class ScreenController : MonoBehaviour
    {
        protected const string VisibleClass = "screen--visible";

        [SerializeField] protected ContentCatalog catalog;

        protected VisualElement Root { get; private set; }
        protected GovernorateData Governorate => AppSession.Resolve(catalog);

        protected virtual void OnEnable()
        {
            Root = GetComponent<UIDocument>().rootVisualElement;
            Root.pickingMode = PickingMode.Ignore;
            Bind(Root);
            var screen = Root.Q(className: "screen");
            screen?.schedule.Execute(() => screen.AddToClassList(VisibleClass)).StartingIn(30);
        }

        protected abstract void Bind(VisualElement root);

        protected virtual void OnBack() { }

        protected virtual void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) OnBack();
        }

        protected Button OnClick(string name, Action action)
        {
            var button = Root.Q<Button>(name) ?? throw new InvalidOperationException($"{GetType().Name}: no button named '{name}'");
            button.clicked += action;
            return button;
        }

        /// <summary>True when a screen position (pixels, origin bottom-left) is over a pickable UI element.</summary>
        public bool IsPointerOverUI(Vector2 screenPosition)
        {
            var panel = Root?.panel;
            if (panel == null) return false;
            var p = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            return panel.Pick(p) != null;
        }
    }
}
