using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

internal abstract class MonoBehaviourNotifier : MonoBehaviour
{
    internal sealed class Destroyed : MonoBehaviourNotifier
    {
        // Subject: ��� �����, ������� ��������� ��� IObservable, ��� � IObserver. �� ����� ��� ����������, ��� � �������� ��������. ��� ������ ��� �������� ��� �������� ������� ��� ��� �������� �������� �� ������ ����� � �������.
        // Unit: ��� ���������, ������� ������������ � UniRx ��� ����������� "������" ��� "������� ��������". ��� ���������� void � ����������� C#, �� � ��������� ����������� ���������������� �� ����� ������������ ��� ��������, ������� ����� ���������� ��� ���������� ������������. Unit ������������, ����� ��� �� ����� ���������� ���������� ��������, � ���������� ������ ��������� �� �����-�� �������.
        internal readonly Subject<Unit> Event = new Subject<Unit>();

        void OnDestroy()
        {
            Event.OnNext(Unit.Default); // ���������� �����������
            Event.OnCompleted(); // ��������� Subject
        }
    }
}

// Example
//if (value && !value.gameObject.GetComponent<MonoBehaviourNotifier>())
//{
//    value.gameObject.AddComponent<MonoBehaviourNotifier.Destroyed>().Event
//        .Subscribe(_ => _item.Value = null)
//        .AddTo(_disposable); // ! �� ������ �� ���� ������������ �������
//}
