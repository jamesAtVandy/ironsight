using UnityEngine;
using System.IO;

namespace Ironsight.RoomSystem
{
    /// <summary>
    /// Simplified room loader that only creates the basic room structure:
    /// four walls, ceiling, and floor. No additional components.
    /// Designed for use with overlay systems.
    /// 
    /// JSON Coordinate System:
    ///   x_m: left ↔ right (east)
    ///   y_m: front (south, 0) → back (north)
    ///   z_m: floor (0) → ceiling
    /// 
    /// Unity Coordinate System (1 unit = 1 meter):
    ///   Unity x = json x_m
    ///   Unity y = json z_m  (floor to ceiling)
    ///   Unity z = json y_m  (front to back)
    /// </summary>
    public class SimpleRoomLoader : MonoBehaviour
    {
        [Header("JSON Configuration")]
        [Tooltip("JSON file name in StreamingAssets folder (e.g. 'room_static.json')")]
        public string roomJsonFileName = "room_static.json";

        [Header("Room Settings")]
        [Tooltip("Root transform for all room objects. If null, will create a new GameObject.")]
        public Transform roomRoot;

        [Tooltip("Meters to Unity units conversion (typically 1.0)")]
        public float metersToUnity = 1f;

        [Header("Wall Settings")]
        [Tooltip("Thickness of walls in meters")]
        public float wallThickness = 0.1f;

        [Tooltip("Material for walls (optional)")]
        public Material wallMaterial;

        [Tooltip("Material for floor (optional)")]
        public Material floorMaterial;

        [Tooltip("Material for ceiling (optional)")]
        public Material ceilingMaterial;

        private void Start()
        {
            LoadRoomFromJSON();
        }

        /// <summary>
        /// Main entry point: Load JSON and build basic room structure.
        /// </summary>
        public void LoadRoomFromJSON()
        {
            string jsonPath = Path.Combine(Application.streamingAssetsPath, roomJsonFileName);

            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"Room JSON file not found: {jsonPath}");
                return;
            }

            string jsonContent = File.ReadAllText(jsonPath);
            Debug.Log($"Loading room from: {jsonPath}");

            // Initialize room root
            if (roomRoot == null)
            {
                GameObject rootObj = new GameObject("RoomRoot");
                roomRoot = rootObj.transform;
            }

            // Parse JSON - try both possible structures
            SimpleRoomJsonData roomData = JsonUtility.FromJson<SimpleRoomJsonData>(jsonContent);
            
            if (roomData != null && roomData.room != null && roomData.room.dimensions != null)
            {
                BuildBasicRoom(roomData.room.dimensions);
            }
            else
            {
                // Try alternative structure (RoomDefinition)
                RoomDefinition roomDef = JsonUtility.FromJson<RoomDefinition>(jsonContent);
                if (roomDef != null && roomDef.dimensions != null)
                {
                    BuildBasicRoom(roomDef.dimensions);
                }
                else
                {
                    Debug.LogError("Failed to parse room dimensions from JSON");
                }
            }
        }

        /// <summary>
        /// Build the basic room structure: four walls, floor, and ceiling.
        /// </summary>
        private void BuildBasicRoom(Dimensions dims)
        {
            float length = dims.length_m * metersToUnity;
            float width = dims.width_m * metersToUnity;
            float height = dims.height_m * metersToUnity;
            float thickness = wallThickness * metersToUnity;

            Debug.Log($"Building room: {length}m x {width}m x {height}m");

            // Room origin is at southwest corner, floor level
            // In Unity: x = east, y = up, z = north
            Vector3 roomOrigin = Vector3.zero;

            // Create floor
            CreateFloor(roomOrigin, length, width, thickness);

            // Create ceiling
            CreateCeiling(roomOrigin, length, width, thickness, height);

            // Create four walls
            // South wall (y=0, facing north)
            CreateWall("SouthWall", 
                new Vector3(roomOrigin.x + length / 2f, roomOrigin.y + height / 2f, roomOrigin.z),
                length, height, thickness, Quaternion.identity);

            // North wall (y=width, facing south)
            CreateWall("NorthWall",
                new Vector3(roomOrigin.x + length / 2f, roomOrigin.y + height / 2f, roomOrigin.z + width),
                length, height, thickness, Quaternion.identity);

            // East wall (x=length, facing west)
            CreateWall("EastWall",
                new Vector3(roomOrigin.x + length, roomOrigin.y + height / 2f, roomOrigin.z + width / 2f),
                width, height, thickness, Quaternion.Euler(0, 90, 0));

            // West wall (x=0, facing east)
            CreateWall("WestWall",
                new Vector3(roomOrigin.x, roomOrigin.y + height / 2f, roomOrigin.z + width / 2f),
                width, height, thickness, Quaternion.Euler(0, 90, 0));

            Debug.Log("Basic room structure complete");
        }

        /// <summary>
        /// Create a wall using a Unity cube primitive.
        /// </summary>
        private void CreateWall(string name, Vector3 center, float width, float height, float thickness, Quaternion rotation)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(roomRoot);
            wall.transform.position = center;
            wall.transform.rotation = rotation;
            wall.transform.localScale = new Vector3(width, height, thickness);

            // Apply material if provided
            if (wallMaterial != null)
            {
                Renderer renderer = wall.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = wallMaterial;
                }
            }
        }

        /// <summary>
        /// Create the floor using a Unity cube primitive.
        /// </summary>
        private void CreateFloor(Vector3 origin, float length, float width, float thickness)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(roomRoot);
            floor.transform.position = new Vector3(
                origin.x + length / 2f,
                origin.y - thickness / 2f,
                origin.z + width / 2f
            );
            floor.transform.localScale = new Vector3(length, thickness, width);

            // Apply material if provided
            if (floorMaterial != null)
            {
                Renderer renderer = floor.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = floorMaterial;
                }
            }
        }

        /// <summary>
        /// Create the ceiling using a Unity cube primitive.
        /// </summary>
        private void CreateCeiling(Vector3 origin, float length, float width, float thickness, float roomHeight)
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(roomRoot);
            ceiling.transform.position = new Vector3(
                origin.x + length / 2f,
                origin.y + roomHeight + thickness / 2f,
                origin.z + width / 2f
            );
            ceiling.transform.localScale = new Vector3(length, thickness, width);

            // Apply material if provided
            if (ceilingMaterial != null)
            {
                Renderer renderer = ceiling.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = ceilingMaterial;
                }
            }
        }
    }

    /// <summary>
    /// Simple data model for room JSON that only contains dimensions.
    /// </summary>
    [System.Serializable]
    public class SimpleRoomJsonData
    {
        public SimpleRoomInfo room;
    }

    [System.Serializable]
    public class SimpleRoomInfo
    {
        public string roomId;
        public Dimensions dimensions;
        public CoordinateSystem coordinateSystem;
    }
}

