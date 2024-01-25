using System.Collections;

using UnityEngine;

namespace Level.IO
{
    /// <summary>
    /// Обёртка над группой корутин, которая обеспечивает одновременное выполнение не более N корутин.
    /// Создаётся очередь корутин, из которой запускается следующая, когда одна из предыдущих заканчивает выполнение.
    /// </summary>
    public class RoutineSplitter
    {
        private readonly MonoBehaviour _routineProvider;
        private readonly int _maxParallels;
        private int _free;
        private readonly Queue<IEnumerator> _routineQueue = new Queue<IEnumerator>();

        public RoutineSplitter(MonoBehaviour routineProvider, int maxParallels = 4)
        {
            _routineProvider = routineProvider;
            _maxParallels = maxParallels;

            _free = _maxParallels;
        }

        public void AddRoutine(IEnumerator routine)
        {
            _routineQueue.Enqueue(routine);
        }

        private IEnumerator WrapRoutine(IEnumerator routine)
        {
            _free -= 1;
            yield return routine;
            _free += 1;
        }

        public IEnumerator Start()
        {
            while (true) {
                while (_routineQueue.Count > 0 && _free > 0) {
                    var routine = _routineQueue.Dequeue();
                    _routineProvider.StartCoroutine(WrapRoutine(routine));
                }
                yield return null;
            }
        }
    }
}