using UnityEngine;
using Ironsite.Core;
using System.Collections.Generic;

namespace Ironsite.UI
{
    /// <summary>
    /// Automatically adds green highlight effect to objects when they are projected into the room
    /// This component listens for newly projected objects and adds ObjectHighlightEffect to them
    /// Designed for HoloLens emulator where objects are projected by external systems
    /// </summary>
    public class ProjectedObjectHighlighter : MonoBehaviour
    {
        [Header("Auto-Highlight Settings")]
        [SerializeField] private bool autoAddHighlightOnRegister = true;
        [SerializeField] private bool autoHighlightOnRegister = false;
        [SerializeField] private float highlightCheckInterval = 0.5f;

        [Header("Object Detection")]
        [SerializeField] private string[] objectTypeTags = { "Window", "Door", "Outlet", "ProjectedObject" };
        [SerializeField] private bool checkAllObjectsWithRenderer = true;

        private HashSet<GameObject> processedObjects = new HashSet<GameObject>();
        private float lastCheckTime = 0f;

        private void Start()
        {
            // Subscribe to object registration events
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectRegistered += OnObjectRegistered;
            }
        }

        private void OnDestroy()
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectRegistered -= OnObjectRegistered;
            }
        }

        private void Update()
        {
            // Periodically check for new objects in the scene that might have been projected
            if (Time.time - lastCheckTime >= highlightCheckInterval)
            {
                lastCheckTime = Time.time;
                CheckForNewProjectedObjects();
            }
        }

        /// <summary>
        /// Called when an object is registered with SceneObjectRegistry
        /// Automatically adds highlight effect if enabled
        /// </summary>
        private void OnObjectRegistered(string id, GameObject obj)
        {
            if (obj == null || processedObjects.Contains(obj)) return;

            // Check if this is a projected 3D room object
            if (IsProjectedRoomObject(obj))
            {
                AddHighlightToObject(obj);
                
                if (autoHighlightOnRegister)
                {
                    // Optionally auto-highlight when registered
                    if (SceneObjectRegistry.Instance != null)
                    {
                        SceneObjectRegistry.Instance.HighlightObject(id);
                    }
                }
            }
        }

        /// <summary>
        /// Periodically check scene for newly projected objects
        /// Useful when objects are projected by external systems that don't use SceneObjectRegistry
        /// </summary>
        private void CheckForNewProjectedObjects()
        {
            if (!checkAllObjectsWithRenderer) return;

            // Find all objects with Renderer components
            Renderer[] allRenderers = FindObjectsOfType<Renderer>();

            foreach (Renderer renderer in allRenderers)
            {
                GameObject obj = renderer.gameObject;

                // Skip if already processed
                if (processedObjects.Contains(obj)) continue;

                // Skip UI elements (Canvas, UI components)
                if (obj.GetComponent<Canvas>() != null || 
                    obj.GetComponent<UnityEngine.UI.Graphic>() != null ||
                    obj.transform.root.GetComponent<Canvas>() != null)
                {
                    continue;
                }

                // Check if this looks like a projected room object
                if (IsProjectedRoomObject(obj))
                {
                    AddHighlightToObject(obj);
                    
                    // Optionally register with SceneObjectRegistry
                    if (SceneObjectRegistry.Instance != null && !string.IsNullOrEmpty(obj.name))
                    {
                        SceneObjectRegistry.Instance.RegisterObject(obj.name, obj);
                    }
                }
            }
        }

        /// <summary>
        /// Check if an object appears to be a projected 3D room object
        /// </summary>
        private bool IsProjectedRoomObject(GameObject obj)
        {
            if (obj == null) return false;

            // Must have a Renderer component (3D object)
            if (obj.GetComponent<Renderer>() == null && obj.GetComponentInChildren<Renderer>() == null)
            {
                return false;
            }

            // Check if it matches any of the object type tags
            foreach (string tag in objectTypeTags)
            {
                if (obj.CompareTag(tag) || obj.name.Contains(tag, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // If checking all objects with renderer, include it
            if (checkAllObjectsWithRenderer)
            {
                // Exclude common non-room objects
                string lowerName = obj.name.ToLower();
                if (lowerName.Contains("camera") || 
                    lowerName.Contains("light") || 
                    lowerName.Contains("canvas") ||
                    lowerName.Contains("ui") ||
                    lowerName.Contains("event"))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add green highlight effect to a projected object
        /// </summary>
        public void AddHighlightToObject(GameObject obj)
        {
            if (obj == null || processedObjects.Contains(obj)) return;

            // Check if object already has highlight effect
            if (obj.GetComponent<ObjectHighlightEffect>() != null)
            {
                processedObjects.Add(obj);
                return;
            }

            // Verify it's a 3D object with Renderer
            if (obj.GetComponent<Renderer>() == null && obj.GetComponentInChildren<Renderer>() == null)
            {
                Debug.LogWarning($"[ProjectedObjectHighlighter] Cannot add highlight to {obj.name} - no Renderer component found");
                return;
            }

            // Add the highlight effect component
            ObjectHighlightEffect highlight = obj.AddComponent<ObjectHighlightEffect>();
            processedObjects.Add(obj);

            Debug.Log($"[ProjectedObjectHighlighter] Added green highlight to projected object: {obj.name}");
        }

        /// <summary>
        /// Manually add highlight to an object (can be called by external systems)
        /// </summary>
        public void HighlightProjectedObject(GameObject obj)
        {
            AddHighlightToObject(obj);
            
            if (SceneObjectRegistry.Instance != null && !string.IsNullOrEmpty(obj.name))
            {
                SceneObjectRegistry.Instance.RegisterObject(obj.name, obj);
                SceneObjectRegistry.Instance.HighlightObject(obj.name);
            }
        }

        /// <summary>
        /// Clear processed objects list (useful for testing or reset)
        /// </summary>
        public void ClearProcessedList()
        {
            processedObjects.Clear();
        }
    }
}

