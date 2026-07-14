using System;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Services
{
    public class ColyseusWebSocketService : IWebSocketService
    {
        private readonly string _serverUrl;

        private ColyseusClient _client;
        private ColyseusRoom<object> _room;

        public bool IsConnected { get; private set; }

        public event Action<SessionDataJson> OnSessionReceived;
        public event Action<StepDataJson> OnStepReceived;
        public event Action<CalamityValuesJson> OnCalamityValuesReceived;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<string> OnConnectionFailed;

        public ColyseusWebSocketService(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public async void Connect(string gamePlayUid)
        {
            try
            {
                Debug.Log($"[ColyseusWebSocketService] Connecting. GamePlayUid={gamePlayUid}");
                _client = new ColyseusClient(_serverUrl);
                var options = new Dictionary<string, object>
                {
                    { "gamePlayUid", gamePlayUid }
                };

                _room = await _client.JoinOrCreate<object>("coincalamity", options);
                IsConnected = true;
                Debug.Log($"[ColyseusWebSocketService] Joined. " +
                          $"RoomId={_room.RoomId} SessionId={_room.SessionId}");

                SubscribeRoomMessages();
                SubscribeRoomLifecycle();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColyseusWebSocketService] Connect failed: {e.Message}");
                OnConnectionFailed?.Invoke(e.Message);
            }
        }

        public async void Disconnect()
        {
            if (_room == null) return;

            try
            {
                await _room.Leave();
                Debug.Log("[ColyseusWebSocketService] Left room.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColyseusWebSocketService] Disconnect error: {e.Message}");
            }
            finally
            {
                IsConnected = false;
                _room = null;
            }
        }

        public async void SimulateUnexpectedDrop()
        {
            if (_room == null) return;

            try
            {
                Debug.LogWarning("[ColyseusWebSocketService] DEBUG: forcing unconsented leave (unexpected drop).");
                await _room.Leave(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ColyseusWebSocketService] SimulateUnexpectedDrop error: {e.Message}");
            }
            finally
            {
                IsConnected = false;
            }
        }

        private void SubscribeRoomMessages()
        {
            _room.OnMessage<SessionDataJson>("round_config", message =>
            {
                Debug.Log($"[ColyseusWebSocketService] Session. " +
                          $"TotalSteps={message.totalSteps} Uid={message.gamePlayUid}");
                OnSessionReceived?.Invoke(message);
            });

            _room.OnMessage<StepDataJson>("step", message =>
            {
                Debug.Log($"[ColyseusWebSocketService] Step={message.step} phase={message.phase}");
                OnStepReceived?.Invoke(message);
            });
            
            _room.OnMessage<CalamityValuesJson>("calamity_values", message =>
            {
                Debug.Log($"[ColyseusWebSocketService] CalamityValues. Count={message.calamity?.Length ?? 0}");
                OnCalamityValuesReceived?.Invoke(message);
            });

            _room.OnMessage<object>("round_end", msg =>
            {
                // now ended by timer
                Debug.Log("[WS] round_end received");
            });
        }

        private void SubscribeRoomLifecycle()
        {
            _room.OnLeave += code =>
            {
                IsConnected = false;
                Debug.Log($"[ColyseusWebSocketService] OnLeave code={code}");
                
                if (code != 4000)
                    OnConnectionFailed?.Invoke($"Connection lost (code {code})");
                else
                    OnDisconnected?.Invoke();
            };

            _room.OnError += (code, message) =>
            {
                Debug.LogError($"[ColyseusWebSocketService] OnError code={code} message={message}");
                OnError?.Invoke(message);
                OnConnectionFailed?.Invoke(message);
            };
        }
        
        public void SendPlayerDeath(CollectionReport data)
        {
            if (_room == null) return;

            Debug.Log($"[ColyseusWebSocketService] Send Player Death");
            _room.Send("player_death", data);
        }
    }
}