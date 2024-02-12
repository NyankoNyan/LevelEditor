using UnityEngine;
using UnityEngine.Assertions;

namespace UI2;

internal class ElementInstance : IElementInstance
{
    private readonly IElementSetup _proto;
    private readonly GameObject _instance;

    public ElementInstance(IElementSetup proto, GameObject instance)
    {
        Assert.IsNotNull(proto);
        Assert.IsNotNull(instance);
        _proto = proto;
        _instance = instance;
    }

    public IElementSetup Proto => _proto;

    public IElementInstance Hide()
    {
        _instance.SetActive(false);
        return this;
    }

    public IElementInstance Show()
    {
        _instance.SetActive(true);
        return this;
    }
}