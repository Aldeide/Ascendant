using AbilitySystem.Scripts;
using Unity.Netcode;
using UnityEngine;
using Ascendant.SystemsExtensions.Movement;

namespace Ascendant.SystemsExtensions.Logistics
{
    [RequireComponent(typeof(ResourceInventory))]
    public class AsteroidMiningRig : NetworkBehaviour
    {
        [SerializeField] private float m_ExtractionInterval = 2.0f;
        [SerializeField] private int m_ExtractionAmount = 5;
        [SerializeField] private float m_MiningRange = 1000f;

        private ResourceInventory m_Inventory;
        private AbilitySystemComponent m_AbilitySystemComp;
        private float m_Timer;
        private ResourceInventory m_TargetAsteroidInventory;

        public float ExtractionInterval
        {
            get => m_ExtractionInterval;
            set => m_ExtractionInterval = value;
        }

        public int ExtractionAmount
        {
            get => m_ExtractionAmount;
            set => m_ExtractionAmount = value;
        }

        private void Awake()
        {
            m_Inventory = GetComponent<ResourceInventory>();
            m_AbilitySystemComp = GetComponent<AbilitySystemComponent>();
        }

        private void Start()
        {
            if (NetworkManager.Singleton != null && !IsServer) return;
            FindNearestAsteroid();
        }

        private void Update()
        {
            // Only simulate extraction on the server
            if (NetworkManager.Singleton != null && !IsServer) return;

            m_Timer += Time.deltaTime;
            if (m_Timer >= m_ExtractionInterval)
            {
                m_Timer -= m_ExtractionInterval;
                ExtractOre();
            }
        }

        private void FindNearestAsteroid()
        {
            var inventories = FindObjectsByType<ResourceInventory>(FindObjectsInactive.Exclude);
            float nearestDist = float.MaxValue;
            ResourceInventory nearest = null;

            foreach (var inv in inventories)
            {
                // Verify it's an asteroid, not a ship or another rig
                if (inv.gameObject != gameObject &&
                    inv.GetComponent<ShipController>() == null &&
                    inv.GetComponent<AsteroidMiningRig>() == null &&
                    (inv.gameObject.name.Contains("Asteroid") || inv.gameObject.name.Contains("Planet")))
                {
                    float dist = Vector3.Distance(transform.position, inv.transform.position);
                    if (dist < nearestDist && dist <= m_MiningRange)
                    {
                        nearestDist = dist;
                        nearest = inv;
                    }
                }
            }
            m_TargetAsteroidInventory = nearest;
        }

        private void ExtractOre()
        {
            if (m_TargetAsteroidInventory == null)
            {
                FindNearestAsteroid();
            }

            if (m_Inventory != null && m_TargetAsteroidInventory != null)
            {
                float multiplier = 1.0f;
                // Scale extraction amount dynamically based on MiningSpeed attribute
                if (m_AbilitySystemComp != null && m_AbilitySystemComp.AbilitySystem != null)
                {
                    var attr = m_AbilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MiningSpeed");
                    if (attr != null)
                    {
                        multiplier = attr.CurrentValue;
                    }
                }
                int wanted = Mathf.RoundToInt(m_ExtractionAmount * multiplier);

                int available = m_TargetAsteroidInventory.GetAmount(ResourceType.Ore);
                int toExtract = Mathf.Min(wanted, available);

                if (toExtract > 0)
                {
                    if (m_Inventory.CanAdd(ResourceType.Ore, toExtract))
                    {
                        m_TargetAsteroidInventory.RemoveResource(ResourceType.Ore, toExtract);
                        m_Inventory.AddResource(ResourceType.Ore, toExtract);
                        Debug.Log($"[AsteroidMiningRig] Extracted {toExtract} ore from {m_TargetAsteroidInventory.gameObject.name}. Rig inventory: {m_Inventory.GetAmount(ResourceType.Ore)}");
                    }
                }
            }
        }
    }
}
