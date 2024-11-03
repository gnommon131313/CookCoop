using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void Blink(this GameObject obj, float interval, int amount)
    {
        CompositeDisposable disposable = new CompositeDisposable();
        CompositeDisposable disposable1 = new CompositeDisposable();
        int i = 1;

        // Первый раз вручную т.к. Interval(TimeSpan.FromSeconds(step)) первый сработатет только после заданой задержки а не сразу
        obj.SetActive(true);
        Observable
            .Timer(TimeSpan.FromSeconds(interval / 2))
            .Subscribe(_ =>
            {
                obj.SetActive(false);

                disposable1.Clear();
            })
            .AddTo(disposable1, obj);

        if (amount == 1)
            return;

        Observable
            .Interval(TimeSpan.FromSeconds(interval))
            .Subscribe(_ =>
            {
                i++;

                obj.SetActive(true);

                Observable
                    .Timer(TimeSpan.FromSeconds(interval / 2))
                    .Subscribe(_ =>
                    {
                        obj.SetActive(false);

                        disposable1.Clear();
                    })
                    .AddTo(disposable1, obj);

                if (i >= amount)
                    disposable.Clear();

            })
            .AddTo(disposable, obj);
    }
}
