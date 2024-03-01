using global::System.Collections.Generic;

using UnityEngine;

namespace UI2
{
    public class BaseElementChain : IElementSetupWrite
    {
        private readonly BaseElement _element;

        public BaseElementChain(BaseElement element)
        {
            _element = element;
        }

        public IElementSetupWrite Id(string id)
        {
            _element.SetId(id);
            return this;
        }

        public IElementSetupWrite Style(string style)
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

        public IElementSetupWrite Apply(params SetupDelegate[] fns)
        {
            _element.Apply(fns);
            return this;
        }

        public IElementSetupWrite Pivot(Vector2 pivot)
        {
            _element.SetPivot(pivot);
            return this;
        }

        public IElementSetupWrite Anchor(Vector2 min, Vector2 max)
        {
            _element.SetAnchor(min, max);
            return this;
        }

        public IElementSetupWrite SizeDelta(Vector2 delta)
        {
            _element.SetSizeDelta(delta);
            return this;
        }

        public IElementSetupWrite AnchoredPos(Vector2 pos)
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
            _element.Feature(f);
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

        public IElementSetupWrite UseState(string varName, SimpleHandleDelegate updateCall = null)
        {
            _element.SetUsedState(varName, updateCall);
            return this;
        }

        public IElementSetupWrite StatesFrom(string elemId)
        {
            _element.SearchProxy(elemId);
            return this;
        }

        public IElementSetupWrite StateFrom(string elemId, string elemState, string newId = null)
        {
            _element.StateProxy(new StateProxyDef {
                name = newId ?? elemId,
                refId = elemId,
                refVarName = elemState
            });
            return this;
        }

        public IElementSetupWrite Grid(Vector2 cellSize, RectOffset padding)
        {
            _element.Grid(cellSize, padding);
            return this;
        }
    }
}