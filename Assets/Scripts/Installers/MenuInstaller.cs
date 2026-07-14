using VContainer;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Menu.Contracts;
using SweetSweeps.Menu.Services;
using SweetSweeps.Server.Contracts;
using SweetSweeps.Infrastructure.UI;

namespace SweetSweeps.Installers
{
    public static class MenuInstaller
    {
        public static void Install(IContainerBuilder builder, MenuAudioSettingsSO menuAudioSettings)
        {
            builder.RegisterInstance(menuAudioSettings);

            builder.Register<IMenuService>(resolver =>
                    new MenuService(
                        resolver.Resolve<ISceneService>(),
                        resolver.Resolve<IWorldSelectService>(),
                        resolver.Resolve<IGameSessionService>()),
                Lifetime.Singleton);

            builder.Register<ConsoleService>(Lifetime.Singleton)
                   .As<IConsoleService>()
                   .As<ITickable>();

            builder.RegisterEntryPoint<MenuAudioCoordinator>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<UiButtonClickBinder>();
        }
    }
}