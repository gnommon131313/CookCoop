using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Zenject;
using System;
using DG.Tweening;
using System.Linq;
using UnityEngine.Profiling;

internal sealed class OrderTable : Station
{
    private readonly Fsm _fsm = new();
    [SerializeField] private AnimationCurve _gameComplaxity;

    [SerializeField] private ReactiveProperty<Item> _order = new();
    [SerializeField] private List<Item> _orderPool = new();
    [SerializeField] private Transform _orderPlacement;

    [SerializeField] private ReactiveProperty<float> _orderWaitTimer = new();
    [SerializeField] private ReactiveProperty<float> _orderActiveTimer = new();
    [SerializeField] private ReactiveProperty<float> _orderProcessingTimer = new();
    private float _orderWaitTimerMax;
    private float _orderActiveTimerMax;
    private float _orderProcessingTimerMax;
    [SerializeField] private float _orderWaitTimerBase = 3;
    [SerializeField] private float _orderActiveTimerBase = 10;
    [SerializeField] private float _orderProcessingTimerBase = 3;

    [SerializeField] private List<GameObject> _customerPool;
    private GameObject _customerCurrent;
    private Vector3 _customerTargetPosition = Vector3.zero;

    internal ReactiveProperty<float> OrderActiveTimer => _orderActiveTimer;
    internal float OrderActiveTimerMax => _orderActiveTimerMax;

    protected override void Awake()
    {
        base.Awake();

        void EquateFieldToMax()
        {
            _orderWaitTimerMax = _orderWaitTimerBase;
            _orderActiveTimerMax = _orderActiveTimerBase;
            _orderProcessingTimerMax = _orderProcessingTimerBase;
        }

        EquateFieldToMax();
    }

    protected override void Start()
    {
        base.Start();

        void SetupFsm()
        {
            _fsm.AddState(new StateOrderWait(this, _fsm));
            _fsm.AddState(new StateOrderActive(this, _fsm));
            _fsm.AddState(new StateOrderExpired(this, _fsm));
            _fsm.AddState(new StateOrderProcessing(this, _fsm));
            _fsm.SetState<StateOrderWait>(); // стартовое состояние
        }

        SetupFsm();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _fsm.FixedUpdate();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _fsm.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _fsm.OnDisable();
    }

    protected override void ReactiveSubscription()
    {
        base.ReactiveSubscription();

        _gameData.GlobalScoreData.Current
            .Subscribe(value =>
            {
                void DetermineGameComplexity(float value)
                {
                    float complexity = _gameComplaxity.Evaluate(value);

                    _orderWaitTimerMax = _orderWaitTimerBase / complexity;
                    _orderActiveTimerMax = _orderActiveTimerBase / complexity;
                    _orderProcessingTimerMax = _orderProcessingTimerBase / complexity;
                }

                DetermineGameComplexity(value);
            })
            .AddTo(_disposable);
    }

    protected override void CountdownTimers(float value)
    {
        base.CountdownTimers(value);

        if (_orderWaitTimer.Value > 0)
            _orderWaitTimer.Value -= value;

        if (_orderActiveTimer.Value > 0)
            _orderActiveTimer.Value -= value;

        if (_orderProcessingTimer.Value > 0)
            _orderProcessingTimer.Value -= value;
    }

    internal sealed class Fsm
    {
        private StateBase _stateCurrent;
        private Dictionary<Type, StateBase> _states = new();

        internal void AddState(StateBase state)
            => _states.Add(state.GetType(), state);

        internal void SetState<T>() where T : StateBase
        {
            var type = typeof(T);

            if (_stateCurrent != null && _stateCurrent.GetType() == type)
                return;

            if (_states.TryGetValue(type, out var newState))
            {
                _stateCurrent?.Exit();
                _stateCurrent = newState;
                _stateCurrent.Enter();
            }
        }

        internal void OnEnable()
            => _stateCurrent?.Enter();

        internal void OnDisable()
            => _stateCurrent?.Exit();

        internal void FixedUpdate()
            => _stateCurrent?.FixedUpdate();
    }

    internal abstract class StateBase
    {
        protected readonly CompositeDisposable _disposable = new();
        protected readonly OrderTable _baseExternalClass;
        protected readonly Fsm _fsm;

        internal StateBase(OrderTable baseExternalClass, Fsm fsm)
        {
            _baseExternalClass = baseExternalClass;
            _fsm = fsm;
        }

        internal virtual void Enter()
        {
            //Debug.Log($"Enter {this}");

            ReactiveSubscription();
        }

        internal virtual void Exit()
        {
            //Debug.Log($"Exit {this}");

            ReactiveUnSubscription();
        }

        internal virtual void FixedUpdate()
        {
            //Debug.Log($"FixedUpdate {this}");
        }

        protected virtual void ReactiveSubscription()
        {
        }

        protected virtual void ReactiveUnSubscription()
        {
            _disposable.Clear();
        }
    }

    internal sealed class StateOrderWait : StateBase
    {
        internal StateOrderWait(OrderTable station, Fsm fsm) : base(station, fsm) { }

        internal override void Enter()
        {
            _baseExternalClass._itemHolderHandler.SetLockForGetContent(false);
            _baseExternalClass._itemHolderHandler.SetLockForGiveAwayContent(false);

            _baseExternalClass._orderWaitTimer.Value = _baseExternalClass._orderWaitTimerMax;

            void LaunchCustomer()
            {
                _baseExternalClass._customerTargetPosition = _baseExternalClass.transform.position + _baseExternalClass.transform.forward * -2.5f;

                void CustomerRemove()
                {
                    if (!_baseExternalClass._customerCurrent)
                        return;

                    GameObject customerCached = _baseExternalClass._customerCurrent;
                    customerCached.transform
                        .DOMove(_baseExternalClass._customerTargetPosition + _baseExternalClass.transform.up * -50, 1.0f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => Destroy(customerCached))
                        .SetLink(_baseExternalClass._customerCurrent);
                }

                void CustomerCreate()
                {
                    _baseExternalClass._customerCurrent = Instantiate(
                        _baseExternalClass._customerPool[UnityEngine.Random.Range(0, _baseExternalClass._customerPool.Count)],
                        _baseExternalClass._customerTargetPosition + _baseExternalClass.transform.up * -5,
                        _baseExternalClass.transform.rotation,
                        _baseExternalClass.transform);
                }

                void CustomerMove()
                {
                    _baseExternalClass._customerCurrent.transform
                        .DOMove(_baseExternalClass._customerTargetPosition, _baseExternalClass._orderWaitTimerMax)
                        //.SetEase(Ease.OutCubic)
                        .SetEase(Ease.OutElastic)
                        .SetLink(_baseExternalClass._customerCurrent);
                }

                CustomerRemove();
                CustomerCreate();
                CustomerMove();
            }

            LaunchCustomer();

            base.Enter();
        }

        internal override void Exit()
        {
            base.Exit();

            _baseExternalClass._orderWaitTimer.Value = 0;
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._orderWaitTimer
                .Subscribe(value =>
                {
                    if (value <= 0)
                        _fsm.SetState<StateOrderActive>();
                })
                .AddTo(_disposable);
        }
    }

    internal sealed class StateOrderActive : StateBase
    {
        internal StateOrderActive(OrderTable station, Fsm fsm) : base(station, fsm) { }

        internal override void Enter()
        {
            _baseExternalClass._itemHolderHandler.SetLockForGetContent(false);
            _baseExternalClass._itemHolderHandler.SetLockForGiveAwayContent(false);

            void CreateAndSetupOrder()
            {
                void CreateOrder()
                {
                    int indexRandom = UnityEngine.Random.Range(0, _baseExternalClass._orderPool.Count);

                    Item newOrder = _baseExternalClass.ItemCreate(_baseExternalClass._orderPool[indexRandom], _baseExternalClass._orderPlacement.transform.position);
                    newOrder.DestroyView();

                    _baseExternalClass._order.Value = newOrder;
                }

                void CreateItemForOrder()
                {
                    if (_baseExternalClass._order.Value.ItemHolderLimitionFor != Item.HolderHandler.Limition.BanFor.AnythingExcept)
                        return;

                    int indexRandom = UnityEngine.Random.Range(0, _baseExternalClass._order.Value.ItemHolderLimitionForInstance.Count);

                    Item newItem = _baseExternalClass.ItemCreate(_baseExternalClass._order.Value.ItemHolderLimitionForInstance[indexRandom], _baseExternalClass._orderPlacement.transform.position);
                    newItem.DestroyView();

                    _baseExternalClass._order.Value.TryGetItem(newItem);
                }

                CreateOrder();
                CreateItemForOrder();

                _baseExternalClass._order.Value.Setup(_baseExternalClass._orderPlacement.transform);
            }

            CreateAndSetupOrder();

            _baseExternalClass._orderActiveTimer.Value = _baseExternalClass._orderActiveTimerMax;
            _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "OrderActive").SetActive(true);

            base.Enter();
        }

        internal override void Exit()
        {
            base.Exit();

            void DestroyOrder()
            {
                if (!_baseExternalClass._order.Value)
                    return;

                Destroy(_baseExternalClass._order.Value.gameObject);
                _baseExternalClass._order.Value = null; // miss после Destroy не тоже самое что и null
            }

            DestroyOrder();

            _baseExternalClass._orderActiveTimer.Value = 0;

            _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "OrderActive").SetActive(false);
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            float step = 0.5f;
            Observable
                .Interval(TimeSpan.FromSeconds(step))
                .Subscribe(_ =>
                {
                    // Нужно делать траи тут а не при обновлении значения продукта т.к. в продукта могут измениться переменный в процессе и он станет соответствовать заказу
                    TryOrderRelease();
                })
                .AddTo(_disposable);

            _baseExternalClass._orderActiveTimer
                .Subscribe(value =>
                {
                    if (value > 0)
                        return;

                    // Последний раз попробывать, т.к. траи происходят каждый шаг а не при обновлении значения
                    if (TryOrderRelease())
                        return;

                    _fsm.SetState<StateOrderExpired>();
                })
                .AddTo(_disposable);
        }

        private bool TryOrderRelease()
        {
            if (!_baseExternalClass._item.Value)
                return false;

            bool ProductAndOrderMatched()
            {
                if (_baseExternalClass._order.Value.Type != _baseExternalClass._item.Value.Type)
                    return false;

                if (_baseExternalClass._order.Value.ItemIsEmpty.Value != _baseExternalClass._item.Value.ItemIsEmpty.Value)
                    return false;

                if (_baseExternalClass._order.Value.ItemType != _baseExternalClass._item.Value.ItemType)
                    return false;

                return true;
            }

            bool matches = ProductAndOrderMatched();

            if (matches)
            {
                void OrderRelease()
                {
                    //Debug.Log("ЗАКАЗ ОТДАН");

                    _fsm.SetState<StateOrderProcessing>();
                }

                OrderRelease();
            }
            else
            {
                void OrderWasntRelease()
                {
                    //Debug.Log("ЗАКАЗ НЕ ВЕРНЫЙ");

                    _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "Attention").Blink(0.5f, 1);
                }

                OrderWasntRelease();
            }

            return matches;
        }
    }

    internal sealed class StateOrderProcessing : StateBase
    {
        internal StateOrderProcessing(OrderTable station, Fsm fsm) : base(station, fsm) { }

        internal override void Enter()
        {
            _baseExternalClass._itemHolderHandler.SetLockForGetContent(true);
            _baseExternalClass._itemHolderHandler.SetLockForGiveAwayContent(true);

            _baseExternalClass._orderProcessingTimer.Value = _baseExternalClass._orderProcessingTimerMax;

            _baseExternalClass._item.Value.DestroyView();

            _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "Success").SetActive(true);

            void CustomerAnimate()
            {
                if (!_baseExternalClass._customerCurrent)
                    return;

                Animator _animator = _baseExternalClass._customerCurrent.GetComponent<Animator>();
                _animator.CrossFadeInFixedTime("Action", 0.1f);
            }

            CustomerAnimate();

            base.Enter();
        }

        internal override void Exit()
        {
            base.Exit();

            void ReplaceItemWithItsItem()
            {
                if (!_baseExternalClass._item.Value)
                    return;

                if(_baseExternalClass._item.Value.ItemIsEmpty.Value)
                {
                    Destroy(_baseExternalClass._item.Value.gameObject);

                    return;
                }

                void ReplaceItemWithNewMetamorphicItem()
                {
                    Item newInstance = null;

                    foreach (var metamorphosis in _baseExternalClass._item.Value.ItemAccessibleMetamorphosis)
                        if (metamorphosis.FromImpact == _baseExternalClass._itemMetamorphosisHandler.Impact)
                            newInstance = _baseExternalClass.ItemCreate(metamorphosis.Result, _baseExternalClass._itemHolderHandler.Placement.position);

                    Destroy(_baseExternalClass._item.Value.gameObject);
                    _baseExternalClass._item.Value = newInstance;
                }

                ReplaceItemWithNewMetamorphicItem();
            }

            ReplaceItemWithItsItem();

            _baseExternalClass._orderProcessingTimer.Value = 0;

            _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "Success").SetActive(false);
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._orderProcessingTimer
                .Subscribe(value =>
                {
                    if (value <= 0)
                        _fsm.SetState<StateOrderWait>();
                })
                .AddTo(_disposable);
        }
    }

    internal sealed class StateOrderExpired : StateBase
    {
        internal StateOrderExpired(OrderTable station, Fsm fsm) : base(station, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass._itemHolderHandler.SetLockForGetContent(true);
            _baseExternalClass._itemHolderHandler.SetLockForGiveAwayContent(true);

            _baseExternalClass._dynamicColors.FirstOrDefault(content => content.name == "Lock").SetActive(true);

            if (_baseExternalClass._order.Value)
                Destroy(_baseExternalClass._order.Value.gameObject);

            _baseExternalClass._cameraManager.ApplyShake(2, 2);

            void RequestGameOverAfterDelay()
            {
                float delay = 3.0f;

                CompositeDisposable disposable = new();
                Observable
                    .Timer(TimeSpan.FromSeconds(delay))
                    .Subscribe(_ =>
                    {
                        _baseExternalClass._signalBus.TryFire(new GoToGameOver());

                        disposable.Clear();
                    })
                    .AddTo(disposable, _baseExternalClass);
            }

            RequestGameOverAfterDelay();
        }
    }
}
