using Cinemachine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor.Build.Content;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterController))]
internal sealed class Player : Entity
{
    [SerializeField] private ReactiveProperty<GameObject> _character = new();
    [SerializeField] private Transform _characterPlace;

    private int _indexCoop;

    private InputHandler _inputHandler;
    private GameData _gameData;
    private CameraManager _cameraManager;

    private readonly Fsm1 _fsm1 = new();
    private readonly Fsm2 _fsm2 = new();
    
    private ReactiveProperty<Vector3> _inputAxis = new();
    private ReactiveCommand _inputAction0 = new();
    private ReactiveCommand _inputAction1 = new();
    private ReactiveProperty<bool> _inputAction2 = new();

    private Vector3 _inputAxisDirection = new();

    private CharacterController _characterController;
    private Animator _animator;
    private Vector3 _moveVelocity = Vector3.zero;
    private float _moveSpeed = 10.0f;
    [SerializeField] private float _moveSpeedMax = 10.0f;
    private float _rotateSpeed;
    [SerializeField] private float _rotateSpeedMax = 180.0f;
    [SerializeField] private Vector3 _gravity = new(0, -1.0f, 0);
    private Vector3 _gravityVelocity = Vector3.zero;
    private Vector3 _gravityMin = new(0, -0.1f, 0);

    private readonly ReactiveProperty<Station> _station = new(null);
    private Station _sationCache;
    private readonly ReactiveProperty<float> _stationRememberBuffer = new(0);

    private float _yPositionConsiderItFall = -15;
    
    private bool _isGrounded => _characterController.isGrounded;

    [Inject]
    private void Construct(
        int indexCoop,
        InputHandler inputHandler,
        GameData gameData,
        CameraManager cameraManager)
    {
        _indexCoop = indexCoop;
        _inputHandler = inputHandler;
        _gameData = gameData;
        _cameraManager = cameraManager;
    }

    protected override void Awake()
    {
        base.Awake();

        _characterController = GetComponent<CharacterController>();

        void CreateCharacter()
        {
            if (_character.Value)
                Destroy(_character.Value.gameObject);

            GameObject newCharacter = Instantiate(
                    _gameData.PlayerPool[_indexCoop].Character,
                    _characterPlace.position,
                    _characterPlace.rotation,
                    _characterPlace);

            _character.Value = newCharacter;
        }

        void SelectingInput()
        {
            _inputAxis = _inputHandler.PlayersAxis[_indexCoop];
            _inputAction0 = _inputHandler.PlayersAction0[_indexCoop];
            _inputAction1 = _inputHandler.PlayersAction1[_indexCoop];
            _inputAction2 = _inputHandler.PlayersAction2[_indexCoop];
        }

        CreateCharacter();
        SelectingInput();
    }

    protected override void Start()
    {
        base.Start();

        void EquateFieldToMax()
        {
            _moveSpeed = _moveSpeedMax;
            _rotateSpeed = _rotateSpeedMax;
        }

        void SetupFsm()
        {
            _fsm1.AddState(new StateIdle(this, _fsm1));
            _fsm1.AddState(new StateWalk(this, _fsm1));
            _fsm1.SetState<StateIdle>(); // стартовое состо€ние

            _fsm2.AddState(new StateNothing(this, _fsm2));
            _fsm2.AddState(new StateHaveSomething(this, _fsm2));
            _fsm2.AddState(new StateUseStation(this, _fsm2));
            _fsm2.SetState<StateNothing>(); // стартовое состо€ние
        }

        EquateFieldToMax();
        SetupFsm();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        _fsm1.FixedUpdate();
        _fsm2.FixedUpdate();

        void DeterminGravityVelocity()
        {
            if (_isGrounded)
                _gravityVelocity = _gravityMin;
            else
                if (transform.position.y > _yPositionConsiderItFall)
                    _gravityVelocity += _gravity;
                else
                    _gravityVelocity.y = 0;
        }

        DeterminGravityVelocity();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _fsm1.OnEnable();
        _fsm2.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _fsm1.OnDisable();
        _fsm2.OnDisable();
    }

    protected override void ReactiveSubscription()
    {
        base.ReactiveSubscription();

        float step = 0.1f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                void FallPrevention()
                {
                    if (transform.position.y > _yPositionConsiderItFall)
                        return;

                    // ќтключение и включение _characterController потому что он не даст просто напр€мую изменить transform.position
                    _characterController.enabled = false;
                    transform.position = Vector3.zero;
                    _characterController.enabled = true;
                }

                FallPrevention();
            })
            .AddTo(_disposable);

        _inputAxis
           .Subscribe(value =>
           {
               void DeterminInputAxisDirection(Vector3 direction)
               {
                   Vector3 cameraForward = _cameraManager.VirtualCamera.transform.forward;
                   Vector3 cameraRight = _cameraManager.VirtualCamera.transform.right;

                   cameraForward.y = 0;
                   cameraRight.y = 0;

                   Vector3 directionForward = cameraForward.normalized * direction.z;
                   Vector3 directionRight = cameraRight.normalized * direction.x;

                   _inputAxisDirection = (directionForward + directionRight);
               }

               DeterminInputAxisDirection(value);
           })
           .AddTo(_disposable);

        _character
            .Subscribe(value =>
            {
                _animator = value ? value.GetComponent<Animator>() : null;
            })
            .AddTo(_disposable);

        _stationRememberBuffer
            .Subscribe(value =>
            {
                if (value <= 0)
                    _station.Value = null;
            })
            .AddTo(_disposable);
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

        _itemHolderHandler.ChangeAmountCurrent(_itemHolderHandler.AmountMax);

        Item cache = _item.Value;
        _item.Value = null;

        return cache;
    }

    internal override Item TryGiveAwayChunkItem()
    {
        throw new NotImplementedException();
    }

    protected override void CountdownTimers(float value)
    {
        base.CountdownTimers(value);

        if (_stationRememberBuffer.Value > 0)
            _stationRememberBuffer.Value -= value;
    }

    private void SmoothChangeAnimatorLayerWeight(int layerIndex, float targetWeight, float duration)
    {
        CompositeDisposable disposable = new();

        float startWeight = _animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;

        Observable
            .EveryFixedUpdate()
            .Subscribe(_ =>
            {
                elapsedTime += Time.deltaTime;

                float newWeight = Mathf.Lerp(startWeight, targetWeight, elapsedTime / duration);

                if (_animator)
                    _animator.SetLayerWeight(layerIndex, newWeight);
                else
                    disposable.Clear();

                if (elapsedTime > duration)
                    disposable.Clear();
            })
            .AddTo(disposable);
    }

    internal abstract class StateBaseMain
    {
        protected readonly CompositeDisposable _disposable = new();

        protected readonly Player _baseExternalClass;

        internal StateBaseMain(Player player)
        {
            _baseExternalClass = player;
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
            void InputSubscription()
            {
                _baseExternalClass._inputAxis
                    .Subscribe(value => LogicForInputAxis(value))
                    .AddTo(_disposable);

                _baseExternalClass._inputAction0
                    .Subscribe(_ => LogicForInputAction0())
                    .AddTo(_disposable);

                _baseExternalClass._inputAction1
                    .Subscribe(_ => LogicForInputAction1())
                    .AddTo(_disposable);

                _baseExternalClass._inputAction2
                    .Subscribe(value => LogicForInputAction2(value))
                    .AddTo(_disposable);
            }

            InputSubscription();
        }

        protected virtual void ReactiveUnSubscription()
        {
            _disposable.Clear();
        }

        protected virtual void LogicForInputAxis(Vector3 value)
        {
        }

        protected virtual void LogicForInputAction0()
        {
        }

        protected virtual void LogicForInputAction1()
        { 
        }

        protected virtual void LogicForInputAction2(bool value)
        { 
        }
    }

    internal sealed class Fsm1
    {
        private StateBase1 _stateCurrent;
        private Dictionary<Type, StateBase1> _states = new();

        internal void AddState(StateBase1 state)
            => _states.Add(state.GetType(), state);

        internal void SetState<T>() where T : StateBase1
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

    internal abstract class StateBase1 : StateBaseMain
    {
        protected readonly Fsm1 _fsm;

        internal StateBase1(Player player,Fsm1 fsm): base(player)
        {
            _fsm = fsm;
        }

        internal override void FixedUpdate()
        {
            base.FixedUpdate();

            ApplyGravity();
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            float step = 0.1f;
            Observable
                .Interval(TimeSpan.FromSeconds(step))
                .Subscribe(_ =>
                {
                    LookingForStation(step);
                })
                .AddTo(_disposable);

            _baseExternalClass._station
                .Subscribe(value =>
                {
                    if (_baseExternalClass._sationCache)
                        _baseExternalClass._sationCache.WasUnselectedByPlayer();

                    _baseExternalClass._sationCache = value;

                    if (value)
                        value.WasSelectedByPlayer();
                })
                .AddTo(_disposable);
        }

        protected virtual void LookingForStation(float step)
        {
            float rayLength = 2;
            Vector3 rayOrigin = _baseExternalClass.transform.position;
            Vector3 rayDirection = (_baseExternalClass.transform.forward) * rayLength;
            Ray ray = new(rayOrigin, rayDirection);

            Debug.DrawRay(rayOrigin, rayDirection, Color.red, step);

            if (Physics.Raycast(ray, out RaycastHit hit, rayLength, LayerMask.GetMask("Station")))
            {
                Station station = hit.collider.transform.parent.transform.parent.GetComponent<Station>();

                if (!station)
                    return;

                _baseExternalClass._station.Value = station;

                _baseExternalClass._stationRememberBuffer.Value = 0.3f;
            }
        }

        protected virtual void ApplyGravity()
            => _baseExternalClass._characterController.Move(_baseExternalClass._gravityVelocity * Time.deltaTime);

    }

    internal sealed class StateIdle : StateBase1
    {
        internal StateIdle(Player player, Fsm1 fsm) : base(player, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass._animator.CrossFadeInFixedTime("Idle", 0.1f);
        }

        protected override void LogicForInputAxis(Vector3 value)
        {
            base.LogicForInputAxis(value);

            if (value != Vector3.zero)
                _fsm.SetState<StateWalk>();
        }
    }

    internal sealed class StateWalk : StateBase1
    {
        private Vector3 _moveVelocity;
        private float _currentVelocity; // only needed for SmoothDampAngle

        internal StateWalk(Player player, Fsm1 fsm) : base(player, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass._animator.CrossFadeInFixedTime("Walk", 0.1f);
        }

        internal override void FixedUpdate()
        {
            base.FixedUpdate();

            void Move()
            {
                _baseExternalClass._characterController.Move((_moveVelocity + _baseExternalClass._gravityVelocity) * Time.deltaTime);
            }

            void Rotate()
            {
                float targetAngle = Mathf.Atan2(_baseExternalClass._inputAxisDirection.x, _baseExternalClass._inputAxisDirection.z) * Mathf.Rad2Deg; // Mathf.Atan2(...) * Mathf.Rad2Deg возвращает преобразованую координату на оси полученную из вектора2 (который равен только -1 до 1)
                float smoothAngle = Mathf.SmoothDampAngle(
                    _baseExternalClass.transform.eulerAngles.y,
                    targetAngle,
                    ref _currentVelocity,
                    0.1f,
                     _baseExternalClass._rotateSpeed);

                _baseExternalClass.transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            }

            Move();
            Rotate();
        }

        protected override void LogicForInputAxis(Vector3 value)
        {
            base.LogicForInputAxis(value);

            if (value == Vector3.zero)
                _fsm.SetState<StateIdle>();

            _moveVelocity = _baseExternalClass._inputAxisDirection * _baseExternalClass._moveSpeed;
        }

        protected override void ApplyGravity()
        {
            // Ќельз€ вызывать CharacterController.Move() последовательно
            // “ребуетс€ переопределить чтобы убрать тело
            // “.к. если последовательно несколько раз вызывать CharacterController.Move() в FixedUpdate 
            // IsGraunded будет false даже если будет касание граунда
            // ѕроизойдет наслоение CharacterController.Move
        }
    }

    internal sealed class Fsm2
    {
        private StateBase2 _stateCurrent;
        private Dictionary<Type, StateBase2> _states = new();

        internal void AddState(StateBase2 state)
            => _states.Add(state.GetType(), state);

        internal void SetState<T>() where T : StateBase2
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

    internal abstract class StateBase2 : StateBaseMain
    {
        protected readonly Fsm2 _fsm;

        internal StateBase2(Player player, Fsm2 fsm): base(player)
        {
            _fsm = fsm;
        }
    }

    private sealed class StateNothing : StateBase2
    {
        internal StateNothing(Player player, Fsm2 fsm) : base(player, fsm) { }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._item
                .Subscribe(value =>
                {
                    if (value != null)
                        _fsm.SetState<StateHaveSomething>();
                })
                .AddTo(_disposable);
        }

        protected override void LogicForInputAction0()
        {
            base.LogicForInputAction0();

            if (_baseExternalClass._station.Value == null)
                return;

            Item received = _baseExternalClass._station.Value.TryGiveAwayItem();
            Item returned = _baseExternalClass.TryGetItem(received);
        }

        protected override void LogicForInputAction1()
        {
            base.LogicForInputAction1();

            if (_baseExternalClass._station.Value == null)
                return;

            Item received = _baseExternalClass._station.Value.TryGiveAwayChunkItem();
            Item returned = _baseExternalClass.TryGetItem(received);
        }

        protected override void LogicForInputAction2(bool value)
        {
            base.LogicForInputAction2(value);

            void UseStation(bool value)
            {
                if (value
                    && _baseExternalClass._station.Value
                    && _baseExternalClass._station.Value.TryUse() == true)
                    _fsm.SetState<StateUseStation>();
            }

            UseStation(value);
        }
    }

    private sealed class StateHaveSomething : StateBase2
    {
        internal StateHaveSomething(Player player, Fsm2 fsm) : base(player, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass._animator.Play("Hold");
            _baseExternalClass.SmoothChangeAnimatorLayerWeight(1, 1, 0.25f);
        }

        internal override void Exit()
        {
            base.Exit();

            _baseExternalClass.SmoothChangeAnimatorLayerWeight(1, 0, 0.25f);
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._item
                .Subscribe(value =>
                {
                    if(value == null)
                        _fsm.SetState<StateNothing>();
                })
                .AddTo(_disposable);
        }

        protected override void LogicForInputAction0()
        {
            base.LogicForInputAction0();

            if (_baseExternalClass._station.Value == null)
                return;

            Item given = _baseExternalClass.TryGiveAwayItem();
            Item returned = _baseExternalClass._station.Value.TryGetItem(given);

            _baseExternalClass.TryGetItem(returned);
        }
    }

    private sealed class StateUseStation : StateBase2
    {
        internal StateUseStation(
            Player player,
            Fsm2 fsm)
            : base(player, fsm)
        {
        }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass.SmoothChangeAnimatorLayerWeight(2, 1, 0.25f);
        }

        internal override void Exit()
        {
            base.Exit();

            _baseExternalClass.SmoothChangeAnimatorLayerWeight(2, 0, 0.25f);
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._station
                .Subscribe(value =>
                {
                    if (!value)
                        _fsm.SetState<StateNothing>();
                })
            .AddTo(_disposable);
        }

        protected override void LogicForInputAction2(bool value)
        {
            base.LogicForInputAction2(value);

            void UnuseStation()
            {
                if (value == true)
                    return;

                if(_baseExternalClass._station.Value)
                    _baseExternalClass._station.Value.Unuse();

                _fsm.SetState<StateNothing>();
            }

            UnuseStation();
        }
    }

    // ≈сли требуетс€ передать доп параметры то в этом классе в конструкторе (или кастомном если это монобех) они должны быть самыми первыми, перед всеми заинжекшеными зависимост€ми - internal Player(int переданый¬‘абрике, OtherClass otherClass)
    internal sealed class Factory : PlaceholderFactory<int, Player> { }
}
