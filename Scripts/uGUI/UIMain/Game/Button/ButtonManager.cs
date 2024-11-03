using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIMain.UIGame
{
    internal sealed class ButtonManager : UIMain
    {
        [Header("Button")]
        [SerializeField] private Button _openMenuButton;
        [SerializeField] private Button _toggleHintButton;

        [Header("Stuff")]
        [SerializeField] private GameObject _hintPanel;

        private ButtonManager() { }

        protected override void Awake()
        {
            base.Awake();

            _hintPanel.SetActive(false);
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _openMenuButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    _signalBus.TryFire(new GoToMenu());
                })
                .AddTo(_disposable);

            _toggleHintButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    _hintPanel.SetActive(!_hintPanel.activeSelf);
                })
                .AddTo(_disposable);
        }
    }
}
