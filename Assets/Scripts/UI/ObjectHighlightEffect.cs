using UnityEngine;
using Ironsite.Core;

namespace Ironsite.UI
{
    /// <summary>
    /// Adds green highlight outline effect to 3D room objects (windows, doors, outlets, etc.)
    /// ONLY attach to rendered 3D objects in the scene - NOT UI elements
    /// This component provides the green glow effect when room objects are highlighted
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class ObjectHighlightEffect : MonoBehaviour
    {
        [Header("Highlight Settings")]
        [SerializeField] private float highlightThickness = 0.02f;
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private float pulseSpeed = 2f;

        private Renderer objectRenderer;
        private Material originalMaterial;
        private Material highlightMaterial;
        private bool isHighlighted = false;
        private UITheme theme;
        private string objectId;

        private void Awake()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer == null)
            {
                Debug.LogWarning($"[ObjectHighlightEffect] No Renderer found on {gameObject.name}. This component is for 3D room objects only (windows, doors, etc.), not UI elements.");
                enabled = false;
                return;
            }

            originalMaterial = objectRenderer.material;
            theme = Resources.Load<UITheme>("UITheme");
            
            // Get object ID from name or generate one
            objectId = gameObject.name;
        }

        private void Start()
        {
            // Subscribe to highlight events
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectHighlighted += OnObjectHighlighted;
                SceneObjectRegistry.Instance.OnObjectUnhighlighted += OnObjectUnhighlighted;
            }
        }

        private void OnDestroy()
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectHighlighted -= OnObjectHighlighted;
                SceneObjectRegistry.Instance.OnObjectUnhighlighted -= OnObjectUnhighlighted;
            }

            // Clean up highlight material
            if (highlightMaterial != null)
            {
                Destroy(highlightMaterial);
            }
        }

        private void OnObjectHighlighted(string id)
        {
            if (id == objectId || gameObject.name.Contains(id))
            {
                SetHighlighted(true);
            }
        }

        private void OnObjectUnhighlighted(string id)
        {
            if (id == objectId || gameObject.name.Contains(id))
            {
                SetHighlighted(false);
            }
        }

        private void SetHighlighted(bool highlighted)
        {
            isHighlighted = highlighted;

            if (objectRenderer == null) return;

            if (highlighted)
            {
                // Create or get highlight material
                if (highlightMaterial == null)
                {
                    highlightMaterial = new Material(originalMaterial);
                }

                // Apply green highlight color
                Color highlightColor = theme != null ? theme.highlightGreen : new Color(0f, 1f, 0.5f, 1f);
                highlightMaterial.color = highlightColor;
                highlightMaterial.EnableKeyword("_EMISSION");
                highlightMaterial.SetColor("_EmissionColor", highlightColor * glowIntensity);

                objectRenderer.material = highlightMaterial;
            }
            else
            {
                // Restore original material
                objectRenderer.material = originalMaterial;
            }
        }

        private void Update()
        {
            // Pulsing effect when highlighted
            if (isHighlighted && highlightMaterial != null && theme != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
                Color highlightColor = theme.highlightGreen;
                highlightMaterial.SetColor("_EmissionColor", highlightColor * glowIntensity * pulse);
            }
        }

        /// <summary>
        /// Manually set highlight state (for external control)
        /// Only use this for 3D room objects, not UI elements
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            SetHighlighted(highlighted);
        }
    }
}
