using UnityEngine;
using TMPro;
using Ironsite.Core;

namespace Ironsite.UI
{
    /// <summary>
    /// Spawns and manages floating tags above highlighted objects
    /// Implements billboard behavior and color matching
    /// </summary>
    public class FloatingTagController : MonoBehaviour
    {
        [Header("Tag Prefab")]
        [SerializeField] private GameObject tagPrefab;

        [Header("Settings")]
        [SerializeField] private float tagOffset = 0.3f;
        [SerializeField] private bool billboardToCamera = true;
        [SerializeField] private float animationSpeed = 2f;

        private GameObject currentTag;
        private Transform targetTransform;
        private Camera mainCamera;
        private UITheme theme;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();

            theme = Resources.Load<UITheme>("UITheme");
        }

        private void Update()
        {
            if (currentTag != null && targetTransform != null)
            {
                // Position tag above object
                Vector3 worldPos = targetTransform.position + Vector3.up * tagOffset;
                currentTag.transform.position = worldPos;

                // Billboard to camera
                if (billboardToCamera && mainCamera != null)
                {
                    currentTag.transform.LookAt(mainCamera.transform);
                    currentTag.transform.Rotate(0f, 180f, 0f); // Flip to face camera
                }

                // Pulsing glow effect with green highlight color
                if (theme != null)
                {
                    float pulse = Mathf.Sin(Time.time * animationSpeed) * 0.5f + 0.5f;
                    // Use green highlight color for highlighted objects
                    Color glowColor = Color.Lerp(theme.highlightGreen, theme.primaryBlue, pulse * 0.5f);
                    
                    // Apply to tag materials/text
                    var text = currentTag.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.color = glowColor;
                    }
                }
            }
        }

        /// <summary>
        /// Show tag above object
        /// </summary>
        public void ShowTag(Transform target, string objectId)
        {
            HideTag();

            targetTransform = target;

            if (tagPrefab != null)
            {
                currentTag = Instantiate(tagPrefab, transform);
            }
            else
            {
                // Create tag manually
                currentTag = CreateTagObject(objectId);
            }

            if (currentTag != null)
            {
                currentTag.SetActive(true);
                
                // Get metadata for display
                var metadata = SceneObjectRegistry.Instance?.GetObjectMetadata(objectId);
                if (metadata != null)
                {
                    UpdateTagText(metadata.name, metadata.type);
                }
            }
        }

        /// <summary>
        /// Hide current tag
        /// </summary>
        public void HideTag()
        {
            if (currentTag != null)
            {
                Destroy(currentTag);
                currentTag = null;
            }
            targetTransform = null;
        }

        private GameObject CreateTagObject(string objectId)
        {
            GameObject tag = new GameObject($"Tag_{objectId}");
            tag.transform.SetParent(transform);

            // Background panel
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(tag.transform, false);
            
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(200f, 60f);

            // Add canvas for world space UI
            Canvas canvas = tag.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;

            CanvasScaler scaler = tag.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10f;

            // Background image
            UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
            if (theme != null)
            {
                bg.color = new Color(theme.darkBackground.r, theme.darkBackground.g, theme.darkBackground.b, 0.9f);
            }
            else
            {
                bg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            }

            // Outline
            GameObject outline = new GameObject("Outline");
            outline.transform.SetParent(panel.transform, false);
            RectTransform outlineRect = outline.AddComponent<RectTransform>();
            outlineRect.anchorMin = Vector2.zero;
            outlineRect.anchorMax = Vector2.one;
            outlineRect.sizeDelta = Vector2.zero;

            UnityEngine.UI.Image outlineImg = outline.AddComponent<UnityEngine.UI.Image>();
            outlineImg.color = theme != null ? theme.outlineColor : new Color(0f, 0.6f, 1f, 0.8f);
            outlineImg.type = UnityEngine.UI.Image.Type.Sliced;

            // Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(10f, 10f);
            textRect.offsetMax = new Vector2(-10f, -10f);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = objectId;
            text.fontSize = theme != null ? theme.bodyFontSize : 18;
            text.color = theme != null ? theme.textPrimary : Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return tag;
        }

        private void UpdateTagText(string name, string type)
        {
            if (currentTag == null) return;

            TextMeshProUGUI text = currentTag.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{name}\n<size=12>{type}</size>";
            }
        }
    }
}

