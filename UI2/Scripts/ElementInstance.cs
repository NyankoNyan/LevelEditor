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
        private bool _initialized;

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

        public T Feature<T>() where T : class, IFacadeFeature
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

        public IEnumerable<StateVar> States => _states.AsReadOnly();

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

        public void LateInit()
        {
            if (_initialized) {
                return;
            }

            _initialized = true;

            foreach (string target in _proto.ProxyTargets) {
                var child = Sub(target);
                if (child != null) {
                    foreach (StateVar state in child.States) {
                        CreateProxy(state, state.name);
                    }
                } else {
                    Debug.LogWarning($"Child {target} for {_proto.Id} not found");
                }
            }

            foreach (StateProxyDef proxy in _proto.Proxies) {
                var child = Sub(proxy.refId);
                if (child != null) {
                    StateVar state = child.State(proxy.refVarName);
                    CreateProxy(state, proxy.name);
                } else {
                    Debug.LogWarning($"Child {proxy.refId} for {_proto.Id} not found");
                }
            }
        }

        private void CreateProxy(StateVar state, string newName)
        {
            StateVar newState = new(state, newName);
            _states.Add(newState);
        }
    }
}