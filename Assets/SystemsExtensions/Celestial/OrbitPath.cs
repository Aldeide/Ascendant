using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    [RequireComponent(typeof(LineRenderer))]
    public class OrbitPath : MonoBehaviour
    {
        [SerializeField] private float m_Width = 0.5f;
        [SerializeField] private Color m_PathColor = new Color(0f, 0.6f, 1f, 0.4f);

        private LineRenderer m_LineRenderer;

        public LineRenderer LineRenderer => m_LineRenderer;

        public float Width
        {
            get => m_Width;
            set => m_Width = value;
        }

        public Color PathColor
        {
            get => m_PathColor;
            set => m_PathColor = value;
        }

        public void InitializePath(float orbitRadius)
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            
            // Set up LineRenderer styling
            m_LineRenderer.useWorldSpace = false; // Local space coordinates relative to parent!
            m_LineRenderer.startWidth = m_Width;
            m_LineRenderer.endWidth = m_Width;
            m_LineRenderer.loop = true;
            
            // Material
            m_LineRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            m_LineRenderer.startColor = m_PathColor;
            m_LineRenderer.endColor = m_PathColor;

            // Generate circular path vertices centered around parent origin (-transform.localPosition)
            Vector3 center = -transform.localPosition;
            int segments = 64;
            m_LineRenderer.positionCount = segments + 1;
            float angleStep = 360f / segments;
            for (int i = 0; i <= segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                Vector3 point = center + new Vector3(Mathf.Cos(rad) * orbitRadius, 0f, Mathf.Sin(rad) * orbitRadius);
                m_LineRenderer.SetPosition(i, point);
            }

            // Start in standard CloseUp state (hidden by default)
            SetViewMode(ViewMode.CloseUp);
        }

        public void SetViewMode(ViewMode mode)
        {
            if (m_LineRenderer == null) return;

            if (mode == ViewMode.Tactical)
            {
                m_LineRenderer.enabled = true;
                m_LineRenderer.startColor = m_PathColor * 1.5f;
                m_LineRenderer.endColor = m_PathColor * 1.5f;
            }
            else
            {
                m_LineRenderer.enabled = false;
            }
        }
    }
}
