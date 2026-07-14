using System;
using UnityEngine;
using VContainer;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using SweetSweeps.Data;
using SweetSweeps.Server.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Gameplay.Input;
using SweetSweeps.Menu.Contracts;
using SweetSweeps.Server.Services;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Menu.UI
{
    public class MenuPresenter : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private GameSettingsSO gameSettings;
        [SerializeField] private CurrencyVisualsSO currencyVisuals;

        [Header("Views")]
        [SerializeField] private TopBarView topBarView;
        [SerializeField] private BottomBarView bottomBarView;
        [SerializeField] private InfoPopupView infoPopupView;
        [SerializeField] private BetLadderPopupView betLadderPopupView;
        [SerializeField] private CurrencySelectorView currencySelectorView;

        private IMenuService _menuService;
        private IConsoleService _consoleService;
        private IWorldSelectService _worldSelectService;
        private IGameSessionService _sessionService;
        private IGameApiService _apiService;
        private ISessionInitializer _sessionInitializer;
        private ILoadingScreenService _loadingScreen;
        private IMessageBoxService _messageBox;

        private bool _menuReady;
        private bool _isStartingRound;

        private PlayerInputActions _input;
        private int _navX;
        private int _navY;

        private const float NavThreshold = 0.5f;

        private void Awake()
        {
            _input = new PlayerInputActions();
        }

        private void OnEnable()  => _input.Enable();
        private void OnDisable() => _input.Disable();

        [Inject]
        public void Construct(
            IMenuService          menuService,
            IConsoleService       consoleService,
            IWorldSelectService   worldSelectService,
            IGameSessionService   sessionService,
            IGameApiService       apiService,
            ISessionInitializer   sessionInitializer,
            ILoadingScreenService loadingScreen,
            IMessageBoxService    messageBox)
        {
            _menuService        = menuService;
            _consoleService     = consoleService;
            _worldSelectService = worldSelectService;
            _sessionService     = sessionService;
            _apiService         = apiService;
            _sessionInitializer = sessionInitializer;
            _loadingScreen      = loadingScreen;
            _messageBox         = messageBox;
        }

        private async void Start()
        {
            betLadderPopupView?.Bind(_menuService);

            SubscribeEvents();
            ApplyInitialConsoleState();
            bottomBarView.SetFeedback(string.Empty);

            await EnsureSessionAsync();

            if (!_sessionService.HasUnfinishedGame)
                _worldSelectService.SelectRandom();

            RefreshBet();
            RefreshWallet();

            _menuReady = true;
        }

        private void Update()
        {
            if (!_menuReady || !CanHandleMenuInput())
            {
                _navX = 0;
                _navY = 0;
                return;
            }

            if (_input.Player.Jump.WasPressedThisFrame())
            {
                HandlePlayPressed();
                return;
            }

            if (_sessionService.HasUnfinishedGame)
                return;

            HandleBetNavigation(_input.Player.Move.ReadValue<Vector2>());
        }

        private void HandleBetNavigation(Vector2 move)
        {
            int x = Mathf.Abs(move.x) >= NavThreshold ? (int)Mathf.Sign(move.x) : 0;
            int y = Mathf.Abs(move.y) >= NavThreshold ? (int)Mathf.Sign(move.y) : 0;

            if (x != _navX)
            {
                if (x > 0 && _menuService.CanIncrement) _menuService.IncrementBet();
                else if (x < 0 && _menuService.CanDecrement) _menuService.DecrementBet();
                _navX = x;
            }

            if (y != _navY)
            {
                if (y > 0) _menuService.SelectMax();
                else if (y < 0) _menuService.SelectMin();
                _navY = y;
            }
        }

        private bool CanHandleMenuInput()
        {
            if (betLadderPopupView != null && betLadderPopupView.IsOpen) return false;
            if (infoPopupView != null && infoPopupView.IsOpen) return false;
            return true;
        }

        private async UniTask EnsureSessionAsync()
        {
            _sessionInitializer.ResolveToken();

            if (gameSettings.UseSimulation)
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                _sessionService.DebugSetBalance(gameSettings.SimulationBalance);
#endif
                _loadingScreen.Hide();
                return;
            }

            _loadingScreen.Show();
            bottomBarView.SetPlayInteractable(false);

            try
            {
                while (true)
                {
                    try
                    {
                        await _sessionInitializer.InitializeAsync();
                        return;
                    }
                    catch (ApiException e) when (e.IsNetworkError)
                    {
                        await AskRetry("Connection Error", "Cannot reach server. Check your network.");
                    }
                    catch (ApiException e)
                    {
                        await AskRetry("Error", e.Message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[MenuPresenter] Init failed: {e.Message}");
                        await AskRetry("Error", "Something went wrong.");
                    }
                }
            }
            finally
            {
                _loadingScreen.Hide();
            }
        }

        private UniTask AskRetry(string title, string message)
        {
            return _messageBox.ShowAsync(title, message, "Retry");
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            _input?.Dispose();
        }

        private void SubscribeEvents()
        {
            bottomBarView.BetSelector.OnIncrement    += _menuService.IncrementBet;
            bottomBarView.BetSelector.OnDecrement    += _menuService.DecrementBet;
            bottomBarView.BetSelector.OnMinPressed   += _menuService.SelectMin;
            bottomBarView.BetSelector.OnMaxPressed   += _menuService.SelectMax;
            bottomBarView.BetSelector.OnLadderPressed += HandleLadderPressed;
            bottomBarView.OnPlayPressed              += HandlePlayPressed;

            if (betLadderPopupView != null)
                betLadderPopupView.OnStakeSelected += HandleStakeSelected;

            _menuService.OnBetChanged           += RefreshBet;
            _menuService.OnAffordabilityChanged += RefreshBet;

            _sessionService.OnWalletsChanged += RefreshWallet;

            if (currencySelectorView != null)
                currencySelectorView.OnCurrencySelected += HandleCurrencySelected;

            topBarView.OnFullscreenPressed += HandleFullscreenPressed;
            topBarView.OnSoundPressed      += HandleSoundPressed;
            topBarView.OnInfoPressed       += HandleInfoPressed;
            topBarView.OnMenuPressed       += HandleMenuPressed;

            _consoleService.OnMuteChanged       += topBarView.SetMuted;
            _consoleService.OnFullscreenChanged += topBarView.SetFullscreen;
        }

        private void UnsubscribeEvents()
        {
            if (_menuService != null)
            {
                bottomBarView.BetSelector.OnIncrement     -= _menuService.IncrementBet;
                bottomBarView.BetSelector.OnDecrement     -= _menuService.DecrementBet;
                bottomBarView.BetSelector.OnMinPressed    -= _menuService.SelectMin;
                bottomBarView.BetSelector.OnMaxPressed    -= _menuService.SelectMax;

                _menuService.OnBetChanged           -= RefreshBet;
                _menuService.OnAffordabilityChanged -= RefreshBet;
            }

            bottomBarView.BetSelector.OnLadderPressed -= HandleLadderPressed;
            bottomBarView.OnPlayPressed               -= HandlePlayPressed;

            if (betLadderPopupView != null)
                betLadderPopupView.OnStakeSelected -= HandleStakeSelected;

            if (_sessionService != null)
                _sessionService.OnWalletsChanged -= RefreshWallet;

            if (currencySelectorView != null)
                currencySelectorView.OnCurrencySelected -= HandleCurrencySelected;

            topBarView.OnFullscreenPressed -= HandleFullscreenPressed;
            topBarView.OnSoundPressed      -= HandleSoundPressed;
            topBarView.OnInfoPressed       -= HandleInfoPressed;
            topBarView.OnMenuPressed       -= HandleMenuPressed;

            if (_consoleService != null)
            {
                _consoleService.OnMuteChanged       -= topBarView.SetMuted;
                _consoleService.OnFullscreenChanged -= topBarView.SetFullscreen;
            }
        }

        private void ApplyInitialConsoleState()
        {
            topBarView.SetMuted(_consoleService.IsMuted);
            topBarView.SetFullscreen(_consoleService.IsFullscreen);
            // topBarView.SetFullscreenSupported(_consoleService.FullscreenSupported);
        }

        private void RefreshBet()
        {
            if (currencyVisuals != null)
                bottomBarView.BetSelector.SetCurrencyIcon(currencyVisuals.GetIcon(_sessionService.ActiveCurrency));

            if (_sessionService.HasUnfinishedGame)
            {
                bottomBarView.SetResumeMode(true);
                bottomBarView.BetSelector.RenderLocked(
                    _sessionService.CurrentRoundData?.totalWager ?? _menuService.SelectedBet);
                bottomBarView.SetPlayInteractable(true);
                betLadderPopupView?.Refresh();
                return;
            }

            bottomBarView.SetResumeMode(false);
            bottomBarView.BetSelector.Render(
                _menuService.SelectedBet,
                _menuService.CanIncrement,
                _menuService.CanDecrement);

            bool insufficient = !_menuService.HasAffordableBet || !_menuService.CanPlay;
            bottomBarView.SetInsufficientBalance(insufficient);
            bottomBarView.SetPlayInteractable(_menuService.CanPlay);

            betLadderPopupView?.Refresh();
        }

        private void RefreshWallet()
        {
            topBarView.Balance.Render(_sessionService.CurrentWallet);

            if (currencySelectorView != null)
                currencySelectorView.Refresh(_sessionService.ActiveCurrency, !_sessionService.HasUnfinishedGame);
        }

        private async void HandleCurrencySelected(string currency)
        {
            if (!_menuReady || _isStartingRound) return;
            if (_sessionService.HasUnfinishedGame) return;
            if (currency == _sessionService.ActiveCurrency) return;
            if (gameSettings.UseSimulation) return;

            _menuReady = false;
            currencySelectorView.Refresh(_sessionService.ActiveCurrency, false);
            _sessionInitializer.SetCurrency(currency);

            await EnsureSessionAsync();

            _worldSelectService.SelectRandom();
            RefreshBet();
            RefreshWallet();

            _menuReady = true;
        }

        private void HandleFullscreenPressed() => _consoleService.ToggleFullscreen();
        private void HandleSoundPressed()      => _consoleService.ToggleMute();
        private void HandleInfoPressed()       => infoPopupView?.Open();
        private void HandleMenuPressed()       => infoPopupView?.Open();
        private void HandleLadderPressed()     => betLadderPopupView?.Open();

        private void HandleStakeSelected(int index)
        {
            _menuService.SelectByIndex(index);
            betLadderPopupView?.Close();
        }

        private async void HandlePlayPressed()
        {
            if (_isStartingRound)
                return;

            if (!_menuService.CanPlay)
            {
                bottomBarView.SetFeedback("Insufficient balance.");
                return;
            }

            if (!_sessionService.IsAuthenticated && !gameSettings.UseSimulation)
            {
                bottomBarView.SetFeedback("Not connected.");
                await _messageBox.ShowAsync("Not Connected", "Please restart the game.");
                return;
            }

            _isStartingRound = true;
            bottomBarView.SetPlayBusy(true);
            bottomBarView.SetFeedback("Starting round...");

            if (gameSettings.UseSimulation || _sessionService.HasUnfinishedGame)
            {
                _menuService.Play();
                return;
            }

            try
            {
                var response = await _apiService.StartRound(
                    _sessionService.SessionUid,
                    _sessionService.Game,
                    _menuService.SelectedBet);

                _sessionService.ApplyStartResponse(response);

                Debug.Log($"[MenuPresenter] StartRound OK. " +
                          $"GamePlay={_sessionService.GamePlayUid} " +
                          $"Steps={_sessionService.CurrentRoundData?.totalSteps} " +
                          $"CalamityAt={_sessionService.CurrentRoundData?.calamityStartStep}");

                _menuService.Play();
            }
            catch (ApiException e) when (e.IsNetworkError)
            {
                bottomBarView.SetFeedback(string.Empty);
                bottomBarView.SetPlayBusy(false);
                _isStartingRound = false;
                await _messageBox.ShowAsync("Connection Error", "Cannot reach server. Check your network.");
            }
            catch (ApiException e) when (e.HttpCode == 402)
            {
                bottomBarView.SetFeedback(string.Empty);
                bottomBarView.SetPlayBusy(false);
                _isStartingRound = false;
                await _messageBox.ShowAsync("Insufficient Balance", e.Message);
            }
            catch (ApiException e)
            {
                bottomBarView.SetFeedback(string.Empty);
                bottomBarView.SetPlayBusy(false);
                _isStartingRound = false;
                await _messageBox.ShowAsync("Error", e.Message);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MenuPresenter] Unexpected: {e.Message}");
                bottomBarView.SetFeedback("Something went wrong.");
                bottomBarView.SetPlayBusy(false);
                _isStartingRound = false;
            }
        }
    }
}