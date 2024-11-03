using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;

namespace UIMain.UIGame
{
    internal sealed class TextUpdate : UIMain
    {
        [SerializeField] private TextMeshProUGUI _globalScoreText;

        private TextUpdate() { }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _signalBus
                .GetStream<GobalScoreChangedInData>()
                .Subscribe(evt => 
                {
                    void UpdateText()
                    {
                        int minutesCurrent = Mathf.FloorToInt(evt.ValueChangedTo / 60);
                        int secondsCurrent = Mathf.FloorToInt(evt.ValueChangedTo % 60);

                        int minutesMax = Mathf.FloorToInt(evt.ValueMaxInIndex / 60);
                        int secondsMax = Mathf.FloorToInt(evt.ValueMaxInIndex % 60);

                        _globalScoreText.text =
                            $"{minutesCurrent.ToString()}:{secondsCurrent.ToString()}" +
                            $" ({minutesMax.ToString()}:{secondsMax.ToString()})";
                    }

                    UpdateText();
                })
                .AddTo(_disposable);
        }
    }
}
