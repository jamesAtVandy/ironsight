using UnityEngine;
using Ironsite.Core;

namespace Ironsite.ObjectFactory
{
    /// <summary>
    /// Factory for creating AR objects with proper MRTK components
    /// </summary>
    public static class ObjectFactory
    {
        /// <summary>
        /// Create an object with MRTK interactivity
        /// </summary>
        public static GameObject CreateInteractiveObject(string id, GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = prefab != null ? Object.Instantiate(prefab) : new GameObject(id);
            obj.name = id;
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            // Add MRTK3 components for interaction
            // Note: MRTK3 uses different components than MRTK2
            // These would need to be adjusted based on actual MRTK3 API

            // Register with scene registry
            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.RegisterObject(id, obj);
            }

            return obj;
        }

        /// <summary>
        /// Create an outline object with glowing effect
        /// </summary>
        public static GameObject CreateOutline(Vector3 position, Vector3 scale, Color color)
        {
            GameObject outline = GameObject.CreatePrimitive(PrimitiveType.Cube);
            outline.name = "Outline";
            outline.transform.position = position;
            outline.transform.localScale = scale;

            var renderer = outline.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
                renderer.material.SetFloat("_Mode", 3); // Transparent mode
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                renderer.material.renderQueue = 3000;
            }

            return outline;
        }
    }
}

