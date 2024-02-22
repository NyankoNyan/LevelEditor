using System;
using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public interface IElementSetupRead
    {
        string Id { get; }
        string Style { get; }
        IEnumerable<IElementSetup> Subs { get; }
        Vector2 Pivot { get; }
        bool NewPivot { get; }
        (Vector2, Vector2) Anchor { get; }
        bool NewAnchor { get; }
        Vector2 SizeDelta { get; }
        bool NewSizeDelta { get; }
        Vector2 AnchoredPosition { get; }
        bool NewAnchoredPosition { get; }
        IEnumerable<SetupHandleDelegate> GetHandlers(string signalName);
        bool HasHandlers { get; }
        bool NeedHide { get; }
        IEnumerable<IFeatureCall> Features { get; }
        bool SignalBlocked { get; }
    }

    public interface IElementSetupWrite
    {
        IElementSetup SetId(string id);
        IElementSetup SetStyle(string style);
        IElementSetup Sub(params IElementSetup[] elements);
        IElementSetup Sub(IEnumerable<IElementSetup> elements);
        IElementSetup Apply(params SetupThenDelegate[] fns);
        IElementSetup SetPivot(Vector2 pivot);
        IElementSetup SetAnchor(Vector2 min, Vector2 max);
        IElementSetup SetSizeDelta(Vector2 delta);
        IElementSetup SetAnchoredPosition(Vector2 pos);
        IElementSetup MoveRelative(Vector2 move);
        IElementSetup Move(Vector2 move);
        IElementSetup Handle(string signalName, SetupHandleDelegate handler);
        IElementSetup DefaultHide();

        IElementSetup Feature<T>(FeatureCall<T>.FuncDelegate f)
            where T : class, IFacadeFeature;

        IElementSetup GroupVertical();
        IElementSetup GroupHorizontal();
        IElementSetup SignalBlock(bool block = true);
        IElementSetup Lazy(bool lazy = true);
        IElementSetup State(string name, object value = null, StateInitDelegate initFunc = null);
    }

    public interface IElementSetup : IElementSetupRead, IElementSetupWrite
    {
        IElementSetup Clone();
    }
}