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
        [SerializeField] private float m_MinZoom = 15f;
        [SerializeField] private float m_MaxZoom = 30000f;
        [SerializeField] private float m_MinPitch = 10f;
        [SerializeField] private float m_MaxPitch = 85f;

        private float m_CurrentZoom = 800f;
        private float m_Yaw = 0f;
        private float m_Pitch = 45f;
        private Vector3 m_FocalPoint = Vector3.zero;

        private Transform m_FollowTarget;
        private bool m_IsFollowing = false;

        private void Start()
        {
            m_Yaw = transform.eulerAngles.y;
            m_Pitch = transform.eulerAngles.x;
            UpdatePosition();
        }

        private void Update()
        {
            HandleFollowInput();

            if (m_IsFollowing && m_FollowTarget != null)
            {
                m_FocalPoint = m_FollowTarget.position;
            }
            else if (m_IsFollowing && m_FollowTarget == null)
            {
                m_IsFollowing = false;
            }

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
                // Panning manually breaks follow mode
                m_IsFollowing = false;
                m_FollowTarget = null;

                // Calculate move direction relative to camera yaw
                Vector3 forward = Quaternion.AngleAxis(m_Yaw, Vector3.up) * Vector3.forward;
                Vector3 right = Quaternion.AngleAxis(m_Yaw, Vector3.up) * Vector3.right;

                Vector3 moveDir = (forward * z + right * x).normalized;
                // Scale pan speed relative to the zoom level so movement isn't sluggish when zoomed out
                float scaledPanSpeed = m_PanSpeed * (m_CurrentZoom / 100f);
                m_FocalPoint += moveDir * scaledPanSpeed * Time.deltaTime;
            }
        }

        private void HandleFollowInput()
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                m_IsFollowing = !m_IsFollowing;
                if (m_IsFollowing)
                {
                    m_FollowTarget = FindLocalPlayerShip();
                    if (m_FollowTarget == null)
                    {
                        m_IsFollowing = false;
                        Debug.LogWarning("[RTSCamera] Follow failed: No local player owned ship found.");
                    }
                    else
                    {
                        Debug.Log($"[RTSCamera] Following local player ship: {m_FollowTarget.name}");
                    }
                }
                else
                {
                    m_FollowTarget = null;
                    Debug.Log("[RTSCamera] Follow disabled.");
                }
            }
        }

        private Transform FindLocalPlayerShip()
        {
            var ships = FindObjectsByType<ShipController>(FindObjectsInactive.Exclude);
            foreach (var ship in ships)
            {
                if (ship.IsOwner)
                {
                    return ship.transform;
                }
            }
            return null;
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll != 0)
                {
                    // Scale scroll speed relative to current zoom level to feel logarithmic/exponential
                    float zoomChange = (scroll / 120f) * m_ZoomSpeed * (m_CurrentZoom * 0.05f);
                    if (Mathf.Abs(zoomChange) < 1f)
                    {
                        zoomChange = Mathf.Sign(zoomChange) * 1f;
                    }
                    m_CurrentZoom -= zoomChange;
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

        [Header("Tactical Integration")]
        [SerializeField] private Celestial.ViewModeManager m_ViewModeManager;

        private void UpdatePosition()
        {
            if (m_ViewModeManager != null)
            {
                m_ViewModeManager.SetZoom(m_CurrentZoom);

                if (m_ViewModeManager.CurrentMode == Celestial.ViewMode.Tactical)
                {
                    var cam = m_ViewModeManager.TargetCamera ?? GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.orthographic = true;
                        cam.orthographicSize = m_CurrentZoom;
                        cam.transform.position = new Vector3(m_FocalPoint.x, m_CurrentZoom, m_FocalPoint.z);
                        cam.transform.rotation = Quaternion.Euler(90f, m_Yaw, 0f);
                    }
                    return;
                }
                else
                {
                    var cam = m_ViewModeManager.TargetCamera ?? GetComponent<Camera>();
                    if (cam != null)
                    {
                        cam.orthographic = false;
                    }
                }
            }

            // Position camera relative to focal point, yaw, pitch, and zoom
            Quaternion rotation = Quaternion.Euler(m_Pitch, m_Yaw, 0);
            Vector3 offset = rotation * Vector3.back * m_CurrentZoom;

            transform.position = m_FocalPoint + offset;
            transform.rotation = rotation;
        }
    }
}
