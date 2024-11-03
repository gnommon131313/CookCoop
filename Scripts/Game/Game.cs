using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using UniRx;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;

internal sealed class Game : MonoBehaviour
{
    private readonly CompositeDisposable _disposable = new();

    private SignalBus _signalBus;
    private InputHandler _inputHandler;
    private GameData _gameData;
    private CameraManager _cameraManager;

    private readonly Fsm _fsm = new();

    private PlayerLoader.Factory _playerLoaderFactory;
    private PlayerLoader _playerLoader;
    private MapLoader.Factory _mapLoaderFactory;
    private MapLoader _mapLoader;

    internal ReactiveProperty<StateBase> StateCurrent => _fsm._stateCurrent;

    private Game() { }

    [Inject]
    private void Construct(
        SignalBus signalBus,
        InputHandler inputHandler,
        GameData gameData,
        CameraManager cameraManager,
        PlayerLoader.Factory playerLoaderFactory,
        MapLoader.Factory mapLoaderFactory)
    {
        _signalBus = signalBus;
        _inputHandler = inputHandler;
        _gameData = gameData;
        _cameraManager = cameraManager;
        _playerLoaderFactory = playerLoaderFactory;
        _mapLoaderFactory = mapLoaderFactory;
    }

    private void Awake()
    {
        void CreateInstanceFromFactory()
        {
            _mapLoader = _mapLoaderFactory.Create();
            _playerLoader = _playerLoaderFactory.Create();
        }
        CreateInstanceFromFactory();
    }

    private void Start()
    {
        void SetupFsm()
        {
            _fsm.AddState(new StateWelcomeScreen(this, _fsm));
            _fsm.AddState(new StateMenu(this, _fsm));
            _fsm.AddState(new StatePause(this, _fsm));
            _fsm.AddState(new StateGame(this, _fsm));
            _fsm.AddState(new StateGameOver(this, _fsm));
            _fsm.SetState<StateWelcomeScreen>();
        }

        void TEST()
        {
            CompositeDisposable _disposableXXX = new();
            Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    if (Keyboard.current.anyKey.isPressed)
                        foreach (KeyControl key in Keyboard.current.allKeys)
                            if (key.isPressed)
                                if (key == Keyboard.current.vKey)
                                {
                                    // Для .ApplyBindingOverride(string newPath) нужно чтобы было newPath = Keyboard/[KeyName]
                                    // А key.ToString() изначально = Key:/Keyboard/[KeyName]
                                    // Поэтому нужно убрать из строки ненужные символы Key:/
                                    int index = key.ToString().IndexOf('/') + 1;
                                    string result = key.ToString().Substring(index);
                                    _inputHandler.REBIND_XXXXXXXXXXXXXXX(result);
                                
                                    Debug.Log(result);

                                    _disposableXXX.Clear();
                                }
                })
                .AddTo(_disposableXXX);
        }

        SetupFsm();
        TEST();
    }

    private void Update()
    {
        _fsm.Update();


        void TEST()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            _signalBus.TryFire(new GoToGameOver());
        }

        TEST();
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    private void OnEnable()
    {
        void ReactiveSubscription()
        {
        }

        ReactiveSubscription();

        _fsm.OnEnable();
    }

    private void OnDisable()
    {
        void ReactiveUnSubscription()
        {
            _disposable.Clear();
        }

        ReactiveUnSubscription();

        _fsm.OnDisable();
    }

    private void DestroyGameplay()
    {
        _mapLoader.DestroyInstance();
        _playerLoader.DestroyInstance();
    }

    internal sealed class Fsm
    {
        internal readonly ReactiveProperty<StateBase> _stateCurrent = new ReactiveProperty<StateBase>();
        private Dictionary<Type, StateBase> _states = new Dictionary<Type, StateBase>();

        internal void AddState(StateBase state)
            => _states.Add(state.GetType(), state);

        internal void SetState<T>() where T : StateBase
        {
            var type = typeof(T);

            if (_stateCurrent != null && _stateCurrent.GetType() == type)
                return;

            if (_states.TryGetValue(type, out var newState))
            {
                _stateCurrent.Value?.Exit();
                _stateCurrent.Value = newState;
                _stateCurrent.Value.Enter();
            }
        }

        internal void OnEnable()
            => _stateCurrent.Value?.Enter();

        internal void OnDisable()
            => _stateCurrent.Value?.Exit();

        internal void Update()
            => _stateCurrent.Value?.Update();

        internal void FixedUpdate()
            => _stateCurrent.Value?.FixedUpdate();
    }

    internal abstract class StateBase
    {
        protected readonly CompositeDisposable _disposable = new();

        protected readonly Fsm _fsm;
        protected readonly Game _baseExternalClass;

        internal StateBase(Game game, Fsm fsm)
        {
            _baseExternalClass = game;
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

        internal virtual void Update()
        {
            //Debug.Log($"Update {this}");
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

    internal sealed class StateWelcomeScreen : StateBase
    {
        internal StateWelcomeScreen(Game game, Fsm fsm) : base(game, fsm) { }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            CompositeDisposable _disposable = new();

            Observable
               .Timer(TimeSpan.FromSeconds(0.1f))
               .Subscribe(_ =>
               {
                   _fsm.SetState<StateMenu>();

                   _disposable.Clear();
               })
               .AddTo(_disposable);
        }
    }

    internal sealed class StateMenu : StateBase
    {
        internal StateMenu(Game game, Fsm fsm) : base(game, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass.DestroyGameplay();
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _baseExternalClass._signalBus
               .GetStream<GoToGame>()
               .Subscribe(evt =>
               {
                   _baseExternalClass._mapLoader.SetMapIndexDesired(evt.MapIndexDesired);
                   _fsm.SetState<StateGame>();
               })
               .AddTo(_disposable);
        }
    }

    internal sealed class StatePause : StateBase
    {
        internal StatePause(Game game, Fsm fsm) : base(game, fsm) { }
    }

    internal sealed class StateGame : StateBase
    {
        internal StateGame(Game game, Fsm fsm) : base(game, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            void LoadGameplay()
            {
                _baseExternalClass._mapLoader.CreateInstance();

                _baseExternalClass._playerLoader.CreateInstance();

                List<Transform> transforms = _baseExternalClass._playerLoader.Players.Select(x => x.transform).ToList();
                _baseExternalClass._cameraManager.SetMemberForCameraTargetGroup(transforms, 4.0f);

                _baseExternalClass._gameData.ResetMapScoreCurrent();
            }

            LoadGameplay();
        }

        internal override void Update()
        {
            base.Update();
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            float step = 0.25f;
            Observable
                .Interval(TimeSpan.FromSeconds(step))
                .Subscribe(_ =>
                {
                    _baseExternalClass._gameData.GlobalScoreData.Increase(_baseExternalClass._mapLoader.MapIndexCurrent, step);
                })
                .AddTo(_disposable);

            _baseExternalClass._inputHandler.Restart
                .Subscribe(value => _fsm.SetState<StateGame>())
                .AddTo(_disposable);

            _baseExternalClass._inputHandler.Menu
                .Subscribe(value => _fsm.SetState<StateMenu>())
                .AddTo(_disposable);

            _baseExternalClass._signalBus
               .GetStream<GoToGameOver>()
               .Subscribe(_ =>
               {
                   _fsm.SetState<StateGameOver>();
               })
               .AddTo(_disposable);

            _baseExternalClass._signalBus
               .GetStream<GoToMenu>()
               .Subscribe(evt =>
               {
                   _fsm.SetState<StateMenu>();
               })
               .AddTo(_disposable);
        }
    }

    internal sealed class StateGameOver : StateBase
    {
        internal StateGameOver(Game game, Fsm fsm) : base(game, fsm) { }

        internal override void Enter()
        {
            base.Enter();

            _baseExternalClass.DestroyGameplay();

            void GameRestart()
            {
                float delay = 3;

                CompositeDisposable disposable = new CompositeDisposable();
                Observable
                    .Timer(TimeSpan.FromSeconds(delay))
                    .Subscribe(_ =>
                    {
                        _fsm.SetState<StateGame>();

                        disposable.Clear();
                    })
                    .AddTo(disposable, _baseExternalClass);
            }

            GameRestart();
        }
    }

    internal sealed class MapLoader
    {
        private List<Map.Factory> _mapFactories;

        private Map _mapCurrent;

        internal int MapIndexCurrent { get; private set; } = 1;
        internal int MapIndexDesired { get; private set; }

        internal MapLoader(
            Game game,
            List<Map.Factory> mapFactories)
        {
            _mapFactories = mapFactories;
        }

        internal void CreateInstance()
        {
            DestroyInstance();

            _mapCurrent = _mapFactories[MapIndexDesired].Create();
            MapIndexCurrent = MapIndexDesired;
        }

        internal void SetMapIndexDesired(int value)
           => MapIndexDesired = Math.Clamp(value, 0, _mapFactories.Count - 1);

        internal void DestroyInstance()
        {
            if (_mapCurrent)
                Destroy(_mapCurrent.gameObject);
        }

        internal sealed class Factory : PlaceholderFactory<MapLoader> { }
    }

    internal sealed class PlayerLoader
    {
        private GameData _gameData;

        private Player.Factory _playerFactory;

        internal List<Player> Players { get; private set; } = new List<Player>();

        internal PlayerLoader(
            Player.Factory playerFactory,
            GameData gameData)
        {
            _playerFactory = playerFactory;
            _gameData = gameData;
        }

        internal void CreateInstance()
        {
            DestroyInstance();

            for (int playerIndex = 0; playerIndex < _gameData.PlayerPool.Length; playerIndex++)
            {
                if (!_gameData.PlayerPool[playerIndex].Online)
                    continue;

                Player newPlayer = _playerFactory.Create(playerIndex);

                Players.Add(newPlayer);
            }
        }

        internal void DestroyInstance()
        {
            foreach (Player player in Players)
                if(player)
                    Destroy(player.gameObject);

            Players.Clear(); // есть аналог с условием  .RemoveAll(x => x == null) (но RemoveAll не сбрасывает Count)
        }

        internal sealed class Factory : PlaceholderFactory<PlayerLoader> { }
    }
}
