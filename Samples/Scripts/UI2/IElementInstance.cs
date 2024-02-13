namespace UI2
{
    public interface IElementInstance
    {
        IElementInstance Show();

        IElementInstance Hide();

        IElementSetupReadWrite Proto { get; }
        IElementInstance Parent { get; }
        void AddChild(IElementInstance child);
        IEnumerable<IElementInstance> Children { get; }
    }
}