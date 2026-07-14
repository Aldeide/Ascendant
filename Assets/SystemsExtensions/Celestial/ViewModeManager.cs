using System;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    public class ViewModeManager : MonoBehaviour
    {
        [SerializeField] private Camera m_TargetCamera;
        [SerializeField] private float m_TacticalZoomThreshold = 1800f;
        [SerializeField] private float m_CurrentZoom = 600f;
        
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

        [SerializeField] private Color m_TacticalBackgroundColor = new Color(0.02f, 0.02f, 0.05f, 1f);
        
        private CameraClearFlags m_OriginalClearFlags = CameraClearFlags.Skybox;
        private Color m_OriginalBackgroundColor = Color.black;
        private bool m_HasCapturedCameraState = false;

        private void ApplyZoomAndProjection()
        {
            if (m_TargetCamera == null) return;

            if (m_CurrentMode == ViewMode.Tactical)
            {
                if (!m_HasCapturedCameraState)
                {
                    m_OriginalClearFlags = m_TargetCamera.clearFlags;
                    m_OriginalBackgroundColor = m_TargetCamera.backgroundColor;
                    m_HasCapturedCameraState = true;
                }

                m_TargetCamera.clearFlags = CameraClearFlags.SolidColor;
                m_TargetCamera.backgroundColor = m_TacticalBackgroundColor;

                m_TargetCamera.orthographic = true;
                m_TargetCamera.orthographicSize = m_CurrentZoom;
                
                m_TargetCamera.transform.position = new Vector3(0f, m_CurrentZoom, 0f);
                m_TargetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                if (m_HasCapturedCameraState)
                {
                    m_TargetCamera.clearFlags = m_OriginalClearFlags;
                    m_TargetCamera.backgroundColor = m_OriginalBackgroundColor;
                    m_HasCapturedCameraState = false;
                }

                m_TargetCamera.orthographic = false;
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

            var paths = FindObjectsByType<OrbitPath>(FindObjectsSortMode.None);
            foreach (var path in paths)
            {
                path.SetViewMode(m_CurrentMode);
            }

            OnViewModeChanged?.Invoke(m_CurrentMode);
        }
    }
}
