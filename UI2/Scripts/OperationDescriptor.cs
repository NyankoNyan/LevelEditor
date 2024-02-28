using UnityEngine;

namespace UI2
{
    public class OperationDescriptor
    {
        private readonly Coroutine _coroutine;

        internal OperationDescriptor(Coroutine coroutine)
        {
            _coroutine = coroutine;
        }
    }
}