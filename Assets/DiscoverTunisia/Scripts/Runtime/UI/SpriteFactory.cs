using System.Collections.Generic;
using UnityEngine;

namespace DiscoverTunisia.UI
{
    /// <summary>
    /// Petit éditeur de textures : permet de générer à la volée des sprites
    /// (rectangles arrondis, halos, dégradés, icônes) sans asset externe.
    /// </summary>
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();

        /// <summary>Texture de halo : reconstruite si Unity l'a détruite entre deux sessions de jeu.</summary>
        public static Texture2D GlowTexture(int size = 256)
        {
            string key = "glowtex_" + size;
            if (_textureCache.TryGetValue(key, out var tex) && tex != null) return tex;

            tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            float cx = size * 0.5f, cy = size * 0.5f, R = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / R;
                    if (d >= 1f) continue;
                    float a = Mathf.Pow(1f - d, 2.2f);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            _textureCache[key] = tex;
            return tex;
        }

        /// <summary>Texture de disque plein (repère des gouvernorats).</summary>
        public static Texture2D DiscTexture(int size = 128)
        {
            string key = "disctex_" + size;
            if (_textureCache.TryGetValue(key, out var tex) && tex != null) return tex;

            var c = new Canvas2D(size, size);
            c.Clear(new Color32(255, 255, 255, 0));
            c.FillCircleAA(size * 0.5f, size * 0.5f, size * 0.5f - 1f, new Color32(255, 255, 255, 255));
            tex = c.ToSprite(100).texture;
            _textureCache[key] = tex;
            return tex;
        }

        // ------------------------------------------------------------------
        //  Toile de dessin
        // ------------------------------------------------------------------
        public class Canvas2D
        {
            public int w, h;
            public Color32[] px;

            public Canvas2D(int w, int h)
            {
                this.w = w; this.h = h;
                px = new Color32[w * h];
            }

            public void Clear(Color32 c)
            {
                for (int i = 0; i < px.Length; i++) px[i] = c;
            }

            private void Blend(int x, int y, Color32 c)
            {
                if ((uint)x >= (uint)w || (uint)y >= (uint)h) return;
                if (c.a == 0) return;
                int i = y * w + x;
                if (c.a == 255) { px[i] = c; return; }
                Color32 d = px[i];
                float ia = c.a / 255f;
                float da = d.a / 255f;
                float oa = ia + da * (1 - ia);
                if (oa <= 0f) { px[i] = new Color32(0, 0, 0, 0); return; }
                byte r = (byte)((c.r * ia + d.r * da * (1 - ia)) / oa);
                byte g = (byte)((c.g * ia + d.g * da * (1 - ia)) / oa);
                byte b = (byte)((c.b * ia + d.b * da * (1 - ia)) / oa);
                byte a = (byte)(oa * 255f);
                px[i] = new Color32(r, g, b, a);
            }

            public void FillRect(int x0, int y0, int x1, int y1, Color32 c)
            {
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                        Blend(x, y, c);
            }

            public void FillCircle(float cx, float cy, float r, Color32 c)
            {
                int x0 = Mathf.FloorToInt(cx - r), x1 = Mathf.CeilToInt(cx + r);
                int y0 = Mathf.FloorToInt(cy - r), y1 = Mathf.CeilToInt(cy + r);
                float r2 = r * r;
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        if (dx * dx + dy * dy <= r2) Blend(x, y, c);
                    }
            }

            /// <summary>Cercle antialiasé (bord adouci).</summary>
            public void FillCircleAA(float cx, float cy, float r, Color32 c)
            {
                int x0 = Mathf.FloorToInt(cx - r - 1), x1 = Mathf.CeilToInt(cx + r + 1);
                int y0 = Mathf.FloorToInt(cy - r - 1), y1 = Mathf.CeilToInt(cy + r + 1);
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float cov = Mathf.Clamp01(r + 0.5f - d);
                        if (cov <= 0f) continue;
                        Color32 cc = c; cc.a = (byte)(c.a * cov);
                        Blend(x, y, cc);
                    }
            }

            public void FillRing(float cx, float cy, float rOuter, float rInner, Color32 c)
            {
                int x0 = Mathf.FloorToInt(cx - rOuter), x1 = Mathf.CeilToInt(cx + rOuter);
                int y0 = Mathf.FloorToInt(cy - rOuter), y1 = Mathf.CeilToInt(cy + rOuter);
                float ro2 = rOuter * rOuter, ri2 = rInner * rInner;
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        float d2 = dx * dx + dy * dy;
                        if (d2 <= ro2 && d2 >= ri2) Blend(x, y, c);
                    }
            }

            public void FillPoly(Vector2[] pts, Color32 c)
            {
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (var p in pts) { minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y); }
                int y0 = Mathf.Max(0, Mathf.FloorToInt(minY));
                int y1 = Mathf.Min(h - 1, Mathf.CeilToInt(maxY));
                int n = pts.Length;
                for (int y = y0; y <= y1; y++)
                {
                    float sy = y + 0.5f;
                    // balayage horizontal : intersections avec les arêtes
                    var xs = new List<float>();
                    for (int i = 0; i < n; i++)
                    {
                        Vector2 a = pts[i], b = pts[(i + 1) % n];
                        if ((a.y <= sy && b.y > sy) || (b.y <= sy && a.y > sy))
                        {
                            float t = (sy - a.y) / (b.y - a.y);
                            xs.Add(a.x + t * (b.x - a.x));
                        }
                    }
                    xs.Sort();
                    for (int k = 0; k + 1 < xs.Count; k += 2)
                    {
                        int xa = Mathf.Max(0, Mathf.CeilToInt(xs[k] - 0.5f));
                        int xb = Mathf.Min(w - 1, Mathf.FloorToInt(xs[k + 1] - 0.5f));
                        for (int x = xa; x <= xb; x++) Blend(x, y, c);
                    }
                }
            }

            public void StrokeLine(float x0, float y0, float x1, float y1, float thick, Color32 c)
            {
                float dx = x1 - x0, dy = y1 - y0;
                float len = Mathf.Sqrt(dx * dx + dy * dy);
                if (len < 0.001f) { FillCircle(x0, y0, thick * 0.5f, c); return; }
                float nx = -dy / len * thick * 0.5f, ny = dx / len * thick * 0.5f;
                FillPoly(new Vector2[]
                {
                    new Vector2(x0 + nx, y0 + ny), new Vector2(x1 + nx, y1 + ny),
                    new Vector2(x1 - nx, y1 - ny), new Vector2(x0 - nx, y0 - ny)
                }, c);
                FillCircle(x0, y0, thick * 0.5f, c);
                FillCircle(x1, y1, thick * 0.5f, c);
            }

            public Sprite ToSprite(int ppu = 100, Vector2? pivot = null)
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                tex.SetPixels32(px);
                tex.Apply(false, false);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                return Sprite.Create(tex, new Rect(0, 0, w, h), pivot ?? new Vector2(0.5f, 0.5f), ppu);
            }
        }

        // ------------------------------------------------------------------
        //  Sprites de base (cache)
        // ------------------------------------------------------------------

        /// <summary>Rectangle arrondi réutilisable en mode Sliced (bord = radius).</summary>
        public static Sprite RoundRect(int radius = 24, int size = 128)
        {
            string key = $"rr_{radius}_{size}";
            if (_cache.TryGetValue(key, out var s) && s != null) return s;

            var c = new Canvas2D(size, size);
            c.Clear(new Color32(255, 255, 255, 255));
            var clear = new Color32(0, 0, 0, 0);
            int r = Mathf.Min(radius, size / 2);

            // découpe les 4 coins
            for (int y = 0; y < r; y++)
                for (int x = 0; x < r; x++)
                {
                    float dx = r - x - 0.5f, dy = r - y - 0.5f;
                    if (dx * dx + dy * dy > r * r)
                    {
                        c.px[y * size + x] = clear;                       // coin HG
                        c.px[y * size + (size - 1 - x)] = clear;           // coin HD
                        c.px[(size - 1 - y) * size + x] = clear;           // coin BG
                        c.px[(size - 1 - y) * size + (size - 1 - x)] = clear; // coin BD
                    }
                }
            // Unity 6 : le bord se transmet à Sprite.Create (propriété en lecture seule)
            var tmp = c.ToSprite(100);
            var sp = Sprite.Create(tmp.texture, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect,
                new Vector4(r, r, r, r));
            _cache[key] = sp;
            return sp;
        }

        /// <summary>Halo circulaire (radial fade) pour les surlignages.</summary>
        public static Sprite Glow(int size = 256)
        {
            string key = $"glow_{size}";
            if (_cache.TryGetValue(key, out var s) && s != null) return s;

            var c = new Canvas2D(size, size);
            c.Clear(new Color32(255, 255, 255, 0));
            float cx = size * 0.5f, cy = size * 0.5f, R = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / R;
                    if (d >= 1f) continue;
                    float a = Mathf.Pow(1f - d, 2.2f);
                    c.px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            var sp = c.ToSprite(100);
            _cache[key] = sp;
            return sp;
        }

        /// <summary>Cercle plein antialiasé.</summary>
        public static Sprite Disc(int size = 128)
        {
            string key = $"disc_{size}";
            if (_cache.TryGetValue(key, out var s) && s != null) return s;
            var c = new Canvas2D(size, size);
            c.Clear(new Color32(255, 255, 255, 0));
            c.FillCircleAA(size * 0.5f, size * 0.5f, size * 0.5f - 1f, new Color32(255, 255, 255, 255));
            var sp = c.ToSprite(100);
            _cache[key] = sp;
            return sp;
        }

        /// <summary>Dégradé vertical plein écran (bleu nuit immersif).</summary>
        public static Sprite VerticalGradient(Color top, Color bottom, int w = 64, int h = 256)
        {
            string key = $"vg_{ColorUtility.ToHtmlStringRGBA(top)}_{ColorUtility.ToHtmlStringRGBA(bottom)}_{w}_{h}";
            if (_cache.TryGetValue(key, out var s) && s != null) return s;
            var c = new Canvas2D(w, h);
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color col = Color.Lerp(top, bottom, t);
                var cc = (Color32)col;
                for (int x = 0; x < w; x++) c.px[y * w + x] = cc;
            }
            var sp = c.ToSprite(100);
            sp.texture.wrapMode = TextureWrapMode.Clamp;
            _cache[key] = sp;
            return sp;
        }

        /// <summary>Sprite issu d'une Texture2D (image importée).</summary>
        public static Sprite FromTexture(Texture2D tex)
        {
            if (tex == null) return null;
            string key = "tex_" + tex.name + "_" + tex.width + "x" + tex.height;
            if (_cache.TryGetValue(key, out var s) && s != null) return s;
            var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
            _cache[key] = sp;
            return sp;
        }

        // ------------------------------------------------------------------
        //  Icônes dessinées (96x96, blanc)
        // ------------------------------------------------------------------
        public static Sprite Icon(string name)
        {
            string key = "icon_" + name;
            if (_cache.TryGetValue(key, out var s) && s != null) return s;

            var c = new Canvas2D(96, 96);
            c.Clear(new Color32(255, 255, 255, 0));
            var col = new Color32(255, 255, 255, 255);

            switch (name)
            {
                case "monuments": // bâtiment classique : fronton + colonnes
                    c.FillPoly(new Vector2[] { new Vector2(48, 10), new Vector2(86, 34), new Vector2(10, 34) }, col);
                    c.FillRect(12, 38, 84, 46, col);
                    for (int i = 0; i < 4; i++)
                    {
                        int x = 16 + i * 18;
                        c.FillRect(x, 48, x + 10, 80, col);
                    }
                    c.FillRect(8, 82, 88, 92, col);
                    break;

                case "food": // fourchette + couteau
                    c.FillRect(26, 10, 32, 40, col);   // dents fourchette
                    c.FillRect(36, 10, 42, 40, col);
                    c.FillRect(46, 10, 52, 40, col);
                    c.FillRect(34, 36, 44, 46, col);
                    c.FillRect(36, 44, 42, 88, col);  // manche
                    // couteau
                    c.FillPoly(new Vector2[] { new Vector2(64, 10), new Vector2(80, 18), new Vector2(80, 44), new Vector2(64, 44) }, col);
                    c.FillRect(68, 42, 76, 88, col);
                    break;

                case "clothing": // t-shirt
                    c.FillPoly(new Vector2[]
                    {
                        new Vector2(30, 16), new Vector2(14, 26), new Vector2(24, 42),
                        new Vector2(30, 38), new Vector2(30, 86), new Vector2(66, 86),
                        new Vector2(66, 38), new Vector2(72, 42), new Vector2(82, 26), new Vector2(66, 16),
                        new Vector2(56, 24), new Vector2(40, 24)
                    }, col);
                    break;

                case "ar": // casque AR / lunettes
                    c.FillRect(8, 30, 88, 74, col);
                    c.FillCircle(30, 52, 12, new Color32(0, 0, 0, 255));
                    c.FillCircle(66, 52, 12, new Color32(0, 0, 0, 255));
                    c.FillRect(40, 60, 56, 74, new Color32(0, 0, 0, 255));
                    break;

                case "home":
                    c.FillPoly(new Vector2[] { new Vector2(48, 8), new Vector2(90, 44), new Vector2(78, 44), new Vector2(78, 88), new Vector2(18, 88), new Vector2(18, 44), new Vector2(6, 44) }, col);
                    c.FillRect(40, 58, 56, 88, new Color32(0, 0, 0, 255));
                    break;

                case "gear":
                    c.FillRing(48, 48, 44, 30, col);
                    for (int i = 0; i < 8; i++)
                    {
                        float a = i * Mathf.PI / 4f;
                        float cx = 48 + Mathf.Cos(a) * 40f, cy = 48 + Mathf.Sin(a) * 40f;
                        c.FillCircle(cx, cy, 9, col);
                    }
                    c.FillCircle(48, 48, 14, new Color32(0, 0, 0, 255));
                    break;

                case "back": // chevron gauche
                    c.StrokeLine(60, 18, 26, 48, 12, col);
                    c.StrokeLine(26, 48, 60, 78, 12, col);
                    break;

                case "next": // chevron droite
                    c.StrokeLine(36, 18, 70, 48, 12, col);
                    c.StrokeLine(70, 48, 36, 78, 12, col);
                    break;

                case "info":
                    c.FillCircleAA(48, 48, 40, col);
                    c.FillRect(43, 40, 53, 74, new Color32(0, 0, 0, 255));
                    c.FillCircle(48, 28, 6, new Color32(0, 0, 0, 255));
                    break;

                case "pin": // marqueur de localisation
                    c.FillCircleAA(48, 36, 26, col);
                    c.FillPoly(new Vector2[] { new Vector2(30, 50), new Vector2(66, 50), new Vector2(48, 88) }, col);
                    c.FillCircle(48, 36, 11, new Color32(0, 0, 0, 255));
                    break;

                case "check":
                    c.StrokeLine(20, 50, 42, 72, 13, col);
                    c.StrokeLine(42, 72, 78, 26, 13, col);
                    break;

                case "scan": // carte d'identité + lignes
                    c.FillRect(8, 22, 88, 74, col);
                    c.FillRect(16, 32, 40, 56, new Color32(0, 0, 0, 255));
                    c.FillRect(48, 34, 80, 42, new Color32(0, 0, 0, 255));
                    c.FillRect(48, 48, 80, 56, new Color32(0, 0, 0, 255));
                    c.FillRect(16, 62, 64, 68, new Color32(0, 0, 0, 255));
                    c.FillRect(8, 78, 88, 84, col);
                    break;

                case "manual": // liste / sélection
                    for (int i = 0; i < 3; i++)
                    {
                        int y = 20 + i * 24;
                        c.FillCircle(20, y + 8, 7, col);
                        c.FillRect(36, y + 4, 84, y + 13, col);
                    }
                    break;

                case "camera":
                    c.FillRect(8, 26, 88, 80, col);
                    c.FillRect(32, 14, 64, 30, col);
                    c.FillCircle(48, 54, 18, new Color32(0, 0, 0, 255));
                    c.FillCircle(48, 54, 12, col);
                    break;

                case "globe":
                    c.FillRing(48, 48, 40, 33, col);
                    c.FillCircleAA(48, 48, 33, new Color32(255, 255, 255, 60));
                    c.FillRect(14, 45, 82, 51, col);
                    c.FillRing(48, 48, 22, 17, col);
                    break;

                case "star":
                    {
                        var pts = new Vector2[10];
                        for (int i = 0; i < 10; i++)
                        {
                            float a = -Mathf.PI / 2 + i * Mathf.PI / 5;
                            float r = (i % 2 == 0) ? 42 : 18;
                            pts[i] = new Vector2(48 + Mathf.Cos(a) * r, 48 + Mathf.Sin(a) * r);
                        }
                        c.FillPoly(pts, col);
                    }
                    break;

                default: // pastille
                    c.FillCircleAA(48, 48, 36, col);
                    break;
            }

            var sprite = c.ToSprite(96);
            _cache[key] = sprite;
            return sprite;
        }
    }
}
