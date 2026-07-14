using VContainer.Unity;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Cheats
{
    public class CheatBinder : IStartable
    {
        private readonly ILevelCatalog _catalog;

        public CheatBinder(ILevelCatalog catalog)
        {
            _catalog = catalog;
        }

        public void Start()
        {
            SROptions.Bind(_catalog);
        }
    }
}
