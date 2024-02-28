using global::System;

using UnityEngine.Assertions;

namespace UI2
{
    public interface IStateVar
    {
        void Set<T>(T v);
        T Get<T>();
        Action onChanged { get; set; }
    }

    public class StateVar : IStateVar, IDisposable
    {
        public string name;
        public object value;
        public bool isProxy;

        public Action onChanged { get; set; }

        public StateVar(string name, object value = null)
        {
            this.name = name;
            this.value = value;
        }

        public StateVar(StateDef stateDef)
        {
            name = stateDef.name;
            if (stateDef.defaultValue != null) {
                value = stateDef.defaultValue switch {
                    bool b => b,
                    int i => i,
                    ICloneable c => c.Clone(),
                    _ => stateDef.defaultValue
                };
            } else if (stateDef.stateInitCall != null) {
                value = stateDef.stateInitCall();
            }
        }

        /// <summary>
        /// Create reference to state
        /// </summary>
        /// <param name="refVar"></param>
        /// <param name="newName"></param>
        public StateVar(StateVar refVar, string newName = null)
        {
            Assert.IsNotNull(refVar);
            name = newName ?? refVar.name;
            isProxy = true;
            value = refVar;
            refVar.onChanged += OnProxyChanged;
        }

        private void OnProxyChanged() => onChanged?.Invoke();

        public void Set<T>(T v)
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    sv.Set(v);
                } else {
                    throw new ElementWorkflowException();
                }
            } // isProxy 
            else {
                bool callOnChanged;
                if (v is IEquatable<T> newEq && value is IEquatable<T> oldEq) {
                    callOnChanged = !newEq.Equals(oldEq);
                } else {
                    callOnChanged = true;
                }

                if (typeof(T) == typeof(bool)
                    || typeof(T) == typeof(int)) {
                    value = v;
                } else if (v is ICloneable c) {
                    value = c.Clone();
                } else {
                    value = v;
                }

                if (callOnChanged) {
                    onChanged?.Invoke();
                }
            } // !isProxy
        }

        public T Get<T>()
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    return sv.Get<T>();
                } else {
                    throw new ElementWorkflowException();
                }
            } // isProxy 
            else {
                if (value is T t) {
                    return t;
                } else {
                    return default;
                }
            } // !isProxy
        }

        public void Dispose()
        {
            if (isProxy) {
                if (value is StateVar sv) {
                    sv.onChanged -= OnProxyChanged;
                }
            }
        }
    }
}