using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

internal abstract class MonoBehaviourNotifier : MonoBehaviour
{
    internal sealed class Destroyed : MonoBehaviourNotifier
    {
        // Subject: Ёто класс, который реализует как IObservable, так и IObserver. ќн может как отправл€ть, так и получать значени€. Ёто делает его полезным дл€ создани€ событий или дл€ передачи значений от одного места к другому.
        // Unit: Ёто структура, котора€ используетс€ в UniRx дл€ обозначени€ "ничего" или "пустого значени€". ќна аналогична void в стандартном C#, но в контексте реактивного программировани€ ее можно использовать как значение, которое будет отправлено или обработано подписчиками. Unit используетс€, когда вам не нужно передавать конкретное значение, а достаточно просто уведомить об каком-то событии.
        internal readonly Subject<Unit> Event = new Subject<Unit>();

        void OnDestroy()
        {
            Event.OnNext(Unit.Default); // ”ведомл€ем подписчиков
            Event.OnCompleted(); // «авершаем Subject
        }
    }
}

// Example
//if (value && !value.gameObject.GetComponent<MonoBehaviourNotifier>())
//{
//    value.gameObject.AddComponent<MonoBehaviourNotifier.Destroyed>().Event
//        .Subscribe(_ => _item.Value = null)
//        .AddTo(_disposable); // ! не уверен на счет корректности отписки
//}
