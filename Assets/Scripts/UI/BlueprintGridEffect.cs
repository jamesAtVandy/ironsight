using UnityEngine;

namespace Ironsite.UI
{
    /// <summary>
    /// Creates a pulsing blueprint grid overlay effect
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class BlueprintGridEffect : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float gridSpacing = 0.5f;
        [SerializeField] private int gridSize = 20;
        [SerializeField] private float lineWidth = 0.02f;

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float minAlpha = 0.2f;
        [SerializeField] private float maxAlpha = 0.5f;

        private LineRenderer lineRenderer;
        private Material gridMaterial;
        private UITheme theme;

        private void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            theme = Resources.Load<UITheme>("UITheme");

            SetupGrid();
        }

        private void Update()
        {
            // Pulsing effect
            if (gridMaterial != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
                
                Color color = theme != null ? theme.gridColor : new Color(0f, 0.6f, 1f, alpha);
                gridMaterial.color = color;
            }
        }

        private void SetupGrid()
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = CreateGridMaterial();

            // Create grid lines
            int lineCount = gridSize * 4; // Horizontal and vertical lines
            lineRenderer.positionCount = lineCount * 2;

            int index = 0;
            float halfSize = gridSize * gridSpacing * 0.5f;

            // Vertical lines
            for (int i = 0; i <= gridSize; i++)
            {
                float x = -halfSize + i * gridSpacing;
                lineRenderer.SetPosition(index++, new Vector3(x, 0f, -halfSize));
                lineRenderer.SetPosition(index++, new Vector3(x, 0f, halfSize));
            }

            // Horizontal lines
            for (int i = 0; i <= gridSize; i++)
            {
                float z = -halfSize + i * gridSpacing;
                lineRenderer.SetPosition(index++, new Vector3(-halfSize, 0f, z));
                lineRenderer.SetPosition(index++, new Vector3(halfSize, 0f, z));
            }
        }

        private Material CreateGridMaterial()
        {
            Material mat = new Material(Shader.Find("Unlit/Color"));
            if (theme != null)
            {
                mat.color = theme.gridColor;
            }
            else
            {
                mat.color = new Color(0f, 0.6f, 1f, 0.3f);
            }
            gridMaterial = mat;
            return mat;
        }
    }
}

