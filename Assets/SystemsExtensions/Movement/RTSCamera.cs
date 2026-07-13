using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant.SystemsExtensions.Movement
{
    public class RTSCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float m_PanSpeed = 20f;
        [SerializeField] private float m_ZoomSpeed = 10f;
        [SerializeField] private float m_OrbitSpeed = 3f;

        [Header("Limits")]
        [SerializeField] private float m_MinZoom = 5f;
        [SerializeField] private float m_MaxZoom = 100f;
        [SerializeField] private float m_MinPitch = 10f;
        [SerializeField] private float m_MaxPitch = 85f;

        private float m_CurrentZoom = 30f;
        private float m_Yaw = 0f;
        private float m_Pitch = 45f;
        private Vector3 m_FocalPoint = Vector3.zero;

        private void Start()
        {
            m_Yaw = transform.eulerAngles.y;
            m_Pitch = transform.eulerAngles.x;
            UpdatePosition();
        }

        private void Update()
        {
            HandlePan();
            HandleZoom();
            HandleOrbit();
            UpdatePosition();
        }

        private void HandlePan()
        {
            float x = 0f;
            float z = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z = 1f;
                else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z = -1f;

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
                else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
            }

            if (x != 0 || z != 0)
            {
                // Calculate move direction relative to camera yaw
                Vector3 forward = Quaternion.AngleAxis(m_Yaw, Vector3.up) * Vector3.forward;
                Vector3 right = Quaternion.AngleAxis(m_Yaw, Vector3.up) * Vector3.right;

                Vector3 moveDir = (forward * z + right * x).normalized;
                m_FocalPoint += moveDir * m_PanSpeed * Time.deltaTime;
            }
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    // Scroll delta is usually around 120 per notch, normalize it to match previous behavior
                    m_CurrentZoom -= (scroll / 120f) * m_ZoomSpeed;
                    m_CurrentZoom = Mathf.Clamp(m_CurrentZoom, m_MinZoom, m_MaxZoom);
                }
            }
        }

        private void HandleOrbit()
        {
            // Orbit holding Middle Mouse Button
            if (Mouse.current != null && Mouse.current.middleButton.isPressed)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                m_Yaw += mouseDelta.x * m_OrbitSpeed * 0.1f;
                m_Pitch -= mouseDelta.y * m_OrbitSpeed * 0.1f;
                m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            }
        }

        private void UpdatePosition()
        {
            // Position camera relative to focal point, yaw, pitch, and zoom
            Quaternion rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0);
            Vector3 offset = rotation * Vector3.back * m_CurrentZoom;

            transform.position = m_FocalPoint + offset;
            transform.rotation = rotation;
        }
    }
}
