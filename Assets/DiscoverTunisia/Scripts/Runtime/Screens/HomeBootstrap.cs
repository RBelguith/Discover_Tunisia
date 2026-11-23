using UnityEngine;
using UnityEngine.UI;
using DiscoverTunisia.Core;
using DiscoverTunisia.UI;

namespace DiscoverTunisia.Screens
{
    /// <summary>F01 — Écran d'accueil : nom, présentation, bouton pour commencer.</summary>
    public class HomeBootstrap : SceneBootstrap
    {
        protected override void Awake()
        {
            base.Awake();
            BuildBackground();
            BuildContent();
        }

        private void BuildBackground()
        {
            // Ciel étoilé procédural dégradé
            var bg = UILib.Panel(Canvas.transform, "Bg");
            bg.sprite = MakeStarfield();
            bg.type = Image.Type.Simple;
            bg.raycastTarget = false;
        }

        private static Sprite _starfield;
        private static Sprite MakeStarfield()
        {
            if (_starfield != null) return _starfield;
            int w = 960, h = 540;
            var c = new SpriteFactory.Canvas2D(w, h);
            var top = Theme.BackgroundTop;
            var bottom = Theme.BackgroundBottom;
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color col = Color.Lerp(top, bottom, t);
                var cc = (Color32)col;
                for (int x = 0; x < w; x++) c.px[y * w + x] = cc;
            }
            var rnd = new System.Random(12345);
            for (int i = 0; i < 220; i++)
            {
                int x = rnd.Next(0, w), y = rnd.Next(0, (int)(h * 0.85f));
                byte a = (byte)(90 + rnd.Next(0, 165));
                int size = rnd.Next(0, 12) == 0 ? 2 : 1;
                for (int dy = 0; dy < size; dy++)
                    for (int dx = 0; dx < size; dx++)
                    {
                        int px = x + dx, py = y + dy;
                        if (px < w && py < h)
                            c.px[py * w + px] = new Color32(255, 255, 255, a);
                    }
            }
            // halo lumineux central
            float cx = w * 0.5f, cy = h * 0.62f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / (w * 0.55f), dy = (y - cy) / (h * 0.55f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d >= 1f) continue;
                    float g = Mathf.Pow(1f - d, 2.5f) * 0.35f;
                    Color32 s = c.px[y * w + x];
                    c.px[y * w + x] = new Color32(
                        (byte)Mathf.Min(255, s.r + (byte)(40 * g)),
                        (byte)Mathf.Min(255, s.g + (byte)(90 * g)),
                        (byte)Mathf.Min(255, s.b + (byte)(150 * g)),
                        s.a);
                }
            _starfield = c.ToSprite(100);
            return _starfield;
        }

        private void BuildContent()
        {
            var root = Canvas.transform;

            // Bandeau logo / titre
            var title = UILib.Label(root, "Title", "<b>Discover</b> <color=#4FC3F7>Tunisia</color>", 110,
                Theme.TextPrimary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -140f), new Vector2(1500f, 140f), TextAnchor.MiddleCenter, FontStyle.Normal);
            title.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.6f);

            UILib.Label(root, "Subtitle",
                "Application interactive de découverte du patrimoine tunisien\nà travers un globe 3D et la réalité augmentée",
                34, Theme.TextSecondary, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -260f), new Vector2(1400f, 110f));

            // Trois "badges" de présentation
            string[] badges = { "Globe 3D interactif", "3 gouvernorats", "Mode réalité augmentée" };
            float badgeW = 430f, gap = 30f;
            float totalW = badges.Length * badgeW + (badges.Length - 1) * gap;
            for (int i = 0; i < badges.Length; i++)
            {
                float x = -totalW * 0.5f + badgeW * 0.5f + i * (badgeW + gap);
                var chip = UILib.Panel(root, "Badge" + i,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(x, 40f), new Vector2(badgeW, 76f), SpriteFactory.RoundRect(38), Theme.PanelSoft);
                UILib.Label(chip.transform, "Text", badges[i], 30, Theme.TextPrimary);
            }

            // Bouton commencer (F01)
            var startBtn = UILib.MakeButton(root, "StartButton", "Commencer",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 170f), new Vector2(520f, 110f),
                () => Nav.Identification(),
                new UILib.ButtonStyle
                {
                    background = Theme.Accent,
                    textColor = new Color(0.02f, 0.08f, 0.16f),
                    fontSize = 44,
                    fontStyle = FontStyle.Bold,
                    icon = "next",
                    iconTint = new Color(0.02f, 0.08f, 0.16f),
                    iconSize = 46,
                });
            var glow = UILib.AddGlow(startBtn.transform, "Glow", new Color(0.31f, 0.76f, 0.97f, 0.55f), 760f);
            glow.transform.SetAsFirstSibling();

            // Mentions basses
            UILib.Label(root, "Footer",
                "Projet de réalité augmentée — Nabeul • Bizerte • Kairouan",
                26, new Color(0.55f, 0.66f, 0.78f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(1400f, 40f));
        }
    }
}
