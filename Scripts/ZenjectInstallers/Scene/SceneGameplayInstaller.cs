using UnityEngine;
using Zenject;
using Cinemachine;
using System;
using TMPro;
using System.Linq;

internal sealed class SceneGameplayInstaller : MonoInstaller<SceneGameplayInstaller>
{
    [SerializeField] private Game _game;

    [SerializeField] private CameraManager _cameraManager;

    [SerializeField] private GameObject _playerPrefab;

    [SerializeField] private Transform _playerParent;
    [SerializeField] private Transform _mapParent;

    private Map[] _mapPrefabs;
    private Item[] _itemPrefabs;

    public override void InstallBindings()
    {
        _mapPrefabs = Resources.LoadAll<Map>("Prefabs/Map");
        _itemPrefabs = Resources.LoadAll<Item>("Prefabs/Gameplay/Item");

        void Factoryes()
        {
            Container.BindFactory<int, Player, Player.Factory>()
                .FromComponentInNewPrefab(_playerPrefab)
                .UnderTransform(_playerParent)
                .AsCached();

            foreach (var map in _mapPrefabs)
                Container.BindFactory<Map, Map.Factory>()
                    .FromComponentInNewPrefab(map)
                    .UnderTransform(_mapParent)
                    .AsCached();

            foreach (var item in _itemPrefabs)
                Container.BindFactory<Item, Item.Factory>()
                   .FromComponentInNewPrefab(item)
                   .WithGameObjectName($"{item.Type} (Clone)")
                   .AsCached();

            Container.BindFactory<Game.PlayerLoader, Game.PlayerLoader.Factory>()
                .AsCached();

            Container.BindFactory<Game.MapLoader, Game.MapLoader.Factory>()
                .AsCached();
        }

        void Uncategorized()
        {
            Container.BindInstance(_game);
            Container.QueueForInject(_game);

            Container.BindInstance(_cameraManager);
            Container.QueueForInject(_cameraManager);
        }

        Factoryes();
        Uncategorized();
    }
}
