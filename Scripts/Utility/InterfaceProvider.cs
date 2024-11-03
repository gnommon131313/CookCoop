using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

internal class InterfaceProvider<T>
{
    [SerializeField] private GameObject _object;
    protected readonly ReactiveProperty<T> _interface = new();

    internal GameObject Object => _object;
    internal IReadOnlyReactiveProperty<T> Interface => _interface;

    internal void OnValidate()
    {
        if (_object && _object.GetComponent<T>() == null)
            _object = null;
    }

    internal void Awaka()
    {
        if (_object)
            _interface.Value = _object.GetComponent<T>();
    }

    // Прямое изменение
    internal void Set(T t)
    {
        _interface.Value = t;
    }

    // Изменение через GameObject
    internal void Set(GameObject gameObject)
    {
        _object = gameObject;
        _interface.Value = _object.GetComponent<T>();
    }
}



// Example
//internal interface IHoldable
//{
//}

//[Serializable]
//internal class IHoldableProvider : InterfaceProvider<IHoldable>
//{
//}