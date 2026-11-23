using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace DiscoverTunisia.UI
{
    /// <summary>Thème visuel : bleu nuit + accents lumineux (cf. maquette).</summary>
    public static class Theme
    {
        public static readonly Color BackgroundTop = new Color(0.031f, 0.075f, 0.176f);
        public static readonly Color BackgroundBottom = new Color(0.012f, 0.031f, 0.086f);
        public static readonly Color Panel = new Color(0.055f, 0.118f, 0.226f, 0.92f);
        public static readonly Color PanelSoft = new Color(0.078f, 0.157f, 0.290f, 0.75f);
        public static readonly Color Accent = new Color(0.310f, 0.765f, 0.969f);   // cyan lumineux
        public static readonly Color Gold = new Color(1.000f, 0.788f, 0.302f);
        public static readonly Color TextPrimary = new Color(0.94f, 0.97f, 1.00f);
        public static readonly Color TextSecondary = new Color(0.70f, 0.80f, 0.90f);
        public static readonly Color Success = new Color(0.20f, 0.82f, 0.45f);
        public static readonly Color Danger = new Color(0.95f, 0.35f, 0.35f);

        private static Font _font;
        public static Font DefaultFont
        {
            get
            {
                if (_font == null)
                {
                    try { _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
                    catch { _font = null; }
                    if (_font == null)
                        _font = Font.CreateDynamicFontFromOSFont(
                            new[] { "Segoe UI", "Arial", "Helvetica", "Roboto", "Noto Sans" }, 16);
                }
                return _font;
            }
        }
    }

    /// <summary>Création rapide d'interfaces uGUI entièrement par code.</summary>
    public static class UILib
    {
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;

        public static Canvas CreateCanvas(string name, int sortOrder = 0)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RefWidth, RefHeight);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ------------------------------------------------------------------
        //  Rect helpers
        // ------------------------------------------------------------------
        public static RectTransform Rect(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            if (parent != null) rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Remplit tout le parent.</summary>
        public static RectTransform Fill(Transform parent, string name)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        // ------------------------------------------------------------------
        //  Éléments
        // ------------------------------------------------------------------
        public static Image Panel(Transform parent, string name, Sprite sprite = null, Color? color = null)
        {
            var rt = Fill(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color ?? Color.white;
            return img;
        }

        public static Image Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 pos, Vector2 size, Sprite sprite, Color color)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            img.color = color;
            return img;
        }

        public static Text Label(Transform parent, string name, string content, int fontSize,
            Color color, TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rt = Fill(parent, name);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.text = content;
            txt.font = Theme.DefaultFont;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.fontStyle = style;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        public static Text Label(Transform parent, string name, string content, int fontSize,
            Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
            var txt = rt.gameObject.AddComponent<Text>();
            txt.text = content;
            txt.font = Theme.DefaultFont;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.alignment = alignment;
            txt.fontStyle = style;
            txt.supportRichText = true;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        public static Image Pic(Transform parent, string name, Sprite sprite, Vector2 anchorMin,
            Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size, Color tint)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.color = tint;
            return img;
        }

        public class ButtonStyle
        {
            public Sprite sprite;
            public Color background = Theme.Panel;
            public Color textColor = Theme.TextPrimary;
            public int fontSize = 34;
            public FontStyle fontStyle = FontStyle.Normal;
            public string icon;                 // nom d'icône SpriteFactory (optionnel)
            public Color iconTint = Color.white;
            public float iconSize = 44f;
        }

        public static Button MakeButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size,
            System.Action onClick, ButtonStyle style = null)
        {
            style = style ?? new ButtonStyle();
            var rt = Rect(parent, name, anchorMin, anchorMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = style.sprite != null ? style.sprite : SpriteFactory.RoundRect(24);
            img.type = Image.Type.Sliced;
            img.color = style.background;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.80f, 0.85f, 0.95f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            float textOffset = 0f;
            if (!string.IsNullOrEmpty(style.icon))
            {
                var iconRt = Rect(rt, "Icon", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(size.y * 0.5f, 0f),
                    new Vector2(style.iconSize, style.iconSize));
                var iconImg = iconRt.gameObject.AddComponent<Image>();
                iconImg.sprite = SpriteFactory.Icon(style.icon);
                iconImg.color = style.iconTint;
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                textOffset = style.iconSize * 0.5f;
            }

            if (!string.IsNullOrEmpty(label))
            {
                var txtRt = textOffset > 0f
                    ? Rect(rt, "Label", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                        new Vector2(textOffset, 0f), Vector2.zero)
                    : Fill(rt, "Label");
                var txt = txtRt.gameObject.AddComponent<Text>();
                txt.text = label;
                txt.font = Theme.DefaultFont;
                txt.fontSize = style.fontSize;
                txt.fontStyle = style.fontStyle;
                txt.color = style.textColor;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.supportRichText = true;
                txt.raycastTarget = false;
                txt.horizontalOverflow = HorizontalWrapMode.Wrap;
                txt.verticalOverflow = VerticalWrapMode.Overflow;
            }
            return btn;
        }

        /// <summary>Fond d'écran dégradé + voile sombre.</summary>
        public static Image Background(Transform canvas, string name = "Background")
        {
            var img = Panel(canvas, name, SpriteFactory.VerticalGradient(Theme.BackgroundTop, Theme.BackgroundBottom));
            img.raycastTarget = false;
            return img;
        }

        /// <summary>Ajoute un halo coloré derrière un élément (effet néon).</summary>
        public static Image AddGlow(Transform parent, string name, Color color, float size, Vector2? pos = null)
        {
            var rt = Rect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), pos ?? Vector2.zero, new Vector2(size, size));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.Glow();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }
    }

    /// <summary>Saisie pointeur unifiée (souris + tactile) via le Input System.</summary>
    public static class InputHelper
    {
        public static Vector2 PointerPosition
        {
            get
            {
                var touch = Touchscreen.current;
                if (touch != null && touch.touches.Count > 0)
                {
                    var t = touch.touches[0];
                    var ph = t.phase.ReadValue();
                    if (ph == UnityEngine.InputSystem.TouchPhase.Began ||
                        ph == UnityEngine.InputSystem.TouchPhase.Moved ||
                        ph == UnityEngine.InputSystem.TouchPhase.Stationary)
                        return t.position.ReadValue();
                }
                var mouse = Mouse.current;
                if (mouse != null) return mouse.position.ReadValue();
                var p = Pointer.current;
                return p != null ? p.position.ReadValue() : Vector2.zero;
            }
        }

        public static Vector2 PointerDelta
        {
            get
            {
                var touch = Touchscreen.current;
                if (touch != null && touch.touches.Count > 0)
                {
                    var t = touch.touches[0];
                    var ph = t.phase.ReadValue();
                    if (ph == UnityEngine.InputSystem.TouchPhase.Began ||
                        ph == UnityEngine.InputSystem.TouchPhase.Moved)
                        return t.delta.ReadValue();
                }
                var mouse = Mouse.current;
                if (mouse != null) return mouse.delta.ReadValue();
                var p = Pointer.current;
                return p != null ? p.delta.ReadValue() : Vector2.zero;
            }
        }

        public static bool PointerDown
        {
            get
            {
                var touch = Touchscreen.current;
                if (touch != null && touch.touches.Count > 0 &&
                    touch.touches[0].phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                    return true;
                var mouse = Mouse.current;
                return mouse != null && mouse.leftButton.wasPressedThisFrame;
            }
        }

        public static bool PointerUp
        {
            get
            {
                var touch = Touchscreen.current;
                if (touch != null && touch.touches.Count > 0)
                {
                    var ph = touch.touches[0].phase.ReadValue();
                    if (ph == UnityEngine.InputSystem.TouchPhase.Ended ||
                        ph == UnityEngine.InputSystem.TouchPhase.Canceled)
                        return true;
                }
                var mouse = Mouse.current;
                return mouse != null && mouse.leftButton.wasReleasedThisFrame;
            }
        }

        public static bool PointerHeld
        {
            get
            {
                var touch = Touchscreen.current;
                if (touch != null && touch.touches.Count > 0)
                {
                    var ph = touch.touches[0].phase.ReadValue();
                    if (ph == UnityEngine.InputSystem.TouchPhase.Began ||
                        ph == UnityEngine.InputSystem.TouchPhase.Moved ||
                        ph == UnityEngine.InputSystem.TouchPhase.Stationary)
                        return true;
                }
                var mouse = Mouse.current;
                return mouse != null && mouse.leftButton.isPressed;
            }
        }

        /// <summary>Vrai si le pointeur commence sur un élément UI (pour ne pas tourner le globe).</summary>
        public static bool IsPointerOverUI()
        {
            var es = EventSystem.current;
            if (es == null) return false;
            if (!PointerDown) return false;
            var ped = new PointerEventData(es) { position = PointerPosition };
            var results = new List<RaycastResult>();
            es.RaycastAll(ped, results);
            return results.Count > 0;
        }
    }
}
