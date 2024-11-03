using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

internal sealed class Eye : MonoBehaviour
{
    private readonly CompositeDisposable _disposable = new();

    private readonly ReactiveProperty<float> _blinkTimer = new(0);
    [SerializeField] private float _blinkTime = 3;
    [SerializeField] private GameObject[] _eyelids;

    private Eye() { }

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
        float step = 0.5f;
        Observable
            .Interval(TimeSpan.FromSeconds(step))
            .Subscribe(_ =>
            {
                if (_blinkTimer.Value > 0)
                    _blinkTimer.Value -= step;
            })
            .AddTo(_disposable);

        _blinkTimer
            .Subscribe(value =>
            {
                if (value > 0)
                    return;

                _blinkTimer.Value = _blinkTime;

                void Blink()
                {
                    foreach (var eyelib in _eyelids)
                    {
                        CompositeDisposable disposable = new CompositeDisposable();

                        Observable
                            .Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(0.1f, 0.4f)))
                            .Subscribe(_ =>
                            {
                                eyelib.SetActive(true);

                                Observable
                                    .Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(0.1f, 0.4f)))
                                    .Subscribe(_ =>
                                    {
                                        eyelib.SetActive(false);

                                        disposable.Clear();
                                    })
                                    .AddTo(disposable, gameObject);
                            })
                            .AddTo(disposable, gameObject);
                    }
                }

                Blink();
            })
            .AddTo(_disposable);
    }

    private void ReactiveUnSubscription()
        => _disposable.Clear();
}
