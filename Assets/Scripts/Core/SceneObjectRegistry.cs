using System.Collections.Generic;
using UnityEngine;
using System;

namespace Ironsite.Core
{
    /// <summary>
    /// Central registry for all generated scene objects, markers, and outlines
    /// Provides lookup by ID and manages object references
    /// </summary>
    public class SceneObjectRegistry : MonoBehaviour
    {
        public static SceneObjectRegistry Instance { get; private set; }

        [Header("Events")]
        public event Action<string, GameObject> OnObjectRegistered;
        public event Action<string> OnObjectHighlighted;
        public event Action<string> OnObjectUnhighlighted;

        // Object storage
        private Dictionary<string, GameObject> registeredObjects = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> registeredMarkers = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> registeredOutlines = new Dictionary<string, GameObject>();
        
        // Current highlight state
        private string currentlyHighlightedId = null;
        private GameObject currentHighlightObject = null;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register a 3D scene object (window, door, outlet, etc.)
        /// These are the rendered room objects that can be highlighted with green glow
        /// Used when objects are projected into the room by external systems
        /// </summary>
        public void RegisterObject(string id, GameObject obj)
        {
            if (string.IsNullOrEmpty(id) || obj == null) return;

            // Verify this is a 3D object with a Renderer (not a UI element)
            if (obj.GetComponent<Renderer>() == null && obj.GetComponentInChildren<Renderer>() == null)
            {
                Debug.LogWarning($"[SceneObjectRegistry] Object '{id}' has no Renderer component. Only 3D room objects (windows, doors, etc.) should be registered, not UI elements.");
                return;
            }

            registeredObjects[id] = obj;
            OnObjectRegistered?.Invoke(id, obj);
            Debug.Log($"[SceneObjectRegistry] Registered projected 3D room object: {id}");
        }

        /// <summary>
        /// Register a marker annotation
        /// </summary>
        public void RegisterMarker(string id, GameObject marker)
        {
            if (string.IsNullOrEmpty(id) || marker == null) return;

            registeredMarkers[id] = marker;
            OnObjectRegistered?.Invoke(id, marker);
        }

        /// <summary>
        /// Register an outline object
        /// </summary>
        public void RegisterOutline(string id, GameObject outline)
        {
            if (string.IsNullOrEmpty(id) || outline == null) return;

            registeredOutlines[id] = outline;
        }

        /// <summary>
        /// Get object by ID
        /// </summary>
        public GameObject GetObject(string id)
        {
            if (registeredObjects.ContainsKey(id))
                return registeredObjects[id];
            if (registeredMarkers.ContainsKey(id))
                return registeredMarkers[id];
            return null;
        }

        /// <summary>
        /// Get all registered object IDs
        /// </summary>
        public List<string> GetAllObjectIds()
        {
            List<string> ids = new List<string>();
            ids.AddRange(registeredObjects.Keys);
            ids.AddRange(registeredMarkers.Keys);
            return ids;
        }

        /// <summary>
        /// Get object metadata (type, position, etc.)
        /// </summary>
        public ObjectMetadata GetObjectMetadata(string id)
        {
            GameObject obj = GetObject(id);
            if (obj == null) return null;

            return new ObjectMetadata
            {
                id = id,
                name = obj.name,
                position = obj.transform.position,
                type = GetObjectType(id),
                isMarker = registeredMarkers.ContainsKey(id),
                isOutline = registeredOutlines.ContainsKey(id)
            };
        }

        /// <summary>
        /// Highlight an object by ID
        /// </summary>
        public void HighlightObject(string id)
        {
            if (currentlyHighlightedId == id) return;

            // Unhighlight previous
            if (!string.IsNullOrEmpty(currentlyHighlightedId))
            {
                OnObjectUnhighlighted?.Invoke(currentlyHighlightedId);
            }

            currentlyHighlightedId = id;
            currentHighlightObject = GetObject(id);

            if (currentHighlightObject != null)
            {
                OnObjectHighlighted?.Invoke(id);
            }
        }

        /// <summary>
        /// Clear current highlight
        /// </summary>
        public void ClearHighlight()
        {
            if (!string.IsNullOrEmpty(currentlyHighlightedId))
            {
                OnObjectUnhighlighted?.Invoke(currentlyHighlightedId);
            }
            currentlyHighlightedId = null;
            currentHighlightObject = null;
        }

        /// <summary>
        /// Get currently highlighted object ID
        /// </summary>
        public string GetHighlightedId()
        {
            return currentlyHighlightedId;
        }

        /// <summary>
        /// Toggle visibility of all outlines
        /// </summary>
        public void SetOutlinesVisible(bool visible)
        {
            foreach (var outline in registeredOutlines.Values)
            {
                if (outline != null)
                    outline.SetActive(visible);
            }
        }

        /// <summary>
        /// Toggle visibility of markers by type
        /// </summary>
        public void SetMarkersVisible(string markerType, bool visible)
        {
            foreach (var kvp in registeredMarkers)
            {
                if (kvp.Value != null && kvp.Key.Contains(markerType))
                {
                    kvp.Value.SetActive(visible);
                }
            }
        }

        /// <summary>
        /// Clear all registered objects
        /// </summary>
        public void ClearAll()
        {
            registeredObjects.Clear();
            registeredMarkers.Clear();
            registeredOutlines.Clear();
            ClearHighlight();
        }

        private string GetObjectType(string id)
        {
            if (id.ToLower().Contains("window")) return "window";
            if (id.ToLower().Contains("door")) return "door";
            if (id.ToLower().Contains("outlet")) return "outlet";
            if (id.ToLower().Contains("marker")) return "marker";
            return "object";
        }

        /// <summary>
        /// Object metadata structure
        /// </summary>
        [Serializable]
        public class ObjectMetadata
        {
            public string id;
            public string name;
            public Vector3 position;
            public string type;
            public bool isMarker;
            public bool isOutline;
        }
    }
}

