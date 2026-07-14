using System;
using VContainer.Unity;
using SweetSweeps.Data;
using SweetSweeps.Core.Contracts;
using SweetSweeps.Infrastructure.UI;

namespace SweetSweeps.Core
{
    public class CoinValuePopupPresenter : IStartable, IDisposable
    {
        private readonly ICollectibleService _collectibles;
        private readonly CoinValuePopupPool _pool;

        public CoinValuePopupPresenter(ICollectibleService collectibles, CoinValuePopupPool pool)
        {
            _collectibles = collectibles;
            _pool = pool;
        }

        public void Start()
        {
            _collectibles.OnPurpleCoinValueRevealed += OnPurpleCoinValueRevealed;
        }

        public void Dispose()
        {
            _collectibles.OnPurpleCoinValueRevealed -= OnPurpleCoinValueRevealed;
        }

        private void OnPurpleCoinValueRevealed(CoinRevealInfo info)
        {
            var popup = _pool.Get();
            if (popup == null) return;

            popup.Play(info.Value, info.WorldPosition, _pool.Return);
        }
    }
}
