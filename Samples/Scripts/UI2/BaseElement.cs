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

            AfterClone(newElem);
            return newElem;
        }

        protected virtual void AfterClone(BaseElement newElem) { }
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

        public void Apply(params SetupThenDelegate[] fns)
        {
            foreach (var fn in fns) {
                fn(this);
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

        public IElementSetupWrite Write() => new BaseElementChain(this);

        private enum GroupType
        {
            None,
            Horizontal,
            Vertical
        }
    }

    public class BaseElementChain : IElementSetupWrite
    {
        private readonly BaseElement _element;

        public BaseElementChain(BaseElement element)
        {
            _element = element;
        }

        public IElementSetupWrite SetId(string id)
        {
            _element.SetId(id);
            return this;
        }

        public IElementSetupWrite SetStyle(string style)
        {
            _element.SetStyle(style);
            return this;
        }

        public IElementSetupWrite Sub(params IElementSetupWrite[] elements)
        {
            _element.Sub(elements);
            return this;
        }

        public IElementSetupWrite Sub(IEnumerable<IElementSetupWrite> elements)
        {
            _element.Sub(elements);
            return this;
        }

        public IElementSetupWrite Apply(params SetupThenDelegate[] fns)
        {
            _element.Apply(fns);
            return this;
        }

        public IElementSetupWrite SetPivot(Vector2 pivot)
        {
            _element.SetPivot(pivot);
            return this;
        }

        public IElementSetupWrite SetAnchor(Vector2 min, Vector2 max)
        {
            _element.SetAnchor(min, max);
            return this;
        }

        public IElementSetupWrite SetSizeDelta(Vector2 delta)
        {
            _element.SetSizeDelta(delta);
            return this;
        }

        public IElementSetupWrite SetAnchoredPosition(Vector2 pos)
        {
            _element.SetAnchoredPosition(pos);
            return this;
        }

        public IElementSetupWrite MoveRelative(Vector2 move)
        {
            _element.MoveRelative(move);
            return this;
        }

        public IElementSetupWrite Move(Vector2 move)
        {
            _element.Move(move);
            return this;
        }

        public IElementSetupWrite Handle(string signalName, SetupHandleDelegate handler)
        {
            _element.Handle(signalName, handler);
            return this;
        }

        public IElementSetupWrite DefaultHide()
        {
            _element.DefaultHide();
            return this;
        }

        public IElementSetupWrite Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature
        {
            _element.Feature<T>(f);
            return this;
        }

        public IElementSetupWrite GroupVertical()
        {
            _element.GroupVertical();
            return this;
        }

        public IElementSetupWrite GroupHorizontal()
        {
            _element.GroupHorizontal();
            return this;
        }

        public IElementSetupWrite SignalBlock(bool block = true)
        {
            _element.SignalBlock(block);
            return this;
        }

        public IElementSetupWrite Lazy(bool lazy = true)
        {
            _element.Lazy(lazy);
            return this;
        }

        public IElementSetupWrite State(string name, object value = null, StateInitDelegate initFunc = null)
        {
            _element.State(name, value, initFunc);
            return this;
        }

        public IElementSetupRead Read()
        {
            return _element;
        }

        public IElementSetupWrite Clone()
        {
            return _element.Clone().Write();
        }

        public IElementSetupWrite Init(SimpleHandleDelegate handler)
            => Handle("__INIT__", (_, ctx) => {
                // Simple wrapper
                handler(ctx);
            });
    }

    public class InputElement : BaseElement
    {
        public InputElement()
        {
            SetStyle("field");
        }

        protected override BaseElement GetEmptyClone() => new InputElement();
    }

    public class FlagElement : BaseElement
    {
        public FlagElement()
        {
            SetStyle("flag");
        }

        protected override BaseElement GetEmptyClone() => new FlagElement();
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