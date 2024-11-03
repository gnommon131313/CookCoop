using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Zenject;

namespace UIMain.Menu.Chapter
{
    internal sealed class Map : Menu
    {
        private GameObject[] _mapPrefabs;

        private readonly ReactiveProperty<int> _indexCurrent = new(1);

        [SerializeField] private Transform _previewPlace;
        [SerializeField] private TextMeshProUGUI _indexText;
        [SerializeField] private Button _changeIndexNextButton;
        [SerializeField] private Button _changeIndexPreviousButton;
        [SerializeField] private Button _applyIndexButton;

        [SerializeField] private TextMeshProUGUI _globalScoreText;

        private Map() { }

        protected override void Awake()
        {
            base.Awake();

            _mapPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateGlobalScoreTextToValueMax();
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _indexCurrent
               .Subscribe(value =>
               {
                   _indexText.text = value.ToString();

                   void LoadPreviewModel()
                   {
                       void DestroyOld()
                       {
                           if (_previewPlace.childCount > 0)
                               foreach (Transform child in _previewPlace.transform)
                                   Destroy(child.gameObject);
                       }

                       void CreateNew()
                       {
                           GameObject newMap = _mapPrefabs[_indexCurrent.Value];
                           MonoBehaviour[] allScripts = newMap.GetComponentsInChildren<MonoBehaviour>();

                           // Откличение скриптов на ПРЕФАБЕ, поэтому в конце нужно снова их включить
                           foreach (MonoBehaviour script in allScripts)
                               script.enabled = false;

                           GameObject newInstance = Instantiate(
                               newMap,
                               _previewPlace.transform.position,
                               _previewPlace.transform.rotation,
                               _previewPlace.transform);

                           foreach (MonoBehaviour script in allScripts)
                               script.enabled = true;
                       }

                       DestroyOld();
                       CreateNew();
                   }

                   LoadPreviewModel();
                   UpdateGlobalScoreTextToValueMax();
               })
               .AddTo(_disposable);

            _applyIndexButton
               .OnClickAsObservable()
               .Subscribe(_ => _signalBus.TryFire(new GoToGame(_indexCurrent.Value)))
               .AddTo(_disposable);

            _changeIndexNextButton
            .OnClickAsObservable()
               .Subscribe(_ => _indexCurrent.Value = Mathf.Clamp(_indexCurrent.Value + 1, 1, _mapPrefabs.Length - 1))
               .AddTo(_disposable);

            _changeIndexPreviousButton
               .OnClickAsObservable()
               .Subscribe(_ => _indexCurrent.Value = Mathf.Clamp(_indexCurrent.Value - 1, 1, _mapPrefabs.Length - 1))
               .AddTo(_disposable);
        }

        private void UpdateGlobalScoreTextToValueMax()
        {
            if (!_gameData.GlobalScoreData.Max.TryGetValue(_indexCurrent.Value, out float value))
                return;

            int minutesMax = Mathf.FloorToInt(value / 60);
            int secondsMax = Mathf.FloorToInt(value % 60);

            _globalScoreText.text = $" {minutesMax.ToString()}:{secondsMax.ToString()}";
        }
    }
}
