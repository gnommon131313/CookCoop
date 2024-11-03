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
using Unity.VisualScripting;

internal class StationView : MonoBehaviour
{
    protected readonly CompositeDisposable _disposable = new();
    protected SignalBus _signalBus;

    protected Station _station;

    [SerializeField] private TextMeshProUGUI _productAmountText;
    [SerializeField] private Slider _fuelSlider;

    [Inject]
    private void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    protected void Awake()
    {
        _station = transform.parent.GetComponent<Station>();
    }

    private void OnEnable()
    {
        ReactiveSubscription();
    }

    private void OnDisable()
    {
        ReactiveUnSubscription();
    }

    protected virtual void ReactiveSubscription()
    {
        //_station.ProductAmountCurrent
        //    .Subscribe(value =>
        //    {
        //        _productAmountText.gameObject.SetActive(value > 0 && _station.ProductAmountMax > 1);
        //        _productAmountText.text = $"{value} / {_station.ProductAmountMax}";
        //    })
        //    .AddTo(_disposable);

        //_station.FuelCurrent
        //    .Subscribe(value =>
        //    {
        //        _fuelSlider.gameObject.SetActive(value > 0);
        //        _fuelSlider.DOValue(value / _station.FuelMax, 0.1f).SetEase(Ease.Linear);
        //    })
        //    .AddTo(_disposable);
    }

    private void ReactiveUnSubscription() 
        => _disposable.Clear();
}