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

internal sealed class Item : Entity
{
    [SerializeField] private string _type;
    [SerializeField] private Classification _class;
    [SerializeField] private GameObject _model;

    private ReactiveProperty<Item.HolderHandler> _ownItemHolderHandler = new();

    [SerializeField] private Metamorphosing _metamorphosing = new();
    [SerializeField] private List<Mixing> _accessibleMixing = new();
    [SerializeField] private Chunking _accessibleChunk = new();
    [SerializeField] private View _view = new();

    private Tween _tweenLocalMove;
    private Tween _tweenResizeModel;

    internal string Type => _type;
    internal Classification Class => _class;

    internal readonly Subject<Unit> OnDestroyed = new();
    internal IReadOnlyList<Metamorphosing.AccessibleMetamorphosis> AccessibleMetamorphosis => _metamorphosing.AccessibleMetamorphoses;
    internal IReadOnlyReactiveProperty<Metamorphosing.Impact> MetamorphosisImpact => _metamorphosing.ImpactProp;
    internal IReadOnlyReactiveProperty<float> MetamorphosisProgressCurrent => _metamorphosing.ProgressCurrent;
    internal float MetamorphosisProgressMax => _metamorphosing.ProgressMax;

    protected override void OnEnable()
    {
        base.OnEnable();

        _metamorphosing.OnEnable(this);
        _view.OnEnable(this);
    }

    private void OnDestroy()
    {
        OnDestroyed.OnNext(Unit.Default);
        OnDestroyed.OnCompleted();
    }

    internal override Item TryGetItem(Item other)
    {
        if (!_itemHolderHandler.CheckOpportunityGet(_item.Value, other))
            return other;

        _item.Value = other;

        _itemHolderHandler.ChangeAmountCurrent(1);

        return null;
    }

    internal override Item TryGiveAwayItem()
    {
        if (!_item.Value || _itemHolderHandler.LockForGiveAway)
            return null;

        _itemHolderHandler.ChangeAmountCurrent(-1);

        Item cache = _item.Value;
        _item.Value = null;

        return cache;
    }

    internal override Item TryGiveAwayChunkItem()
    {
        return null;
    }

    internal Item TryGiveAwayChunkSelf()
    {
        if (_ownItemHolderHandler == null
           || _ownItemHolderHandler.Value.LockForGiveAway
           || _ownItemHolderHandler.Value.IsStorage
           || _ownItemHolderHandler.Value.IsWarehouse
           || !_accessibleChunk.Result
           || !_accessibleChunk.TurnInto)
            return null;

        Item cacheResult = ItemCreate(_accessibleChunk.Result, transform.position);
        ReplaceYourself(_accessibleChunk.TurnInto);

        return cacheResult;
    }

    internal Item TryMixingWith(Item other)
    {
        if (_item.Value || !other.ItemIsEmpty.Value)
            return null;

        Item result = null;

        foreach (var value in _accessibleMixing)
            if (other.Type == value.With.Type)
                result = value.Result;

        if (!result)
            foreach (var value in other._accessibleMixing)
                if (_type == value.With.Type)
                    result = value.Result;

        if (!result)
            return null;

        MonoBehaviour.Destroy(other.gameObject);
        ReplaceYourself(result);

        return result;
    }

    internal void Setup(Transform parent, Item.HolderHandler ownItemHolderHandler = null)
    {
        _ownItemHolderHandler.Value = ownItemHolderHandler;
        enabled = true; // иногда непонятно почему скрипт на обьекта может выключиться (часто при отдаче заказа и последующем замене его на трансформированый вложеный обьект)
        //_metamorphosis.SetValues(ItemMetamorph.Influence.None, 1);

        transform.SetParent(parent);
        transform.localRotation = Quaternion.identity;

        void DOSetupTransform()
        {
            if (_tweenLocalMove != null && _tweenLocalMove.IsActive())
                _tweenLocalMove.Kill();

            _tweenLocalMove = transform
               .DOLocalMove(Vector3.zero, 2.0f)
               .SetEase(Ease.OutElastic)
               .SetLink(gameObject);
        }

        void DOResizeModel()
        {
            if (_tweenResizeModel != null && _tweenResizeModel.IsActive())
                _tweenResizeModel.Kill();

            _model.transform.localScale = new(0.75f, 0.75f, 0.75f);

            _tweenResizeModel = _model.transform
                .DOScale(1.0f, 2.0f)
                .SetEase(Ease.OutElastic)
                .SetLink(gameObject);
        }

        DOSetupTransform();
        DOResizeModel();
    }

    internal void SetMetamorphicValues(Metamorphosing.Impact impact, float multiplier)
        => _metamorphosing.SetValues(impact, multiplier);

    internal void DOMoveAndThenDestroy(Transform parent)
    {
        DestroyView();

        if (_tweenLocalMove != null && _tweenLocalMove.IsActive())
            _tweenLocalMove.Kill();

        transform.SetParent(parent);

        _tweenLocalMove = transform
           .DOMove(parent.position, 2.0f)
           .SetEase(Ease.OutElastic)
           .SetLink(gameObject)
           .OnComplete(() => { Destroy(gameObject); });
    }

    internal void DestroyView()
        => Destroy(_view.GameObject); 

    private void ReplaceYourself(Item other)
    {
        _signalBus.TryFire(new ReplaceItem(other, _ownItemHolderHandler.Value));

        Destroy(gameObject);
    }
    
    internal enum Classification
    {
        Food = 1 << 0,
        Plate = 1 << 1,
        Garbage = 1 << 30,
        Trash = 1 << 31,
    }

    [Serializable]
    internal sealed class Metamorphosing
    {
        private Item _baseExternalClass;

        [SerializeField] private List<AccessibleMetamorphosis> _accessibleMetamorphoses = new();
        private ReactiveProperty<Impact> _impact = new();
        private ReactiveProperty<float> _progressCurrent = new();
        [SerializeField] private float _progressMax = 1;
        private float _progressMultiplier = 1;
        private Item _targetResult;

        internal IReadOnlyList<AccessibleMetamorphosis> AccessibleMetamorphoses => _accessibleMetamorphoses;
        internal IReadOnlyReactiveProperty<Impact> ImpactProp=> _impact;
        internal IReadOnlyReactiveProperty<float> ProgressCurrent => _progressCurrent;
        internal float ProgressMax => _progressMax;

        internal void OnEnable(Item baseExternalClass)
        {
            _baseExternalClass = baseExternalClass;

            void ReactiveSubscription()
            {
                float step = 0.2f;
                Observable
                    .Interval(TimeSpan.FromSeconds(step))
                    .Subscribe(_ =>
                    {
                        void DoProgress()
                        {
                            if (_targetResult && _impact.Value != Impact.None && !_baseExternalClass._item.Value)
                                _progressCurrent.Value = Mathf.Clamp(ProgressCurrent.Value + step * _progressMultiplier, 0, _progressMax);
                            else
                                _progressCurrent.Value = Mathf.Clamp(ProgressCurrent.Value - step * _progressMultiplier, 0, _progressMax);
                        }

                        DoProgress();
                    })
                    .AddTo(_baseExternalClass._disposable);

                _impact
                    .Subscribe(value =>
                    {
                        void FindTargetResult()
                        {
                            if (value == Impact.None)
                            {
                                _targetResult = null;

                                return;
                            }

                            foreach (var pair in _accessibleMetamorphoses)
                                if (pair.FromImpact == _impact.Value)
                                    _targetResult = pair.Result;
                        }

                        FindTargetResult();
                    })
                    .AddTo(_baseExternalClass._disposable);

                _progressCurrent
                    .Subscribe(value =>
                    {
                        void TryCompleteMetamorphosis()
                        {
                            if (value >= _progressMax && _targetResult != null)
                                _baseExternalClass.ReplaceYourself(_targetResult);
                        }

                        void DOShakeModel()
                        {
                            if (value <= 0)
                                return;

                            _baseExternalClass._model.transform
                                .DOShakePosition(step, (value / _progressMax) / 2)
                                .SetLink(_baseExternalClass.gameObject);
                        }

                        TryCompleteMetamorphosis();
                        DOShakeModel();
                    })
                    .AddTo(_baseExternalClass._disposable);
            }

            ReactiveSubscription();
        }

        internal void SetValues(Impact impact, float multiplier)
        {
            _impact.Value = impact;
            _progressMultiplier = multiplier;
        }

        internal enum Impact
        {
            None = 1 << 0,
            Fry = 1 << 1,
            Cut = 1 << 2,
            Wash = 1 << 3,
            Pollute = 1 << 4,
            Recycle = 1 << 30,
            Crush = 1 << 31,
        }

        [Serializable]
        internal sealed class AccessibleMetamorphosis
        {
            [SerializeField] internal Impact FromImpact;
            [SerializeField] internal Item Result;
        }
    }

    [Serializable]
    internal sealed class Mixing
    {
        [SerializeField] internal Item With;
        [SerializeField] internal Item Result;
    }

    [Serializable]
    internal sealed class Chunking
    {
        [SerializeField] internal Item TurnInto;
        [SerializeField] internal Item Result;
    }

    [Serializable]
    internal sealed class View
    {
        private Item _baseExternalClass;

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private GameObject _metamorphosisImpactIconPanel;
        [SerializeField] private List<GameObject> _metamorphosisImpactIconImages = new List<GameObject>();
        [SerializeField] private GameObject _metamorphosisProgressPanel;
        [SerializeField] private GameObject _metamorphosisProgressWarningPanel;
        [SerializeField] private Slider _metamorphosisProgressSlider;
        [SerializeField] private Image _metamorphosisProgressSliderFill;
        [SerializeField] private GameObject _chunkingIconImage;

        internal GameObject GameObject => _gameObject;

        internal void OnEnable(Item baseExternalClass)
        {
            _baseExternalClass = baseExternalClass;

            void ReactiveSubscription()
            {
                _baseExternalClass._ownItemHolderHandler
                    .Subscribe(value =>
                    {
                        if (value == null)
                            return;

                        void DisplayNecessaryTransformationImpactIcons()
                        {
                            void HideAllIcons()
                            {
                                foreach (var icon in _metamorphosisImpactIconImages)
                                    icon.SetActive(false);
                            }

                            void DisplayIcons()
                            {
                                if (value.OwnerType != null && value.OwnerType == typeof(Player))
                                    foreach (var metamorphosis in _baseExternalClass.AccessibleMetamorphosis)
                                        foreach (var icon in _metamorphosisImpactIconImages)
                                            if (icon.name == metamorphosis.FromImpact.ToString())
                                                icon.SetActive(true);

                                //if (value.OwnerType != null && value.OwnerType == typeof(Station))
                                //    foreach (var metamorphosis in _baseExternalClass.AccessibleMetamorphosis)
                                //        foreach (var icon in _metamorphosisImpactIconImages)
                                //            if (icon.name == metamorphosis.FromImpact.ToString() && icon.name == heir.ItemMetamorphImpact.ToString())
                                //                icon.SetActive(true);
                            }

                            HideAllIcons();
                            DisplayIcons();
                        }

                        DisplayNecessaryTransformationImpactIcons();
                        DisplayChinkingIcon();
                    })
                    .AddTo(_baseExternalClass._disposable);

                _baseExternalClass.MetamorphosisProgressCurrent
                    .Subscribe(value =>
                    {
                        void DisplayProgress(float value)
                        {
                            float valueMax = _baseExternalClass.MetamorphosisProgressMax;

                            _metamorphosisProgressPanel.SetActive(value > 0 && _baseExternalClass.MetamorphosisImpact.Value != Metamorphosing.Impact.None);

                            _metamorphosisProgressSlider.DOValue(value / valueMax, 0.2f).SetEase(Ease.Linear);
                            _metamorphosisProgressSliderFill.color = new Color(1 - (value / valueMax), 1, 1 - (value / valueMax));
                        }

                        DisplayProgress(value);
                    })
                    .AddTo(_baseExternalClass._disposable);

                _baseExternalClass.ItemIsEmpty
                    .Subscribe(value =>
                    {
                        _metamorphosisImpactIconPanel.SetActive(value);

                        DisplayChinkingIcon();
                    })
                    .AddTo(_baseExternalClass._disposable);
            }

            ReactiveSubscription();
        }

        private void DisplayChinkingIcon()
        {
            _chunkingIconImage.SetActive(false);

            if (_baseExternalClass._ownItemHolderHandler.Value != null && _baseExternalClass._ownItemHolderHandler.Value.OwnerType != null && _baseExternalClass._ownItemHolderHandler.Value.OwnerType == typeof(Station))
                _chunkingIconImage.SetActive(
                    _baseExternalClass._accessibleChunk.Result != null);
                    //&& !_baseExternalClass._selfItemHolder.LockForGiveAwayContent && _baseExternalClass._selfItemHolder.IsStorage && _baseExternalClass._selfItemHolder.IsWarehouse
                    //&& (!_baseExternalClass._item.Value || _baseExternalClass._item.Value.ItemIsEmpty.Value));
        }
    }

    [Serializable]
    internal sealed class HolderHandler
    {
        [SerializeField] private Entity _owner;

        [SerializeField] private Limition _limit;
        [SerializeField] private ReactiveProperty<int> _amountCurrent = new();
        [SerializeField] private int _amountMax = 1;
        [SerializeField] private Transform _placement;
        [SerializeField] private bool _lockForGet = false;
        [SerializeField] private bool _lockForGiveAway = false;
        [SerializeField] Vector3 _newItemScale = Vector3.one;

        internal Limition.BanFor LimitionFor => _limit.For;
        internal List<Item> LimitionForInstance => _limit.ForInstance;
        internal IReadOnlyReactiveProperty<int> AmountCurrent => _amountCurrent;
        internal int AmountMax => _amountMax;
        internal Transform Placement => _placement;
        internal bool LockForGet => _lockForGet;
        internal bool LockForGiveAway => _lockForGiveAway;
        internal bool IsStorage => AmountMax == -1;
        internal bool IsWarehouse => AmountMax > 1;
        internal Vector3 NewItemScale => _newItemScale;
        internal Type OwnerType => _owner is Player ? _owner.GetType() : _owner is Station ? _owner.GetType() : _owner is Item ? _owner.GetType() : null;

        internal bool CheckOpportunityGet(Item own, Item other)
        {
            if (!other)
                return false;

            if (_limit.For == Limition.BanFor.Anything || _lockForGet)
                return false;

            if (_limit.For == Limition.BanFor.Nothing)
                return true;

            bool coincidence = false;

            foreach (var limitValue in _limit.ForInstance)
                if (other.Type == limitValue.Type)
                    coincidence = true;

            if (_limit.For == Limition.BanFor.AnythingExcept)
                if (!coincidence)
                    return false;

            if (_limit.For == Limition.BanFor.NothingExcept)
                if (coincidence)
                    return false;

            if (own)
            {
                if (!other.ItemIsEmpty.Value)
                    return false;

                if (!IsStorage && _amountCurrent.Value >= _amountMax)
                    return false;
            }

            return true;
        }

        internal void SetupReceived(Item other)
        {
            if (other == null)
            {
                _amountCurrent.Value = 0;

                return;
            }

            other.Setup(_placement, this);

            void TryDestroyUselessTrash()
            {
                if (other.Class != Item.Classification.Trash)
                    return;

                _amountCurrent.Value = 0;

                MonoBehaviour.Destroy(other.gameObject);
            }

            TryDestroyUselessTrash();
        }

        internal void ChangeAmountCurrent(int value)
            => _amountCurrent.Value = Mathf.Clamp(_amountCurrent.Value + value, 0, _amountMax);

        internal void SetLockForGetContent(bool value)
            => _lockForGet = value;

        internal void SetLockForGiveAwayContent(bool value)
            => _lockForGiveAway = value;

        [Serializable]
        internal sealed class Limition
        {
            [SerializeField] private BanFor _for = BanFor.Nothing;
            [SerializeField] private List<Item> _forInstance;

            internal BanFor For => _for;
            internal List<Item> ForInstance => _forInstance;

            internal enum BanFor
            {
                Nothing = 1 << 0,
                NothingExcept = 1 << 1,
                Anything = 1 << 2,
                AnythingExcept = 1 << 3,
            }
        }
    }

    [Serializable]
    internal sealed class MetamorphosisHandler
    {
        [SerializeField] private Entity _owner;

        private Item _itemReceived;

        [SerializeField] private Item.Metamorphosing.Impact _impact = Item.Metamorphosing.Impact.None;
        [SerializeField] private float _multiplier = 1;
        [SerializeField] private ReactiveProperty<bool> _activity = new(false);
        [SerializeField] private Fuel _fuelness = new();

        internal Item.Metamorphosing.Impact Impact => _impact;

        internal void OnEnable()
        {
            _fuelness.OnEnable(this);
        }

        internal void OnDisable()
        {
            _fuelness.OnDisable();
        }

        internal void SetupReceived(Item other)
        {
            _itemReceived = other;

            TryApplyImpact();

            _fuelness.TryComsume(other);
        }

        internal void TryApplyImpact()
        {
            if (!_itemReceived)
                return;

            if (_activity.Value || _fuelness.FuelInfinite || _fuelness.FuelCurrent.Value > 0)
            {
                _itemReceived.SetMetamorphicValues(_impact, _multiplier);

                return;
            }

            _itemReceived.SetMetamorphicValues(Item.Metamorphosing.Impact.None, 2);
        }

        internal bool CheckOpportunityChangeActivity()
        {
            if (_fuelness.FuelCurrent.Value > 0 || _fuelness.FuelInfinite || _impact == Item.Metamorphosing.Impact.None)
                return false;

            return true;
        }

        internal void ChangeActivity(bool value)
        {
            _activity.Value = value;

            TryApplyImpact();
        }

        [Serializable]
        internal sealed class Fuel
        {
            private readonly CompositeDisposable _disposable = new();
            private Action _externalMethodTryApplyImpact;

            [SerializeField] private List<Item> _consume = new();
            [SerializeField] private ReactiveProperty<float> _fuelCurrent = new();
            [SerializeField] private float _fuelMax = 1;
            [SerializeField] private bool _fuelInfinite = false;

            internal IReadOnlyReactiveProperty<float> FuelCurrent => _fuelCurrent;
            internal float FuelMax => _fuelMax;
            internal bool FuelInfinite => _fuelInfinite;

            internal void OnEnable(Item.MetamorphosisHandler baseExternalClass)
            {
                _externalMethodTryApplyImpact = baseExternalClass.TryApplyImpact;

                void ReactiveSubscription()
                {
                    float step = 0.2f;
                    Observable
                        .Interval(TimeSpan.FromSeconds(step))
                        .Subscribe(_ =>
                        {
                            void FuelSpend()
                            {
                                if (!_fuelInfinite)
                                    _fuelCurrent.Value = Mathf.Clamp(_fuelCurrent.Value - step, 0, _fuelMax);
                            }

                            FuelSpend();
                        })
                        .AddTo(_disposable);

                    _fuelCurrent
                        .Subscribe(value =>
                        {
                            _externalMethodTryApplyImpact();
                        })
                        .AddTo(_disposable);
                }

                ReactiveSubscription();
            }

            internal void OnDisable()
            {
                void ReactiveUnSubscription()
                {
                    _disposable.Clear();
                }

                ReactiveUnSubscription();
            }

            internal void TryComsume(Item other)
            {
                if (!other || !other.ItemIsEmpty.Value || _fuelInfinite)
                    return;

                foreach (var consume in _consume)
                    if (other.Type == consume.Type)
                    {
                        _fuelCurrent.Value = _fuelMax;

                        MonoBehaviour.Destroy(other.gameObject);

                        break;
                    }
            }
        }
    }

    internal sealed class Factory : PlaceholderFactory<Item> { }
}
