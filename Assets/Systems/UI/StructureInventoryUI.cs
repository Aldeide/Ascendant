using System;
using System.Collections.Generic;
using AbilitySystem.Scripts;
using Ascendant.Systems.Inventory;
using Ascendant.Systems.Structures;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Ascendant.Systems.UI
{
    public class StructureInventoryUI : MonoBehaviour
    {
        public static StructureInventoryUI Instance { get; private set; }

        private UIDocument m_UIDocument;
        private VisualElement m_WindowPanel;
        private Label m_StructureNameLbl;
        private Label m_StatusVal;
        private VisualElement m_ConstructionProgressContainer;
        private Label m_ProgressLbl;
        private VisualElement m_ProgressBarFill;
        private Label m_HealthVal;
        private Label m_FuelVal;
        private ScrollView m_InventoryScroll;
        private VisualElement m_InventoryList;
        private Label m_CargoCapacityLbl;
        private VisualElement m_CapacityBarFill;
        private Button m_CloseBtn;

        private StructureBase m_TargetStructure;
        private NetworkInventory m_Inventory;
        private bool m_IsOpen = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            var go = new GameObject("StructureInventoryUI");
            go.AddComponent<StructureInventoryUI>();
            DontDestroyOnLoad(go);
            Debug.Log("[StructureInventoryUI] System initialized automatically on start.");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load UXML and USS from Resources
            var uxml = Resources.Load<VisualTreeAsset>("UI/StructureInventory");
            var uss = Resources.Load<StyleSheet>("UI/StructureInventory");

            if (uxml == null || uss == null)
            {
                Debug.LogError("[StructureInventoryUI] Failed to load UI resources. Verify paths in Resources/UI/.");
                return;
            }

            // Create EventSystem if it doesn't exist in the scene
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<InputSystemUIInputModule>(); // New Input System module
                Debug.Log("[StructureInventoryUI] EventSystem not found in scene. Spawned default EventSystem with InputSystemUIInputModule.");
            }

            // Set up UIDocument
            m_UIDocument = gameObject.AddComponent<UIDocument>();
            
            // Find or setup PanelSettings dynamically
            PanelSettings panelSettings = null;
            var existingSettings = Resources.FindObjectsOfTypeAll<PanelSettings>();
            foreach (var settings in existingSettings)
            {
                if (settings != null && settings.themeStyleSheet != null)
                {
                    panelSettings = settings;
                    break;
                }
            }

            if (panelSettings == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;
                panelSettings.sortingOrder = 100;

                // Try to find any active ThemeStyleSheet
                var themes = Resources.FindObjectsOfTypeAll<ThemeStyleSheet>();
                if (themes.Length > 0)
                {
                    panelSettings.themeStyleSheet = themes[0];
                }
                else
                {
#if UNITY_EDITOR
                    var defaultThemePath = "Packages/com.unity.ui.builder/DefaultTheme.tss";
                    var loadedTheme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(defaultThemePath);
                    if (loadedTheme != null)
                    {
                        panelSettings.themeStyleSheet = loadedTheme;
                    }
#endif
                }
            }
            m_UIDocument.panelSettings = panelSettings;

            m_UIDocument.visualTreeAsset = uxml;

            // Apply Stylesheet
            var root = m_UIDocument.rootVisualElement;
            root.styleSheets.Add(uss);

            // Bind Elements
            m_WindowPanel = root.Q<VisualElement>("window-panel");
            m_StructureNameLbl = root.Q<Label>("structure-name-lbl");
            m_StatusVal = root.Q<Label>("status-val");
            m_ConstructionProgressContainer = root.Q<VisualElement>("construction-progress-container");
            m_ProgressLbl = root.Q<Label>("progress-lbl");
            m_ProgressBarFill = root.Q<VisualElement>("progress-bar-fill");
            m_HealthVal = root.Q<Label>("health-val");
            m_FuelVal = root.Q<Label>("fuel-val");
            m_InventoryScroll = root.Q<ScrollView>("inventory-scroll");
            m_InventoryList = root.Q<VisualElement>("inventory-list");
            m_CargoCapacityLbl = root.Q<Label>("cargo-capacity-lbl");
            m_CapacityBarFill = root.Q<VisualElement>("capacity-bar-fill");
            m_CloseBtn = root.Q<Button>("close-btn");

            if (m_CloseBtn != null)
            {
                m_CloseBtn.clicked += CloseInventory;
            }

            // Start closed
            CloseInventory();
        }

        private void OnDestroy()
        {
            if (m_Inventory != null)
            {
                m_Inventory.Items.OnListChanged -= OnInventoryChanged;
            }
        }

        private void Update()
        {
            if (m_IsOpen)
            {
                if (m_TargetStructure == null || m_TargetStructure.gameObject == null)
                {
                    CloseInventory();
                    return;
                }

                // Check distance to player ship
                var playerShip = FindLocalPlayerShip();
                if (playerShip == null)
                {
                    CloseInventory();
                    return;
                }

                float dist = Vector3.Distance(playerShip.position, m_TargetStructure.transform.position);
                if (dist > 1000f)
                {
                    Debug.Log($"[StructureInventoryUI] Closing UI: out of range ({dist:F1}m)");
                    CloseInventory();
                    return;
                }

                // Dynamic values refresh
                RefreshStatus();
            }

            // Click detection to open
            HandleClickDetection();
        }

        private void HandleClickDetection()
        {
            if (Mouse.current == null || Camera.main == null) return;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Prevent raycasting if clicking on UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var structure = hit.collider.GetComponentInParent<StructureBase>();
                    if (structure != null)
                    {
                        var playerShip = FindLocalPlayerShip();
                        if (playerShip != null)
                        {
                            float dist = Vector3.Distance(playerShip.position, structure.transform.position);
                            float interactionRange = 1000f;

                            if (dist <= interactionRange)
                            {
                                ShowInventory(structure);
                            }
                            else
                            {
                                Debug.Log($"[StructureInventoryUI] Cannot inspect: Structure is too far ({dist:F1}m). Range: {interactionRange}m");
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[StructureInventoryUI] Cannot inspect structure: No player ship found.");
                        }
                    }
                }
            }
        }

        public void ShowInventory(StructureBase structure)
        {
            if (m_Inventory != null)
            {
                m_Inventory.Items.OnListChanged -= OnInventoryChanged;
            }

            m_TargetStructure = structure;
            m_Inventory = structure.GetComponent<NetworkInventory>();

            if (m_Inventory != null)
            {
                m_Inventory.Items.OnListChanged += OnInventoryChanged;
            }

            m_StructureNameLbl.text = structure.gameObject.name.ToUpper();
            
            // Show window panel
            m_WindowPanel.style.display = DisplayStyle.Flex;
            m_IsOpen = true;

            // Populate initial inventory and statuses
            RefreshInventoryList();
            RefreshStatus();
            
            Debug.Log($"[StructureInventoryUI] Displaying inventory for {structure.gameObject.name}.");
        }

        public void CloseInventory()
        {
            if (m_Inventory != null)
            {
                m_Inventory.Items.OnListChanged -= OnInventoryChanged;
                m_Inventory = null;
            }

            m_TargetStructure = null;
            m_IsOpen = false;

            if (m_WindowPanel != null)
            {
                m_WindowPanel.style.display = DisplayStyle.None;
            }
        }

        private void OnInventoryChanged(NetworkListEvent<InventoryItem> changeEvent)
        {
            RefreshInventoryList();
        }

        private void RefreshStatus()
        {
            if (m_TargetStructure == null) return;

            // Health & Fuel from AbilitySystemComponent
            float hp = 100f;
            float maxHp = 100f;
            float fuel = 0f;
            float maxFuel = 500f;

            var abilitySystemComp = m_TargetStructure.GetComponent<AbilitySystemComponent>();
            if (abilitySystemComp != null && abilitySystemComp.AbilitySystem != null)
            {
                var hpAttr = abilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("StructureHealth");
                var maxHpAttr = abilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("StructureMaxHealth");
                var fuelAttr = abilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("UpkeepFuel");
                var maxFuelAttr = abilitySystemComp.AbilitySystem.AttributeSetManager.GetAttribute("MaxUpkeepFuel");

                if (hpAttr != null) hp = hpAttr.CurrentValue;
                if (maxHpAttr != null) maxHp = maxHpAttr.CurrentValue;
                if (fuelAttr != null) fuel = fuelAttr.CurrentValue;
                if (maxFuelAttr != null) maxFuel = maxFuelAttr.CurrentValue;
            }

            m_HealthVal.text = $"{hp:F0} / {maxHp:F0} HP";
            m_FuelVal.text = $"{fuel:F1} / {maxFuel:F0}";

            // Construction Progress
            if (m_TargetStructure.IsUnderConstruction.Value)
            {
                m_StatusVal.text = "UNDER CONSTRUCTION";
                m_StatusVal.RemoveFromClassList("status-active");
                m_StatusVal.RemoveFromClassList("status-disabled");
                m_StatusVal.AddToClassList("status-under-construction");
                
                m_ConstructionProgressContainer.RemoveFromClassList("hidden");
                float progressVal = m_TargetStructure.ConstructionProgress.Value;
                m_ProgressLbl.text = $"Construction Progress: {progressVal * 100f:F1}%";
                m_ProgressBarFill.style.width = new Length(progressVal * 100f, LengthUnit.Percent);
            }
            else if (m_TargetStructure.IsDisabled.Value)
            {
                m_StatusVal.text = "DISABLED";
                m_StatusVal.RemoveFromClassList("status-active");
                m_StatusVal.RemoveFromClassList("status-under-construction");
                m_StatusVal.AddToClassList("status-disabled");
                m_ConstructionProgressContainer.AddToClassList("hidden");
            }
            else
            {
                m_StatusVal.text = "ACTIVE";
                m_StatusVal.RemoveFromClassList("status-disabled");
                m_StatusVal.RemoveFromClassList("status-under-construction");
                m_StatusVal.AddToClassList("status-active");
                m_ConstructionProgressContainer.AddToClassList("hidden");
            }
        }

        private void RefreshInventoryList()
        {
            m_InventoryList.Clear();

            if (m_Inventory == null || m_Inventory.Items.Count == 0)
            {
                var placeholder = new VisualElement();
                placeholder.AddToClassList("inventory-item-placeholder");
                var label = new Label("No cargo detected");
                label.AddToClassList("placeholder-text");
                placeholder.Add(label);
                m_InventoryList.Add(placeholder);

                m_CargoCapacityLbl.text = $"Capacity: 0 / {(m_Inventory != null ? m_Inventory.MaxCapacity : 1000)} t";
                m_CapacityBarFill.style.width = new Length(0, LengthUnit.Percent);
                return;
            }

            int totalAmount = 0;
            int maxCap = m_Inventory.MaxCapacity;

            for (int i = 0; i < m_Inventory.Items.Count; i++)
            {
                var item = m_Inventory.Items[i];
                totalAmount += item.Quantity;

                var row = new VisualElement();
                row.AddToClassList("item-row");

                var nameContainer = new VisualElement();
                nameContainer.AddToClassList("item-name-container");

                var indicator = new VisualElement();
                indicator.AddToClassList("item-icon-indicator");
                
                // Color code resources based on name
                string itemIdStr = item.ItemId.ToString().ToLower();
                if (itemIdStr.Contains("ore")) indicator.AddToClassList("icon-ore");
                else if (itemIdStr.Contains("gas")) indicator.AddToClassList("icon-gas");
                else if (itemIdStr.Contains("fuel")) indicator.AddToClassList("icon-fuel");
                else if (itemIdStr.Contains("munition")) indicator.AddToClassList("icon-munitions");
                else if (itemIdStr.Contains("component")) indicator.AddToClassList("icon-components");
                else indicator.AddToClassList("icon-default");

                nameContainer.Add(indicator);

                var nameLbl = new Label(item.ItemId.ToString());
                nameLbl.AddToClassList("item-name");
                nameContainer.Add(nameLbl);

                row.Add(nameContainer);

                var qtyLbl = new Label(item.Quantity.ToString());
                qtyLbl.AddToClassList("item-qty");
                row.Add(qtyLbl);

                m_InventoryList.Add(row);
            }

            m_CargoCapacityLbl.text = $"Capacity: {totalAmount} / {maxCap} t";
            float percentage = maxCap > 0 ? ((float)totalAmount / maxCap) * 100f : 0f;
            m_CapacityBarFill.style.width = new Length(Mathf.Min(100f, percentage), LengthUnit.Percent);
        }

        private Transform FindLocalPlayerShip()
        {
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude);
            foreach (var mono in allMonoBehaviours)
            {
                if (mono != null && mono.GetType().Name == "ShipController")
                {
                    var isOwnerProp = mono.GetType().GetProperty("IsOwner");
                    if (isOwnerProp != null && (bool)isOwnerProp.GetValue(mono))
                    {
                        return mono.transform;
                    }
                }
            }
            return null;
        }
    }
}
