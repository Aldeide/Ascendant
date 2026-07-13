using System;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    public class ViewModeManager : MonoBehaviour
    {
        [SerializeField] private Camera m_TargetCamera;
        [SerializeField] private float m_TacticalZoomThreshold = 80f;
        [SerializeField] private float m_CurrentZoom = 15f;
        
        private ViewMode m_CurrentMode = ViewMode.CloseUp;

        public event Action<ViewMode> OnViewModeChanged;

        public Camera TargetCamera
        {
            get => m_TargetCamera;
            set => m_TargetCamera = value;
        }

        public float TacticalZoomThreshold
        {
            get => m_TacticalZoomThreshold;
            set => m_TacticalZoomThreshold = value;
        }

        public float CurrentZoom => m_CurrentZoom;
        public ViewMode CurrentMode => m_CurrentMode;

        private void Start()
        {
            if (m_TargetCamera == null)
            {
                m_TargetCamera = Camera.main;
            }
            ApplyZoomAndProjection();
        }

        public void SetZoom(float zoomValue)
        {
            m_CurrentZoom = Mathf.Max(5f, zoomValue);
            
            ViewMode targetMode = m_CurrentZoom >= m_TacticalZoomThreshold ? ViewMode.Tactical : ViewMode.CloseUp;
            if (targetMode != m_CurrentMode)
            {
                m_CurrentMode = targetMode;
                NotifyViewModeChanged();
            }
            ApplyZoomAndProjection();
        }

        private void ApplyZoomAndProjection()
        {
            if (m_TargetCamera == null) return;

            if (m_CurrentMode == ViewMode.Tactical)
            {
                m_TargetCamera.orthographic = true;
                // Scale orthographic size directly with zoom for smooth zooming
                m_TargetCamera.orthographicSize = m_CurrentZoom;
                
                // Position overhead looking straight down
                m_TargetCamera.transform.position = new Vector3(0f, m_CurrentZoom, 0f);
                m_TargetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                m_TargetCamera.orthographic = false;
                // Position at an angle for a detailed perspective close-up
                m_TargetCamera.transform.position = new Vector3(0f, m_CurrentZoom * 0.7f, -m_CurrentZoom * 0.7f);
                m_TargetCamera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
            }
        }

        public void NotifyViewModeChanged()
        {
            var icons = FindObjectsByType<TacticalIcon>(FindObjectsSortMode.None);
            foreach (var icon in icons)
            {
                icon.SetViewMode(m_CurrentMode);
            }
            OnViewModeChanged?.Invoke(m_CurrentMode);
        }
    }
}
