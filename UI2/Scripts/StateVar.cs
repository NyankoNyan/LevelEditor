using global::System;

using Level;

using UnityEngine.Assertions;

namespace UI2
{
    public delegate object StateInitDelegate();

    public class StateDef
    {
        public string name;
        public object defaultValue;
        public StateInitDelegate stateInitCall;
    }

    public class StateProxyDef
    {
        public string name;
        public string refVarName;
        public string refId;
    }

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
                    throw new ElementWorkflowException($"variable [{name}] is not [{nameof(T)}]");
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
                    throw new ElementWorkflowException($"variable [{name}] is not [{nameof(T)}]");
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

    public interface IVarProxy
    {
        object Value { get; set; }
        bool IsReadOnly { get; }
        Action<object, object> Changed { get; set; }
    }

    public interface IVarProxy<T>
    {
        T Value { get; set; }
        bool IsReadOnly { get; }
        Action<T, T> Changed { get; set; }
    }

    public class VarProxy<T> : IVarProxy, IVarProxy<T>
        where T : IEquatable<T>
    {
        public delegate T GetValueDelegate();

        private GetValueDelegate _valueGetter;
        private Action<T> _valueSetter;

        private T _value;

        public VarProxy(GetValueDelegate valueGetter,
            ref Action valueChangeEvent,
            Action<T> valueSetter = null)
        {
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
            valueChangeEvent += OnValueChanged;
        }

        public void Destroy(ref Action valueChangeEvent)
        {
            valueChangeEvent -= OnValueChanged;
        }

        private void OnValueChanged()
        {
            T newValue = _valueGetter();
            if (!newValue.Equals(_value)) {
                T oldValue = _value;
                _value = newValue;
                ((IVarProxy<T>)this).Changed?.Invoke(oldValue, _value);
                ((IVarProxy)this).Changed?.Invoke(oldValue, _value);
            }
        }

        object IVarProxy.Value {
            get => Value;
            set => Value = (T)value;
        }

        public T Value {
            get => _value;
            set {
                if (!IsReadOnly) {
                    _valueSetter(value);
                }
            }
        }

        Action<T, T> IVarProxy<T>.Changed { get; set; }

        public bool IsReadOnly => _valueSetter == null;

        Action<object, object> IVarProxy.Changed { get; set; }
    }
}