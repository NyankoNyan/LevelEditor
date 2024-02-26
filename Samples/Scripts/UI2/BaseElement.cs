using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

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
            Func.Invoke(instance.GetFacadeFeature<T>());
        }
    }

    public abstract class BaseElement : IElementSetup
    {
        private string _id;
        private string _style;
        private List<IElementSetup> _children;
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

        public string Style => _style;

        public IEnumerable<IElementSetup> Subs {
            get {
                if (_children != null) {
                    return _children.AsReadOnly();
                } else {
                    return new IElementSetup[0];
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

        public IElementSetup MoveRelative(Vector2 move)
        {
            var (min, max) = Anchor;
            min += move;
            max += move;
            SetAnchor(min, max);
            return this;
        }

        public IElementSetup Move(Vector2 move)
        {
            SetAnchoredPosition(AnchoredPosition + move);
            return this;
        }

        // public IElementSetup Handle(SetupHandleDelegate handler)
        // {
        //     _handler = handler;
        //     return this;
        // }

        public IElementSetup Handle(string signalName, SetupHandleDelegate handler)
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
            return this;
        }

        public IElementSetup DefaultHide()
        {
            NeedHide = true;
            return this;
        }

        public IElementSetup Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature
        {
            _featureCalls.Add(new FeatureCall<T>(typeof(T), f));
            return this;
        }

        public IElementSetup GroupVertical()
        {
            _groupType = GroupType.Vertical;
            return this;
        }

        public IElementSetup GroupHorizontal()
        {
            _groupType = GroupType.Horizontal;
            return this;
        }

        public IElementSetup SignalBlock(bool block = true)
        {
            _signalBlock = block;
            return this;
        }

        public IElementSetup Lazy(bool lazy = true)
        {
            _lazy = lazy;
            return this;
        }

        public IElementSetup State(string name, object value = null, StateInitDelegate initFunc = null)
        {
            var stateDef = new StateDef() {
                defaultValue = value,
                name = name,
                stateInitCall = initFunc
            };
            if (!_stateDefinitions.TryAdd(name, stateDef)) {
                _stateDefinitions[name] = stateDef;
            }

            return this;
        }

        public IElementSetup Clone()
        {
            var newElem = GetEmptyClone();
            newElem._id = _id;
            newElem._style = _style;
            newElem.Sub(_children);
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

            AfterClone(newElem);
            return newElem;
        }

        protected virtual void AfterClone(IElementSetup newElem) { }
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

        public IElementSetup SetStyle(string style)
        {
            _style = style;
            return this;
        }

        public IElementSetup SetId(string id)
        {
            _id = id;
            return this;
        }

        public string Id => _id;

        public IElementSetup Sub(params IElementSetup[] elements)
        {
            _children ??= new();

            foreach (var element in elements) {
                _children.Add(element);
            }

            return this;
        }

        public IElementSetup Sub(IEnumerable<IElementSetup> elements)
        {
            Sub(elements.ToArray());

            return this;
        }

        public IElementSetup Apply(params SetupThenDelegate[] fns)
        {
            foreach (var fn in fns) {
                fn(this);
            }

            return this;
        }

        public IElementSetup SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
            _newPivot = true;
            return this;
        }

        public IElementSetup SetAnchor(Vector2 min, Vector2 max)
        {
            _anchorMin = min;
            _anchorMax = max;
            _newAnchor = true;
            return this;
        }

        public IElementSetup SetSizeDelta(Vector2 delta)
        {
            _sizeDelta = delta;
            _newSizeDelta = true;
            return this;
        }

        public IElementSetup SetAnchoredPosition(Vector2 pos)
        {
            _anchoredPosition = pos;
            _newAnchorPos = true;
            return this;
        }

        private enum GroupType
        {
            None,
            Horizontal,
            Vertical
        }
    }

    public class InputElement : BaseElement
    {
        public InputElement()
        {
            SetStyle("field");
        }

        protected override BaseElement GetEmptyClone() => new InputElement();
    }

    public class ButtonElement : BaseElement
    {
        private readonly string _name;

        public ButtonElement(string id = null, string name = null)
        {
            _name = name;
            if (!string.IsNullOrWhiteSpace(id)) {
                SetId(id);
            }

            SetStyle("button");
            if (!string.IsNullOrWhiteSpace(name)) {
                Feature<MainTextFeature>(f => f.SetText(_name));
            } else {
                Feature<MainTextFeature>(f => f.SetText(""));
            }
        }

        protected override BaseElement GetEmptyClone() => new ButtonElement(Id, _name);
    }
}