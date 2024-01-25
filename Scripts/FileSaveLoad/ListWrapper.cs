namespace Level.IO
{
    [Serializable]
    internal class ListWrapper<T>
    {
        public T[] list;

        public ListWrapper(T[] list)
        {
            this.list = list;
        }
    }
}