using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public enum GroupType
    {
        None,
        Horizontal,
        Vertical,
        Grid
    }

    public struct GridSetup
    {
        public Vector2 cellSize;
        public RectOffset padding;
    }

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
        GroupType Group { get; }
        GridSetup GridSetup { get; }
    }

    public interface IElementSetupWrite
    {
        IElementSetupRead Read();

        IElementSetupWrite Id(string id);

        IElementSetupWrite Style(string style);

        IElementSetupWrite Sub(params IElementSetupWrite[] elements);

        IElementSetupWrite Sub(IEnumerable<IElementSetupWrite> elements);

        IElementSetupWrite Apply(params SetupDelegate[] fns);

        IElementSetupWrite SetPivot(Vector2 pivot);

        IElementSetupWrite SetPivot(float x, float y) => SetPivot(new Vector2(x, y));

        IElementSetupWrite SetAnchor(Vector2 min, Vector2 max);

        IElementSetupWrite SetAnchor((float, float) min, (float, float) max) => SetAnchor(new Vector2(min.Item1, min.Item2), new Vector2(max.Item1, max.Item2));

        IElementSetupWrite SetSizeDelta(Vector2 delta);

        IElementSetupWrite SetSizeDelta(float x, float y) => SetSizeDelta(new Vector2(x, y));

        IElementSetupWrite SetAnchoredPosition(Vector2 pos);

        IElementSetupWrite SetAnchoredPosition(float x, float y) => SetAnchoredPosition(new Vector2(x, y));

        IElementSetupWrite MoveRelative(Vector2 move);

        IElementSetupWrite MoveRelative(float x, float y) => MoveRelative(new Vector2(x, y));

        IElementSetupWrite Move(Vector2 move);

        IElementSetupWrite Move(float x, float y) => Move(new Vector2(x, y));

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

        IElementSetupWrite Grid(Vector2 cellSize, RectOffset padding = default);

        IElementSetupWrite Grid((float, float) cellSize, (int, int, int, int) padding = default)
            => Grid(new Vector2(cellSize.Item1, cellSize.Item2),
                new RectOffset(padding.Item1, padding.Item2, padding.Item3, padding.Item4));

        IElementSetupWrite Timer(float timer, SimpleHandleDelegate handler);
    }
}