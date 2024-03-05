using System.Collections.Generic;

namespace UI2
{
    public interface IElementInstance
    {
        void Show();

        void Hide();

        Action OnDestroy { get; set; }
        bool Active { get; }

        IElementSetupRead Proto { get; }
        IElementInstance Parent { get; }
        void AddChild(IElementInstance child);
        IEnumerable<IElementInstance> Children { get; }

        void SendFacadeSignal(string id);
        T Feature<T>() where T : class, IFacadeFeature;

        /// <summary>
        /// Возвращает переменную состояния. В случае отсутствия, переменная будет создана.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        StateVar State(string name);

        IEnumerable<StateVar> States { get; }

        #region Sugar

        IElementInstance TreeSearch(string id)
        {
            // Searching top-element
            var top = this;
            while (top.Parent != null && !top.Proto.SignalBlocked) {
                top = top.Parent;
            }

            return top.Sub(id);
        }

        IElementInstance Sub(string id)
        {
            // Own children
            foreach (var child in Children) {
                if (child.Proto.Id == id) {
                    return child;
                }
            }

            // Ok, just go to a hierarchy
            foreach (var child in Children) {
                if (child.Proto.SignalBlocked) {
                    continue;
                }

                if (child.Proto.Id == id) {
                    var result = child.Sub(id);
                    if (result != null) {
                        return result;
                    }
                }
            }

            return null;
        }

        #endregion

        #region Service // TODO Move to another interface

        void LateInit();

        #endregion
    }
}