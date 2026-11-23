using UnityEngine;

namespace DiscoverTunisia.UI
{
    /// <summary>Fonds d'écran procéduraux (ciel étoilé bleu nuit).</summary>
    public static class Backgrounds
    {
        private static Sprite _starfield;

        public static Sprite Starfield()
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
            for (int i = 0; i < 240; i++)
            {
                int x = rnd.Next(0, w), y = rnd.Next(0, (int)(h * 0.88f));
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

            // halo lumineux diffus
            float cx = w * 0.5f, cy = h * 0.55f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / (w * 0.60f), dy = (y - cy) / (h * 0.60f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d >= 1f) continue;
                    float g = Mathf.Pow(1f - d, 2.5f) * 0.40f;
                    Color32 s = c.px[y * w + x];
                    c.px[y * w + x] = new Color32(
                        (byte)Mathf.Min(255, s.r + (byte)(35 * g)),
                        (byte)Mathf.Min(255, s.g + (byte)(85 * g)),
                        (byte)Mathf.Min(255, s.b + (byte)(150 * g)),
                        s.a);
                }

            _starfield = c.ToSprite(100);
            _starfield.texture.filterMode = FilterMode.Bilinear;
            _starfield.texture.wrapMode = TextureWrapMode.Clamp;
            return _starfield;
        }
    }
}
