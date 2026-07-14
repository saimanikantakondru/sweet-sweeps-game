using SweetSweeps.Core.Contracts;
using SweetSweeps.Infrastructure.UI;

namespace SweetSweeps.Core.Services
{
    public class LoadingScreenService : ILoadingScreenService
    {
        private readonly LoadingScreenView _view;

        public LoadingScreenService(LoadingScreenView view)
        {
            _view = view;
        }

        public void Show(string message = null)
        {
            _view.Show(message);
        }

        public void Hide()
        {
            _view.Hide();
        }
    }
}
