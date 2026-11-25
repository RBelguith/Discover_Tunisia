using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>A governorate marker on the globe: a bead, or a pin with a ring that pings outward when it is the chosen one.</summary>
    public sealed class MarkerView : MonoBehaviour
    {
        private const string TintedMaterial = "M_Pin";
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private Renderer bead;
        [SerializeField] private Renderer pinBody;
        [SerializeField] private GameObject pin;
        [SerializeField] private Transform head;
        [SerializeField] private Renderer ring;
        [SerializeField] private float pingsPerSecond = 0.7f;
        [SerializeField] private float ringGrowth = 2.6f;
        [SerializeField, Range(0f, 1f)] private float glow = 0.35f;

        private MaterialPropertyBlock _block;
        private Color _accent;
        private bool _selected;
        private Vector3 _ringScale;

        public Transform Anchor => _selected && head != null ? head : transform;

        public void Setup(Color accent, bool selected)
        {
            _block ??= new MaterialPropertyBlock();
            if (_ringScale == Vector3.zero) _ringScale = ring.transform.localScale;
            _accent = accent;
            _selected = selected;
            bead.gameObject.SetActive(!selected);
            pin.SetActive(selected);
            ring.gameObject.SetActive(selected);
            Tint(selected ? pinBody : bead, accent);
        }

        private void Tint(Renderer target, Color color)
        {
            var materials = target.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || materials[i].name != TintedMaterial) continue;
                target.GetPropertyBlock(_block, i);
                _block.SetColor(BaseColor, color);
                _block.SetColor(EmissionColor, color * glow);
                target.SetPropertyBlock(_block, i);
            }
        }

        private void Update()
        {
            if (!_selected) return;
            float t = Mathf.Repeat(Time.time * pingsPerSecond, 1f);
            float spread = 1f - (1f - t) * (1f - t);
            ring.transform.localScale = _ringScale * Mathf.Lerp(1f, ringGrowth, spread);
            var c = _accent;
            c.a = 0.9f * (1f - t);
            _block.Clear();
            _block.SetColor(BaseColor, c);
            ring.SetPropertyBlock(_block);
        }
    }
}
