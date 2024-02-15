using System;
using System.Collections;

using UnityEngine;

namespace UI2
{
    public interface IOperation
    {
        IOperation Do(Action callback);

        IOperation Wait(YieldInstruction wait);

        IEnumerator Exec();
    }
}