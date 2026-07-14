using AbilitySystem.Runtime.Abilities;
using AbilitySystem.Scripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Ascendant.SystemsExtensions.Celestial;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.SystemsExtensions.Movement
{
    public class ShipController : NetworkBehaviour
    {
        [SerializeField]
        private float m_MoveSpeed = 15000f; // Cruising speed multiplied by 100 for fast testing!
        [SerializeField]
        private float m_TurnSpeed = 3600f;

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

        // Build System
        private bool m_ShowBuildMenu = false;
        private bool m_IsPlacingStructure = false;
        private Vector3 m_PlacementPosition;
        private GameObject m_PlacementPreview;
        private float m_BuildRange = 1000f; // Max distance from player ship to build

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
            if (m_AbilitySystemComp != null)
            {
                m_AbilitySystemComp.Initialise();

                // Dynamically register ShipAttributeSet if not already present
                if (m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttributeSet("ShipAttributeSet") == null)
                {
                    var attributeSet = new ShipAttributeSet(m_AbilitySystemComp.AbilitySystem);
                    m_AbilitySystemComp.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(ShipAttributeSet), attributeSet);
                    Debug.Log("[ShipController] Dynamically registered ShipAttributeSet.");
                }

                if (m_MoveAbilityDef != null)
                {
                    m_AbilitySystemComp.AbilitySystem.AbilityManager.GrantAbility(m_MoveAbilityDef);
                    Debug.Log($"[ShipController] Granted ability '{m_MoveAbilityDef.UniqueName}' on server={IsServer} client={IsClient}");
                }
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

            // Toggle build menu with 'B'
            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                m_ShowBuildMenu = !m_ShowBuildMenu;
                if (m_IsPlacingStructure)
                {
                    CancelPlacement();
                }
            }

            if (m_IsPlacingStructure)
            {
                // Update placement position based on mouse raycast on plane parallel to Y=0 passing through the ship
                Plane plane = new Plane(Vector3.up, transform.position);
                Ray ray = Camera.main.ScreenPointToRay(mousePos);
                if (plane.Raycast(ray, out float enter))
                {
                    m_PlacementPosition = ray.GetPoint(enter);
                    if (m_PlacementPreview != null)
                    {
                        m_PlacementPreview.transform.position = m_PlacementPosition;
                    }
                }

                // Update holo color based on placement validity
                if (m_PlacementPreview != null)
                {
                    var inventories = FindObjectsByType<ResourceInventory>(FindObjectsInactive.Exclude);
                    float distToAsteroid = float.MaxValue;
                    foreach (var inv in inventories)
                    {
                        if (inv.gameObject != gameObject &&
                            inv.GetComponent<ShipController>() == null && 
                            inv.GetComponent<AsteroidMiningRig>() == null &&
                            (inv.gameObject.name.Contains("Asteroid") || inv.gameObject.name.Contains("Planet")))
                        {
                            float d = Vector3.Distance(m_PlacementPosition, inv.transform.position);
                            if (d < distToAsteroid)
                            {
                                distToAsteroid = d;
                            }
                        }
                    }

                    float distToShip = Vector3.Distance(transform.position, m_PlacementPosition);
                    bool isValid = (distToShip <= m_BuildRange) && (distToAsteroid <= 1000f);

                    var renderer = m_PlacementPreview.GetComponent<MeshRenderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        renderer.material.color = isValid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
                    }
                }

                // Cancel placement with Escape
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CancelPlacement();
                }

                // Place structure with Left Click
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    TryPlaceStructure();
                }
                
                // Block normal movement input while placing structures
                return;
            }

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
                Vector3 target = m_IsSettingAltitude ? m_CurrentVisualTarget : m_CurrentInput.TargetCoordinate.ToWorldPosition();
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
                TargetCoordinate = new GridCoordinate(targetPosition),
                HasTarget = true,
                Velocity = m_CurrentInput.Velocity
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
            float speed = m_MoveSpeed;
            if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
            {
                var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MoveSpeed");
                if (attr != null)
                {
                    speed = attr.CurrentValue;
                }
            }
            return new ShipStats
            {
                MoveSpeed = speed,
                TurnSpeed = m_TurnSpeed
            };
        }

        private void StartPlacement()
        {
            m_ShowBuildMenu = false;
            m_IsPlacingStructure = true;
            
            // Create local holographic placement preview (smaller cylinder)
            m_PlacementPreview = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            m_PlacementPreview.name = "PlacementPreview";
            m_PlacementPreview.transform.localScale = new Vector3(25f, 40f, 25f);
            
            var collider = m_PlacementPreview.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);

            var renderer = m_PlacementPreview.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Sprites/Default"));
                material.color = new Color(0f, 1f, 0f, 0.4f); // Transparent green
                renderer.sharedMaterial = material;
            }
        }

        private void CancelPlacement()
        {
            m_IsPlacingStructure = false;
            if (m_PlacementPreview != null)
            {
                Destroy(m_PlacementPreview);
                m_PlacementPreview = null;
            }
        }

        private void TryPlaceStructure()
        {
            var inventories = FindObjectsByType<ResourceInventory>(FindObjectsInactive.Exclude);
            float distToAsteroid = float.MaxValue;
            ResourceInventory nearestAsteroid = null;
            foreach (var inv in inventories)
            {
                if (inv.gameObject != gameObject &&
                    inv.GetComponent<ShipController>() == null && 
                    inv.GetComponent<AsteroidMiningRig>() == null &&
                    (inv.gameObject.name.Contains("Asteroid") || inv.gameObject.name.Contains("Planet")))
                {
                    float d = Vector3.Distance(m_PlacementPosition, inv.transform.position);
                    if (d < distToAsteroid)
                    {
                        distToAsteroid = d;
                        nearestAsteroid = inv;
                    }
                }
            }

            float distToShip = Vector3.Distance(transform.position, m_PlacementPosition);
            
            if (distToShip > m_BuildRange)
            {
                Debug.LogWarning("[ShipController] Cannot build: Too far from player ship!");
            }
            else if (nearestAsteroid == null || distToAsteroid > 1000f)
            {
                Debug.LogWarning("[ShipController] Cannot build: Must place near an asteroid (within 1km)!");
            }
            else
            {
                PlaceStructureServerRpc(m_PlacementPosition);
                CancelPlacement();
            }
        }

        [ServerRpc]
        private void PlaceStructureServerRpc(Vector3 position)
        {
            var rigObj = new GameObject("AsteroidMiner");
            rigObj.transform.position = position;
            
            var netObj = rigObj.AddComponent<NetworkObject>();
            
            var inventory = rigObj.AddComponent<ResourceInventory>();
            inventory.MaxCapacity = 500;
            
            var rig = rigObj.AddComponent<AsteroidMiningRig>();
            rig.ExtractionAmount = 10;
            rig.ExtractionInterval = 1.0f; // Fast mining for quick testing!
            
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(rigObj.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(25f, 40f, 25f);
            
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.9f, 0.6f, 0.1f); // Mining orange
                renderer.sharedMaterial = material;
            }
            
            var collider = visual.GetComponent<Collider>();
            if (collider != null) DestroyImmediate(collider);

            netObj.Spawn();
            Debug.Log($"[ShipController] Server successfully spawned Asteroid Miner at {position}");
        }

        private void OnGUI()
        {
            if (!IsOwner) return;

            // Draw build instructions during placement
            if (m_IsPlacingStructure)
            {
                GUI.Box(new Rect(10, 10, 320, 75), "");
                GUILayout.BeginArea(new Rect(20, 15, 300, 60));
                GUILayout.Label("<color=yellow><b>BUILD MODE: PLACING ASTEROID MINER</b></color>", new GUIStyle { richText = true, fontSize = 14 });
                GUILayout.Label("Left Click near an Asteroid to place structure.");
                GUILayout.Label("Press <b>ESC</b> to cancel.", new GUIStyle { richText = true });
                GUILayout.EndArea();
            }

            // Draw build menu
            if (m_ShowBuildMenu)
            {
                GUI.Box(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), "SPACE CONSTRUCTION BUILD MENU");
                
                GUILayout.BeginArea(new Rect(Screen.width / 2 - 130, Screen.height / 2 - 60, 260, 150));
                
                GUILayout.Space(10);
                GUILayout.Label("Select a structure to construct:", new GUIStyle { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } });
                GUILayout.Space(10);

                if (GUILayout.Button("Asteroid Miner (Cylinder Rig)\nExtracts Ore over time", GUILayout.Height(50)))
                {
                    StartPlacement();
                }

                GUILayout.Space(10);
                if (GUILayout.Button("Close Menu", GUILayout.Height(30)))
                {
                    m_ShowBuildMenu = false;
                }

                GUILayout.EndArea();
            }
        }
    }
}
