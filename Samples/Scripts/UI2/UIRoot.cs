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

        public IElementInstance Attach(IElementSetupReadWrite setupReadWrite, Transform parent)
        {
            if (!parent) {
                throw new ArgumentException("Empty parent");
            }

            if (_styles.TryGetValue(setupReadWrite.Style, out Style style)) {
                var newGO = GameObject.Instantiate(style.prefab, parent);
                var parentInstance = parent.GetComponent<ElementInstanceFacade>()?.ElementInstance;
                var instance = new ElementInstance(setupReadWrite, newGO, parentInstance);

                var facade = newGO.GetComponent<ElementInstanceFacade>();
                if (facade) {
                    foreach (var sub in setupReadWrite.Subs) {
                        Attach(sub, facade.SubZone);
                    }
                }

                var rectTransform = newGO.GetComponent<RectTransform>();
                if (rectTransform) {
                    if (setupReadWrite.NewAnchor) {
                        (rectTransform.anchorMin, rectTransform.anchorMax) = setupReadWrite.Anchor;
                    }

                    if (setupReadWrite.NewAnchoredPosition) {
                        rectTransform.anchoredPosition = setupReadWrite.AnchoredPosition;
                    }

                    if (setupReadWrite.NewPivot) {
                        rectTransform.pivot = setupReadWrite.Pivot;
                    }

                    if (setupReadWrite.NewSizeDelta) {
                        rectTransform.sizeDelta = setupReadWrite.SizeDelta;
                    }
                }

                _instances.Add(instance);
                if (setupReadWrite.Handler != null) {
                    _listeners.Add(instance);
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
                        listener.Proto.Handler(signal, new ElementRuntimeContext(listener, this));
                        if (consumable && signal.Consumed) {
                            break;
                        }
                    }

                    break;
                }
                case SignalDirection.Self: {
                    sender.Proto.Handler?.Invoke(signal, new ElementRuntimeContext(sender, this));
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

                        current.Proto.Handler?.Invoke(signal, new ElementRuntimeContext(sender, this));
                        if (consumable && signal.Consumed) {
                            break;
                        }

                        current = current.Parent;
                    }

                    break;
                }
                case SignalDirection.DrillDown: {
                    HashSet<IElementInstance> antiRecursion = new() { sender };

                    void DeepSeach(IElementInstance elem)
                    {
                        if (elem.Children != null) {
                            foreach (var sub in elem.Children) {
                                if (!antiRecursion.Add(sub)) {
                                    Debug.LogWarning($"found recursion in element {elem.Proto.Id}");
                                    continue;
                                }

                                sub.Proto.Handler?.Invoke(signal, new ElementRuntimeContext(sender, this));
                                if (consumable && signal.Consumed) {
                                    return;
                                }

                                DeepSeach(sub);
                                if (consumable && signal.Consumed) {
                                    break;
                                }
                            }
                        }
                    }

                    DeepSeach(sender);

                    break;
                }
                default:
                    throw new NotImplementedException();
            }
        }
    }
}