using System;

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
            Func.Invoke(instance.Feature<T>());
        }
    }
}