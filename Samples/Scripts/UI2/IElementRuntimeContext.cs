namespace UI2
{
    public interface IElementRuntimeContext
    {
        public IElementInstance Element { get; }
        public void DrillUpSignal(string name, object data = null, bool consumable = true);
        public void DrillDownSignal(string name, object data = null, bool consumable = true);
        OperationDescriptor Start(IOperation operation);
        IElementInstance Sub(string id);
    }
}