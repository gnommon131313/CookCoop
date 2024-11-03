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

internal sealed class ItemView : MonoBehaviour
{
    private readonly CompositeDisposable _disposable = new();
    private SignalBus _signalBus;

    private Item _item;

    [SerializeField] private GameObject _metamorphosisImpactIconFrame;
    [SerializeField] private List<GameObject> _metamorphosisImpactIcons = new List<GameObject>();
    [SerializeField] private GameObject _metamorphosisProgressFrame;
    [SerializeField] private GameObject _metamorphosisProgressWarningFrame;
    [SerializeField] private Slider _metamorphosisProgressSlider;
    [SerializeField] private Image _metamorphosisProgressSliderFill;
    [SerializeField] private GameObject _chunkingIcon;

    //[Inject]
    //private void Construct(SignalBus signalBus)
    //{
    //    _signalBus = signalBus;
    //}

    //private void Awake()
    //{
    //    _item = transform.parent.GetComponent<Item>();
    //}

    //private void OnEnable()
    //{
    //    void ReactiveSubscription()
    //    {
    //        //_signalBus
    //        //    .GetStream<HolderChangeItem>()
    //        //    .Subscribe(evt =>
    //        //    {
    //        //        if (_item != evt.NewItem)
    //        //            return;

    //        //        void DisplayNecessaryTransformationImpactIcons()
    //        //        {
    //        //            void HideAllIcons()
    //        //            {
    //        //                foreach (var icon in _metamorphosisImpactIcons)
    //        //                    icon.SetActive(false);
    //        //            }

    //        //            void DisplayOnlyOneNecessaryIcon()
    //        //            {
    //        //                if (evt.HolderOwner is Station heir)
    //        //                    foreach (var transformation in _item.AccessibleMetamorphosis)
    //        //                        foreach (var icon in _metamorphosisImpactIcons)
    //        //                            if (icon.name == transformation.FromImpact.ToString() && icon.name == evt.HolderOwner.ItemMetamorphImpact.ToString())
    //        //                                icon.SetActive(true);
    //        //            }

    //        //            void DisplayAllAccessibleIcons()
    //        //            {
    //        //                if (evt.HolderOwner is Player heir)
    //        //                    foreach (var transformation in _item.AccessibleMetamorphosis)
    //        //                        foreach (var icon in _metamorphosisImpactIcons)
    //        //                            if (icon.name == transformation.FromImpact.ToString())
    //        //                                icon.SetActive(true);
    //        //            }

    //        //            HideAllIcons();
    //        //            DisplayOnlyOneNecessaryIcon();
    //        //            DisplayAllAccessibleIcons();
    //        //        }

    //        //        DisplayNecessaryTransformationImpactIcons();
    //        //        DisplayChinkingIcon();
    //        //    })
    //        //    .AddTo(_disposable);

    //        _item.MetamorphosisProgressCurrent
    //            .Subscribe(value =>
    //            {
    //                void DisplayProgress(float value)
    //                {
    //                    float valueMax = _item.MetamorphosisProgressMax;

    //                    _metamorphosisProgressFrame.SetActive(value > 0 && _item.MetamorphosisImpact.Value != Item.Metamorphosis.Impact.None);
    //                    //_transformationProgressWarningFrame.SetActive(TargetTransformationResultIsNecessary);

    //                    _metamorphosisProgressSlider.DOValue(value / valueMax, 0.2f).SetEase(Ease.Linear);
    //                    _metamorphosisProgressSliderFill.color = new Color(1 - (value / valueMax), 1, 1 - (value / valueMax));
    //                }

    //                DisplayProgress(value);
    //            })
    //            .AddTo(_disposable);

    //        _item.ItemIsEmpty
    //            .Subscribe(value =>
    //            {
    //                _metamorphosisImpactIconFrame.SetActive(!value);

    //                DisplayChinkingIcon();
    //            })
    //            .AddTo(_disposable);
    //    }

    //    ReactiveSubscription();
    //}

    //private void OnDisable()
    //{
    //    void ReactiveUnSubscription()
    //    {
    //        _disposable.Clear();
    //    }

    //    ReactiveUnSubscription();
    //}

    //private void DisplayChinkingIcon()
    //{
    //    _chunkingIcon.SetActive(false);

    //    //// Ќе показывать когда есть вложеный
    //    //if (_item.Holder.Value is Station station)
    //    //    _chunkingIcon.SetActive(_item.ChunkingAccessible
    //    //        && station.ProductChunkingAccessible
    //    //        && _item.ProductCurrent.Value == null);

    //    //// ѕоказывать когда когда есть вложеный
    //    //if (_product.Holder.Value is Station station)
    //    //    _chunkingIcon.SetActive((_product.ChunkingAccessible
    //    //        && station.ProductChunkingAccessible)
    //    //        || _product.ProductCurrent.Value != null);
    //}
}
