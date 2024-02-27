using System;
using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public interface IElementSetupRead
    {
        IElementSetupWrite Write();
        string Id { get; }
        string Style { get; }
        IEnumerable<IElementSetupRead> Subs { get; }
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
        IEnumerable<StateDef> DefaultStates { get; }
        string UsedState { get; }
        IEnumerable<string> ProxyTargets { get; }
        IEnumerable<StateProxyDef> Proxies { get; }
    }

    public interface IElementSetupWrite
    {
        IElementSetupRead Read();
        IElementSetupWrite SetId(string id);
        IElementSetupWrite SetStyle(string style);
        IElementSetupWrite Sub(params IElementSetupWrite[] elements);
        IElementSetupWrite Sub(IEnumerable<IElementSetupWrite> elements);
        IElementSetupWrite Apply(params SetupThenDelegate[] fns);
        IElementSetupWrite SetPivot(Vector2 pivot);
        IElementSetupWrite SetAnchor(Vector2 min, Vector2 max);
        IElementSetupWrite SetSizeDelta(Vector2 delta);
        IElementSetupWrite SetAnchoredPosition(Vector2 pos);
        IElementSetupWrite MoveRelative(Vector2 move);
        IElementSetupWrite Move(Vector2 move);
        IElementSetupWrite Handle(string signalName, SetupHandleDelegate handler);
        IElementSetupWrite DefaultHide();
        IElementSetupWrite Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature;
        IElementSetupWrite GroupVertical();
        IElementSetupWrite GroupHorizontal();
        IElementSetupWrite SignalBlock(bool block = true);
        IElementSetupWrite Lazy(bool lazy = true);
        IElementSetupWrite State(string name, object value = null, StateInitDelegate initFunc = null);
        IElementSetupWrite Clone();
        IElementSetupWrite Init(SimpleHandleDelegate handler);
        IElementSetupWrite UseState(string varName);
        IElementSetupWrite StatesFrom(string elemId);
        IElementSetupWrite StateFrom(string elemId, string elemState, string newId = null);
        IElementSetupWrite Grid(Vector2 cellSize, Vector2 padding = default);
    }
}