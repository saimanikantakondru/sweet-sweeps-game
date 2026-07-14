using UnityEngine;
using VContainer;
using VContainer.Unity;
using SweetSweeps.Bootstrap;

namespace SweetSweeps.Installers
{
    public class BootstrapLifetimeScope : LifetimeScope
    {
        [SerializeField] private BootstrapController bootstrapController;

        protected override void Configure(IContainerBuilder builder)
        {
            if (bootstrapController == null)
                Debug.LogError("[BootstrapLifetimeScope] bootstrapController is not assigned.");

            builder.RegisterComponent(bootstrapController);
        }
    }
}