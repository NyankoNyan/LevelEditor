using System.Collections.Generic;

namespace UI2
{
    public interface IElementInstance
    {
        IElementInstance Show();

        IElementInstance Hide();
        bool Active { get; }

        IElementSetupRead Proto { get; }
        IElementInstance Parent { get; }
        void AddChild(IElementInstance child);
        IEnumerable<IElementInstance> Children { get; }
        IElementInstance Sub(string id);
        void SendFacadeSignal(string id);
        T Feature<T>() where T : class, IFacadeFeature;
        /// <summary>
        /// Возвращает переменную состояния. В случае отсутствия, переменная будет создана.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        StateVar State(string name);
        IEnumerable<StateVar> States { get; }

        #region Service // TODO Move to another interface

        void LateInit();

        #endregion
    }
}