using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SweetSweeps.Menu.Contracts;

namespace SweetSweeps.Menu.UI
{
    public class BetLadderPopupView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private BetLadderItemView itemPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        private readonly List<BetLadderItemView> _items = new();

        private IMenuService _menuService;

        public bool IsOpen => root != null && root.activeSelf;

        public event Action<int> OnStakeSelected;

        private void Awake()
        {
            if (closeButton    != null) closeButton.onClick.AddListener(Close);
            if (backdropButton != null) backdropButton.onClick.AddListener(Close);

            Close();
        }

        public void Bind(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public void Open()
        {
            if (_menuService == null)
            {
                Debug.LogError("[BetLadderPopupView] Bind() must be called before Open().");
                return;
            }

            Rebuild();
            if (root != null) root.SetActive(true);
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        public void Refresh()
        {
            if (!IsOpen) return;
            RefreshStates();
        }

        private void Rebuild()
        {
            EnsureItemCount(_menuService.BetLadder.Length);

            for (int i = 0; i < _menuService.BetLadder.Length; i++)
            {
                var item = _items[i];
                item.gameObject.SetActive(true);
                item.Render(
                    i,
                    _menuService.BetLadder[i],
                    i == _menuService.SelectedBetIndex,
                    _menuService.IsAffordable(i));
            }

            for (int i = _menuService.BetLadder.Length; i < _items.Count; i++)
                _items[i].gameObject.SetActive(false);
        }

        private void RefreshStates()
        {
            for (int i = 0; i < _menuService.BetLadder.Length && i < _items.Count; i++)
            {
                _items[i].Render(
                    i,
                    _menuService.BetLadder[i],
                    i == _menuService.SelectedBetIndex,
                    _menuService.IsAffordable(i));
            }
        }

        private void EnsureItemCount(int required)
        {
            while (_items.Count < required)
            {
                var item = Instantiate(itemPrefab, itemsContainer);
                item.OnPressed += HandleItemPressed;
                _items.Add(item);
            }
        }

        private void HandleItemPressed(int index)
        {
            OnStakeSelected?.Invoke(index);
        }

        private void OnDestroy()
        {
            if (closeButton    != null) closeButton.onClick.RemoveAllListeners();
            if (backdropButton != null) backdropButton.onClick.RemoveAllListeners();

            foreach (var item in _items)
                if (item != null) item.OnPressed -= HandleItemPressed;
        }
    }
}
