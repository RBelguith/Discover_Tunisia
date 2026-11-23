using UnityEngine;

namespace DiscoverTunisia.EditorTools.Placeholders
{
    /// <summary>Placeholder monuments built from primitives, one silhouette per style, baked by PlaceholderModelBaker.</summary>
    public static class PlaceholderMonuments
    {
        public static GameObject Build(PlaceholderStyle style, Color? accent, Transform parent)
        {
            var root = new GameObject("Monument_" + style);
            if (parent != null) root.transform.SetParent(parent, false);

            Color stone = new Color(0.86f, 0.80f, 0.66f);
            Color stoneDark = new Color(0.66f, 0.58f, 0.45f);
            Color brick = new Color(0.78f, 0.55f, 0.38f);
            Color blue = new Color(0.20f, 0.55f, 0.80f);
            Color white = new Color(0.92f, 0.92f, 0.90f);
            Color gold = accent.HasValue ? accent.Value : new Color(1f, 0.8f, 0.3f);

            switch (style)
            {
                case PlaceholderStyle.MinaretDome: BuildMinaret(root, stone, stoneDark, gold); break;
                case PlaceholderStyle.SculptedFacade: BuildFacade(root, stone, stoneDark); break;
                case PlaceholderStyle.MausoléeDome: BuildMausolee(root, white, brick, blue); break;
                case PlaceholderStyle.RuinsColumns: BuildRuins(root, stone, stoneDark); break;
                case PlaceholderStyle.MedinaSouk: BuildMedina(root, brick, stone, blue); break;
                case PlaceholderStyle.FortPort: BuildFort(root, stoneDark, stone, blue); break;
                default: BuildMinaret(root, stone, stoneDark, gold); break;
            }

            Palette.StripColliders(root);
            root.transform.localScale = Vector3.one * 1.6f;
            return root;
        }

        // ------------------------------------------------------------------
        private static GameObject Prim(GameObject parent, PrimitiveType type, string name,
            Vector3 pos, Vector3 scale, Color color, Quaternion? rot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.transform.localRotation = rot ?? Quaternion.identity;
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = Palette.Lit(color);
            return go;
        }

        /// <summary>Grand bloc avec un « toit » sphérique (coupole).</summary>
        private static void Dome(GameObject parent, Vector3 pos, float radius, Color color)
        {
            var d = Prim(parent, PrimitiveType.Sphere, "Coupole", pos, Vector3.one * radius * 2f, color);
            // on coupe visuellement la moitié basse en l'enfonçant
            d.transform.localScale = new Vector3(radius * 2f, radius * 1.5f, radius * 2f);
        }

        // --- Grande mosquée : carré + minaret + coupole -------------------
        private static void BuildMinaret(GameObject root, Color stone, Color stoneDark, Color gold)
        {
            Prim(root, PrimitiveType.Cube, "Enceinte", new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 2.0f), stone);
            Prim(root, PrimitiveType.Cube, "Cour", new Vector3(0f, 0.95f, 0f), new Vector3(2.2f, 0.16f, 1.6f), stoneDark);
            Dome(root, new Vector3(-0.5f, 1.15f, 0.4f), 0.42f, stone);
            Dome(root, new Vector3(0.55f, 1.10f, -0.3f), 0.34f, stone);

            // minaret massif (3 étages)
            Prim(root, PrimitiveType.Cube, "Minaret1", new Vector3(1.05f, 1.55f, 0.75f), new Vector3(0.72f, 1.4f, 0.72f), stone);
            Prim(root, PrimitiveType.Cube, "Minaret2", new Vector3(1.05f, 2.55f, 0.75f), new Vector3(0.60f, 0.7f, 0.60f), stoneDark);
            Prim(root, PrimitiveType.Cube, "Minaret3", new Vector3(1.05f, 3.15f, 0.75f), new Vector3(0.48f, 0.55f, 0.48f), stone);
            Dome(root, new Vector3(1.05f, 3.55f, 0.75f), 0.30f, gold);

            // arcades de la cour (rangée de colonnes)
            for (int i = 0; i < 5; i++)
            {
                Prim(root, PrimitiveType.Cylinder, "Col" + i,
                    new Vector3(-1.0f + i * 0.5f, 1.15f, -0.95f), new Vector3(0.10f, 0.22f, 0.10f), stoneDark);
            }
        }

        // --- Mosquée des Trois Portes : façade à trois arcs ----------------
        private static void BuildFacade(GameObject root, Color stone, Color stoneDark)
        {
            Prim(root, PrimitiveType.Cube, "Mur", new Vector3(0f, 1.1f, 0f), new Vector3(3.0f, 2.2f, 0.35f), stone);
            Prim(root, PrimitiveType.Cube, "Frieze", new Vector3(0f, 2.05f, 0.20f), new Vector3(3.1f, 0.28f, 0.10f), stoneDark);
            Prim(root, PrimitiveType.Cube, "Frieze2", new Vector3(0f, 1.70f, 0.20f), new Vector3(3.1f, 0.14f, 0.10f), stoneDark);

            // trois portes en arc (cylindre posé sur un rectangulaire)
            for (int i = 0; i < 3; i++)
            {
                float x = -0.95f + i * 0.95f;
                Prim(root, PrimitiveType.Cube, "PorteB" + i, new Vector3(x, 0.55f, 0.19f), new Vector3(0.52f, 1.1f, 0.10f), new Color(0.18f, 0.13f, 0.09f));
                Prim(root, PrimitiveType.Cylinder, "PorteH" + i, new Vector3(x, 1.10f, 0.19f), new Vector3(0.52f, 0.06f, 0.10f), new Color(0.18f, 0.13f, 0.09f), Quaternion.Euler(90f, 0f, 0f));
            }

            // petit minaret latéral
            Prim(root, PrimitiveType.Cube, "Minaret", new Vector3(1.75f, 2.6f, 0f), new Vector3(0.42f, 1.1f, 0.42f), stone);
            Dome(root, new Vector3(1.75f, 3.2f, 0f), 0.24f, stoneDark);

            Prim(root, PrimitiveType.Cube, "Sol", new Vector3(0f, -0.03f, 0.4f), new Vector3(3.4f, 0.12f, 1.4f), stoneDark);
        }

        // --- Mausolée du Barbier : enceinte + coupoles côtelées -----------
        private static void BuildMausolee(GameObject root, Color white, Color brick, Color blue)
        {
            Prim(root, PrimitiveType.Cube, "Enceinte", new Vector3(0f, 0.5f, 0f), new Vector3(2.8f, 1.0f, 2.2f), white);
            Prim(root, PrimitiveType.Cube, "Toit", new Vector3(0f, 1.06f, 0f), new Vector3(3.0f, 0.14f, 2.4f), brick);

            // grandes coupoles côtelées
            Dome(root, new Vector3(-0.55f, 1.45f, 0f), 0.62f, brick);
            Dome(root, new Vector3(0.75f, 1.32f, 0.35f), 0.44f, blue);
            Dome(root, new Vector3(0.75f, 1.30f, -0.55f), 0.38f, blue);
            Dome(root, new Vector3(-0.55f, 1.30f, 0.75f), 0.32f, white);

            // minaret
            Prim(root, PrimitiveType.Cube, "Minaret", new Vector3(1.65f, 1.7f, -0.85f), new Vector3(0.44f, 1.4f, 0.44f), white);
            Dome(root, new Vector3(1.65f, 2.45f, -0.85f), 0.26f, brick);

            // porte
            Prim(root, PrimitiveType.Cube, "Porte", new Vector3(-0.55f, 0.45f, 1.12f), new Vector3(0.6f, 0.9f, 0.08f), new Color(0.25f, 0.16f, 0.10f));
        }

        // --- Ruines antiques : colonnes + arcs ----------------------------
        private static void BuildRuins(GameObject root, Color stone, Color stoneDark)
        {
            Prim(root, PrimitiveType.Cube, "Socle", new Vector3(0f, 0.08f, 0f), new Vector3(3.4f, 0.16f, 2.4f), stoneDark);
            // gradins d'un petit théâtre
            for (int i = 0; i < 4; i++)
            {
                Prim(root, PrimitiveType.Cylinder, "Gradin" + i,
                    new Vector3(0f, 0.20f + i * 0.12f, -0.4f - i * 0.16f),
                    new Vector3(2.4f - i * 0.35f, 0.06f, 2.4f - i * 0.35f),
                    stone);
            }
            // colonnes (certaines cassées)
            float[] h = { 1.5f, 1.5f, 0.9f, 1.5f, 1.2f, 0f };
            for (int i = 0; i < 6; i++)
            {
                if (h[i] <= 0f) continue;
                float x = -1.4f + i * 0.56f;
                Prim(root, PrimitiveType.Cylinder, "Colonne" + i,
                    new Vector3(x, 0.16f + h[i] * 0.5f, 0.85f),
                    new Vector3(0.16f, h[i] * 0.5f, 0.16f), stone);
                Prim(root, PrimitiveType.Cube, "Chapiteau" + i,
                    new Vector3(x, 0.16f + h[i] + 0.06f, 0.85f),
                    new Vector3(0.28f, 0.12f, 0.28f), stoneDark);
            }
            // arc survécu
            Prim(root, PrimitiveType.Cube, "ArcG", new Vector3(-1.75f, 0.9f, 0.85f), new Vector3(0.22f, 1.8f, 0.22f), stone);
            Prim(root, PrimitiveType.Cube, "ArcD", new Vector3(1.75f, 0.9f, 0.85f), new Vector3(0.22f, 1.8f, 0.22f), stone);
            Prim(root, PrimitiveType.Cube, "ArcH", new Vector3(0f, 1.85f, 0.85f), new Vector3(3.7f, 0.22f, 0.22f), stone);
        }

        // --- Médina / souks : maisons blanches + portes en arc ------------
        private static void BuildMedina(GameObject root, Color brick, Color white, Color blue)
        {
            Prim(root, PrimitiveType.Cube, "Sol", new Vector3(0f, 0.06f, 0f), new Vector3(3.4f, 0.12f, 2.6f), new Color(0.75f, 0.68f, 0.55f));

            // rangée de maisons
            float[] w = { 1.1f, 0.9f, 1.25f, 0.85f };
            float x = -1.45f;
            for (int i = 0; i < 4; i++)
            {
                float h = 0.7f + (i % 2) * 0.35f;
                Prim(root, PrimitiveType.Cube, "Maison" + i,
                    new Vector3(x + w[i] * 0.5f, 0.12f + h * 0.5f, -0.5f),
                    new Vector3(w[i], h, 1.2f), i % 2 == 0 ? white : brick);
                // porte en arc
                Prim(root, PrimitiveType.Cube, "Porte" + i,
                    new Vector3(x + w[i] * 0.5f, 0.12f + 0.28f, 0.11f),
                    new Vector3(0.34f, 0.56f, 0.06f), blue);
                Prim(root, PrimitiveType.Cylinder, "PorteH" + i,
                    new Vector3(x + w[i] * 0.5f, 0.12f + 0.56f, 0.11f),
                    new Vector3(0.34f, 0.04f, 0.06f), blue, Quaternion.Euler(90f, 0f, 0f));
                x += w[i] + 0.08f;
            }

            // coupoles de souk
            Dome(root, new Vector3(-0.9f, 1.25f, -0.5f), 0.42f, white);
            Dome(root, new Vector3(0.45f, 1.35f, -0.5f), 0.50f, white);
            Dome(root, new Vector3(1.45f, 1.20f, -0.5f), 0.38f, brick);

            // porte monumentale de médina
            Prim(root, PrimitiveType.Cube, "Bab", new Vector3(0f, 0.85f, 0.95f), new Vector3(1.3f, 1.7f, 0.3f), brick);
            Prim(root, PrimitiveType.Cube, "BabTrou", new Vector3(0f, 0.62f, 1.12f), new Vector3(0.55f, 1.1f, 0.08f), new Color(0.16f, 0.12f, 0.09f));
            Prim(root, PrimitiveType.Cylinder, "BabArc", new Vector3(0f, 1.17f, 1.12f), new Vector3(0.55f, 0.05f, 0.08f), new Color(0.16f, 0.12f, 0.09f), Quaternion.Euler(90f, 0f, 0f));
        }

        // --- Vieux port : tour de fort + bassin + bateau ------------------
        private static void BuildFort(GameObject root, Color stoneDark, Color stone, Color blue)
        {
            // eau
            Prim(root, PrimitiveType.Cylinder, "Eau", new Vector3(0f, 0.03f, 0f), new Vector3(3.4f, 0.03f, 3.4f),
                new Color(0.10f, 0.35f, 0.55f));

            // rempart + tour ronde
            Prim(root, PrimitiveType.Cube, "Rempart", new Vector3(-0.6f, 0.5f, 0f), new Vector3(2.2f, 1.0f, 0.7f), stoneDark);
            Prim(root, PrimitiveType.Cylinder, "Tour", new Vector3(0.9f, 0.85f, 0.2f), new Vector3(1.0f, 0.85f, 1.0f), stone);
            // créneaux
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                Prim(root, PrimitiveType.Cube, "Cren" + i,
                    new Vector3(0.9f + Mathf.Cos(a) * 0.44f, 1.78f, 0.2f + Mathf.Sin(a) * 0.44f),
                    new Vector3(0.16f, 0.18f, 0.16f), stoneDark);
            }
            Prim(root, PrimitiveType.Cube, "ToitTour", new Vector3(0.9f, 1.72f, 0.2f), new Vector3(1.05f, 0.14f, 1.05f), stoneDark);

            // phare
            Prim(root, PrimitiveType.Cylinder, "Phare", new Vector3(-1.5f, 1.0f, -0.6f), new Vector3(0.34f, 1.0f, 0.34f), stone);
            Prim(root, PrimitiveType.Cylinder, "PhareTop", new Vector3(-1.5f, 2.1f, -0.6f), new Vector3(0.42f, 0.10f, 0.42f), new Color(0.85f, 0.25f, 0.2f));

            // petit bateau
            Prim(root, PrimitiveType.Cube, "Coque", new Vector3(1.3f, 0.22f, 1.2f), new Vector3(0.9f, 0.25f, 0.4f), new Color(0.85f, 0.85f, 0.8f), Quaternion.Euler(0f, -25f, 0f));
            Prim(root, PrimitiveType.Cube, "Mast", new Vector3(1.3f, 0.7f, 1.2f), new Vector3(0.05f, 0.8f, 0.05f), new Color(0.5f, 0.4f, 0.3f));
            var sail = Prim(root, PrimitiveType.Cube, "Voile", new Vector3(1.45f, 0.75f, 1.2f), new Vector3(0.02f, 0.6f, 0.5f), Color.white, Quaternion.Euler(0f, -25f, 0f));
        }
    }
}
