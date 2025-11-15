using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json;

namespace Ironsite.Networking
{
    /// <summary>
    /// Polls Python server for LLM commands and emits events for UI + object system
    /// </summary>
    public class HTTPCommandPoller : MonoBehaviour
    {
        [Header("Server Configuration")]
        [SerializeField] private string serverURL = "http://localhost:5000";
        [SerializeField] private float pollInterval = 0.5f;
        [SerializeField] private bool autoStart = true;

        [Header("Events")]
        public event Action<CommandData> OnCommandReceived;
        public event Action<string> OnStatusUpdate;
        public event Action<string> OnError;

        private Coroutine pollCoroutine;
        private bool isPolling = false;

        private void Start()
        {
            if (autoStart)
            {
                StartPolling();
            }
        }

        /// <summary>
        /// Start polling the Python server
        /// </summary>
        public void StartPolling()
        {
            if (isPolling) return;

            isPolling = true;
            pollCoroutine = StartCoroutine(PollServer());
            OnStatusUpdate?.Invoke("Connected - Listening for commands...");
        }

        /// <summary>
        /// Stop polling
        /// </summary>
        public void StopPolling()
        {
            if (!isPolling) return;

            isPolling = false;
            if (pollCoroutine != null)
            {
                StopCoroutine(pollCoroutine);
            }
            OnStatusUpdate?.Invoke("Disconnected");
        }

        private IEnumerator PollServer()
        {
            while (isPolling)
            {
                yield return StartCoroutine(FetchCommand());
                yield return new WaitForSeconds(pollInterval);
            }
        }

        private IEnumerator FetchCommand()
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{serverURL}/get_command"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    
                    if (!string.IsNullOrEmpty(jsonResponse) && jsonResponse != "{}" && jsonResponse != "null")
                    {
                        try
                        {
                            CommandData command = JsonConvert.DeserializeObject<CommandData>(jsonResponse);
                            if (command != null && !string.IsNullOrEmpty(command.action))
                            {
                                OnCommandReceived?.Invoke(command);
                                OnStatusUpdate?.Invoke($"Command received: {command.action}");
                            }
                        }
                        catch (Exception e)
                        {
                            OnError?.Invoke($"JSON Parse Error: {e.Message}");
                            Debug.LogError($"[HTTPCommandPoller] Parse error: {e.Message}\nResponse: {jsonResponse}");
                        }
                    }
                }
                else if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    OnError?.Invoke("Connection error - Is Python server running?");
                }
            }
        }

        /// <summary>
        /// Send a response back to Python server
        /// </summary>
        public void SendResponse(string response)
        {
            StartCoroutine(PostResponse(response));
        }

        private IEnumerator PostResponse(string response)
        {
            WWWForm form = new WWWForm();
            form.AddField("response", response);

            using (UnityWebRequest request = UnityWebRequest.Post($"{serverURL}/send_response", form))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    OnError?.Invoke($"Failed to send response: {request.error}");
                }
            }
        }

        private void OnDestroy()
        {
            StopPolling();
        }
    }

    /// <summary>
    /// Command data structure from Python/LLM
    /// </summary>
    [Serializable]
    public class CommandData
    {
        public string action;
        public string objectId;
        public Dictionary<string, object> parameters;
        public string transcription;
        public string agentStatus;

        public T GetParameter<T>(string key, T defaultValue = default(T))
        {
            if (parameters != null && parameters.ContainsKey(key))
            {
                try
                {
                    return (T)Convert.ChangeType(parameters[key], typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }
}

