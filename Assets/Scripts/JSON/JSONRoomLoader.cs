using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Ironsite.Core;

namespace Ironsite.JSON
{
    /// <summary>
    /// Loads room configuration from JSON and generates scene objects
    /// </summary>
    public class JSONRoomLoader : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject windowPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject outletPrefab;
        [SerializeField] private GameObject markerPrefab;
        [SerializeField] private GameObject outlinePrefab;

        [Header("Settings")]
        [SerializeField] private string jsonFileName = "room_config.json";
        [SerializeField] private Transform roomParent;

        private RoomData currentRoomData;

        private void Start()
        {
            if (roomParent == null)
            {
                roomParent = new GameObject("Room").transform;
            }

            // Auto-load on start
            LoadRoomFromFile();
        }

        /// <summary>
        /// Load room from JSON file
        /// </summary>
        public void LoadRoomFromFile()
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
            
            if (File.Exists(filePath))
            {
                string jsonContent = File.ReadAllText(filePath);
                LoadRoomFromJSON(jsonContent);
            }
            else
            {
                Debug.LogError($"[JSONRoomLoader] JSON file not found at: {filePath}");
            }
        }

        /// <summary>
        /// Load room from JSON string
        /// </summary>
        public void LoadRoomFromJSON(string jsonContent)
        {
            try
            {
                currentRoomData = JsonConvert.DeserializeObject<RoomData>(jsonContent);
                
                if (currentRoomData != null)
                {
                    ClearRoom();
                    GenerateRoom();
                }
                else
                {
                    Debug.LogError("[JSONRoomLoader] Failed to parse JSON data");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[JSONRoomLoader] JSON parse error: {e.Message}");
            }
        }

        private void ClearRoom()
        {
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.ClearAll();
            }

            // Destroy all children
            for (int i = roomParent.childCount - 1; i >= 0; i--)
            {
                Destroy(roomParent.GetChild(i).gameObject);
            }
        }

        private void GenerateRoom()
        {
            if (currentRoomData == null) return;

            // Generate room outline
            if (currentRoomData.room != null)
            {
                GenerateRoomOutline(currentRoomData.room);
            }

            // Generate objects
            if (currentRoomData.objects != null)
            {
                foreach (var obj in currentRoomData.objects)
                {
                    GenerateObject(obj);
                }
            }

            // Generate markers
            if (currentRoomData.markers != null)
            {
                foreach (var marker in currentRoomData.markers)
                {
                    GenerateMarker(marker);
                }
            }

            // Generate outlines
            if (currentRoomData.outlines != null)
            {
                foreach (var outline in currentRoomData.outlines)
                {
                    GenerateOutline(outline);
                }
            }

            // Update minimap
            var minimap = FindObjectOfType<MiniMapController>();
            if (minimap != null && currentRoomData.room != null)
            {
                Vector2 dimensions = new Vector2(currentRoomData.room.width, currentRoomData.room.length);
                Vector3 center = new Vector3(currentRoomData.room.centerX, 0f, currentRoomData.room.centerZ);
                minimap.SetRoomDimensions(dimensions, center);
            }
        }

        private void GenerateRoomOutline(RoomData.RoomInfo room)
        {
            // Create room floor/outline visualization
            GameObject roomObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomObj.name = "Room_Outline";
            roomObj.transform.SetParent(roomParent);
            roomObj.transform.position = new Vector3(room.centerX, 0f, room.centerZ);
            roomObj.transform.localScale = new Vector3(room.width, 0.1f, room.length);
            
            // Make it transparent/wireframe
            var renderer = roomObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0f, 0.6f, 1f, 0.1f);
            }
        }

        private void GenerateObject(RoomData.ObjectInfo obj)
        {
            GameObject prefab = GetPrefabForType(obj.type);
            if (prefab == null)
            {
                // Create primitive as fallback
                prefab = CreatePrimitiveForType(obj.type);
            }

            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, roomParent);
                instance.name = obj.id;
                instance.transform.position = new Vector3(obj.x, obj.y, obj.z);
                instance.transform.rotation = Quaternion.Euler(0f, obj.rotationY, 0f);
                instance.transform.localScale = new Vector3(obj.scaleX, obj.scaleY, obj.scaleZ);

                // Register with registry
                if (SceneObjectRegistry.Instance != null)
                {
                    SceneObjectRegistry.Instance.RegisterObject(obj.id, instance);
                }
            }
        }

        private void GenerateMarker(RoomData.MarkerInfo marker)
        {
            GameObject prefab = markerPrefab;
            if (prefab == null)
            {
                prefab = CreateMarkerPrimitive();
            }

            GameObject instance = Instantiate(prefab, roomParent);
            instance.name = marker.id;
            instance.transform.position = new Vector3(marker.x, marker.y, marker.z);

            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.RegisterMarker(marker.id, instance);
            }
        }

        private void GenerateOutline(RoomData.OutlineInfo outline)
        {
            GameObject prefab = outlinePrefab;
            if (prefab == null)
            {
                prefab = CreateOutlinePrimitive();
            }

            GameObject instance = Instantiate(prefab, roomParent);
            instance.name = outline.id;
            instance.transform.position = new Vector3(outline.x, outline.y, outline.z);
            instance.transform.localScale = new Vector3(outline.width, outline.height, outline.depth);

            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.RegisterOutline(outline.id, instance);
            }
        }

        private GameObject GetPrefabForType(string type)
        {
            switch (type.ToLower())
            {
                case "window": return windowPrefab;
                case "door": return doorPrefab;
                case "outlet": return outletPrefab;
                default: return null;
            }
        }

        private GameObject CreatePrimitiveForType(string type)
        {
            PrimitiveType primitive = PrimitiveType.Cube;
            
            switch (type.ToLower())
            {
                case "window":
                    primitive = PrimitiveType.Quad;
                    break;
                case "door":
                    primitive = PrimitiveType.Cube;
                    break;
                case "outlet":
                    primitive = PrimitiveType.Cylinder;
                    break;
            }

            GameObject obj = GameObject.CreatePrimitive(primitive);
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0f, 0.6f, 1f, 0.5f);
            }

            return obj;
        }

        private GameObject CreateMarkerPrimitive()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.localScale = Vector3.one * 0.1f;
            var renderer = marker.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 0f, 0f, 0.8f);
            }
            return marker;
        }

        private GameObject CreateOutlinePrimitive()
        {
            GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var renderer = outline.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0f, 0.6f, 1f, 0.3f);
            }
            return outline;
        }
    }

    /// <summary>
    /// JSON data structures
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public RoomInfo room;
        public List<ObjectInfo> objects;
        public List<MarkerInfo> markers;
        public List<OutlineInfo> outlines;

        [System.Serializable]
        public class RoomInfo
        {
            public float width;
            public float length;
            public float height;
            public float centerX;
            public float centerY;
            public float centerZ;
        }

        [System.Serializable]
        public class ObjectInfo
        {
            public string id;
            public string type;
            public float x, y, z;
            public float rotationY;
            public float scaleX = 1f, scaleY = 1f, scaleZ = 1f;
        }

        [System.Serializable]
        public class MarkerInfo
        {
            public string id;
            public string type;
            public float x, y, z;
        }

        [System.Serializable]
        public class OutlineInfo
        {
            public string id;
            public float x, y, z;
            public float width, height, depth;
        }
    }
}

