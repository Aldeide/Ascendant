using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Ascendant.Systems.Inventory;
using Ascendant.Systems.Structures;

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

        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
                
                // Handle case where server is already active on Start
                if (NetworkManager.Singleton.IsServer)
                {
                    OnServerStarted();
                }
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }
            // Auto-persist current states when shutting down or transitioning
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                SaveCurrentSystemState();
            }
        }

        private void OnApplicationQuit()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                SaveCurrentSystemState();
            }
        }

        private void OnServerStarted()
        {
            LoadSystemState();
        }

        public void TransitionShip(GameObject ship, string destinationSystem)
        {
            Debug.Log($"[SystemConnectionManager] Transitioning ship '{ship.name}' to system: {destinationSystem}");
            
            // Execute transition logic (saving current system structures to DB)
            SaveCurrentSystemState();

            // Invoke event so clients or tests can react
            OnShipTransitioned?.Invoke(ship, destinationSystem);
        }

        public void SaveCurrentSystemState()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

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

                    // Find all active StructureBase structures and persist them
                    var structures = FindObjectsByType<StructureBase>(FindObjectsInactive.Exclude);
                    foreach (var s in structures)
                    {
                        var inventory = s.GetComponent<NetworkInventory>();
                        ResourceInventoryState invState = default;
                        if (inventory != null)
                        {
                            invState.Ore = inventory.GetAmount("Ore");
                            invState.Gas = inventory.GetAmount("Gas");
                            invState.Fuel = inventory.GetAmount("Fuel");
                            invState.Munitions = inventory.GetAmount("Munitions");
                            invState.Components = inventory.GetAmount("Components");
                        }
                        
                        string typeStr = "AsteroidMiner";
                        if (s is GaseousFuelScoop) typeStr = "GaseousFuelScoop";
                        else if (s is Refinery) typeStr = "Refinery";
                        else if (s is MunitionsFactory) typeStr = "MunitionsFactory";

                        repo.SaveStructure(
                            s.gameObject.name,
                            "SystemAlpha",
                            typeStr,
                            s.transform.position,
                            s.transform.rotation,
                            100.0f,
                            invState
                        );
                    }
                }
                Debug.Log("[SystemConnectionManager] Successfully saved system state to database.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemConnectionManager] Error saving system state to SQLite: {ex.Message}");
            }
        }

        public void LoadSystemState()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

            Debug.Log("[SystemConnectionManager] Loading system state from database...");
            var dbPath = DatabaseConnectionManager.DefaultDbPath;
            try
            {
                using (var conn = DatabaseConnectionManager.CreateConnection(dbPath))
                {
                    var repo = new WorldDatabaseRepository(conn);
                    repo.CreateTables();

                    var savedStructures = repo.LoadStructures("SystemAlpha");
                    Debug.Log($"[SystemConnectionManager] Found {savedStructures.Count} saved structures in database.");

                    // Clean up existing StructureBase structures to avoid duplication
                    var existingStructures = FindObjectsByType<StructureBase>(FindObjectsInactive.Exclude);
                    foreach (var s in existingStructures)
                    {
                        var netObj = s.GetComponent<NetworkObject>();
                        if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
                        else Destroy(s.gameObject);
                    }

                    foreach (var data in savedStructures)
                    {
                        SpawnStructureFromDatabase(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SystemConnectionManager] Error loading system state: {ex.Message}");
            }
        }

        private void SpawnStructureFromDatabase(SavedStructureData data)
        {
            var rigObj = new GameObject(data.StructureId);
            rigObj.transform.position = data.Position;
            rigObj.transform.rotation = data.Rotation;

            var netObj = rigObj.AddComponent<NetworkObject>();
            var inventory = rigObj.AddComponent<NetworkInventory>();
            inventory.MaxCapacity = 500;

            StructureBase structure = null;
            GameObject visual = null;
            Color primaryColor = Color.gray;

            if (data.Type == "AsteroidMiner")
            {
                var rig = rigObj.AddComponent<AsteroidMiningRig>();
                rig.ExtractionAmount = 10;
                rig.ExtractionInterval = 1.0f;
                structure = rig;
                primaryColor = new Color(0.9f, 0.6f, 0.1f); // Orange

                // Load custom visual FBX
                var minerPrefab = Resources.Load<GameObject>("Models/alpha_asteroid_miner");
                if (minerPrefab != null)
                {
                    visual = Instantiate(minerPrefab);
                    visual.name = "Visual";
                    visual.transform.SetParent(rigObj.transform);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.transform.localScale = new Vector3(15f, 15f, 15f);
                }
            }
            else if (data.Type == "GaseousFuelScoop")
            {
                var scoop = rigObj.AddComponent<GaseousFuelScoop>();
                scoop.HarvestAmount = 5;
                scoop.HarvestInterval = 2.0f;
                structure = scoop;
                primaryColor = new Color(0.9f, 0.9f, 0.1f); // Yellow
            }
            else if (data.Type == "Refinery")
            {
                var refinery = rigObj.AddComponent<Refinery>();
                structure = refinery;
                primaryColor = new Color(0.1f, 0.6f, 0.9f); // Cyan/Blue
            }
            else if (data.Type == "MunitionsFactory")
            {
                var factory = rigObj.AddComponent<MunitionsFactory>();
                structure = factory;
                primaryColor = new Color(0.9f, 0.1f, 0.1f); // Red
            }

            // Fallback visual cylinder
            if (visual == null)
            {
                visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                visual.name = "Visual";
                visual.transform.SetParent(rigObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localScale = new Vector3(25f, 40f, 25f);
            }

            var renderers = visual.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = primaryColor;
                    r.sharedMaterial = material;
                }
            }

            var collider = visual.GetComponentInChildren<Collider>();
            if (collider == null)
            {
                var capCollider = visual.AddComponent<CapsuleCollider>();
                capCollider.radius = 1.5f;
                capCollider.height = 4.0f;
                capCollider.direction = 1;
                collider = capCollider;
            }
            collider.isTrigger = true;

            // Loaded structures start fully constructed (100% progress) and active
            if (structure != null)
            {
                structure.IsUnderConstruction.Value = false;
                structure.ConstructionProgress.Value = 1.0f;
                structure.IsDisabled.Value = false;
            }

            // Set resources in inventory
            inventory.AddResource("Ore", data.Inventory.Ore);
            inventory.AddResource("Gas", data.Inventory.Gas);
            inventory.AddResource("Fuel", data.Inventory.Fuel);
            inventory.AddResource("Munitions", data.Inventory.Munitions);
            inventory.AddResource("Components", data.Inventory.Components);

            netObj.Spawn();
            Debug.Log($"[SystemConnectionManager] Spawned saved structure '{data.StructureId}' of type '{data.Type}' at {data.Position}");
        }
    }
}
