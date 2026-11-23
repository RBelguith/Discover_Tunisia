using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>Sizes a model by its footprint so any prefab, placeholder or real, sits the same way on a stand or a target.</summary>
    public static class ModelFitter
    {
        /// <summary>Bounds at unit scale. Measure a fresh, active instance: inactive renderers report empty bounds.</summary>
        public static Bounds Measure(GameObject model)
        {
            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            model.transform.localScale = Vector3.one;
            var bounds = new Bounds();
            bool first = true;
            foreach (var r in model.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = r.bounds; first = false; }
                else bounds.Encapsulate(r.bounds);
            }
            return bounds;
        }

        /// <summary>Parents the model and scales it so its footprint spans the given world size, base on the parent's plane.</summary>
        public static void Fit(Transform model, Bounds bounds, Transform parent, float worldFootprint)
        {
            float footprint = Mathf.Max(bounds.size.x, bounds.size.z, 0.001f);
            float scale = worldFootprint / footprint / Mathf.Max(parent.lossyScale.x, 0.0001f);
            model.SetParent(parent, false);
            model.localRotation = Quaternion.identity;
            model.localScale = Vector3.one * scale;
            model.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * scale;
        }
    }
}
