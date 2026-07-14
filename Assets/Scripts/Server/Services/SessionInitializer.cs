using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SweetSweeps.Infrastructure;
using SweetSweeps.Server.Data;
using SweetSweeps.Server.Contracts;

namespace SweetSweeps.Server.Services
{
    public class SessionInitializer : ISessionInitializer
    {
        private const string DefaultBrand = "sweetsweeps-dev";
        private const string DefaultGame = "coin-calamity";
        private const string DemoTokenPrefsKey = "demo_token";
        private const int DemoTokenRange = 100000;

        private const string ObsoleteProgressKey = "game_progress";

        private readonly IGameApiService _apiService;
        private readonly IGameSessionService _sessionService;

        public string SelectedCurrency { get; private set; }

        public SessionInitializer(IGameApiService apiService, IGameSessionService sessionService)
        {
            _apiService = apiService;
            _sessionService = sessionService;
        }

        public void SetCurrency(string currency)
        {
            if (string.IsNullOrEmpty(currency))
                return;

            SelectedCurrency = currency;
        }

        public void ResolveToken()
        {
            PurgeObsoleteProgress();

            if (_sessionService.IsAuthenticated)
                return;

            var token = UrlParams.GetParam("token");
            if (!string.IsNullOrEmpty(token))
            {
                _sessionService.SetToken(token);
                return;
            }

            var demoToken = ResolveDemoToken();
            _sessionService.SetToken(demoToken);
            Debug.Log($"[SessionInitializer] token not found in URL. Using persisted demo token {demoToken}.");
        }

        private static void PurgeObsoleteProgress()
        {
            if (!EncryptedStorage.Has(ObsoleteProgressKey))
                return;

            EncryptedStorage.Remove(ObsoleteProgressKey);
            Debug.Log("[SessionInitializer] Removed obsolete mid-run progress snapshot.");
        }

        private static string ResolveDemoToken()
        {
            var saved = PlayerPrefs.GetString(DemoTokenPrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                return saved;

            int suffix = Math.Abs(Guid.NewGuid().GetHashCode()) % DemoTokenRange;
            var generated = $"DEMO-{suffix:D5}";

            PlayerPrefs.SetString(DemoTokenPrefsKey, generated);
            PlayerPrefs.Save();
            return generated;
        }

        public async UniTask InitializeAsync()
        {
            var brand = UrlParams.GetParam("brand") ?? DefaultBrand;
            var game = UrlParams.GetParam("game") ?? DefaultGame;
            _sessionService.SetGame(game);

            var response = await _apiService.Initialize(_sessionService.Token, brand, game, SelectedCurrency);
            _sessionService.ApplyInitializeResponse(response);

            Debug.Log($"[SessionInitializer] Initialized. " +
                      $"Session={_sessionService.SessionUid} " +
                      $"Currency={_sessionService.ActiveCurrency} " +
                      $"Balance={_sessionService.CurrentWallet?.totalBalance} " +
                      $"UnfinishedGames={_sessionService.UnfinishedGames?.Length ?? 0}");
        }
    }
}
