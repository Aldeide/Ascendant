using System;
using AbilitySystem.Scripts;
using Ascendant.Systems.Inventory;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.Systems.Structures
{
    [RequireComponent(typeof(AbilitySystemComponent))]
    [RequireComponent(typeof(NetworkInventory))]
    public class StructureBase : NetworkBehaviour
    {
        [Header("Upkeep & Durability Settings")]
        [SerializeField] protected float m_UpkeepBurnRate = 1.0f; // Fuel units consumed per second
        [SerializeField] protected float m_DecayRate = 5.0f;       // HP lost per second when fuel is depleted
        [SerializeField] protected int m_ConstructionComponentCost = 200;

        protected AbilitySystemComponent m_AbilitySystemComp;
        protected StructureAttributeSet m_AttributeSet;
        protected NetworkInventory m_Inventory;

        public readonly NetworkVariable<bool> IsDisabled = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public readonly NetworkVariable<bool> IsUnderConstruction = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public readonly NetworkVariable<float> ConstructionProgress = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private float m_ConstructionTimer;
        private const float CONSTRUCTION_TICK_RATE = 1.0f; // 1 tick per second

        protected virtual void Awake()
        {
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
            m_Inventory = GetComponent<NetworkInventory>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Dynamically register StructureAttributeSet if not already present
            if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
            {
                if (m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttributeSet("StructureAttributeSet") == null)
                {
                    m_AttributeSet = new StructureAttributeSet(m_AbilitySystemComp.AbilitySystem);
                    m_AbilitySystemComp.AbilitySystem.AttributeSetManager.AddAttributeSet(typeof(StructureAttributeSet), m_AttributeSet);
                }
                else
                {
                    m_AttributeSet = (StructureAttributeSet)m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttributeSet("StructureAttributeSet");
                }
            }

            IsDisabled.OnValueChanged += OnDisabledStateChanged;
            OnDisabledStateChanged(false, IsDisabled.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            IsDisabled.OnValueChanged -= OnDisabledStateChanged;
        }

        protected virtual void Update()
        {
            if (!IsServer) return;

            if (IsUnderConstruction.Value)
            {
                UpdateConstruction(Time.deltaTime);
                return;
            }

            // Only tick upkeep and decay if structures have target and are not disabled
            if (IsDisabled.Value) return;

            UpdateUpkeepAndDecay(Time.deltaTime);
        }

        protected virtual void UpdateConstruction(float deltaTime)
        {
            m_ConstructionTimer += deltaTime;
            if (m_ConstructionTimer < CONSTRUCTION_TICK_RATE) return;
            m_ConstructionTimer -= CONSTRUCTION_TICK_RATE;

            // Search for builder ship in range
            var builder = FindNearestBuilderShip();
            if (builder == null)
            {
                Debug.Log($"[StructureBase] Construction of {gameObject.name} is halted: No builder ship in range.");
                return;
            }

            // Support both new NetworkInventory and old ResourceInventory via reflection to avoid circular assembly reference
            var netInventory = builder.GetComponent<NetworkInventory>();
            var oldInventoryComponent = builder.GetComponent("ResourceInventory");

            int componentsPerTick = 5;
            bool consumed = false;

            if (netInventory != null && netInventory.GetAmount("Components") >= componentsPerTick)
            {
                netInventory.RemoveResource("Components", componentsPerTick);
                consumed = true;
            }
            else if (oldInventoryComponent != null && GetOldInventoryAmount(oldInventoryComponent, "Components") >= componentsPerTick)
            {
                consumed = RemoveOldInventoryResource(oldInventoryComponent, "Components", componentsPerTick);
            }

            if (consumed)
            {
                float progressStep = (float)componentsPerTick / m_ConstructionComponentCost;
                ConstructionProgress.Value = Mathf.Min(1.0f, ConstructionProgress.Value + progressStep);
                Debug.Log($"[StructureBase] Construction of {gameObject.name} progressed to {ConstructionProgress.Value * 100f:F1}% (consumed {componentsPerTick} Components).");

                if (ConstructionProgress.Value >= 1.0f)
                {
                    FinishConstruction();
                }
            }
            else
            {
                Debug.LogWarning($"[StructureBase] Construction of {gameObject.name} is halted: Nearest ship lacks Components.");
            }
        }

        private int GetOldInventoryAmount(Component oldInventory, string resourceName)
        {
            if (oldInventory == null) return 0;
            try
            {
                int resourceTypeEnumVal = resourceName switch
                {
                    "Ore" => 0,
                    "Gas" => 1,
                    "Fuel" => 2,
                    "Munitions" => 3,
                    "Components" => 4,
                    _ => -1
                };
                if (resourceTypeEnumVal == -1) return 0;

                var getAmountMethod = oldInventory.GetType().GetMethod("GetAmount");
                if (getAmountMethod != null)
                {
                    var enumType = oldInventory.GetType().Assembly.GetType("Ascendant.SystemsExtensions.Logistics.ResourceType");
                    if (enumType != null)
                    {
                        var enumVal = Enum.ToObject(enumType, resourceTypeEnumVal);
                        return (int)getAmountMethod.Invoke(oldInventory, new object[] { enumVal });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StructureBase] Failed to invoke GetAmount on ResourceInventory via reflection: {ex.Message}");
            }
            return 0;
        }

        private bool RemoveOldInventoryResource(Component oldInventory, string resourceName, int amount)
        {
            if (oldInventory == null) return false;
            try
            {
                int resourceTypeEnumVal = resourceName switch
                {
                    "Ore" => 0,
                    "Gas" => 1,
                    "Fuel" => 2,
                    "Munitions" => 3,
                    "Components" => 4,
                    _ => -1
                };
                if (resourceTypeEnumVal == -1) return false;

                var removeResourceMethod = oldInventory.GetType().GetMethod("RemoveResource");
                if (removeResourceMethod != null)
                {
                    var enumType = oldInventory.GetType().Assembly.GetType("Ascendant.SystemsExtensions.Logistics.ResourceType");
                    if (enumType != null)
                    {
                        var enumVal = Enum.ToObject(enumType, resourceTypeEnumVal);
                        return (bool)removeResourceMethod.Invoke(oldInventory, new object[] { enumVal, amount });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StructureBase] Failed to invoke RemoveResource on ResourceInventory via reflection: {ex.Message}");
            }
            return false;
        }

        private GameObject FindNearestBuilderShip()
        {
            // Dynamically scan active MonoBehaviours to locate ShipController to avoid circular dependency
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
            float nearestDist = float.MaxValue;
            GameObject nearestShip = null;
            float constructionRange = 1000f; // 1km range

            foreach (var mono in allMonoBehaviours)
            {
                if (mono != null && mono.GetType().Name == "ShipController")
                {
                    float dist = Vector3.Distance(transform.position, mono.transform.position);
                    if (dist < nearestDist && dist <= constructionRange)
                    {
                        nearestDist = dist;
                        nearestShip = mono.gameObject;
                    }
                }
            }
            return nearestShip;
        }

        protected virtual void FinishConstruction()
        {
            IsUnderConstruction.Value = false;
            Debug.Log($"[StructureBase] Construction of {gameObject.name} is complete!");

            if (m_AttributeSet != null)
            {
                float maxHealth = m_AttributeSet.StructureMaxHealth.CurrentValue;
                m_AttributeSet.StructureHealth.SetBaseValue(maxHealth);
                m_AttributeSet.StructureHealth.SetCurrentValue(maxHealth);

                float initialFuel = m_AttributeSet.MaxUpkeepFuel.CurrentValue * 0.2f;
                m_AttributeSet.UpkeepFuel.SetBaseValue(initialFuel);
                m_AttributeSet.UpkeepFuel.SetCurrentValue(initialFuel);
            }
        }

        protected virtual void UpdateUpkeepAndDecay(float deltaTime)
        {
            if (m_AttributeSet == null) return;

            float currentFuel = m_AttributeSet.UpkeepFuel.CurrentValue;

            if (currentFuel > 0f)
            {
                // Consume fuel
                float newFuel = Mathf.Max(0f, currentFuel - m_UpkeepBurnRate * deltaTime);
                m_AttributeSet.UpkeepFuel.SetBaseValue(newFuel);
                m_AttributeSet.UpkeepFuel.SetCurrentValue(newFuel);
            }
            else
            {
                // Fuel is depleted: apply structural integrity decay
                float currentHealth = m_AttributeSet.StructureHealth.CurrentValue;
                float newHealth = Mathf.Max(0f, currentHealth - m_DecayRate * deltaTime);
                m_AttributeSet.StructureHealth.SetBaseValue(newHealth);
                m_AttributeSet.StructureHealth.SetCurrentValue(newHealth);

                if (newHealth <= 0f)
                {
                    EnterDisabledState();
                }
            }
        }

        protected virtual void EnterDisabledState()
        {
            if (!IsServer) return;
            IsDisabled.Value = true;
        }

        protected virtual void OnDisabledStateChanged(bool oldVal, bool newVal)
        {
            if (newVal)
            {
                OnDisabled();
            }
            else
            {
                OnReactivated();
            }
        }

        protected virtual void OnDisabled()
        {
            Debug.Log($"[StructureBase] {gameObject.name} is now DISABLED due to upkeep depletion.");
        }

        protected virtual void OnReactivated()
        {
            Debug.Log($"[StructureBase] {gameObject.name} has been REACTIVATED.");
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReactivateStructureServerRpc(NetworkObjectReference senderInventoryRef)
        {
            if (!IsServer) return;
            if (!IsDisabled.Value) return;

            if (senderInventoryRef.TryGet(out NetworkObject senderObject))
            {
                var senderInventory = senderObject.GetComponent<NetworkInventory>();
                int cost = Mathf.CeilToInt(m_ConstructionComponentCost * 0.5f);

                if (senderInventory != null && senderInventory.GetAmount("Components") >= cost)
                {
                    // Consume resources
                    senderInventory.RemoveResource("Components", cost);

                    // Reactivate
                    ReactivateStructure();
                }
                else
                {
                    Debug.LogWarning($"[StructureBase] Reactivation failed: Sender inventory does not contain {cost} Components.");
                }
            }
        }

        protected virtual void ReactivateStructure()
        {
            if (!IsServer) return;

            IsDisabled.Value = false;

            if (m_AttributeSet != null)
            {
                float maxHealth = m_AttributeSet.StructureMaxHealth.CurrentValue;
                m_AttributeSet.StructureHealth.SetBaseValue(maxHealth);
                m_AttributeSet.StructureHealth.SetCurrentValue(maxHealth);

                float initialFuel = m_AttributeSet.MaxUpkeepFuel.CurrentValue * 0.1f;
                m_AttributeSet.UpkeepFuel.SetBaseValue(initialFuel);
                m_AttributeSet.UpkeepFuel.SetCurrentValue(initialFuel);
            }
        }
    }
}
