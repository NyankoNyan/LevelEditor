using System.Collections.Generic;

namespace UI2
{
    public interface IElementInstance
    {
        IElementInstance Show();

        IElementInstance Hide();
        bool Active { get; }

        IElementSetupReadWrite Proto { get; }
        IElementInstance Parent { get; }
        void AddChild(IElementInstance child);
        IEnumerable<IElementInstance> Children { get; }
        IElementInstance Sub(string id);
        void SendFacadeSignal(string id);
        T GetFacadeFeature<T>() where T : class, IFacadeFeature;
    }
}