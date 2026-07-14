using UnityEngine;
using VContainer;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Menu.UI;

namespace SweetSweeps.Installers
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [Header("Scene References")]
        [SerializeField] private MenuPresenter menuPresenter;

        [Header("Audio")]
        [SerializeField] private MenuAudioSettingsSO menuAudioSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log($"[MenuLifetimeScope] Building. Parent={Parent?.name ?? "NONE"}");

            if (menuPresenter == null)
                Debug.LogError("[MenuLifetimeScope] menuPresenter is not assigned.");

            if (menuAudioSettings == null)
                Debug.LogError("[MenuLifetimeScope] menuAudioSettings is not assigned.");

            MenuInstaller.Install(builder, menuAudioSettings);
            builder.RegisterComponent(menuPresenter);
        }
    }
}