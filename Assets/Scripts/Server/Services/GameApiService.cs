using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Services
{
    public class GameApiService : IGameApiService
    {
        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public GameApiService(string baseUrl, int timeoutSeconds)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
        }

        public async UniTask<InitializeResponse> Initialize(string token, string brand, string game, string currency)
        {
            object payload = string.IsNullOrEmpty(currency)
                ? new InitializeRequest { token = token, brand = brand, game = game }
                : new InitializeRequestWithCurrency { token = token, brand = brand, game = game, currency = currency };

            return await Post<InitializeResponse>("/initialize", payload);
        }

        public async UniTask<PlayStartResponse> StartRound(string token, string game, float wager)
        {
            return await Post<PlayStartResponse>(
                "/play",
                new PlayStartRequest { token = token, game = game, wager = wager });
        }

        public async UniTask<PlayCompleteResponse> CompleteRound(
            string token,
            string game,
            string gameRound,
            CollectionReport report)
        {
            return await Post<PlayCompleteResponse>(
                "/play",
                new PlayCompleteRequest { token = token, game = game, gameRound = gameRound, data = report });
        }

        private async UniTask<T> Post<T>(string endpoint, object payload)
        {
            string url = _baseUrl + endpoint;
            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);

            Debug.Log($"[GameApiService] POST {endpoint} payload={json}");

            using var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = _timeoutSeconds;

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: timeoutCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                Debug.LogError($"[GameApiService] {endpoint} timed out after {_timeoutSeconds}s.");
                throw new ApiException(0, "Request timed out.");
            }
            catch (UnityWebRequestException e)
            {
                Debug.LogError($"[GameApiService] {endpoint} transport error: {e.Message}");
                throw new ApiException(0, e.Message);
            }

            string responseJson = request.downloadHandler.text;

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError($"[GameApiService] {endpoint} network error: {request.error}");
                throw new ApiException(0, request.error);
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                var errorBody = TryParseError(responseJson);
                string message = errorBody?.message ?? request.error;
                int code = errorBody?.code > 0 ? errorBody.code : (int)request.responseCode;

                Debug.LogError($"[GameApiService] {endpoint} HTTP {request.responseCode}: {message}");
                throw new ApiException(code, message);
            }

            Debug.Log($"[GameApiService] {endpoint} response={responseJson}");
            return JsonUtility.FromJson<T>(responseJson);
        }
        
        private static ApiErrorResponse TryParseError(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            try
            {
                return JsonUtility.FromJson<ApiErrorResponse>(json);
            }
            catch
            {
                return null;
            }
        }

        [Serializable]
        private class ApiErrorResponse
        {
            public string status;
            public int code;
            public string message;
        }
    }

    public class ApiException : Exception
    {
        public int HttpCode { get; }

        public ApiException(int code, string message) : base(message)
        {
            HttpCode = code;
        }

        public bool IsNetworkError => HttpCode == 0;
        public bool IsAuthError => HttpCode == 401;
        public bool IsClientError => HttpCode is >= 400 and < 500;
        public bool IsServerError => HttpCode is >= 500;
    }
}