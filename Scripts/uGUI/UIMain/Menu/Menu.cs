using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace UIMain.Menu
{
    internal abstract class Menu : UIMain
    {
        protected static readonly ReactiveProperty<Chapter> ChapterCurrent = new(Chapter.Map);

        protected Menu() { }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            ChapterCurrent
               .Subscribe(value =>
               {
                   SetupCamera();
               })
               .AddTo(_disposable);

            _game.StateCurrent
                .Subscribe(value =>
                {
                    if (value is Game.StateMenu)
                        SetupCamera();
                })
               .AddTo(_disposable);
        }

        protected void SetupCamera()
        {
            if (ChapterCurrent.Value == Chapter.Map)
                _cameraManager.SetMemberForCameraTargetGroup(new List<Transform> { transform }, 8.0f);
            if (ChapterCurrent.Value == Chapter.Character)
                _cameraManager.SetMemberForCameraTargetGroup(new List<Transform> { transform }, 6.0f);
            if (ChapterCurrent.Value == Chapter.InputSetup)
                _cameraManager.SetMemberForCameraTargetGroup(new List<Transform> { transform }, 1.0f);
            if (ChapterCurrent.Value == Chapter.SoundSetup)
                _cameraManager.SetMemberForCameraTargetGroup(new List<Transform> { transform }, 1.0f);
        }

        protected enum Chapter
        {
            Map = 1 << 0,
            Character = 1 << 1,
            InputSetup = 1 << 2,
            SoundSetup = 1 << 3,
        }
    }
}
