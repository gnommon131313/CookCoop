using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIMain.Menu.Chapter
{
    internal sealed class Switcher : Menu
    {
        [Header("Chapters")]
        [SerializeField] private GameObject _mapChapter;
        [SerializeField] private GameObject _characterChapter;
        [SerializeField] private GameObject _inputSetupChapter;
        [SerializeField] private GameObject _soundSetupChapter;

        [Header("Chapter Buttons")]
        [SerializeField] private Button _mapButton;
        [SerializeField] private Button _characterButton;
        [SerializeField] private Button _inputSetupButton;
        [SerializeField] private Button _soundSetupButton;

        private Switcher() { }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            ChapterCurrent
               .Subscribe(value =>
               {
                   void DisplayCurrent()
                   {
                       _mapChapter.SetActive(ChapterCurrent.Value == Chapter.Map);
                       _characterChapter.SetActive(ChapterCurrent.Value == Chapter.Character);
                       _inputSetupChapter.SetActive(ChapterCurrent.Value == Chapter.InputSetup);
                       _soundSetupChapter.SetActive(ChapterCurrent.Value == Chapter.SoundSetup);
                   }

                   DisplayCurrent();
               })
               .AddTo(_disposable);

            _mapButton
               .OnClickAsObservable()
               .Subscribe(_ => ChapterCurrent.Value = Chapter.Map)
               .AddTo(_disposable);

            _characterButton
              .OnClickAsObservable()
              .Subscribe(_ => ChapterCurrent.Value = Chapter.Character)
              .AddTo(_disposable);

            _inputSetupButton
                .OnClickAsObservable()
                .Subscribe(_ => ChapterCurrent.Value = Chapter.InputSetup)
                .AddTo(_disposable);

            _soundSetupButton
                .OnClickAsObservable()
                .Subscribe(_ => ChapterCurrent.Value = Chapter.SoundSetup)
                .AddTo(_disposable);
        }
    }
}

