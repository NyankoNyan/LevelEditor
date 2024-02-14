namespace UI2
{
    internal class ElementRuntimeContext : IElementRuntimeContext
    {
        private readonly UIRoot _root;

        public IElementInstance Element { get; }

        public void DrillUpSignal(string name, object data, bool consumable)
            => _root.SendSignal(name, data, Element, SignalDirection.DrillUp, consumable);

        public void DrillDownSignal(string name, object data = null, bool consumable = true)
            => _root.SendSignal(name, data, Element, SignalDirection.DrillDown, consumable);

        public OperationDescriptor Start(IOperation operation) 
            => _root.StartOperation(operation);

        public IElementInstance Sub(string id)
        {
            return Element.Sub(id);
        }

        public ElementRuntimeContext(IElementInstance element, UIRoot root)
        {
            Element = element;
            _root = root;
        }
    }
}