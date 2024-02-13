using System;
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
        private SetupHandleDelegate _handler;
        private Action _onClick;

        public string Style => _style;

        public IEnumerable<IElementSetupReadWrite> Subs => _children.AsReadOnly();

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

        public IElementSetupReadWrite Handle(SetupHandleDelegate handler)
        {
            _handler = handler;
            return this;
        }

        public SetupHandleDelegate Handler => _handler;
        public Action OnClick => _onClick;

        public IElementSetupReadWrite Click(Action handler)
        {
            _onClick = handler;
            return this;
        }

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

        public IElementSetupReadWrite Sub(IElementSetupReadWrite element)
        {
            if (_children == null) {
                _children = new();
            }

            _children.Add(element);
            return this;
        }

        public IElementSetupReadWrite Sub(IEnumerable<IElementSetupReadWrite> elements)
        {
            foreach (var element in elements) {
                Sub(element);
            }

            return this;
        }

        public IElementSetupReadWrite Then(SetupThenDelegate fn)
        {
            fn(this);
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