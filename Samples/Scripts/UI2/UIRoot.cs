using System;
using System.Collections.Generic;

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

    public class UIRoot
    {
        private readonly Dictionary<string, Style> _styles = new();
        private readonly HashSet<IElementInstance> _instances = new();
        private readonly HashSet<IElementInstance> _listeners = new();
        private readonly MonoBehaviour _provider;

        public UIRoot(MonoBehaviour provider)
        {
            _provider = provider;
        }

        public bool Reg(Style style)
        {
            return _styles.TryAdd(style.name, style);
        }

        public IEnumerable<Style> Reg(IEnumerable<Style> styles)
        {
            foreach (var style in styles) {
                if (!Reg(style)) {
                    yield return style;
                }
            }
        }

        public IElementInstance Attach(IElementSetupReadWrite setup, Transform parent)
        {
            if (!parent) {
                throw new ArgumentException("Empty parent");
            }

            if (_styles.TryGetValue(setup.Style, out Style style)) {
                var newGO = GameObject.Instantiate(style.prefab, parent);
                var parentInstance = parent.GetComponent<ElementInstanceFacade>()?.ElementInstance;
                var instance = new ElementInstance(setup, newGO, parentInstance);

                var facade = newGO.GetComponent<ElementInstanceFacade>();
                if (facade) {
                    foreach (var sub in setup.Subs) {
                        Attach(sub, facade.SubZone);
                    }
                }

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

                _instances.Add(instance);
                if (setup.HasHandlers) {
                    _listeners.Add(instance);
                }

                if (setup.NeedHide) {
                    newGO.SetActive(false);
                }

                return instance;
            } else {
                throw new ElementWorkflowException();
            }
        }

        internal void SendSignal(
            string name,
            object data,
            IElementInstance sender,
            SignalDirection direction,
            bool consumable)
        {
            SignalContext signal = new SignalContext(name, data);
            switch (direction) {
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
                case SignalDirection.Self: {
                    _ = sender.Proto.GetHandlers(name)?.All(h => {
                        h(signal, new ElementRuntimeContext(sender, this));
                        return true;
                    });
                    break;
                }
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

                        current = current.Parent;
                    }

                    break;
                }
                case SignalDirection.DrillDown: {
                    HashSet<IElementInstance> antiRecursion = new() { sender };

                    DeepSearch(sender);

                    break;

                    void DeepSearch(IElementInstance elem)
                    {
                        if (elem.Children == null) {
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