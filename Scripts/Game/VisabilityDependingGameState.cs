using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using Zenject;

internal sealed class VisabilityDependingGameState : MonoBehaviour
{
    private readonly CompositeDisposable _disposable = new();

    private Game _game;

    [SerializeField] private UIVisibleInState _visibleInState;
    [SerializeField] private GameObject _container;

    private VisabilityDependingGameState() { }

    [Inject]
    private void Construct(Game game)
    {
        _game = game;
    }

    private void OnEnable()
    {
        ReactiveSubscription();
    }

    private void OnDisable()
    {
        ReactiveUnSubscription();
    }

    private void ReactiveSubscription()
    {
        _game.StateCurrent
            .Subscribe(value =>
            {
                void DetermineContentVisible()
                {
                    if (value == null)
                        return;

                    // Зависимоть от .ToString() не самый надежный и эффективный способ, зато ОЧЕНЬ простой
                    bool visible = value.ToString() == "Game+State" + _visibleInState.ToString()
                        || _visibleInState == UIVisibleInState.Always;

                    // SetActive не подходит потому что часто нужно чтобы обьект продолжал выполнение скриптов или нужно его дочерним обьектам
                    _container.SetActive(visible);
                }

                DetermineContentVisible();
            })
            .AddTo(_disposable);
    }

    private void ReactiveUnSubscription() => _disposable.Clear();

    private enum UIVisibleInState
    {
        WelcomeScreen = 1 << 0,
        Game = 1 << 1,
        GameOver = 1 << 2,
        Pause = 1 << 28,
        Menu = 1 << 29,
        Always = 1 << 31,
    }
}

