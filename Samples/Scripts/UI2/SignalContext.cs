namespace UI2
{
    internal class SignalContext : ISignalContext
    {
        private bool _consumed = true;
        public string Name { get; }

        internal bool Consumed => _consumed;

        public void Consume()
        {
            _consumed = true;
        }

        public void Resume()
        {
            _consumed = false;
        }

        public SignalContext(string name, object data)
        {
            Name = name;
            Data = data;
        }

        public object Data { get; }
    }
}