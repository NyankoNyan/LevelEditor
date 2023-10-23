using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace RuntimeEditTools
{
    public enum ParameterMode
    {
        ReadOnly,
        ReadWrite
    }

    public struct ParameterDescription
    {
        public string name;
        public Type type;
        public ParameterMode mode;

        public ParameterDescription(string name, Type type, ParameterMode mode)
        {
            this.name = name;
            this.type = type;
            this.mode = mode;
        }
    }

    public interface IParametersConnector
    {
        IEnumerable<ParameterDescription> ParametersDescriptions { get; }

        object GetParameter(string name);

        void SetParameter(string name, object value);

        UnityAction<string, object> OnParamModelUpdate { get; set; }
    }
}