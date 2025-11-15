using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ironsite.Core;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;

namespace Ironsite.UI
{
    /// <summary>
    /// Builds the left hierarchy panel dynamically from registered objects
    /// Handles expandable markers and tap-to-highlight functionality
    /// </summary>
    public class UIObjectListBuilder : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private UITheme theme;

        [Header("Settings")]
        [SerializeField] private bool showMarkersExpanded = false;
        [SerializeField] private bool showOutlinesInList = true;

        private Dictionary<string, GameObject> listItemObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, bool> expandedStates = new Dictionary<string, bool>();

        private void Start()
        {
            if (listContainer == null)
            {
                Debug.LogError("[UIObjectListBuilder] List container not assigned!");
                return;
            }

            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectRegistered += OnObjectRegistered;
            }

            RefreshList();
        }

        private void OnDestroy()
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectRegistered -= OnObjectRegistered;
            }
        }

        private void OnObjectRegistered(string id, GameObject obj)
        {
            RefreshList();
        }

        /// <summary>
        /// Refresh the entire object list
        /// </summary>
        public void RefreshList()
        {
            if (listContainer == null) return;

            // Clear existing items
            foreach (var item in listItemObjects.Values)
            {
                if (item != null)
                    Destroy(item);
            }
            listItemObjects.Clear();

            // Check if registry exists (optional - UI works without it)
            if (SceneObjectRegistry.Instance == null)
            {
                // Show empty state message
                CreateEmptyStateMessage();
                return;
            }

            // Get all registered objects
            List<string> objectIds = SceneObjectRegistry.Instance.GetAllObjectIds();

            // Show empty state if no objects
            if (objectIds.Count == 0)
            {
                CreateEmptyStateMessage();
                return;
            }

            // Group by type
            Dictionary<string, List<string>> grouped = new Dictionary<string, List<string>>();
            foreach (string id in objectIds)
            {
                var metadata = SceneObjectRegistry.Instance.GetObjectMetadata(id);
                if (metadata == null) continue;

                string type = metadata.type;
                if (!grouped.ContainsKey(type))
                    grouped[type] = new List<string>();

                grouped[type].Add(id);
            }

            // Build list items
            foreach (var group in grouped)
            {
                CreateGroupHeader(group.Key, group.Value.Count);
                
                bool isExpanded = expandedStates.ContainsKey(group.Key) ? expandedStates[group.Key] : showMarkersExpanded;
                
                if (isExpanded)
                {
                    foreach (string id in group.Value)
                    {
                        CreateListItem(id);
                    }
                }
            }
        }

        private void CreateEmptyStateMessage()
        {
            GameObject emptyMsg = new GameObject("EmptyState");
            emptyMsg.transform.SetParent(listContainer, false);

            RectTransform rect = emptyMsg.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(theme != null ? theme.panelWidth - 40f : 360f, 60f);

            TextMeshProUGUI text = emptyMsg.AddComponent<TextMeshProUGUI>();
            text.text = "No objects registered.\nObjects will appear here when generated.";
            text.fontSize = theme != null ? theme.smallFontSize : 14;
            text.color = theme != null ? theme.textSecondary : new Color(0.7f, 0.7f, 0.7f, 1f);
            text.alignment = TextAlignmentOptions.Center;
        }

        private void CreateGroupHeader(string type, int count)
        {
            if (listItemPrefab == null)
            {
                // Create header manually if no prefab
                GameObject header = new GameObject($"Header_{type}");
                header.transform.SetParent(listContainer, false);

                RectTransform rect = header.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(theme != null ? theme.panelWidth - 40f : 360f, 
                                            theme != null ? theme.listItemHeight : 40f);

                TextMeshProUGUI text = header.AddComponent<TextMeshProUGUI>();
                text.text = $"{type.ToUpper()} ({count})";
                text.fontSize = theme != null ? theme.bodyFontSize : 18;
                text.color = theme != null ? theme.primaryBlue : Color.cyan;

                // Add expand/collapse button
                PressableButton button = header.AddComponent<PressableButton>();
                button.OnClicked.AddListener(() => ToggleGroup(type));
            }
        }

        private void CreateListItem(string objectId)
        {
            if (listContainer == null) return;

            var metadata = SceneObjectRegistry.Instance?.GetObjectMetadata(objectId);
            if (metadata == null) return;

            GameObject listItem;
            
            if (listItemPrefab != null)
            {
                listItem = Instantiate(listItemPrefab, listContainer);
            }
            else
            {
                // Create list item manually
                listItem = new GameObject($"Item_{objectId}");
                listItem.transform.SetParent(listContainer, false);

                RectTransform rect = listItem.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(theme != null ? theme.panelWidth - 40f : 360f, 
                                            theme != null ? theme.listItemHeight : 40f);

                // Background
                Image bg = listItem.AddComponent<Image>();
                bg.color = theme != null ? new Color(theme.panelBackground.r, theme.panelBackground.g, theme.panelBackground.b, 0.5f) : new Color(0.1f, 0.1f, 0.1f, 0.5f);

                // Text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(listItem.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.offsetMin = new Vector2(10f, 0f);
                textRect.offsetMax = new Vector2(-10f, 0f);

                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = metadata.name;
                text.fontSize = theme != null ? theme.smallFontSize : 14;
                text.color = theme != null ? theme.textPrimary : Color.white;
                text.alignment = TextAlignmentOptions.Left;

                // Icon based on type
                string icon = GetIconForType(metadata.type);
                if (!string.IsNullOrEmpty(icon))
                {
                    GameObject iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(listItem.transform, false);
                    RectTransform iconRect = iconObj.AddComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0f, 0.5f);
                    iconRect.anchorMax = new Vector2(0f, 0.5f);
                    iconRect.sizeDelta = new Vector2(30f, 30f);
                    iconRect.anchoredPosition = new Vector2(15f, 0f);

                    TextMeshProUGUI iconText = iconObj.AddComponent<TextMeshProUGUI>();
                    iconText.text = icon;
                    iconText.fontSize = 20;
                    iconText.color = theme != null ? theme.primaryBlue : Color.cyan;
                    iconText.alignment = TextAlignmentOptions.Center;
                }

                // Button component for tap-to-highlight
                PressableButton button = listItem.AddComponent<PressableButton>();
                button.OnClicked.AddListener(() => OnItemClicked(objectId));
            }

            listItemObjects[objectId] = listItem;
        }

        private void OnItemClicked(string objectId)
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.HighlightObject(objectId);
            }
        }

        private void ToggleGroup(string type)
        {
            bool currentState = expandedStates.ContainsKey(type) ? expandedStates[type] : false;
            expandedStates[type] = !currentState;
            RefreshList();
        }

        private string GetIconForType(string type)
        {
            switch (type.ToLower())
            {
                case "window": return "⊞";
                case "door": return "⊡";
                case "outlet": return "⚡";
                case "marker": return "📍";
                default: return "■";
            }
        }

        /// <summary>
        /// Toggle visibility of an object type in the list
        /// </summary>
        public void SetTypeVisible(string type, bool visible)
        {
            // This would filter the list display
            RefreshList();
        }
    }
}

