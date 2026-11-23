using UnityEngine;

namespace DiscoverTunisia.UI
{
    /// <summary>
    /// Chargement des images (Resources/Images) avec repli graphique
    /// si le fichier n'est pas encore importé.
    /// </summary>
    public static class ImageLib
    {
        public static Sprite Load(string key)
        {
            if (string.IsNullOrEmpty(key)) return Placeholder(key);
            var tex = Resources.Load<Texture2D>("Images/" + key);
            if (tex == null) return Placeholder(key);
            return SpriteFactory.FromTexture(tex);
        }

        public static Sprite Load(string key, int fallbackW, int fallbackH)
        {
            if (string.IsNullOrEmpty(key)) return Placeholder(key, fallbackW, fallbackH);
            var tex = Resources.Load<Texture2D>("Images/" + key);
            if (tex == null) return Placeholder(key, fallbackW, fallbackH);
            return SpriteFactory.FromTexture(tex);
        }

        private static Sprite Placeholder(string key, int w = 480, int h = 320)
        {
            string cacheKey = $"ph_{key}_{w}x{h}";
            // couleur déterministe à partir de la clé
            uint hash = 2166136261;
            foreach (char ch in (key ?? "?")) { hash ^= ch; hash *= 16777619; }
            var c = new SpriteFactory.Canvas2D(w, h);
            var baseCol = Color.HSVToRGB((hash % 360) / 360f, 0.35f, 0.35f);
            var top = (Color32)baseCol;
            var bottom = (Color32)(baseCol * 0.55f);
            for (int y = 0; y < h; y++)
            {
                var col = Color32.Lerp(top, bottom, y / (float)h);
                for (int x = 0; x < w; x++) c.px[y * w + x] = col;
            }
            // cadre + pastille centrale
            for (int x = 0; x < w; x++) { c.px[x] = new Color32(255, 255, 255, 90); c.px[(h - 1) * w + x] = new Color32(255, 255, 255, 90); }
            for (int y = 0; y < h; y++) { c.px[y * w] = new Color32(255, 255, 255, 90); c.px[y * w + w - 1] = new Color32(255, 255, 255, 90); }
            c.FillCircleAA(w * 0.5f, h * 0.5f, Mathf.Min(w, h) * 0.18f, new Color32(255, 255, 255, 70));
            return c.ToSprite(100);
        }
    }
}
