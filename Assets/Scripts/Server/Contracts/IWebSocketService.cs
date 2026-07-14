using System;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Server.Contracts
{
    public interface IWebSocketService
    {
        bool IsConnected { get; }

        event Action<SessionDataJson> OnSessionReceived;
        event Action<StepDataJson> OnStepReceived;
        event Action<CalamityValuesJson> OnCalamityValuesReceived;
        event Action OnDisconnected;
        event Action<string> OnError;
        event Action<string> OnConnectionFailed;

        void Connect(string gamePlayUid);
        void SendPlayerDeath(CollectionReport data);
        void Disconnect();
        void SimulateUnexpectedDrop();
    }
}