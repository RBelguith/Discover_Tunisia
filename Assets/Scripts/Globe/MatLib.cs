using UnityEngine;
using UnityEngine.Rendering;

namespace DiscoverTunisia.Globe
{
    /// <summary>
    /// Fabrique de matériaux URP. Privilégie les matériaux placés dans
    /// Assets/Resources/Materials (garantis inclus dans les builds),
    /// avec repli sur Shader.Find à l'exécution (éditeur).
    /// </summary>
    public static class MatLib
    {
        private static Shader _lit;
        private static Shader _unlit;

        public static Shader LitShader
        {
            get
            {
                if (_lit == null)
                {
                    var m = Resources.Load<Material>("Materials/URP_Lit");
                    _lit = m != null ? m.shader : Shader.Find("Universal Render Pipeline/Lit");
                }
                return _lit;
            }
        }

        public static Shader UnlitShader
        {
            get
            {
                if (_unlit == null)
                {
                    var m = Resources.Load<Material>("Materials/URP_Unlit");
                    _unlit = m != null ? m.shader : Shader.Find("Universal Render Pipeline/Unlit");
                }
                return _unlit;
            }
        }

        public static Material Lit(Color color)
        {
            var mat = new Material(LitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        public static Material Unlit(Color color)
        {
            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            return mat;
        }

        public static Material UnlitTexture(Texture tex)
        {
            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
            return mat;
        }

        /// <summary>Matériau transparent (halos, contours).</summary>
        public static Material Transparent(Color color, bool additive = false)
        {
            var mat = new Material(UnlitShader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            // bascule en mode transparent du shader URP Unlit
            mat.SetFloat("_Surface", 1f);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)(additive ? BlendMode.SrcAlpha : BlendMode.SrcAlpha));
            mat.SetInt("_DstBlend", (int)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
            if (additive) mat.EnableKeyword("_ALPHABLEND_ON");
            return mat;
        }

        /// <summary>Supprime les colliders de primitive (inutiles ici).</summary>
        public static void StripColliders(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                Object.Destroy(c);
        }
    }
}
