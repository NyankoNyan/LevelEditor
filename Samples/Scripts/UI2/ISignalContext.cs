namespace UI2
{
    public interface ISignalContext
    {
        string Name { get; }
        object Data { get; }
        void Consume();
    }
}