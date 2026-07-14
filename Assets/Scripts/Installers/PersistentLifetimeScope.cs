using UnityEngine;
using VContainer;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Core.Services;
using SweetSweeps.Server.Data;
using SweetSweeps.Menu.Contracts;
using SweetSweeps.Menu.Services;
using SweetSweeps.Server.Services;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Infrastructure.UI;

namespace SweetSweeps.Installers
{
    public class PersistentLifetimeScope : LifetimeScope
    {
        [SerializeField] private MessageBoxView messageBoxView;
        [SerializeField] private LoadingScreenView loadingScreenView;
        [SerializeField] private AudioService audioService;
        [SerializeField] private AudioMixerConfigSO audioMixerConfig;
        [SerializeField] private SoundDefinition uiClickSound;
        [SerializeField] private WorldDataSO[] worlds;
        [SerializeField] private LevelCatalogSO levelCatalog;

        protected override void Configure(IContainerBuilder builder)
        {
            var serverSettings = Resources.Load<ServerSettings>("ServerSettings");

            if (serverSettings == null)
                Debug.LogError("[PersistentLifetimeScope] BackendSettings not found in Resources.");

            if (audioService == null)
                Debug.LogError("[PersistentLifetimeScope] audioService is not assigned.");

            if (audioMixerConfig == null)
                Debug.LogError("[PersistentLifetimeScope] audioMixerConfig is not assigned.");

            if (loadingScreenView == null)
                Debug.LogError("[PersistentLifetimeScope] loadingScreenView is not assigned.");

            if (worlds == null || worlds.Length == 0)
                Debug.LogError("[PersistentLifetimeScope] worlds is empty.");

            if (levelCatalog == null)
                Debug.LogError("[PersistentLifetimeScope] levelCatalog is not assigned.");

            builder.RegisterComponent(audioService).As<IAudioService>();

            builder.RegisterInstance(audioMixerConfig);
            builder.Register<AudioVolumeService>(Lifetime.Singleton)
                   .AsImplementedInterfaces()
                   .AsSelf();

            builder.Register<UiAudioService>(Lifetime.Singleton)
                   .As<IUiAudioService>()
                   .WithParameter(uiClickSound);

            builder.Register<IWorldSelectService>(_ =>
                new WorldSelectService(worlds), Lifetime.Singleton);

            builder.RegisterInstance(levelCatalog).As<ILevelCatalog>();
            builder.RegisterEntryPoint<SweetSweeps.Cheats.CheatBinder>(Lifetime.Singleton);

            builder.Register<IGameSessionService, GameSessionService>(Lifetime.Singleton);
            builder.Register<ISceneService, SceneService>(Lifetime.Singleton);

            builder.Register<IGameApiService>(_ =>
                    new GameApiService(
                        serverSettings.ApiBaseUrl,
                        serverSettings.RequestTimeoutSeconds),
                Lifetime.Singleton);

            builder.Register<ISessionInitializer>(resolver =>
                    new SessionInitializer(
                        resolver.Resolve<IGameApiService>(),
                        resolver.Resolve<IGameSessionService>()),
                Lifetime.Singleton);

            builder.Register<ColyseusWebSocketService>(_ =>
                        new ColyseusWebSocketService(serverSettings.WebSocketUrl),
                    Lifetime.Singleton)
                .As<IWebSocketService>()
                .AsSelf();

            builder.RegisterComponent(messageBoxView);
            builder.Register<IMessageBoxService>(resolver =>
                    new MessageBoxService(resolver.Resolve<MessageBoxView>()),
                Lifetime.Singleton);

            builder.RegisterComponent(loadingScreenView);
            builder.Register<ILoadingScreenService>(resolver =>
                    new LoadingScreenService(resolver.Resolve<LoadingScreenView>()),
                Lifetime.Singleton);
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            Debug.Log("[PersistentLifetimeScope] DontDestroyOnLoad set.");
        }
    }
}
