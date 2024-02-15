using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public abstract class BaseElement : IElementSetupReadWrite
    {
        private string _id;
        private string _style;
        private List<IElementSetupReadWrite> _children;
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

        public string Style => _style;

        public IEnumerable<IElementSetupReadWrite> Subs {
            get {
                if (_children != null) {
                    return _children.AsReadOnly();
                } else {
                    return new IElementSetupReadWrite[0];
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

        public IElementSetupReadWrite MoveRelative(Vector2 move)
        {
            var (min, max) = Anchor;
            min += move;
            max += move;
            SetAnchor(min, max);
            return this;
        }

        public IElementSetupReadWrite Move(Vector2 move)
        {
            SetAnchoredPosition(AnchoredPosition + move);
            return this;
        }

        // public IElementSetupReadWrite Handle(SetupHandleDelegate handler)
        // {
        //     _handler = handler;
        //     return this;
        // }

        public IElementSetupReadWrite Handle(string signalName, SetupHandleDelegate handler)
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

        public IElementSetupReadWrite DefaultHide()
        {
            NeedHide = true;
            return this;
        }

        public IEnumerable<SetupHandleDelegate> GetHandlers(string signalName)
            => _handlers?.GetValueOrDefault(signalName);

        public bool HasHandlers => _handlers != null;
        public bool NeedHide { get; private set; }

        public virtual void Init()
        {
        }

        public IElementSetupReadWrite SetStyle(string style)
        {
            _style = style;
            return this;
        }

        public IElementSetupReadWrite SetId(string id)
        {
            _id = id;
            return this;
        }

        public string Id => _id;

        public IElementSetupReadWrite Sub(params IElementSetupReadWrite[] elements)
        {
            _children ??= new();

            foreach (var element in elements) {
                _children.Add(element);
            }

            return this;
        }

        public IElementSetupReadWrite Sub(IEnumerable<IElementSetupReadWrite> elements)
        {
            Sub(elements.ToArray());

            return this;
        }

        public IElementSetupReadWrite Apply(params SetupThenDelegate[] fns)
        {
            foreach (var fn in fns) {
                fn(this);
            }

            return this;
        }

        public IElementSetupReadWrite SetPivot(Vector2 pivot)
        {
            _pivot = pivot;
            _newPivot = true;
            return this;
        }

        public IElementSetupReadWrite SetAnchor(Vector2 min, Vector2 max)
        {
            _anchorMin = min;
            _anchorMax = max;
            _newAnchor = true;
            return this;
        }

        public IElementSetupReadWrite SetSizeDelta(Vector2 delta)
        {
            _sizeDelta = delta;
            _newSizeDelta = true;
            return this;
        }

        public IElementSetupReadWrite SetAnchoredPosition(Vector2 pos)
        {
            _anchoredPosition = pos;
            _newAnchorPos = true;
            return this;
        }
    }
}