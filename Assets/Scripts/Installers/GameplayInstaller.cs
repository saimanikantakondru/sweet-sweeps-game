using UnityEngine;
using VContainer;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Core.StateMachine;
using SweetSweeps.Core.StateMachine.States;
using SweetSweeps.Gameplay.Input;
using SweetSweeps.Gameplay.Level;
using SweetSweeps.Gameplay.Player;
using SweetSweeps.Gameplay.Calamities;
using SweetSweeps.Gameplay.Collectibles;
using SweetSweeps.Infrastructure;
using SweetSweeps.Infrastructure.UI;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Server.Services;

namespace SweetSweeps.Installers
{
    public static class GameplayInstaller
    {
        public static void Install(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            RegisterData(builder, scope);
            RegisterServices(builder);
            RegisterStateMachine(builder);
            RegisterInput(builder, scope);
            RegisterPlayer(builder, scope);
            RegisterLevel(builder, scope);
            RegisterCollectibles(builder, scope);
            RegisterCalamities(builder, scope);
            RegisterSceneComponents(builder, scope);
            RegisterServerServices(builder);
            RegisterCoordinators(builder);
            RegisterAudio(builder, scope);
        }

        private static void RegisterAudio(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.RegisterInstance(scope.SfxLibrary);

            builder.Register<AudioSourcePool>(Lifetime.Singleton)
                   .AsImplementedInterfaces()
                   .AsSelf();

            builder.Register<ISfxService, SfxService>(Lifetime.Singleton);

            builder.Register<BiomeAudioState>(Lifetime.Singleton)
                   .As<IBiomeAudioState>()
                   .AsSelf();

            builder.RegisterEntryPoint<PlayerAudioCoordinator>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CalamityAudioCoordinator>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<UiButtonClickBinder>();
        }

        private static void RegisterCoordinators(IContainerBuilder builder)
        {
            builder.Register<RespawnService>(resolver =>
                new RespawnService(
                    resolver.Resolve<IFrameService>(),
                    resolver.Resolve<GameSettingsSO>().GroundLayer),
                Lifetime.Singleton);

            builder.Register<LevelCoordinator>(Lifetime.Singleton);
            builder.Register<NetworkCoordinator>(Lifetime.Singleton);
            builder.Register<RoundCoordinator>(Lifetime.Singleton);
            builder.Register<AudioCoordinator>(Lifetime.Singleton);
        }

        private static void RegisterData(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.RegisterInstance(scope.GameSettings);
            builder.RegisterInstance(scope.PlayerStats);
            builder.RegisterInstance(scope.CalamityThresholds);
        }

        private static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<TimeService>(Lifetime.Singleton)
                   .As<ITimeService>()
                   .As<ITickable>()
                   .AsSelf();

            builder.Register<IScoreService, ScoreService>(Lifetime.Singleton);
            builder.Register<IPurpleCoinValueService, PurpleCoinValueService>(Lifetime.Singleton);
            builder.Register<IHealthService>(resolver =>
            {
                var stats = resolver.Resolve<PlayerStatsSO>();
                var healthService = new HealthService(stats);
                return healthService;
            }, Lifetime.Singleton);
            builder.Register<IFrameService>(resolver =>
            {
                var stats = resolver.Resolve<PlayerStatsSO>();
                var coroutineRunner = resolver.Resolve<CoroutineRunner>();
                return new InvincibilityService(stats, coroutineRunner);
            }, Lifetime.Singleton);
            builder.Register<IPhaseService>(resolver =>
                    new PhaseService(resolver.Resolve<CalamityThresholdsSO>()),
                Lifetime.Singleton);
            builder.Register<IMovementModifierRegistry, MovementModifierRegistry>(Lifetime.Singleton);
            builder.Register<IPlatformService, PlatformService>(Lifetime.Singleton);

            
            builder.Register<StepService>(Lifetime.Singleton)
                .As<IStepService>()
                .As<ITickable>()
                .AsSelf();
            
            builder.Register<ILevelHistoryService, LevelHistoryService>(Lifetime.Singleton);
        }

        private static void RegisterStateMachine(IContainerBuilder builder)
        {
            builder.Register<IdleState>(Lifetime.Singleton).AsSelf();
            builder.Register<CountdownState>(Lifetime.Singleton).AsSelf();
            builder.Register<PlayingState>(Lifetime.Singleton).AsSelf();
            builder.Register<PausedState>(Lifetime.Singleton).AsSelf();
            builder.Register<GameOverState>(Lifetime.Singleton).AsSelf();
            builder.Register<ResettingState>(Lifetime.Singleton).AsSelf();

            builder.Register<GameStateMachine>(Lifetime.Singleton)
                   .As<IGameStateMachine>()
                   .AsSelf();

            builder.Register<StateMachineInitializer>(Lifetime.Singleton)
                   .As<IInitializable>();
        }
        
        private static void RegisterInput(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var isMobile = Infrastructure.DeviceDetector.IsMobile();

            if (isMobile)
            {
                scope.MobileInputProvider.gameObject.SetActive(true);
                builder.RegisterComponentInHierarchy<MobileInputProvider>().As<IInputProvider>();
            }
            else
            {
                scope.MobileInputProvider.gameObject.SetActive(false);
                builder.Register<InputProvider>(Lifetime.Singleton).As<IInputProvider>();
            }
#elif UNITY_STANDALONE || UNITY_EDITOR
            scope.MobileInputProvider.gameObject.SetActive(false);
            builder.Register<InputProvider>(Lifetime.Singleton).As<IInputProvider>();
#else
            scope.MobileInputProvider.gameObject.SetActive(true);
            builder.RegisterComponentInHierarchy<MobileInputProvider>().As<IInputProvider>();
#endif
        }

        private static void RegisterPlayer(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.RegisterComponentInNewPrefab(scope.PlayerControllerPrefab, Lifetime.Singleton)
                   .As<IPlayerController>()
                   .AsSelf();
        }

        private static void RegisterLevel(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.Register<ILevelService>(resolver =>
                new LevelService(
                    resolver.Resolve<ILevelCatalog>(),
                    resolver.Resolve<ILevelHistoryService>(),
                    scope.LevelRoot),
                Lifetime.Singleton);
        }

        private static void RegisterCollectibles(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            var settings = scope.SpawnSystemSettings;
            
            builder.Register<SpawnPointScanner>(_ =>
                    new SpawnPointScanner(
                        settings.HeightAboveGround,
                        settings.MinDistanceBetweenPoints,
                        settings.GroundMask,
                        settings.ObstacleMask),
                Lifetime.Singleton);

            builder.Register<SpawnPoolService>(_ =>
                        new SpawnPoolService(), Lifetime.Singleton).AsSelf();

            builder.Register<SpawnPointSelector>(_ =>
                    new SpawnPointSelector(
                        Camera.main,
                        settings.MinAheadDistance),
                Lifetime.Singleton);

            builder.Register<ICollectibleService>(resolver =>
                    new CollectibleService(
                        resolver.Resolve<IScoreService>(),
                        resolver.Resolve<ICollectionTracker>(),
                        resolver.Resolve<IPhaseService>(),
                        resolver.Resolve<IPurpleCoinValueService>(),
                        resolver.Resolve<SpawnPoolService>(),
                        resolver.Resolve<SpawnPointSelector>()),
                Lifetime.Singleton);

            builder.Register<CoinValuePopupPool>(_ =>
                    new CoinValuePopupPool(
                        scope.CoinValuePopupPrefab,
                        scope.CoinValuePopupRoot,
                        scope.CoinValuePopupPrewarm),
                Lifetime.Singleton);

            builder.RegisterEntryPoint<CoinValuePopupPresenter>(Lifetime.Singleton);
        }

        private static void RegisterSceneComponents(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.RegisterComponent(scope.CinemachineService).As<ICameraService>();
            builder.RegisterComponent(scope.GameBootstrap);
            builder.RegisterComponent(scope.UiPresenter);
            builder.RegisterComponent(scope.CoroutineRunner);
            builder.RegisterInstance(scope.AudioSettings);
        }
        
        private static void RegisterServerServices(IContainerBuilder builder)
        {
            builder.Register<ICollectionTracker, CollectionTracker>(Lifetime.Singleton);
        }
        
        private static void RegisterCalamities(IContainerBuilder builder, GameplayLifetimeScope scope)
        {
            builder.Register<ICalamityService>(resolver =>
            {
                var modifierRegistry = resolver.Resolve<IMovementModifierRegistry>();
                var playerController = resolver.Resolve<PlayerController>();
                var coroutineRunner = resolver.Resolve<CoroutineRunner>();
                var groundMask = scope.SpawnSystemSettings.GroundMask;
                ICalamityVisualPresenter visualPresenter = scope.CalamityVisualController;

                var frostEffect = new FrostCalamityEffect(
                    scope.FrostCalamityData,
                    modifierRegistry,
                    scope.LevelRoot,
                    coroutineRunner,
                    groundMask);

                var heavyEffect = new HeavyCalamityEffect(
                    scope.HeavyCalamityData,
                    modifierRegistry,
                    scope.LevelRoot,
                    coroutineRunner,
                    groundMask);

                var decayEffect = new WorldDecayCalamityEffect(
                    scope.WorldDecayCalamityData,
                    modifierRegistry,
                    scope.LevelRoot,
                    coroutineRunner,
                    playerController.transform);

                var frostEffects = new ICalamityEffect[]
                {
                    frostEffect,
                    new CalamityVisualEffect(scope.FrostCalamityData.VisualProfile, visualPresenter)
                };

                var heavyEffects = new ICalamityEffect[]
                {
                    heavyEffect,
                    new CalamityVisualEffect(scope.HeavyCalamityData.VisualProfile, visualPresenter)
                };

                var service = new CalamityService();
                service.RegisterCalamity(new FrostCalamity(frostEffects));
                service.RegisterCalamity(new HeavyCalamity(heavyEffects));
                service.RegisterCalamity(new WorldDecayCalamity(decayEffect));

                return service;
            }, Lifetime.Singleton);
        }
    }
}
