using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace UI2
{
    internal class ElementInstance : IElementInstance
    {
        private readonly IElementSetupRead _proto;
        private readonly GameObject _instance;
        private readonly ElementInstanceFacade _facade;
        private readonly IElementInstance _parent;
        private List<IElementInstance> _children;
        private readonly UIRoot _root;
        private readonly List<StateVar> _states = new();

        public ElementInstance(IElementSetupRead proto, GameObject instance, IElementInstance parent, UIRoot root)
        {
            Assert.IsNotNull(proto);
            Assert.IsNotNull(instance);
            _proto = proto;
            _instance = instance;

            if (parent != null) {
                parent.AddChild(this);
                _parent = parent;
            }

            _root = root;

            _facade = _instance.GetComponent<ElementInstanceFacade>();

            // Init states
            foreach (var stateDef in proto.DefaultStates) {
                StateVar state = new(stateDef);
                _states.Add(state);
            }
        }

        public IElementSetupRead Proto => _proto;
        public IElementInstance Parent => _parent;

        public void AddChild(IElementInstance child)
        {
            _children ??= new();
            if (child.Parent != null) {
                throw new ArgumentException();
            }

            _children.Add(child);
        }

        public IEnumerable<IElementInstance> Children => _children;

        public IElementInstance Sub(string id)
        {
            // Own children
            foreach (var child in Children) {
                if (child.Proto.Id == id) {
                    return child;
                }
            }

            // Ok, just go to a hierarchy
            foreach (var child in Children) {
                if (child.Proto.Id == id) {
                    var result = child.Sub(id);
                    if (result != null) {
                        return result;
                    }
                }
            }

            return null;
        }

        public void SendFacadeSignal(string id)
        {
            _root.SendSignal(id, null, this, SignalDirection.Self, false);
        }

        public T GetFacadeFeature<T>() where T : class, IFacadeFeature
            => _facade?.GetFeature<T>();

        public StateVar State(string name)
        {
            StateVar result = _states.SingleOrDefault(x => x.name == name);
            if (result == null) {
                result = new StateVar(name);
                _states.Add(result);
            }
            return result;
        }

        public IElementInstance Hide()
        {
            _instance.SetActive(false);
            return this;
        }

        public bool Active => _instance.activeSelf;

        public IElementInstance Show()
        {
            _instance.SetActive(true);
            return this;
        }
    }
}