using UnityEngine;

namespace DiscoverTunisia.World
{
    /// <summary>A governorate marker on the globe: halo, dot and, when selected, a pulsing pin.</summary>
    public sealed class MarkerView : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [SerializeField] private Renderer glow;
        [SerializeField] private Renderer dot;
        [SerializeField] private Renderer pinHead;
        [SerializeField] private GameObject pin;
        [SerializeField] private float pulseSpeed = 3.5f;

        private MaterialPropertyBlock _block;
        private Color _accent;
        private bool _selected;
        private Vector3 _glowScale;

        public Transform Anchor => pinHead != null && _selected ? pinHead.transform : transform;

        public void Setup(Color accent, bool selected)
        {
            _block ??= new MaterialPropertyBlock();
            _accent = accent;
            _selected = selected;
            _glowScale = Vector3.one * (selected ? 1.5f : 1f);
            glow.transform.localScale = _glowScale;
            pin.SetActive(selected);
            SetColor(glow, accent * (selected ? 1f : 0.55f));
            SetColor(dot, selected ? Color.white : accent);
            SetColor(pinHead, accent);
        }

        private void SetColor(Renderer r, Color c)
        {
            _block.SetColor(BaseColor, c);
            r.SetPropertyBlock(_block);
        }

        private void Update()
        {
            if (!_selected) return;
            float wave = Mathf.Sin(Time.time * pulseSpeed);
            glow.transform.localScale = _glowScale * (1f + 0.22f * wave);
            var c = _accent;
            c.a = 0.75f + 0.25f * wave;
            SetColor(glow, c);
        }

#if UNITY_EDITOR
        public void EditorSetup(Renderer glow, Renderer dot, Renderer pinHead, GameObject pin)
        {
            this.glow = glow;
            this.dot = dot;
            this.pinHead = pinHead;
            this.pin = pin;
        }
#endif
    }
}
