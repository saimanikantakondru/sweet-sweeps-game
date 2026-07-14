using UnityEngine;

namespace SweetSweeps.Server.Data
{
    [CreateAssetMenu(fileName = "ServerSettings", menuName = "SweetSweeps/ServerSettings")]
    public class ServerSettings : ScriptableObject
    {
        [Header("API")]
        [SerializeField] private string apiBaseUrl = "https://stage.qubitgamez.com/api/mrgs/games";

        [Header("WebSocket")]
        [SerializeField] private string webSocketUrl = "wss://stage.qubitgamez.com/api/mrgs/games";

        [Header("Resilience")]
        [SerializeField, Min(1)] private int requestTimeoutSeconds = 5;

        public string ApiBaseUrl => apiBaseUrl;
        public string WebSocketUrl => webSocketUrl;
        public int RequestTimeoutSeconds => requestTimeoutSeconds;
    }
}