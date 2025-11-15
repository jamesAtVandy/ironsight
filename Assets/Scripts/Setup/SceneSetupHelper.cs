using UnityEngine;
using Ironsite.Core;
using Ironsite.Networking;
using Ironsite.UI;

namespace Ironsite.Setup
{
    /// <summary>
    /// Helper script to quickly set up the scene with all required UI components
    /// Attach to an empty GameObject and run in editor to auto-setup
    /// </summary>
    public class SceneSetupHelper : MonoBehaviour
    {
        [ContextMenu("Setup UI-Only Scene")]
        public void SetupUIOnlyScene()
        {
            Debug.Log("[SceneSetupHelper] Starting UI-only scene setup...");

            // 1. Create SceneObjectRegistry (for highlight system)
            if (FindObjectOfType<SceneObjectRegistry>() == null)
            {
                GameObject registryObj = new GameObject("SceneObjectRegistry");
                registryObj.AddComponent<SceneObjectRegistry>();
                Debug.Log("✓ Created SceneObjectRegistry (for highlight system)");
            }

            // 2. Create HTTPCommandPoller (optional - for LLM commands)
            if (FindObjectOfType<HTTPCommandPoller>() == null)
            {
                GameObject pollerObj = new GameObject("HTTPCommandPoller");
                pollerObj.AddComponent<HTTPCommandPoller>();
                Debug.Log("✓ Created HTTPCommandPoller (optional)");
            }

            // 3. Create Blueprint Grid Effect
            if (FindObjectOfType<BlueprintGridEffect>() == null)
            {
                GameObject gridObj = new GameObject("BlueprintGrid");
                gridObj.AddComponent<LineRenderer>();
                gridObj.AddComponent<BlueprintGridEffect>();
                Debug.Log("✓ Created BlueprintGridEffect (blue gridlines)");
            }

            // 4. Create Projected Object Highlighter (auto-highlights projected objects)
            if (FindObjectOfType<ProjectedObjectHighlighter>() == null)
            {
                GameObject highlighterObj = new GameObject("ProjectedObjectHighlighter");
                highlighterObj.AddComponent<ProjectedObjectHighlighter>();
                Debug.Log("✓ Created ProjectedObjectHighlighter (auto-adds green highlights to projected objects)");
            }

            // 5. Create UI Canvas
            SetupUICanvas();

            // 6. Create FloatingTagController
            if (FindObjectOfType<FloatingTagController>() == null)
            {
                GameObject tagControllerObj = new GameObject("FloatingTagController");
                tagControllerObj.AddComponent<FloatingTagController>();
                Debug.Log("✓ Created FloatingTagController (for green highlight tags)");
            }

            Debug.Log("[SceneSetupHelper] UI-only scene setup complete!");
            Debug.Log("✓ Blue gridlines will appear when room is scanned");
            Debug.Log("✓ Green highlights will automatically appear when objects are projected into the room");
            Debug.Log("✓ ProjectedObjectHighlighter will detect and highlight projected objects (windows, doors, outlets, etc.)");
            Debug.LogWarning("⚠ Please manually configure UI panels and assign references in UIController!");
        }

        private void SetupUICanvas()
        {
            // Check if canvas already exists
            Canvas existingCanvas = FindObjectOfType<Canvas>();
            if (existingCanvas != null && existingCanvas.renderMode == RenderMode.WorldSpace)
            {
                Debug.Log("✓ UI Canvas already exists");
                return;
            }

            // Create main canvas
            GameObject canvasObj = new GameObject("MainUICanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create UIController
            GameObject uiControllerObj = new GameObject("UIController");
            uiControllerObj.transform.SetParent(canvasObj.transform);
            UIController uiController = uiControllerObj.AddComponent<UIController>();

            Debug.Log("✓ Created UI Canvas and UIController");
        }
    }
}
