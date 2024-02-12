using UnityEngine;

namespace UI2;

public interface IElementSetup
{
    IElementSetup SetId(string id);

    IElementSetup SetStyle(string style);

    string Style { get; }

    IElementSetup Sub(IElementSetup element);

    IElementSetup Sub(IEnumerable<IElementSetup> elements);

    IEnumerable<IElementSetup> Subs { get; }

    IElementSetup Then(SetupThenDelegate fn);

    IElementSetup SetPivot(Vector2 pivot);

    Vector2 Pivot { get; }
    bool NewPivot { get; }

    IElementSetup SetAnchor(Vector2 min, Vector2 max);

    (Vector2, Vector2) Anchor { get; }
    bool NewAnchor { get; }

    IElementSetup SetSizeDelta(Vector2 delta);

    Vector2 SizeDelta { get; }
    bool NewSizeDelta { get; }

    IElementSetup SetAnchoredPosition(Vector2 pos);

    Vector2 AnchoredPosition { get; }
    bool NewAnchoredPosition { get; }

    IElementSetup MoveRelative(Vector2 move);
    IElementSetup Move(Vector2 move);
    IElementSetup Handle(SetupHandleDelegate handler);
}