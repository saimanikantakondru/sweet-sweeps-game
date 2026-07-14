using UnityEngine;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Data;
using SweetSweeps.Server.Data;

namespace SweetSweeps.Core
{
    public static class SimulationCoordinator
    {
        public static void Run(
            IStepService stepService,
            StepService stepServiceConcrete,
            LevelPresetSO preset,
            StepSimulationPresetSO simulationPreset)
        {
            string endingCalamityId = preset?.CalamityBinding?
                .ResolveEndingCalamityId() ?? "world_decay";

            int total;
            int decayStartStep;

            if (simulationPreset != null)
            {
                total = simulationPreset.TotalSteps;
                decayStartStep = Mathf.RoundToInt(total * 0.8f);
            }
            else
            {
                total = 30;
                decayStartStep = Mathf.RoundToInt(total * 0.6f);
            }

            var session = new SessionDataJson
            {
                totalSteps = total,
                gamePlayUid = "sim-local"
            };

            stepServiceConcrete.StartSession(session);

            for (int i = 1; i <= total; i++)
            {
                bool isDecayPhase = i >= decayStartStep;

                var step = new StepData
                {
                    step = i,
                    phase = isDecayPhase ? "calamity" : "normal",
                    goldCoins = simulationPreset != null
                        ? Random.Range(simulationPreset.GoldCoinsRange.x,
                                       simulationPreset.GoldCoinsRange.y)
                        : Random.Range(1, 3),
                    purpleCoins = simulationPreset != null
                        ? Random.Range(simulationPreset.SsCoinsRange.x,
                                       simulationPreset.SsCoinsRange.y)
                        : Random.Range(0, 2),
                    spikedCandies = 0,
                    sourCandies = simulationPreset != null
                        ? Random.Range(simulationPreset.SourCandiesRange.x,
                                       simulationPreset.SourCandiesRange.y)
                        : Random.Range(0, 2)
                };

                stepService.ReceiveStep(step);
            }

            Debug.Log($"[SimulationCoordinator] Steps={total} " +
                      $"DecayFrom={decayStartStep} " +
                      $"EndingCalamity={endingCalamityId}");
        }
    }
}