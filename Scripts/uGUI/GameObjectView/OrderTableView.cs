using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zenject;
using UniRx;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

internal sealed class OrderTableView : StationView
{
    private OrderTable _orderTable;

    [SerializeField] private Slider _orderActiveSlider;

    private new void Awake()
    {
        base.Awake();

        _orderTable = (OrderTable)_station;
    }

    protected override void ReactiveSubscription()
    {
        base.ReactiveSubscription();

        _orderTable.OrderActiveTimer
            .Subscribe(value =>
            {
                _orderActiveSlider.gameObject.SetActive(value > 0);
                _orderActiveSlider.DOValue(value / _orderTable.OrderActiveTimerMax, 0.1f).SetEase(Ease.Linear);
            })
            .AddTo(_disposable);
    }
}
