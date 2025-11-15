using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Ironsite.Core;
using Ironsite.Networking;
using System.IO;

namespace Ironsite.UI
{
    /// <summary>
    /// Handles button actions for the right controls panel
    /// Manages JSON reload, toggles, and manual commands
    /// </summary>
    public class UIControlsHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_InputField manualCommandInput;
        [SerializeField] private UITheme theme;

        [Header("Toggle References")]
        [SerializeField] private Interactable toggleOutlinesButton;
        [SerializeField] private Interactable toggleMarkersButton;
        [SerializeField] private Interactable toggleObjectsButton;
        [SerializeField] private Interactable clearHighlightsButton;
        [SerializeField] private Interactable reloadJSONButton;

        private bool outlinesVisible = true;
        private bool markersVisible = true;
        private bool objectsVisible = true;

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Reload JSON
            if (reloadJSONButton != null)
            {
                reloadJSONButton.OnClick.AddListener(OnReloadJSON);
            }

            // Toggle Outlines
            if (toggleOutlinesButton != null)
            {
                toggleOutlinesButton.OnClick.AddListener(OnToggleOutlines);
            }

            // Toggle Markers
            if (toggleMarkersButton != null)
            {
                toggleMarkersButton.OnClick.AddListener(OnToggleMarkers);
            }

            // Toggle Objects
            if (toggleObjectsButton != null)
            {
                toggleObjectsButton.OnClick.AddListener(OnToggleObjects);
            }

            // Clear Highlights
            if (clearHighlightsButton != null)
            {
                clearHighlightsButton.OnClick.AddListener(OnClearHighlights);
            }
        }

        /// <summary>
        /// Reload JSON configuration file (optional - only if JSONRoomLoader exists)
        /// </summary>
        private void OnReloadJSON()
        {
            // Try to find JSON loader if it exists (optional component)
            var jsonLoader = FindObjectOfType<MonoBehaviour>();
            if (jsonLoader != null && jsonLoader.GetType().Name == "JSONRoomLoader")
            {
                string jsonPath = Path.Combine(Application.streamingAssetsPath, "room_config.json");
                
                if (File.Exists(jsonPath))
                {
                    string jsonContent = File.ReadAllText(jsonPath);
                    Debug.Log("[UIControlsHandler] JSON reload requested - JSONRoomLoader not required for UI-only mode");
                    // JSONRoomLoader would handle this if present
                }
                else
                {
                    Debug.LogWarning($"[UIControlsHandler] JSON file not found at: {jsonPath} (UI-only mode - this is optional)");
                }
            }
            else
            {
                Debug.Log("[UIControlsHandler] Reload JSON button pressed - UI-only mode (no room generation)");
            }
        }

        /// <summary>
        /// Toggle outline visibility
        /// </summary>
        private void OnToggleOutlines()
        {
            outlinesVisible = !outlinesVisible;
            
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.SetOutlinesVisible(outlinesVisible);
            }

            UpdateToggleButton(toggleOutlinesButton, outlinesVisible);
        }

        /// <summary>
        /// Toggle marker visibility
        /// </summary>
        private void OnToggleMarkers()
        {
            markersVisible = !markersVisible;
            
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.SetMarkersVisible("marker", markersVisible);
            }

            UpdateToggleButton(toggleMarkersButton, markersVisible);
        }

        /// <summary>
        /// Toggle object visibility
        /// </summary>
        private void OnToggleObjects()
        {
            objectsVisible = !objectsVisible;
            
            // This would hide/show all registered objects
            var registry = SceneObjectRegistry.Instance;
            if (registry != null)
            {
                var allIds = registry.GetAllObjectIds();
                foreach (string id in allIds)
                {
                    var obj = registry.GetObject(id);
                    if (obj != null && !id.ToLower().Contains("marker") && !id.ToLower().Contains("outline"))
                    {
                        obj.SetActive(objectsVisible);
                    }
                }
            }

            UpdateToggleButton(toggleObjectsButton, objectsVisible);
        }

        /// <summary>
        /// Clear all highlights
        /// </summary>
        private void OnClearHighlights()
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.ClearHighlight();
            }
        }

        /// <summary>
        /// Send manual command
        /// </summary>
        public void OnSendManualCommand()
        {
            if (manualCommandInput == null || string.IsNullOrEmpty(manualCommandInput.text))
                return;

            string command = manualCommandInput.text;
            
            // Parse and execute command
            // This could send to HTTPCommandPoller or execute directly
            Debug.Log($"[UIControlsHandler] Manual command: {command}");
            
            // Example: Parse simple commands
            if (command.ToLower().StartsWith("highlight"))
            {
                string objectId = command.Substring("highlight".Length).Trim();
                if (SceneObjectRegistry.Instance != null)
                {
                    SceneObjectRegistry.Instance.HighlightObject(objectId);
                }
            }
            else if (command.ToLower().StartsWith("toggle"))
            {
                string toggleType = command.Substring("toggle".Length).Trim();
                if (toggleType.ToLower() == "outlines")
                    OnToggleOutlines();
                else if (toggleType.ToLower() == "markers")
                    OnToggleMarkers();
            }

            manualCommandInput.text = "";
        }

        private void UpdateToggleButton(Interactable button, bool isOn)
        {
            if (button != null && theme != null)
            {
                // Update button visual state
                // MRTK3 Interactable has IsToggled property
                button.IsToggled = isOn;
            }
        }
    }
}

