using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

internal sealed class GameData : IInitializable
{
    private SignalBus _signalBus;

    private GameObject[] _playerCharacterPrefabs = new GameObject[0];
    private GameObject[] _mapPrefabs = new GameObject[0];

    // Количество элементов = максимальное количество игроков
    internal readonly PlayerData[] PlayerPool =
    {
        new(true),
        new(false),
        new(false),
        new(false)
    };
    internal readonly GlobalScore GlobalScoreData;

    private GameData(SignalBus signalBus)
    {
        _signalBus = signalBus;

        GlobalScoreData = new(this);
    }

    public void Initialize()
    {

        void Setup()
        {
            void LoadResources()
            {
                _playerCharacterPrefabs = Resources.LoadAll<GameObject>("Prefabs/Character/Player");
                _mapPrefabs = Resources.LoadAll<GameObject>("Prefabs/Map");
            }

            void FirstSetupData()
            {
                foreach (var player in PlayerPool)
                    player.Character = _playerCharacterPrefabs[0];

                for (int i = 0; i < _mapPrefabs.Length; i++)
                    GlobalScoreData.Max.Add(i, 0);

            }

            LoadResources();
            FirstSetupData();
        }

        Setup();
    }

    internal void ResetMapScoreCurrent()
        => GlobalScoreData.Current.Value = 0;

    internal void SetPlayerCharacter(int playerIndex, int characterIndex)
    {
        if (_playerCharacterPrefabs == null || _playerCharacterPrefabs.Length == 0)
            return;

        playerIndex = Math.Clamp(playerIndex, 0, PlayerPool.Length - 1);
        characterIndex = Math.Clamp(characterIndex, 0, _playerCharacterPrefabs.Length - 1);

        PlayerPool[playerIndex].Character = _playerCharacterPrefabs[characterIndex];
    }

    internal void TogglePlayerOnline(int playerIndex)
    {
        PlayerPool[playerIndex].Online = !PlayerPool[playerIndex].Online;
    }

    internal sealed class PlayerData
    {
        internal bool Online { get; set; } = true;
        internal GameObject Character { get; set; } = null;

        internal PlayerData(bool firstOnline)
        {
            Online = firstOnline;
        }
    }

    internal sealed class GlobalScore
    {
        private readonly GameData _baseExternalClass;

        internal readonly ReactiveProperty<float> Current = new(0);
        internal readonly Dictionary<int, float> Max = new();

        internal GlobalScore(GameData gameData)
        {
            _baseExternalClass = gameData;
        }

        internal void Increase(int inIndex, float value)
        {
            if (!Max.ContainsKey(inIndex))
            {
                Debug.Log("Data for this Map - Miss");

                return;
            }

            Current.Value += value;

            if (Max[inIndex] < Current.Value)
                Max[inIndex] = Current.Value;

            _baseExternalClass._signalBus.TryFire(new GobalScoreChangedInData(inIndex, Max[inIndex], Current.Value));
        }
    }
}
