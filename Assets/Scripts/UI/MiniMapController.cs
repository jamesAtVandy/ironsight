using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ironsite.Core;
using Ironsite.UI;
using System.Collections.Generic;

namespace Ironsite.UI
{
    /// <summary>
    /// Manages the top-right minimap/compass overlay
    /// Shows room top-down view with object positions and compass orientation
    /// </summary>
    public class MiniMapController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private RectTransform compassContainer;
        [SerializeField] private Image minimapBackground;
        [SerializeField] private GameObject objectMarkerPrefab;

        [Header("Settings")]
        [SerializeField] private float minimapSize = 200f;
        [SerializeField] private float compassSize = 100f;
        [SerializeField] private float markerBlinkSpeed = 2f;

        private Dictionary<string, GameObject> minimapMarkers = new Dictionary<string, GameObject>();
        private string highlightedObjectId = null;
        private Vector2 roomDimensions = Vector2.zero;
        private Vector3 roomCenter = Vector3.zero;

        private void Start()
        {
            InitializeMinimap();
            InitializeCompass();
        }

        private void Update()
        {
            UpdateCompass();
            UpdateHighlightedMarker();
        }

        private void InitializeMinimap()
        {
            if (minimapContainer == null) return;

            minimapContainer.sizeDelta = new Vector2(minimapSize, minimapSize);

            // Create background grid
            if (minimapBackground != null)
            {
                minimapBackground.color = new Color(0.05f, 0.05f, 0.1f, 0.8f);
            }

            // Subscribe to object registration
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectRegistered += OnObjectRegistered;
            }
        }

        private void InitializeCompass()
        {
            if (compassContainer == null) return;

            compassContainer.sizeDelta = new Vector2(compassSize, compassSize);

            // Create compass labels (N, E, S, W)
            CreateCompassLabels();
        }

        private void CreateCompassLabels()
        {
            if (compassContainer == null) return;

            string[] directions = { "N", "E", "S", "W" };
            float[] angles = { 0f, 90f, 180f, 270f };
            float radius = compassSize * 0.4f;

            for (int i = 0; i < directions.Length; i++)
            {
                GameObject label = new GameObject($"Label_{directions[i]}");
                label.transform.SetParent(compassContainer, false);

                RectTransform rect = label.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(30f, 30f);

                float angleRad = angles[i] * Mathf.Deg2Rad;
                rect.anchoredPosition = new Vector2(
                    Mathf.Sin(angleRad) * radius,
                    Mathf.Cos(angleRad) * radius
                );

                TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
                text.text = directions[i];
                text.fontSize = 16;
                text.color = new Color(0f, 0.6f, 1f, 1f);
                text.alignment = TextAlignmentOptions.Center;
            }
        }

        private void OnObjectRegistered(string id, GameObject obj)
        {
            if (obj == null || minimapContainer == null) return;

            // Create minimap marker
            GameObject marker;
            if (objectMarkerPrefab != null)
            {
                marker = Instantiate(objectMarkerPrefab, minimapContainer);
            }
            else
            {
                marker = CreateMarkerObject(id);
            }

            if (marker != null)
            {
                minimapMarkers[id] = marker;
                UpdateMarkerPosition(id, obj.transform.position);
            }
        }

        private GameObject CreateMarkerObject(string id)
        {
            GameObject marker = new GameObject($"Marker_{id}");
            marker.transform.SetParent(minimapContainer, false);

            RectTransform rect = marker.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(8f, 8f);

            Image img = marker.AddComponent<Image>();
            img.color = new Color(0f, 0.6f, 1f, 0.8f);

            // Add icon based on type
            var metadata = SceneObjectRegistry.Instance?.GetObjectMetadata(id);
            if (metadata != null)
            {
                GameObject icon = new GameObject("Icon");
                icon.transform.SetParent(marker.transform, false);
                RectTransform iconRect = icon.AddComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.sizeDelta = Vector2.zero;

                TextMeshProUGUI iconText = icon.AddComponent<TextMeshProUGUI>();
                iconText.text = GetIconForType(metadata.type);
                iconText.fontSize = 12;
                iconText.color = Color.white;
                iconText.alignment = TextAlignmentOptions.Center;
            }

            return marker;
        }

        /// <summary>
        /// Set room dimensions for minimap scaling
        /// </summary>
        public void SetRoomDimensions(Vector2 dimensions, Vector3 center)
        {
            roomDimensions = dimensions;
            roomCenter = center;
        }

        private void UpdateMarkerPosition(string id, Vector3 worldPosition)
        {
            if (!minimapMarkers.ContainsKey(id) || minimapContainer == null) return;
            if (roomDimensions.x == 0f || roomDimensions.y == 0f) return;

            GameObject marker = minimapMarkers[id];
            RectTransform rect = marker.GetComponent<RectTransform>();
            if (rect == null) return;

            // Convert world position to minimap coordinates
            Vector3 relativePos = worldPosition - roomCenter;
            float xPercent = (relativePos.x / roomDimensions.x) + 0.5f;
            float zPercent = (relativePos.z / roomDimensions.y) + 0.5f;

            // Clamp to minimap bounds
            xPercent = Mathf.Clamp01(xPercent);
            zPercent = Mathf.Clamp01(zPercent);

            // Set anchored position
            rect.anchorMin = new Vector2(xPercent, zPercent);
            rect.anchorMax = new Vector2(xPercent, zPercent);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Highlight an object on the minimap
        /// </summary>
        public void HighlightObject(string objectId)
        {
            highlightedObjectId = objectId;

            // Update all markers
            foreach (var kvp in minimapMarkers)
            {
                if (kvp.Value != null)
                {
                    Image img = kvp.Value.GetComponent<Image>();
                    if (img != null)
                    {
                        if (kvp.Key == objectId)
                        {
                            // Use theme highlight green if available
                            UITheme theme = Resources.Load<UITheme>("UITheme");
                            Color highlightColor = theme != null ? theme.highlightGreen : new Color(0f, 1f, 0.5f, 1f);
                            img.color = highlightColor; // Green highlight
                        }
                        else
                        {
                            img.color = new Color(0f, 0.6f, 1f, 0.8f); // Default blue
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear highlight
        /// </summary>
        public void ClearHighlight()
        {
            highlightedObjectId = null;

            foreach (var marker in minimapMarkers.Values)
            {
                if (marker != null)
                {
                    Image img = marker.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = new Color(0f, 0.6f, 1f, 0.8f);
                    }
                }
            }
        }

        private void UpdateHighlightedMarker()
        {
            if (string.IsNullOrEmpty(highlightedObjectId) || !minimapMarkers.ContainsKey(highlightedObjectId))
                return;

            GameObject marker = minimapMarkers[highlightedObjectId];
            if (marker == null) return;

            // Blinking effect
            float alpha = Mathf.Sin(Time.time * markerBlinkSpeed) * 0.3f + 0.7f;
            Image img = marker.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                img.color = new Color(c.r, c.g, c.b, alpha);
            }
        }

        private void UpdateCompass()
        {
            if (compassContainer == null || Camera.main == null) return;

            // Rotate compass to match camera orientation
            float cameraYaw = Camera.main.transform.eulerAngles.y;
            compassContainer.localRotation = Quaternion.Euler(0f, 0f, -cameraYaw);
        }

        private string GetIconForType(string type)
        {
            switch (type.ToLower())
            {
                case "window": return "⊞";
                case "door": return "⊡";
                case "outlet": return "⚡";
                case "marker": return "📍";
                default: return "•";
            }
        }
    }
}

