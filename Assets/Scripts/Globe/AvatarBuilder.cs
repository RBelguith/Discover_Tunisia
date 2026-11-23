using UnityEngine;
using DiscoverTunisia.Core;

namespace DiscoverTunisia.Globe
{
    /// <summary>
    /// F05 — Avatar 3D procédural portant une tenue traditionnelle aux couleurs
    /// du gouvernorat sélectionné (placeholder stylisé, remplaçable par un vrai modèle).
    /// </summary>
    public static class AvatarBuilder
    {
        public static GameObject Create(GovernorateInfo gov, Transform parent)
        {
            var root = new GameObject("Avatar_" + gov.id);
            if (parent != null) root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;

            Color dress = gov.accentColor;
            Color dressDark = Color.Lerp(dress, Color.black, 0.35f);
            Color skin = new Color(0.94f, 0.78f, 0.62f);
            Color hair = new Color(0.16f, 0.10f, 0.07f);
            Color gold = new Color(1.0f, 0.82f, 0.35f);

            // --- jupe / robe (cone) -------------------------------------------------
            var robe = Prim(root, PrimitiveType.Cylinder, "Robe", new Vector3(0f, 0.62f, 0f), new Vector3(0.62f, 0.62f, 0.62f));
            Taper(robe, 0.55f, 1.15f);           // évasée vers le bas
            Apply(robe, MatLib.Lit(dress));

            // ceinture dorée
            var belt = Prim(root, PrimitiveType.Cylinder, "Ceinture", new Vector3(0f, 1.02f, 0f), new Vector3(0.40f, 0.045f, 0.40f));
            Apply(belt, MatLib.Lit(gold));

            // --- buste ----------------------------------------------------------------
            var torso = Prim(root, PrimitiveType.Cylinder, "Buste", new Vector3(0f, 1.28f, 0f), new Vector3(0.34f, 0.24f, 0.30f));
            Apply(torso, MatLib.Lit(dressDark));

            // --- épaules / châle ------------------------------------------------------
            var shawl = Prim(root, PrimitiveType.Sphere, "Chale", new Vector3(0f, 1.46f, 0f), new Vector3(0.52f, 0.20f, 0.40f));
            Apply(shawl, MatLib.Lit(dress));

            // --- bras ------------------------------------------------------------------
            var armL = Prim(root, PrimitiveType.Cylinder, "BrasG", new Vector3(-0.42f, 1.20f, 0f), new Vector3(0.11f, 0.26f, 0.11f));
            armL.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            Apply(armL, MatLib.Lit(dressDark));
            var armR = Prim(root, PrimitiveType.Cylinder, "BrasD", new Vector3(0.42f, 1.20f, 0f), new Vector3(0.11f, 0.26f, 0.11f));
            armR.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
            Apply(armR, MatLib.Lit(dressDark));

            var handL = Prim(root, PrimitiveType.Sphere, "MainG", new Vector3(-0.50f, 0.94f, 0f), new Vector3(0.13f, 0.13f, 0.13f));
            Apply(handL, MatLib.Lit(skin));
            var handR = Prim(root, PrimitiveType.Sphere, "MainD", new Vector3(0.50f, 0.94f, 0f), new Vector3(0.13f, 0.13f, 0.13f));
            Apply(handR, MatLib.Lit(skin));

            // --- cou + tête -----------------------------------------------------------
            var neck = Prim(root, PrimitiveType.Cylinder, "Cou", new Vector3(0f, 1.62f, 0f), new Vector3(0.12f, 0.07f, 0.12f));
            Apply(neck, MatLib.Lit(skin));

            var head = Prim(root, PrimitiveType.Sphere, "Tete", new Vector3(0f, 1.86f, 0f), new Vector3(0.40f, 0.44f, 0.40f));
            Apply(head, MatLib.Lit(skin));

            // cheveux (demi-sphère)
            var hairGo = Prim(root, PrimitiveType.Sphere, "Cheveux", new Vector3(0f, 1.94f, -0.03f), new Vector3(0.44f, 0.42f, 0.44f));
            Apply(hairGo, MatLib.Lit(hair));
            hairGo.transform.localPosition += new Vector3(0f, 0.05f, 0f);

            // voile / chéchia
            var hat = Prim(root, PrimitiveType.Cylinder, "Chechia", new Vector3(0f, 2.14f, 0f), new Vector3(0.24f, 0.06f, 0.24f));
            Apply(hat, MatLib.Lit(new Color(0.72f, 0.12f, 0.15f)));

            // bijoux (collier)
            var necklace = Prim(root, PrimitiveType.Sphere, "Collier", new Vector3(0f, 1.60f, 0.13f), new Vector3(0.16f, 0.10f, 0.08f));
            Apply(necklace, MatLib.Lit(gold));

            // --- socle -----------------------------------------------------------------
            var base1 = Prim(root, PrimitiveType.Cylinder, "Socle", new Vector3(0f, 0.03f, 0f), new Vector3(0.75f, 0.03f, 0.75f));
            Apply(base1, MatLib.Lit(new Color(0.10f, 0.18f, 0.30f)));

            root.transform.localScale = Vector3.one * 1.05f;
            MatLib.StripColliders(root);
            return root;
        }

        // ------------------------------------------------------------------
        private static GameObject Prim(GameObject parent, PrimitiveType type, string name,
            Vector3 localPos, Vector3 localScale)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            return go;
        }

        private static void Apply(GameObject go, Material mat)
        {
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        /// <summary>Évase un cylindre : échelle X/Z interpolée en hauteur.</summary>
        private static void Taper(GameObject cylinder, float topScale, float bottomScale)
        {
            var mf = cylinder.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;
            var mesh = UnityEngine.Object.Instantiate(mf.sharedMesh);
            var verts = mesh.vertices;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in verts) { minY = Mathf.Min(minY, v.y); maxY = Mathf.Max(maxY, v.y); }
            for (int i = 0; i < verts.Length; i++)
            {
                float t = Mathf.InverseLerp(minY, maxY, verts[i].y);
                float s = Mathf.Lerp(bottomScale, topScale, t);
                verts[i] = new Vector3(verts[i].x * s, verts[i].y, verts[i].z * s);
            }
            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mf.sharedMesh = mesh;
        }
    }
}
