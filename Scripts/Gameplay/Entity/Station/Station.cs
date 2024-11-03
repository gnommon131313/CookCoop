using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Zenject;
using UnityEditor;
using System;
using System.Linq;

internal class Station : Entity
{
    protected Game _game;
    protected GameData _gameData;
    protected CameraManager _cameraManager;

    [SerializeField] private ReactiveProperty<int> _selectedByQuantity = new(0);
    [SerializeField] protected bool _lockForUse = false;

    [SerializeField] protected List<GameObject> _dynamicColors = new();

    internal IReadOnlyReactiveProperty<int> ProductAmountCurrent => _itemHolderHandler.AmountCurrent;
    internal int ProductAmountMax => _itemHolderHandler.AmountMax;

    [Inject]
    private void Construct(
        Game game,
        GameData gameData,
        CameraManager cameraManager)
    {
        _game = game;
        _gameData = gameData;
        _cameraManager = cameraManager;
    }

    internal void WasSelectedByPlayer() =>
        _selectedByQuantity.Value = Math.Clamp(_selectedByQuantity.Value + 1, 0, _gameData.PlayerPool.Length);

    internal void WasUnselectedByPlayer() =>
        _selectedByQuantity.Value = Math.Clamp(_selectedByQuantity.Value - 1, 0, _gameData.PlayerPool.Length);

    protected override void ReactiveSubscription()
    {
        base.ReactiveSubscription();

        _selectedByQuantity
            .Subscribe(value =>
            {
                _dynamicColors.FirstOrDefault(content => content.name == "Select").SetActive(value > 0);

                if (value <= 0)
                    Unuse();
            })
            .AddTo(_disposable);
    }

    internal bool TryUse()
    {
        if (!_itemMetamorphosisHandler.CheckOpportunityChangeActivity()
            || _lockForUse
            || (_item.Value && !_item.Value.ItemIsEmpty.Value))
            return false;

        _itemMetamorphosisHandler.ChangeActivity(true);

        return true;
    }

    internal void Unuse()
    {
        _itemMetamorphosisHandler.ChangeActivity(false);
    }

    internal override Item TryGetItem(Item other)
    {
        if (!_itemHolderHandler.CheckOpportunityGet(_item.Value, other))
        {
            _dynamicColors.FirstOrDefault(content => content.name == "Error").Blink(0.5f, 1);

            return other;
        }

        if (_item.Value == null)
        {
            Item ForEmptyPlace()
            {
                _itemHolderHandler.ChangeAmountCurrent(1);
                _item.Value = other;

                return null;
            }

            return ForEmptyPlace();
        }
        else
        {
            Item ForBusyPlace()
            {
                bool ForStorage()
                {
                    if (!_itemHolderHandler.IsStorage)
                        return false;

                    other.DOMoveAndThenDestroy(_itemHolderHandler.Placement);

                    return true;
                }

                bool ForWarehouse()
                {
                    if (!_itemHolderHandler.IsWarehouse)
                        return false;

                    _itemHolderHandler.ChangeAmountCurrent(1);

                    other.DOMoveAndThenDestroy(_itemHolderHandler.Placement);

                    return true;
                }

                bool TryUniteTwoInstance()
                {
                    if (!_item.Value.TryGetItem(other))
                        return true;

                    if (!other.TryGetItem(_item.Value))
                    {
                        _item.Value = other;

                        return true;
                    }

                    return false;
                }

                Item TrySwap()
                {
                    if (_itemHolderHandler.IsStorage || _itemHolderHandler.IsWarehouse || _itemHolderHandler.LockForGiveAway)
                        return other;

                    Item cache = _item.Value;
                    _item.Value = other;

                    return cache;
                }

                if (ForStorage())
                    return null;

                if (ForWarehouse())
                    return null;

                if (TryUniteTwoInstance())
                    return null;

                if (_item.Value.TryMixingWith(other))
                    return null;

                return TrySwap();
            }

            return ForBusyPlace();
        }
    }

    internal override Item TryGiveAwayItem()
    {
        if (!_item.Value || _itemHolderHandler.LockForGiveAway)
            return null;

        if (!_itemHolderHandler.IsStorage)
            _itemHolderHandler.ChangeAmountCurrent(-1);

        if (_itemHolderHandler.IsStorage || _itemHolderHandler.AmountCurrent.Value > 0)
            return ItemCreate(_item.Value, _itemHolderHandler.Placement.position);

        Item cache = _item.Value;
        _item.Value = null;

        return cache;
    }

    internal override Item TryGiveAwayChunkItem()
    {
        Item innerItem = null;

        if (_item.Value)
            innerItem = _item.Value.TryGiveAwayItem();

        if (innerItem)
            return innerItem;

        return _item.Value.TryGiveAwayChunkSelf();
    }
}