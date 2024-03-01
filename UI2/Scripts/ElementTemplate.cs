using System.Diagnostics;
using System.Reflection;

using global::System.Collections.Generic;
using global::System.Linq;

using UnityEngine;

namespace UI2
{
    /// <summary>
    /// Шаблон настроек элемента.
    /// Позволяет создать отдельный блок настроек и затем применить его к другим настройкам. 
    /// </summary>
    public class ElementTemplate : IElementSetupWrite
    {
        private readonly List<Command> _commands = new();

        public ElementTemplate(params SetupDelegate[] setupCalls)
        {
            foreach (var call in setupCalls) {
                call(this);
            }
        }

        private ElementTemplate(IEnumerable<Command> commands)
        {
            _commands.AddRange(commands);
        }

        private IElementSetupWrite _Add(params object[] args)
        {
            StackTrace stackTrace = new StackTrace();

            _commands.Add(new Command() {
                func = stackTrace.GetFrame(1)!.GetMethod(),
                args = args
            });
            return this;
        }

        public SetupDelegate All => (setup) => {
            foreach (var command in _commands) {
                var method = setup!.GetType().GetMethod(command.func.Name);
                setup = (IElementSetupWrite)method!.Invoke(setup, command.args);
            }
        };

        public IElementSetupWrite Clone() => new ElementTemplate(_commands);

        public IElementSetupRead Read() =>
            throw new ElementWorkflowException($"template can't be converted into readable object");

        public IElementSetupWrite Id(string id) => _Add(id);

        public IElementSetupWrite Style(string style) => _Add(style);

        public IElementSetupWrite Sub(params IElementSetupWrite[] elements) =>
            _Add(elements.Select(x => (object)x).ToArray());

        public IElementSetupWrite Sub(IEnumerable<IElementSetupWrite> elements) => _Add(elements);

        public IElementSetupWrite Apply(params SetupDelegate[] fns) => _Add(fns.Select(x => (object)x).ToArray());

        public IElementSetupWrite Pivot(Vector2 pivot) => _Add(pivot);

        public IElementSetupWrite Anchor(Vector2 min, Vector2 max) => _Add(min, max);

        public IElementSetupWrite SizeDelta(Vector2 delta) => _Add(delta);

        public IElementSetupWrite AnchoredPos(Vector2 pos) => _Add(pos);

        public IElementSetupWrite MoveRelative(Vector2 move) => _Add(move);

        public IElementSetupWrite Move(Vector2 move) => _Add(move);

        public IElementSetupWrite Handle(string signalName, SetupHandleDelegate handler) => _Add(signalName, handler);

        public IElementSetupWrite DefaultHide() => _Add();

        public IElementSetupWrite Feature<T>(FeatureCall<T>.FuncDelegate f) where T : class, IFacadeFeature => _Add(f);

        public IElementSetupWrite GroupVertical() => _Add();

        public IElementSetupWrite GroupHorizontal() => _Add();

        public IElementSetupWrite SignalBlock(bool block = true) => _Add(block);

        public IElementSetupWrite Lazy(bool lazy = true) => _Add(lazy);

        public IElementSetupWrite State(string name, object value = null, StateInitDelegate initFunc = null)
            => _Add(name, value, initFunc);

        public IElementSetupWrite UseState(string varName, SimpleHandleDelegate updateCall = null) 
            => _Add(varName, updateCall);

        public IElementSetupWrite StatesFrom(string elemId) => _Add(elemId);

        public IElementSetupWrite StateFrom(string elemId, string elemState, string newId = null)
            => _Add(elemId, elemState, newId);

        public IElementSetupWrite Grid(Vector2 cellSize, RectOffset padding) => _Add(cellSize, padding);

        private struct Command
        {
            public MethodBase func;
            public object[] args;
        }
    }
}