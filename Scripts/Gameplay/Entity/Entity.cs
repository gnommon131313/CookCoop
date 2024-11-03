using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

internal abstract class Entity : MonoBehaviour
{
    protected readonly CompositeDisposable _disposable = new();
    private readonly CompositeDisposable _observeForItemDestroy = new();

    protected SignalBus _signalBus;
    protected List<Item.Factory> _itemFactories;

    private Item[] _itemPrefabs;

    [SerializeField] protected ReactiveProperty<Item> _item;
    [SerializeField] protected Item.HolderHandler _itemHolderHandler = new();
    [SerializeField] protected Item.MetamorphosisHandler _itemMetamorphosisHandler = new();

    // Не даю прямую ссылку на Item а просто показываю поля для чтения (закон деметры)
    internal IReadOnlyReactiveProperty<bool> ItemIsEmpty { get; private set; }
    internal string ItemType => _item.Value != null ? _item.Value.Type : "";
    internal IReadOnlyList<Item.Metamorphosing.AccessibleMetamorphosis> ItemAccessibleMetamorphosis => _item.Value != null ? _item.Value.AccessibleMetamorphosis : null;
    internal Item.HolderHandler.Limition.BanFor ItemHolderLimitionFor => _itemHolderHandler.LimitionFor;
    internal IReadOnlyList<Item> ItemHolderLimitionForInstance => _itemHolderHandler.LimitionForInstance;
    internal Item.Metamorphosing.Impact ItemMetamorphImpact => _itemMetamorphosisHandler.Impact;

    [Inject]
    private void Construct(
        SignalBus signalBus,
        List<Item.Factory> itemFactories)
    {
        _signalBus = signalBus;
        _itemFactories = itemFactories;
    }

    protected virtual void Awake()
    {
        _itemPrefabs = Resources.LoadAll<Item>("Prefabs/Gameplay/Item");
    }

    protected virtual void Start()
    {
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void OnEnable()
    {
        void ReplacePrefabForInstance()
        {
            if (_item.Value && _item.Value.transform.parent == null)
                _item.Value = ItemCreate(_item.Value, _itemHolderHandler.Placement.position);
        }

        ReplacePrefabForInstance();
        ReactiveSubscription();

        _itemMetamorphosisHandler.OnEnable();
    }

    protected virtual void OnDisable()
    {
        ReactiveUnSubscription();

        _itemMetamorphosisHandler.OnDisable();
    }

    internal abstract Item TryGetItem(Item other);
    internal abstract Item TryGiveAwayItem();
    internal abstract Item TryGiveAwayChunkItem();

    protected virtual void ReactiveSubscription()
    {
        float step = 0.2f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                CountdownTimers(step);
            })
            .AddTo(_disposable);

        _item
            .Subscribe(value =>
            {
                void ChangeScale()
                {
                    if (value)
                        value.transform.localScale = _itemHolderHandler.NewItemScale;
                }

                void ObserveForDestroy()
                {
                    _observeForItemDestroy.Clear();

                    if (value)
                        value.OnDestroyed.Subscribe(_ => { /*Debug.Log($"{this} | {value} = destroy");*/ _item.Value = null; }).AddTo(_observeForItemDestroy);
                }

                _itemHolderHandler.SetupReceived(value);
                _itemMetamorphosisHandler.SetupReceived(value);

                ChangeScale();
                ObserveForDestroy();
            })
            .AddTo(_disposable);

        ItemIsEmpty = _item
            .Select(value => value == null)
            .ToReactiveProperty();

        if (_signalBus != null)
        {
            _signalBus
                .GetStream<ReplaceItem>()
                .Subscribe(evt =>
                {
                    if (evt.ForItemHolderHandler == _itemHolderHandler)
                        _item.Value = ItemCreate(evt.NewItem, _itemHolderHandler.Placement.position);
                })
                .AddTo(_disposable);
        }
    }

    protected virtual void ReactiveUnSubscription()
    {
        _disposable.Clear();
    }

    protected Item ItemCreate(Item desired, Vector3 startPosition)
    {
        // Создание идет через фабрику
        // Нужный префаба для создания находится по индексу самого префаба, т.к. фабрики биндят в себя префабы в томже порядке
        for (int i = 0; i < _itemPrefabs.Length; i++)
            if (_itemPrefabs[i].Type == desired.Type)
            {
                Item newInstance = _itemFactories[i].Create();
                newInstance.transform.position = startPosition;

                return newInstance;
            }

        throw new NotImplementedException("Prefab or Factoried loading correctly = FIX IT");
    }

    protected virtual void CountdownTimers(float value)
    {
    }
}