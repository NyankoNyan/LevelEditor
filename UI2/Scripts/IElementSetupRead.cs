using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public delegate void SetupDelegate(IElementSetupWrite setup);

    public delegate void SetupHandleDelegate(ISignalContext signal, IElementRuntimeContext context);

    public delegate void SimpleHandleDelegate(IElementRuntimeContext context);

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
}