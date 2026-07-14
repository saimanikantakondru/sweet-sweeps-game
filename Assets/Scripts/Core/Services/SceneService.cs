using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class SceneService : ISceneService
    {
        private const string BootstrapScene = "Bootstrap";
        private const string MenuScene = "Menu";
        private const string GameplayScene = "Gameplay";

        private readonly ILoadingScreenService _loadingScreen;

        public SceneService(ILoadingScreenService loadingScreen)
        {
            _loadingScreen = loadingScreen;
        }

        public UniTask LoadBootstrapAsync() => LoadAsync(BootstrapScene);

        public UniTask LoadMenuAsync() => LoadAsync(MenuScene);

        public UniTask LoadGameplayAsync() => LoadAsync(GameplayScene);

        private async UniTask LoadAsync(string sceneName)
        {
            Debug.Log($"[SceneService] Loading {sceneName}.");

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[SceneService] Scene '{sceneName}' is not in Build Settings — cannot load.");
                _loadingScreen.Hide();
                throw new SceneLoadException(sceneName);
            }

            _loadingScreen.Show();
            await UniTask.NextFrame();

            await SceneManager.LoadSceneAsync(sceneName).ToUniTask();
        }
    }

    public class SceneLoadException : Exception
    {
        public SceneLoadException(string sceneName)
            : base($"Scene '{sceneName}' is not in Build Settings.") { }
    }
}
