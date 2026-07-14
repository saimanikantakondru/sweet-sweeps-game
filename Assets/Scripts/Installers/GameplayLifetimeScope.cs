using UnityEngine;
using VContainer;
using VContainer.Unity;
using SweetSweeps.Core;
using SweetSweeps.Core.Services;
using SweetSweeps.Data;
using SweetSweeps.Gameplay.Input;
using SweetSweeps.Gameplay.Player;
using SweetSweeps.Gameplay.Calamities;
using SweetSweeps.Infrastructure;
using SweetSweeps.Infrastructure.UI;
using SweetSweeps.Infrastructure.Camera;

namespace SweetSweeps.Installers
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [Header("Data")]
        [SerializeField] private GameSettingsSO gameSettings;
        [SerializeField] private PlayerStatsSO playerStats;
        [SerializeField] private SpawnSystemSettingsSO spawnSystemSettings;

        [Header("Prefabs")]
        [SerializeField] private PlayerController playerControllerPrefab;

        [Header("Scene References")]
        [SerializeField] private CinemachineService cinemachineService;
        [SerializeField] private UIPresenter uiPresenter;
        [SerializeField] private GameBootstrap gameBootstrap;
        [SerializeField] private MobileInputProvider mobileInputProvider;
        [SerializeField] private CoroutineRunner coroutineRunner;
        
        [Header("Calamities")]
        [SerializeField] private FrostCalamityDataSO frostCalamityData;
        [SerializeField] private HeavyCalamityDataSO heavyCalamityData;
        [SerializeField] private WorldDecayCalamityDataSO worldDecayCalamityData;
        [SerializeField] private CalamityThresholdsSO calamityThresholds;
        [SerializeField] private CalamityVisualController calamityVisualController;
        [SerializeField] private Grid levelRoot;
        
        [Header("Audio")]
        [SerializeField] private AudioService audioService;
        [SerializeField] private AudioSettingsSO audioSettings;
        [SerializeField] private SfxLibrarySO sfxLibrary;

        [Header("Coin Value Popup")]
        [SerializeField] private CoinValuePopupView coinValuePopupPrefab;
        [SerializeField] private Transform coinValuePopupRoot;
        [SerializeField] private int coinValuePopupPrewarm = 8;

        private PlayerStatsSO _runtimePlayerStats;

        protected override void Configure(IContainerBuilder builder)
        {
            GameplayInstaller.Install(builder, this);
        }

        protected override void Awake()
        {
            _runtimePlayerStats = Instantiate(playerStats);
            
            base.Awake();
        }
        
        public GameSettingsSO GameSettings => gameSettings;
        public PlayerStatsSO PlayerStats => _runtimePlayerStats;
        public SpawnSystemSettingsSO SpawnSystemSettings => spawnSystemSettings;
        public PlayerController PlayerControllerPrefab => playerControllerPrefab;
        public CinemachineService CinemachineService => cinemachineService;
        public UIPresenter UiPresenter => uiPresenter;
        public GameBootstrap GameBootstrap => gameBootstrap;
        public MobileInputProvider MobileInputProvider => mobileInputProvider;
        public FrostCalamityDataSO FrostCalamityData => frostCalamityData;
        public HeavyCalamityDataSO HeavyCalamityData => heavyCalamityData;
        public WorldDecayCalamityDataSO WorldDecayCalamityData => worldDecayCalamityData;
        public CalamityThresholdsSO CalamityThresholds => calamityThresholds;
        public CalamityVisualController CalamityVisualController => calamityVisualController;
        public Grid LevelRoot => levelRoot;
        public CoroutineRunner CoroutineRunner => coroutineRunner;
        public AudioService AudioService => audioService;
        public AudioSettingsSO AudioSettings => audioSettings;
        public SfxLibrarySO SfxLibrary => sfxLibrary;
        public CoinValuePopupView CoinValuePopupPrefab => coinValuePopupPrefab;
        public Transform CoinValuePopupRoot => coinValuePopupRoot;
        public int CoinValuePopupPrewarm => coinValuePopupPrewarm;
    }
}