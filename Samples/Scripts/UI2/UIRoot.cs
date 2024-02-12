using UnityEngine;

namespace UI2;

public class UIRoot
{
    private readonly Dictionary<string, Style> _styles = new();
    private readonly HashSet<IElementInstance> _instances = new();

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

    public IElementInstance Attach(IElementSetup setup, Transform parent)
    {
        if (!parent) {
            throw new ArgumentException("Empty parent");
        }

        if (_styles.TryGetValue(setup.Style, out Style style)) {
            var newGO = GameObject.Instantiate(style.prefab, parent);
            var instance = new ElementInstance(setup, newGO);

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
            return instance;
        } else {
            throw new ElementWorkflowException();
        }
    }
}