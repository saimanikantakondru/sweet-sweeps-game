using System;
using SweetSweeps.Data;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Core.Contracts
{
    public interface IStepService
    {
        int CurrentStep { get; }
        int TotalSteps { get; }
        bool IsRunning { get; }

        event Action<StepData> OnStepReceived;
        event Action OnAllStepsComplete;
        event Action OnStreamStalled;

        void StartSession(SessionDataJson session);
        void ReceiveStep(StepData step);
        void Stop();
        void Reset();
    }
}