using UnityEngine;

namespace Ironsite.UI
{
    /// <summary>
    /// ScriptableObject defining the techy blue blueprint theme for the HoloLens UI
    /// </summary>
    [CreateAssetMenu(fileName = "UITheme", menuName = "Ironsite/UI Theme")]
    public class UITheme : ScriptableObject
    {
        [Header("Color Palette")]
        public Color primaryBlue = new Color(0f, 0.6f, 1f, 1f); // #0099FF
        public Color accentBlue = new Color(0f, 0.8f, 1f, 1f);
        public Color darkBackground = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        public Color panelBackground = new Color(0.05f, 0.05f, 0.1f, 0.85f);
        public Color textPrimary = Color.white;
        public Color textSecondary = new Color(0.8f, 0.8f, 0.9f, 1f);
        public Color highlightGreen = new Color(0f, 1f, 0.5f, 1f);
        public Color outlineColor = new Color(0f, 0.6f, 1f, 0.8f);
        public Color gridColor = new Color(0f, 0.6f, 1f, 0.3f);

        [Header("UI Dimensions")]
        public float panelWidth = 400f;
        public float panelHeight = 600f;
        public float panelPadding = 20f;
        public float buttonHeight = 50f;
        public float listItemHeight = 40f;

        [Header("Fonts")]
        public int titleFontSize = 32;
        public int bodyFontSize = 18;
        public int smallFontSize = 14;

        [Header("Animation")]
        public float fadeDuration = 0.3f;
        public float pulseSpeed = 2f;
        public float glowIntensity = 1.5f;

        [Header("Grid/Blueprint")]
        public float gridLineWidth = 0.02f;
        public float gridSpacing = 0.5f;
        public float blueprintPulseSpeed = 1f;
    }
}

