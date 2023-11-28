public static class ReactiveTools
{
    public static Action<TValue1> SubscribeCollection<TValue1>(
        IEnumerable<TValue1> collection,
        Action<TValue1> action,
        Action<TValue1> handler)
    {
        action += handler;
        foreach (TValue1 value in collection) {
            handler(value);
        }
        return action;
    }

    public static Action<TValue1, TValue2> SubscribeCollection<TValue1, TValue2>(
        IEnumerable<TValue1> collection,
        Action<TValue1, TValue2> action,
        TValue2 value2,
        Action<TValue1, TValue2> handler)
    {
        action += handler;
        foreach (TValue1 value in collection) {
            handler(value, value2);
        }
        return action;
    }
}