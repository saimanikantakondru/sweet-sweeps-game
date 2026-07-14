using Cysharp.Threading.Tasks;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Infrastructure.UI;

namespace SweetSweeps.Core.Services
{
    public class MessageBoxService : IMessageBoxService
    {
        private readonly MessageBoxView _view;

        public MessageBoxService(MessageBoxView view)
        {
            _view = view;
        }

        public UniTask ShowAsync(string title, string message, string confirmLabel = "OK")
        {
            return _view.ShowAsync(title, message, confirmLabel);
        }
    }
}