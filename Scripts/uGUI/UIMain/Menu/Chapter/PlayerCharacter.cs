using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Zenject;

namespace UIMain.Menu.Chapter
{
    internal sealed class PlayerCharacter : Menu
    {
        private readonly ReactiveProperty<int> _playerCharacterIndexCurrent = new(0);
        private readonly ReactiveProperty<int> _playerIndexCurrent = new(0);

        private GameObject[] _playerCharacterPrefabs = new GameObject[0];

        [Header("Player Stuff")]
        [SerializeField] private TextMeshProUGUI _playerIndexText;
        [SerializeField] private Button _playerChangeIndexNextButton;
        [SerializeField] private Button _playerChangeIndexPreviousButton;
        [SerializeField] private Button _playerToggleOnlineButton;
        [SerializeField] private GameObject _playerToggleOnlineButtonLockImage;
        [SerializeField] private GameObject _playerOnlineOnImage;
        [SerializeField] private GameObject _playerOnlineOffImage;

        [Header("Player Character Stuff")]
        [SerializeField] private TextMeshProUGUI _playerCharacterIndexText;
        [SerializeField] private Button _playerCharacterChangeIndexNextButton;
        [SerializeField] private Button _playerCharacterChangeIndexPreviousButton;
        [SerializeField] private Transform _playerCharacterPreviewPlace;

        private PlayerCharacter() { }

        protected override void Awake()
        {
            base.Awake();

            _playerCharacterPrefabs = Resources.LoadAll<GameObject>("Prefabs/Character/Player");
        }

        protected override void ReactiveSubscription()
        {
            base.ReactiveSubscription();

            _playerIndexCurrent
                .Subscribe(value =>
                {
                    // индекс отсчет с 0, но игроку привычнее видеть с 1
                    _playerIndexText.text = $"{value + 1}"; 

                    DisplayPlayerToggleOnlineButtonAndImage();

                    int playerCharacterIndexInPool = Array.IndexOf(_playerCharacterPrefabs, _gameData.PlayerPool[value].Character);

                    _playerCharacterIndexCurrent.Value = playerCharacterIndexInPool;
                })
               .AddTo(_disposable);

            _playerCharacterIndexCurrent
               .Subscribe(value =>
               {
                   // индекс отсчет с 0, но игроку привычнее видеть с 1
                   _playerCharacterIndexText.text = $"{value + 1}";

                   _gameData.SetPlayerCharacter(_playerIndexCurrent.Value, value);

                   void LoadPreviewModel()
                   {
                       void DestroyOld()
                       {
                           if (_playerCharacterPreviewPlace.childCount > 0)
                               foreach (Transform child in _playerCharacterPreviewPlace.transform)
                                   Destroy(child.gameObject);
                       }

                       void CreateNew()
                       {
                           GameObject newCharacter = _playerCharacterPrefabs[_playerCharacterIndexCurrent.Value];

                           GameObject newInstance = Instantiate(
                               newCharacter,
                               _playerCharacterPreviewPlace.transform.position,
                               _playerCharacterPreviewPlace.transform.rotation,
                               _playerCharacterPreviewPlace.transform);
                       }

                       DestroyOld();
                       CreateNew();
                   }

                   LoadPreviewModel();
               })
               .AddTo(_disposable);

            _playerToggleOnlineButton
               .OnClickAsObservable()
               .Subscribe(_ =>
               {
                   _gameData.TogglePlayerOnline(_playerIndexCurrent.Value);

                   DisplayPlayerToggleOnlineButtonAndImage();
               })
               .AddTo(_disposable);

            _playerCharacterChangeIndexNextButton
               .OnClickAsObservable()
               .Subscribe(_ => _playerCharacterIndexCurrent.Value = Mathf.Clamp(_playerCharacterIndexCurrent.Value + 1, 0, _playerCharacterPrefabs.Length - 1))
               .AddTo(_disposable);

            _playerCharacterChangeIndexPreviousButton
               .OnClickAsObservable()
               .Subscribe(_ => _playerCharacterIndexCurrent.Value = Mathf.Clamp(_playerCharacterIndexCurrent.Value - 1, 0, _playerCharacterPrefabs.Length - 1))
               .AddTo(_disposable);

            _playerChangeIndexNextButton
            .OnClickAsObservable()
               .Subscribe(_ => _playerIndexCurrent.Value = Mathf.Clamp(_playerIndexCurrent.Value + 1, 0, _gameData.PlayerPool.Length - 1))
               .AddTo(_disposable);

            _playerChangeIndexPreviousButton
            .OnClickAsObservable()
               .Subscribe(_ => _playerIndexCurrent.Value = Mathf.Clamp(_playerIndexCurrent.Value - 1, 0, _gameData.PlayerPool.Length - 1))
               .AddTo(_disposable);
        }

        private void DisplayPlayerToggleOnlineButtonAndImage()
        {
            // Первого игрока нельзя отключить
            _playerToggleOnlineButton.enabled = _playerIndexCurrent.Value > 0;
            _playerToggleOnlineButtonLockImage.SetActive(_playerIndexCurrent.Value == 0);

            _playerOnlineOnImage.SetActive(_gameData.PlayerPool[_playerIndexCurrent.Value].Online == true && _playerIndexCurrent.Value > 0);
            _playerOnlineOffImage.SetActive(_gameData.PlayerPool[_playerIndexCurrent.Value].Online == false && _playerIndexCurrent.Value > 0);
        }
    }
}
