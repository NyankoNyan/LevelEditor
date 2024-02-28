using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2
{
    public interface IFeatureCall
    {
        void Call(IElementInstance instance);
    }

    public class FeatureCall<T> : IFeatureCall
        where T : class, IFacadeFeature
    {
        public delegate void FuncDelegate(T val);

        private Type _type;
        public FuncDelegate Func { get; private set; }

        public FeatureCall(Type type, FuncDelegate func)
        {
            Assert.IsNotNull(type);
            Assert.IsNotNull(func);
            _type = type;
            Func = func;
        }

        public void Call(IElementInstance instance)
        {
            Func.Invoke(instance.Feature<T>());
        }
    }

    public abstract class BaseElement : IElementSetupRead
    {
        private string _id;
        private string _style;
        private List<IElementSetupRead> _children;
        private Vector2 _pivot;
        private bool _newPivot;
        private Vector2 _anchorMin;
        private Vector2 _anchorMax;
        private bool _newAnchor;
        private Vector2 _sizeDelta;
        private bool _newSizeDelta;
        private Vector2 _anchoredPosition;
        private bool _newAnchorPos;
        private Dictionary<string, List<SetupHandleDelegate>> _handlers;
        private List<IFeatureCall> _featureCalls = new();
        private GroupType _groupType = GroupType.None;
        private bool _signalBlock;
        private bool _lazy;
        private Dictionary<string, StateDef> _stateDefinitions = new();
        private string _usedState;
        private Dictionary<string, StateProxyDef> _stateProxies = new();
        private HashSet<string> _searchProxies = new();
        private GridSetup _gridSetup;

        public string Style => _style;

        public IEnumerable<IElementSetupRead> Subs {
            get {
                if (_children != null) {
                    return _children.AsReadOnly();
                } else {
                    return Array.Empty<IElementSetupRead>();
                }
            }
        }

        public Vector2 Pivot => _pivot;

        public (Vector2, Vector2) Anchor => (_anchorMin, _anchorMax);

        public Vector2 SizeDelta => _sizeDelta;

        public Vector2 AnchoredPosition => _anchoredPosition;

        public bool NewPivot => _newPivot;

        public bool NewAnchor => _newAnchor;

        public bool NewSizeDelta => _newSizeDelta;

        public bool NewAnchoredPosition => _newAnchorPos;

        public void MoveRelative(Vector2 move)
        {
            var (min, max) = Anchor;
            min += move;
            max += move;
            SetAnchor(min, max);
        }

        public void Move(Vector2 move)
        {
            SetAnchoredPosition(AnchoredPosition + move);
        }

        public void Handle(string signalName, SetupHandleDelegate handler)
        {
            if (_handlers == null) {
                _handlers = new();
            }

            List<SetupHandleDelegate> namedHandlers;
            if (!_handlers.TryGetValue(signalName, out namedHandlers)) {
                namedHandlers = new();
                _handlers.Add(signalName, namedHandlers);
            }

            namedHandlers.Add(handler);
        }

        public void DefaultHide()
        {
            NeedHide = true;
        }

        public void Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature
        {
            _featureCalls.Add(new FeatureCall<T>(typeof(T), f));
        }

        public void GroupVertical()
        {
            _groupType = GroupType.Vertical;
        }

        public void GroupHorizontal()
        {
            _groupType = GroupType.Horizontal;
        }

        public void SignalBlock(bool block = true)
        {
            _signalBlock = block;
        }

        public void Lazy(bool lazy = true)
        {
            _lazy = lazy;
        }

        public void State(string name, object value = null, StateInitDelegate initFunc = null)
        {
            var stateDef = new StateDef() {
                defaultValue = value,
                name = name,
                stateInitCall = initFunc
            };
            if (!_stateDefinitions.TryAdd(name, stateDef)) {
                _stateDefinitions[name] = stateDef;
            }
        }

        public BaseElement Clone()
        {
            var newElem = GetEmptyClone();
            newElem._id = _id;
            newElem._style = _style;
            if (_children != null) {
                newElem._children = new(_children);
            }

            newElem._pivot = _pivot;
            newElem._newPivot = _newPivot;
            newElem._anchorMin = _anchorMin;
            newElem._anchorMax = _anchorMax;
            newElem._newAnchor = _newAnchor;
            newElem._sizeDelta = _sizeDelta;
            newElem._newSizeDelta = _newSizeDelta;
            newElem._anchoredPosition = _anchoredPosition;
            newElem._newAnchorPos = _newAnchorPos;
            foreach (var kvp in _handlers) {
                foreach (var f in kvp.Value) {
                    newElem.Handle(kvp.Key, f);
                }
            }

            newElem._featureCalls.AddRange(_featureCalls);
            newElem._groupType = _groupType;
            newElem._signalBlock = _signalBlock;
            newElem._lazy = _lazy;
            newElem._stateDefinitions = new(_stateDefinitions);
            newElem._usedState = _usedState;
            _ = _searchProxies.Select(x => newElem._searchProxies.Add(x));
            _ = _stateProxies.Select(x => newElem._stateProxies.TryAdd(x.Key, x.Value));

            AfterClone(newElem);
            return newElem;
        }

        protected virtual void AfterClone(BaseElement newElem)
        {
        }

        protected abstract BaseElement GetEmptyClone();

        public IEnumerable<SetupHandleDelegate> GetHandlers(string signalName)
            => _handlers?.GetValueOrDefault(signalName);

        public bool HasHandlers => _handlers != null;
        public bool NeedHide { get; private set; }
        public IEnumerable<IFeatureCall> Features => _featureCalls;
        public bool SignalBlocked => _signalBlock;
        public IEnumerable<StateDef> DefaultStates => _stateDefinitions.Values;

        public virtual void Init()
        {
        }

        public void SetStyle(string style)
        {
            _style = style;
        }

        public void SetId(string id)
        {
            _id = id;
        }

        public string Id => _id;

        public string UsedState => _usedState;
        public IEnumerable<string> ProxyTargets => _searchProxies;
        public IEnumerable<StateProxyDef> Proxies => _stateProxies.Values;

        public void Sub(params IElementSetupWrite[] elements)
        {
            _children ??= new();

            foreach (var element in elements) {
                _children.Add(element.Read());
            }
        }

        public void Sub(IEnumerable<IElementSetupWrite> elements)
        {
            Sub(elements.ToArray());
        }

        public void Apply(params SetupDelegate[] fns)
        {
            foreach (var fn in fns) {
                fn(Write());
            }
        }

        public void SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
            _newPivot = true;
        }

        public void SetAnchor(Vector2 min, Vector2 max)
        {
            _anchorMin = min;
            _anchorMax = max;
            _newAnchor = true;
        }

        public void SetSizeDelta(Vector2 delta)
        {
            _sizeDelta = delta;
            _newSizeDelta = true;
        }

        public void SetAnchoredPosition(Vector2 pos)
        {
            _anchoredPosition = pos;
            _newAnchorPos = true;
        }

        public void SetUsedState(string name)
        {
            _usedState = name;
        }

        public IElementSetupWrite Write() => new BaseElementChain(this);

        public void StateProxy(StateProxyDef proxy)
        {
            if (!_stateProxies.TryAdd(proxy.name, proxy)) {
                _stateProxies[proxy.name] = proxy;
            }
        }

        public void SearchProxy(string elemId) 
            => _searchProxies.Add(elemId);

        public void Grid(Vector2 cellSize, Vector2 padding = default)
        {
            _groupType = GroupType.Grid;
            _gridSetup = new GridSetup() {
                cellSize = cellSize,
                padding = padding
            };
        }

        private enum GroupType
        {
            None,
            Horizontal,
            Vertical,
            Grid
        }

        private struct GridSetup
        {
            public Vector2 cellSize;
            public Vector2 padding;
        }
    }
}