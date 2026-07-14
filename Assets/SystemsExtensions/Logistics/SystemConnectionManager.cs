using System;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Logistics
{
    public class SystemConnectionManager : MonoBehaviour
    {
        public static SystemConnectionManager Instance { get; private set; }

        public event Action<GameObject, string> OnShipTransitioned;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void TransitionShip(GameObject ship, string destinationSystem)
        {
            Debug.Log($"[SystemConnectionManager] Transitioning ship '{ship.name}' to system: {destinationSystem}");
            
            // Execute transition logic (saving current system structures to DB)
            SaveCurrentSystemState();

            // Invoke event so clients or tests can react
            OnShipTransitioned?.Invoke(ship, destinationSystem);
        }

        private void SaveCurrentSystemState()
        {
            // Database persistence invocation
            var dbPath = DatabaseConnectionManager.DefaultDbPath;
            try
            {
                using (var conn = DatabaseConnectionManager.CreateConnection(dbPath))
                {
                    var repo = new WorldDatabaseRepository(conn);
                    repo.CreateTables();

                    // Find all active storage hubs and persist them
                    var storageHubs = FindObjectsByType<ResourceStorageHub>(FindObjectsInactive.Exclude);
                    foreach (var hub in storageHubs)
                    {
                        var inventory = hub.GetComponent<ResourceInventory>();
                        var invState = inventory != null ? inventory.State.Value : default;
                        
                        repo.SaveStructure(
                            hub.gameObject.name,
                            "SystemAlpha",
                            "ResourceStorageHub",
                            hub.transform.position,
                            hub.transform.rotation,
                            100.0f,
                            invState
                        );
                    }

                    // Find all active asteroid miners and persist them
                    var miners = FindObjectsByType<AsteroidMiningRig>(FindObjectsInactive.Exclude);
                    foreach (var miner in miners)
                    {
                        var inventory = miner.GetComponent<ResourceInventory>();
                        var invState = inventory != null ? inventory.State.Value : default;
                        
                        repo.SaveStructure(
                            miner.gameObject.name,
                            "SystemAlpha",
                            "AsteroidMiner",
                            miner.transform.position,
                            miner.transform.rotation,
                            100.0f,
                            invState
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemConnectionManager] Error saving system state to SQLite: {ex.Message}");
            }
        }
    }
}
