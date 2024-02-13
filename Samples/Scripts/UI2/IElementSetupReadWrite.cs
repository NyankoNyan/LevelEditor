using System;
using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public interface IElementSetupRead
    {
        string Id { get; }
        string Style { get; }
        IEnumerable<IElementSetupReadWrite> Subs { get; }
        Vector2 Pivot { get; }
        bool NewPivot { get; }
        (Vector2, Vector2) Anchor { get; }
        bool NewAnchor { get; }
        Vector2 SizeDelta { get; }
        bool NewSizeDelta { get; }
        Vector2 AnchoredPosition { get; }
        bool NewAnchoredPosition { get; }
        SetupHandleDelegate Handler { get; }
        Action OnClick { get; }
    }

    public interface IElementSetupWrite
    {
        IElementSetupReadWrite SetId(string id);
        IElementSetupReadWrite SetStyle(string style);
        IElementSetupReadWrite Sub(IElementSetupReadWrite element);
        IElementSetupReadWrite Sub(IEnumerable<IElementSetupReadWrite> elements);
        IElementSetupReadWrite Then(SetupThenDelegate fn);
        IElementSetupReadWrite SetPivot(Vector2 pivot);
        IElementSetupReadWrite SetAnchor(Vector2 min, Vector2 max);
        IElementSetupReadWrite SetSizeDelta(Vector2 delta);
        IElementSetupReadWrite SetAnchoredPosition(Vector2 pos);
        IElementSetupReadWrite MoveRelative(Vector2 move);
        IElementSetupReadWrite Move(Vector2 move);
        IElementSetupReadWrite Handle(SetupHandleDelegate handler);
        IElementSetupReadWrite Click(Action handler);
    }

    public interface IElementSetupReadWrite : IElementSetupRead, IElementSetupWrite
    {
    }
}