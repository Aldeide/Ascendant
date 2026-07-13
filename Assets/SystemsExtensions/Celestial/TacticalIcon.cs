using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    public class TacticalIcon : MonoBehaviour
    {
        [SerializeField] private Color m_TacticalColor = Color.white;
        [SerializeField] private float m_IconScaleMultiplier = 1.2f;

        private GameObject m_3DVisual;
        private GameObject m_TacticalVisual;

        public GameObject Visual3D => m_3DVisual;
        public GameObject VisualTactical => m_TacticalVisual;

        public Color TacticalColor
        {
            get => m_TacticalColor;
            set => m_TacticalColor = value;
        }

        public float IconScaleMultiplier
        {
            get => m_IconScaleMultiplier;
            set => m_IconScaleMultiplier = value;
        }

        private void Awake()
        {
            InitializeVisuals();
        }

        public void InitializeVisuals()
        {
            // Try to find the 3D visual child
            var t3D = transform.Find("Visual");
            if (t3D != null)
            {
                m_3DVisual = t3D.gameObject;
            }

            // Create flat billboard tactical representation if not already created
            var tTactical = transform.Find("TacticalVisual");
            if (tTactical != null)
            {
                m_TacticalVisual = tTactical.gameObject;
            }
            else
            {
                // Create a flat quad/disc billboard representing the tactical icon
                m_TacticalVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
                m_TacticalVisual.name = "TacticalVisual";
                m_TacticalVisual.transform.SetParent(transform);
                m_TacticalVisual.transform.localPosition = Vector3.zero;
                m_TacticalVisual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // Face top-down camera
                
                // Scale it slightly larger than body radius to make it pop visually
                var body = GetComponent<CelestialBody>();
                float radius = body != null ? body.Radius : 1f;
                m_TacticalVisual.transform.localScale = Vector3.one * (radius * 2f * m_IconScaleMultiplier);

                // Apply unlit colored material for tactical view
                var renderer = m_TacticalVisual.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Unlit/Color"));
                    material.color = m_TacticalColor;
                    renderer.sharedMaterial = material;
                }

                // Remove collider to prevent physics overlap
                var collider = m_TacticalVisual.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }
                
                // Hide it by default until we switch to Tactical View
                m_TacticalVisual.SetActive(false);
            }
        }

        public void SetViewMode(ViewMode mode)
        {
            if (m_3DVisual == null || m_TacticalVisual == null)
            {
                InitializeVisuals();
            }

            if (mode == ViewMode.Tactical)
            {
                if (m_3DVisual != null) m_3DVisual.SetActive(false);
                if (m_TacticalVisual != null) m_TacticalVisual.SetActive(true);
            }
            else
            {
                if (m_3DVisual != null) m_3DVisual.SetActive(true);
                if (m_TacticalVisual != null) m_TacticalVisual.SetActive(false);
            }
        }
    }
}
