using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ironsite.Core;
using Ironsite.Networking;
using System.Collections.Generic;
using System;

namespace Ironsite.UI
{
    /// <summary>
    /// Main UI controller that initializes all UI panels and manages the HUD overlay
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("UI Theme")]
        [SerializeField] private UITheme theme;

        [Header("Panel References")]
        [SerializeField] private GameObject leftPanel;
        [SerializeField] private GameObject rightPanel;
        [SerializeField] private GameObject bottomPanel;
        [SerializeField] private GameObject topCenterPanel;
        [SerializeField] private GameObject topRightPanel;

        [Header("Component References")]
        [SerializeField] private UIObjectListBuilder objectListBuilder;
        [SerializeField] private UIControlsHandler controlsHandler;
        [SerializeField] private MiniMapController miniMapController;
        [SerializeField] private FloatingTagController floatingTagController;

        [Header("Text References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI roomIdText;
        [SerializeField] private TextMeshProUGUI transcriptionText;
        [SerializeField] private TextMeshProUGUI agentStatusText;
        [SerializeField] private TextMeshProUGUI logText;

        [Header("Log Settings")]
        [SerializeField] private int maxLogEntries = 10;
        [SerializeField] private float logFadeDuration = 0.3f;

        private Queue<string> logQueue = new Queue<string>();
        private HTTPCommandPoller commandPoller;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            commandPoller = FindObjectOfType<HTTPCommandPoller>();
        }

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void InitializeUI()
        {
            if (theme == null)
            {
                Debug.LogWarning("[UIController] UI Theme not assigned! Using defaults.");
                return;
            }

            ApplyTheme();
            UpdateTitle("Ironsite AR Construction Aid");
            UpdateRoomId("Room: Not Loaded");
            UpdateTranscription("");
            UpdateAgentStatus("Initializing...");
            ClearLog();
        }

        private void ApplyTheme()
        {
            // Apply theme colors to panels
            if (leftPanel != null)
            {
                Image leftImg = leftPanel.GetComponent<Image>();
                if (leftImg != null) leftImg.color = theme.panelBackground;
            }

            if (rightPanel != null)
            {
                Image rightImg = rightPanel.GetComponent<Image>();
                if (rightImg != null) rightImg.color = theme.panelBackground;
            }

            if (bottomPanel != null)
            {
                Image bottomImg = bottomPanel.GetComponent<Image>();
                if (bottomImg != null) bottomImg.color = theme.darkBackground;
            }
        }

        private void SubscribeToEvents()
        {
            if (commandPoller != null)
            {
                commandPoller.OnCommandReceived += HandleCommand;
                commandPoller.OnStatusUpdate += UpdateAgentStatus;
                commandPoller.OnError += LogError;
            }

            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectHighlighted += OnObjectHighlighted;
                SceneObjectRegistry.Instance.OnObjectUnhighlighted += OnObjectUnhighlighted;
            }
        }

        private void OnDestroy()
        {
            if (commandPoller != null)
            {
                commandPoller.OnCommandReceived -= HandleCommand;
                commandPoller.OnStatusUpdate -= UpdateAgentStatus;
                commandPoller.OnError -= LogError;
            }

            if (SceneObjectRegistry.Instance != null)
            {
                SceneObjectRegistry.Instance.OnObjectHighlighted -= OnObjectHighlighted;
                SceneObjectRegistry.Instance.OnObjectUnhighlighted -= OnObjectUnhighlighted;
            }
        }

        /// <summary>
        /// Handle incoming command from Python/LLM
        /// </summary>
        private void HandleCommand(CommandData command)
        {
            if (command == null) return;

            // Update transcription if provided
            if (!string.IsNullOrEmpty(command.transcription))
            {
                UpdateTranscription($"User said: {command.transcription}");
            }

            // Log the command
            AddLogEntry($"[Command] {command.action} - {command.objectId ?? "N/A"}");

            // Update object list if needed
            if (objectListBuilder != null)
            {
                objectListBuilder.RefreshList();
            }
        }

        /// <summary>
        /// Update project title
        /// </summary>
        public void UpdateTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        /// <summary>
        /// Update room identification
        /// </summary>
        public void UpdateRoomId(string roomId)
        {
            if (roomIdText != null)
                roomIdText.text = roomId;
        }

        /// <summary>
        /// Update voice transcription display
        /// </summary>
        public void UpdateTranscription(string transcription)
        {
            if (transcriptionText != null)
            {
                transcriptionText.text = transcription;
                // Fade in animation
                StartCoroutine(FadeText(transcriptionText, logFadeDuration));
            }
        }

        /// <summary>
        /// Update agent status (listening, processing, speaking)
        /// </summary>
        public void UpdateAgentStatus(string status)
        {
            if (agentStatusText != null)
            {
                agentStatusText.text = $"Agent: {status}";
                
                // Color code status
                if (theme != null)
                {
                    if (status.ToLower().Contains("listening"))
                        agentStatusText.color = theme.primaryBlue;
                    else if (status.ToLower().Contains("processing"))
                        agentStatusText.color = theme.accentBlue;
                    else if (status.ToLower().Contains("speaking"))
                        agentStatusText.color = theme.highlightGreen;
                    else
                        agentStatusText.color = theme.textSecondary;
                }
            }
        }

        /// <summary>
        /// Add entry to log
        /// </summary>
        public void AddLogEntry(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return;

            logQueue.Enqueue($"[{DateTime.Now:HH:mm:ss}] {entry}");
            
            while (logQueue.Count > maxLogEntries)
            {
                logQueue.Dequeue();
            }

            UpdateLogDisplay();
        }

        /// <summary>
        /// Clear log
        /// </summary>
        public void ClearLog()
        {
            logQueue.Clear();
            UpdateLogDisplay();
        }

        private void UpdateLogDisplay()
        {
            if (logText != null)
            {
                logText.text = string.Join("\n", logQueue);
            }
        }

        private void LogError(string error)
        {
            AddLogEntry($"[ERROR] {error}");
        }

        private void OnObjectHighlighted(string objectId)
        {
            AddLogEntry($"Highlighted: {objectId}");
            
            // Show floating tag
            if (floatingTagController != null)
            {
                GameObject obj = SceneObjectRegistry.Instance?.GetObject(objectId);
                if (obj != null)
                {
                    floatingTagController.ShowTag(obj.transform, objectId);
                }
            }

            // Update minimap
            if (miniMapController != null)
            {
                miniMapController.HighlightObject(objectId);
            }
        }

        private void OnObjectUnhighlighted(string objectId)
        {
            if (floatingTagController != null)
            {
                floatingTagController.HideTag();
            }

            if (miniMapController != null)
            {
                miniMapController.ClearHighlight();
            }
        }

        /// <summary>
        /// Show/hide entire UI
        /// </summary>
        public void SetUIVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        /// <summary>
        /// Show "Regenerating Scene..." overlay
        /// </summary>
        public void ShowRegeneratingOverlay()
        {
            AddLogEntry("Regenerating scene from JSON...");
            // Could add a full-screen overlay here
        }

        private System.Collections.IEnumerator FadeText(TextMeshProUGUI text, float duration)
        {
            if (text == null) yield break;

            Color originalColor = text.color;
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            text.color = originalColor;
        }
    }
}

