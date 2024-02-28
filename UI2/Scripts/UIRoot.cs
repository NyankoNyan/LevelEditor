using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace UI2
{
    public enum SignalDirection
    {
        Broadcast,
        Self,
        DrillUp,
        DrillDown
    }

    /// <summary>
    /// Отвечает за инициализацию и хранение состояния всего UI
    /// </summary>
    public class UIRoot
    {
        private readonly Dictionary<string, Style> _styles = new();
        private readonly HashSet<IElementInstance> _instances = new();
        private readonly HashSet<IElementInstance> _listeners = new();
        private readonly MonoBehaviour _provider;
        private readonly Dictionary<Transform, IElementInstance> _viewToModelMap = new();

        /// <summary>
        ///
        /// </summary>
        /// <param name="provider">Объект Юнити, через который осуществляется запуск корутин</param>
        public UIRoot(MonoBehaviour provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Регистрирует стиль и делает его доступным для использования в элементах
        /// </summary>
        /// <param name="style">Стиль - это по сути описание префаба, который будет заспавнен для соответствующего
        /// элемента</param>
        /// <returns>Стиль новый и был добавлен</returns>
        public bool Reg(Style style)
        {
            return _styles.TryAdd(style.name, style);
        }

        /// <summary>
        /// Регистрирует коллекцию стилей. Каждый из стилей станет доступным для использования в элементах.
        /// </summary>
        /// <param name="styles">Коллекция стилей. Стиль - это по сути описание префаба, который будет заспавнен для соответствующего
        /// элемента</param>
        /// <returns>Возвращает ту же коллекцию в режиме итератора</returns>
        public IEnumerable<Style> Reg(IEnumerable<Style> styles)
        {
            foreach (var style in styles) {
                if (!Reg(style)) {
                    yield return style;
                }
            }
        }

        /// <summary>
        /// Спавнит элемент UI
        /// </summary>
        /// <param name="setup">Описание элемента</param>
        /// <param name="parent">Куда спавним. Это может быть объект канваса, либо другой элемент UI</param>
        /// <returns>Заспавненная инстанция</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ElementWorkflowException"></exception>
        public IElementInstance Attach(IElementSetupRead setup, Transform parent)
        {
            // TODO Добавить привязку к 3D-трансформам
            if (!parent) {
                throw new ArgumentException("Empty parent");
            }

            if (_styles.TryGetValue(setup.Style, out Style style)) {
                // TODO May be merge facade and instance?
                // Если родитель - элемент UI, привяжемся к специальной зоне для потомков
                IElementInstance parentInstance = parent ? _viewToModelMap.GetValueOrDefault(parent, null) : null;
                Transform parentPivot = null;
                if (parentInstance != null) {
                    var parentFacade = parent.GetComponent<ElementInstanceFacade>();
                    if (parentFacade) {
                        parentPivot = parentFacade.SubZone;
                    }
                }

                // По уполчанию привязываемся напрямую к родителю
                if (!parentPivot) {
                    parentPivot = parent;
                }

                // Спавним вьюху, которая будет видна на сцене
                var newGO = GameObject.Instantiate(style.prefab, parentPivot);
                // Создаём инстанцию элемента (это будет наша модель)
                var instance = new ElementInstance(setup, newGO, parentInstance, this);

                // Индекс для поиска модели по вьюхе
                _viewToModelMap.Add(newGO.transform, instance);

                // TODO А может это всё вынести в конструктор?
                // Для canvas-фасадов доступно переопределение привязок и размеров
                var rectTransform = newGO.GetComponent<RectTransform>();
                if (rectTransform) {
                    if (setup.NewAnchor) {
                        (rectTransform.anchorMin, rectTransform.anchorMax) = setup.Anchor;
                    }

                    if (setup.NewAnchoredPosition) {
                        rectTransform.anchoredPosition = setup.AnchoredPosition;
                    }

                    if (setup.NewPivot) {
                        rectTransform.pivot = setup.Pivot;
                    }

                    if (setup.NewSizeDelta) {
                        rectTransform.sizeDelta = setup.SizeDelta;
                    }
                }

                // Обязательно сохраняем элемент, чтобы не потерять
                // TODO А оно надо, если ранее создали индекс?
                _instances.Add(instance);

                // Если у элемента есть перехватчики событий, регистрируем его как событийный
                // (типа так быстрее должно работать, ведь так?)
                if (setup.HasHandlers) {
                    _listeners.Add(instance);
                }

                // Скрытые по умолчанию элементы скрываем
                if (setup.NeedHide) {
                    newGO.SetActive(false);
                }

                // У вьюхи может быть специальный фасад, позволяющий тоньше взаимодействовать с ней
                var facade = newGO.GetComponent<ElementInstanceFacade>();
                if (facade) {
                    // Спавним потомков этого элемента
                    foreach (var sub in setup.Subs) {
                        Attach(sub, facade.transform);
                    }

                    // Связываем фичи фасада с моделью
                    // TODO переименовать во что-то типа Link
                    facade.InitFeatures(instance);
                }

                // Постинициализация от потомков к родителю
                // Внутри будет в т.ч. инициализация ссылок на переменные потомков
                if (instance.Children != null) {
                    foreach (var child in instance.Children) {
                        child.LateInit();
                    }
                }
                instance.LateInit();

                if (facade) {
                    // Запускаем инициализацию фич
                    foreach (var featureCall in setup.Features) {
                        featureCall.Call(instance);
                    }
                }

                return instance;
            } else {
                throw new ElementWorkflowException($"missing prefab for style [{setup.Style}]");
            }
        }

        /// <summary>
        /// Посылает управляющий сигнал в дерево UI
        /// </summary>
        /// <param name="name">Имя/идентификатор сигнала, по которому он будет приниматься</param>
        /// <param name="data">Дополнительные данные, прикрепляемые к сигналу</param>
        /// <param name="sender">Элемент, пославший сигнал (элемент должен передать сам себя)</param>
        /// <param name="direction">Направление передачи сигнала (
        /// Broadcast - на все элементы UI,
        /// Self - только на этот элемент,
        /// DrillUp - на все родительские элементы от ближайшего,
        /// DrillDown - на всех потомков поиском в глубину)</param>
        /// <param name="consumable">Сигнал не передаётся дальше, если нашелся элемент, принявший его
        /// (элемент может отказаться принимать сигнал вызвав метод Resume)</param>
        /// <exception cref="NotImplementedException"></exception>
        internal void SendSignal(
            string name,
            object data,
            IElementInstance sender,
            SignalDirection direction,
            bool consumable)
        {
            // Создаём объект сингнала
            SignalContext signal = new SignalContext(name, data);
            switch (direction) {
                // Сигнал должен попасть во все подписанные на него элементы сцены
                case SignalDirection.Broadcast: {
                        foreach (var listener in _listeners) {
                            var handlers = listener.Proto.GetHandlers(name);
                            if (handlers == null) {
                                continue;
                            }

                            if (handlers.Any(h => {
                                h(signal, new ElementRuntimeContext(listener, this));
                                return consumable && signal.Consumed;
                            })) {
                                break;
                            }
                        }

                        break;
                    }

                // Это типа loopback. Сигнал уходит на этот же элемент.
                case SignalDirection.Self: {
                        _ = sender.Proto.GetHandlers(name)?.All(h => {
                            h(signal, new ElementRuntimeContext(sender, this));
                            return true;
                        });
                        break;
                    }

                // Сигнал уходит вверх по родительской иерархии
                case SignalDirection.DrillUp: {
                        HashSet<IElementInstance> antiRecursion = new() { sender };
                        var current = sender.Parent;
                        while (current != null) {
                            if (!antiRecursion.Add(current)) {
                                Debug.LogWarning($"found recursion in element {current.Proto.Id}");
                                break;
                            }

                            var result = current.Proto.GetHandlers(name)?.Any(h => {
                                h(signal, new ElementRuntimeContext(current, this));
                                return consumable && signal.Consumed;
                            });
                            if (result.HasValue && result.Value) {
                                break;
                            }

                            current = current.Proto.SignalBlocked ? null : current.Parent;
                        }

                        break;
                    }

                // Сигнал уходит по потомкам путём поиска в глубину
                case SignalDirection.DrillDown: {
                        HashSet<IElementInstance> antiRecursion = new() { sender };

                        DeepSearch(sender);

                        break;

                        void DeepSearch(IElementInstance elem)
                        {
                            if (elem.Children == null || elem.Proto.SignalBlocked) {
                                return;
                            }

                            foreach (var sub in elem.Children) {
                                if (!antiRecursion.Add(sub)) {
                                    Debug.LogWarning($"found recursion in element {elem.Proto.Id}");
                                    continue;
                                }

                                var result = sub.Proto.GetHandlers(name)?.Any(h => {
                                    h(signal, new ElementRuntimeContext(sub, this));
                                    return consumable && signal.Consumed;
                                });
                                if (result.HasValue && result.Value) {
                                    break;
                                }

                                DeepSearch(sub);
                                if (consumable && signal.Consumed) {
                                    break;
                                }
                            }
                        }
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        internal OperationDescriptor StartOperation(IOperation operation)
            => new(_provider.StartCoroutine(operation.Exec()));
    }
}