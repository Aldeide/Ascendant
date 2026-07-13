using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ascendant.SystemsExtensions.Movement
{
    public class ShipController : NetworkBehaviour
    {
        [SerializeField]
        private float m_MoveSpeed = 20f;
        [SerializeField]
        private float m_TurnSpeed = 180f;

        [Header("Abilities")]
        [SerializeField]
        private AbilityDefinition m_MoveAbilityDef;

        private ShipInput m_CurrentInput;
        
        // Input tracking
        private Vector3 m_CommandTargetPos;
        private bool m_IsSettingAltitude;
        private float m_InitialMouseY;
        private Vector3 m_CurrentVisualTarget;

        private LineRenderer m_LineRenderer;
        private AbilitySystemComponent m_AbilitySystemComp;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Automatically spawn the simulation manager on the server if it is missing in the scene
            if (ShipSimulationManager.Instance == null && IsServer)
            {
                var go = new GameObject("[ShipSimulationManager]");
                go.AddComponent<ShipSimulationManager>();
                Debug.Log("[ShipController] Created missing ShipSimulationManager GameObject on server.");
            }

            if (ShipSimulationManager.Instance != null)
            {
                ShipSimulationManager.Instance.RegisterShip(this);
            }

            // Check for duplicate components to help the user debug setup issues
            var abilityComponents = GetComponents<AbilitySystemComponent>();
            if (abilityComponents.Length > 1)
            {
                Debug.LogWarning($"[ShipController] Warning: GameObject '{gameObject.name}' has {abilityComponents.Length} AbilitySystemComponents attached. Please remove duplicate AbilitySystemComponents in the Inspector.");
            }

            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
            if (m_AbilitySystemComp == null)
            {
                Debug.LogError($"[ShipController] Error: GameObject '{gameObject.name}' is missing an AbilitySystemComponent. Please attach one.");
            }

            // Grant the move ability to the ship on BOTH client and server to support prediction and host-mode
            if (m_AbilitySystemComp != null && m_MoveAbilityDef != null)
            {
                m_AbilitySystemComp.Initialise();
                m_AbilitySystemComp.AbilitySystem.AbilityManager.GrantAbility(m_MoveAbilityDef);
                Debug.Log($"[ShipController] Granted ability '{m_MoveAbilityDef.UniqueName}' on server={IsServer} client={IsClient}");
            }

            if (IsOwner)
            {
                // Create a line renderer to display target path and altitude offset
                m_LineRenderer = gameObject.AddComponent<LineRenderer>();
                m_LineRenderer.startWidth = 0.05f;
                m_LineRenderer.endWidth = 0.05f;
                m_LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                m_LineRenderer.startColor = Color.cyan;
                m_LineRenderer.endColor = Color.cyan;
                m_LineRenderer.positionCount = 3;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (ShipSimulationManager.Instance != null)
            {
                ShipSimulationManager.Instance.UnregisterShip(this);
            }
        }

        public override void OnDestroy()
        {
            if (ShipSimulationManager.Instance != null)
            {
                ShipSimulationManager.Instance.UnregisterShip(this);
            }
            base.OnDestroy();
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleInput();
                UpdateVisualPath();
            }
        }

        private void HandleInput()
        {
            if (Camera.main == null || Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Start setting destination on right-click down
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                if (plane.Raycast(ray, out float enter))
                {
                    m_CommandTargetPos = ray.GetPoint(enter);
                    m_IsSettingAltitude = true;
                    m_InitialMouseY = mousePos.y;
                    m_CurrentVisualTarget = m_CommandTargetPos;
                }
            }

            // Drag mouse vertically to set altitude (height above/below Y=0 orbital plane)
            if (Mouse.current.rightButton.isPressed && m_IsSettingAltitude)
            {
                float deltaY = mousePos.y - m_InitialMouseY;
                float altitudeOffset = deltaY * 0.1f; // Adjust sensitivity
                m_CurrentVisualTarget = m_CommandTargetPos + Vector3.up * altitudeOffset;
            }

            // Confirm movement command on button release
            if (Mouse.current.rightButton.wasReleasedThisFrame && m_IsSettingAltitude)
            {
                m_IsSettingAltitude = false;

                // Trigger movement using the Gameplay Ability System
                if (m_AbilitySystemComp != null && m_MoveAbilityDef != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    Debug.Log($"[ShipController] Requesting TryActivateAbility for '{m_MoveAbilityDef.UniqueName}' with target: {m_CurrentVisualTarget}");
                    bool success = m_AbilitySystemComp.AbilitySystem.AbilityManager.TryActivateAbility(m_MoveAbilityDef.UniqueName, new AbilityData 
                    { 
                        TargetPosition = m_CurrentVisualTarget 
                    });
                    Debug.Log($"[ShipController] TryActivateAbility result: {success}");
                }
                else
                {
                    Debug.LogWarning("[ShipController] Missing AbilitySystemComponent or MoveAbilityDef. Falling back to local movement.");
                    SetTargetFromAbility(m_CurrentVisualTarget);
                }
            }
        }

        private void UpdateVisualPath()
        {
            if (m_LineRenderer == null) return;

            // If we are currently setting altitude or have an active target
            if (m_IsSettingAltitude || m_CurrentInput.HasTarget)
            {
                Vector3 target = m_IsSettingAltitude ? m_CurrentVisualTarget : m_CurrentInput.TargetPosition;
                Vector3 targetBase = new Vector3(target.x, 0, target.z);

                m_LineRenderer.enabled = true;
                m_LineRenderer.SetPosition(0, transform.position); // Line from ship to target
                m_LineRenderer.SetPosition(1, target);             // Line to target altitude
                m_LineRenderer.SetPosition(2, targetBase);         // Vertical line to orbital plane
            }
            else
            {
                m_LineRenderer.enabled = false;
            }
        }

        public void SetTargetFromAbility(Vector3 targetPosition)
        {
            m_CurrentInput = new ShipInput
            {
                TargetPosition = targetPosition,
                HasTarget = true
            };
        }

        public void SetInputFromServer(ShipInput input)
        {
            // Sync input back to owner to turn off path rendering when target is reached
            m_CurrentInput = input;
        }

        public ShipInput GetCurrentInput()
        {
            return m_CurrentInput;
        }

        public ShipStats GetStats()
        {
            return new ShipStats
            {
                MoveSpeed = m_MoveSpeed,
                TurnSpeed = m_TurnSpeed
            };
        }
    }
}
