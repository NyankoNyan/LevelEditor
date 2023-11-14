using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Level
{
    public interface IHasKey<T>
    {
        T Key { get; }
    }

    public interface IInitializable<T>
    {
        void Init(T value, uint counter);
    }

    public interface IDestroy
    {
        Action OnDestroyAction { get; set; }

        void Destroy();
    }

    public class Registry<TKey, TValue>
        where TValue : IHasKey<TKey>, IDestroy
    {
        public UnityAction<TValue> onAdd;
        public UnityAction<TValue> onRemove;

        private Dictionary<TKey, TValue> _values = new();

        public void Add(TValue value)
        {
            _values.Add( value.Key, value );

            value.OnDestroyAction += () => Remove( value );

            if (onAdd != null) {
                onAdd( value );
            }
        }

        public void Remove(TKey key) => Remove( _values[key] );

        public void Remove(TValue value)
        {
            _values.Remove( value.Key );
            if (onRemove != null) {
                onRemove( value );
            }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values => _values.Values;
        public IReadOnlyDictionary<TKey, TValue> Dict => _values;
    }

    public class Fabric<T, TCreateParams>
        where T : IInitializable<TCreateParams>, new()
    {
        public UnityAction<T> onCreate;
        private uint _counter;
        public uint Counter => _counter;

        public T Create(TCreateParams createParams)
        {
            T value = new();
            _counter++;
            value.Init( createParams, _counter );
            if (onCreate != null) {
                onCreate( value );
            }
            return value;
        }

        public T CreateWithCounter(TCreateParams createParams, uint counter)
        {
            T value = new();
            value.Init( createParams, counter );
            if (onCreate != null) {
                onCreate( value );
            }
            _counter = Math.Max( _counter, counter );
            return value;
        }
    }

    /// <summary>
    /// На вопрос нахуя это сделано, я наверное не смогу дать внятного ответа. Пусть просто будет.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TCreateParams"></typeparam>
    /// <typeparam name="TFabric"></typeparam>
    /// <typeparam name="TRegistry"></typeparam>
    public class TypeEnv<T, TKey, TCreateParams, TFabric, TRegistry>
        where T : IHasKey<TKey>, IInitializable<TCreateParams>, IDestroy, new()
        where TFabric : Fabric<T, TCreateParams>, new()
        where TRegistry : Registry<TKey, T>, new()
    {
        public readonly TFabric Fabric;
        public readonly TRegistry Registry;

        public TypeEnv()
        {
            Fabric = new();
            Registry = new();
            Fabric.onCreate += x => Registry.Add( x );
        }
    }
}