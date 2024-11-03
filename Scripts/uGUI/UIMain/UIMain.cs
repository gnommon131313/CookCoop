using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace UIMain
{
    internal abstract class UIMain : MonoBehaviour
    {
        protected readonly CompositeDisposable _disposable = new();

        protected SignalBus _signalBus;
        protected Game _game;
        protected GameData _gameData;
        protected CameraManager _cameraManager;

        protected UIMain() { }

        [Inject]
        private void Construct(
            SignalBus signalBus,
            Game game, 
            GameData gameData,
            CameraManager cameraManager)
        {
            _signalBus = signalBus;
            _game = game;
            _gameData = gameData;
            _cameraManager = cameraManager;
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }
        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

        protected virtual void OnEnable()
        {
            ReactiveSubscription();
        }

        protected virtual void OnDisable()
        {
            ReactiveUnSubscription();
        }

        protected virtual void ReactiveSubscription()
        {
        }

        protected virtual void ReactiveUnSubscription()
        {
            _disposable.Clear();
        }
    }
}
