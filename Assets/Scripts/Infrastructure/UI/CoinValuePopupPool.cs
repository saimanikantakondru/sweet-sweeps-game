using UnityEngine;
using System.Collections.Generic;

namespace SweetSweeps.Infrastructure.UI
{
    public class CoinValuePopupPool
    {
        private readonly CoinValuePopupView _prefab;
        private readonly Transform _root;
        private readonly Queue<CoinValuePopupView> _available = new();

        public CoinValuePopupPool(CoinValuePopupView prefab, Transform root, int prewarm)
        {
            _prefab = prefab;
            _root = root;

            if (_prefab == null)
            {
                Debug.LogWarning("[CoinValuePopupPool] Prefab not assigned — popups disabled.");
                return;
            }

            for (int i = 0; i < prewarm; i++)
                _available.Enqueue(CreateInstance());
        }

        public CoinValuePopupView Get()
        {
            if (_prefab == null) return null;
            return _available.Count > 0 ? _available.Dequeue() : CreateInstance();
        }

        public void Return(CoinValuePopupView view)
        {
            if (view == null) return;
            view.StopAndHide();
            _available.Enqueue(view);
        }

        private CoinValuePopupView CreateInstance()
        {
            var view = Object.Instantiate(_prefab, _root);
            view.gameObject.SetActive(false);
            return view;
        }
    }
}
