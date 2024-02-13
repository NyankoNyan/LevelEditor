using UnityEngine;
using UnityEngine.Assertions;

namespace UI2
{
    internal class ElementInstance : IElementInstance
    {
        private readonly IElementSetupReadWrite _proto;
        private readonly GameObject _instance;
        private readonly IElementInstance _parent;
        private List<IElementInstance> _children;

        public ElementInstance(IElementSetupReadWrite proto, GameObject instance, IElementInstance parent)
        {
            Assert.IsNotNull(proto);
            Assert.IsNotNull(instance);
            _proto = proto;
            _instance = instance;

            if (parent != null) {
                parent.AddChild(parent);
                _parent = parent;
            }
        }

        public IElementSetupReadWrite Proto => _proto;
        public IElementInstance Parent => _parent;

        public void AddChild(IElementInstance child)
        {
            _children ??= new();
            if (child.Parent != null) {
                throw new ArgumentException();
            }

            _children.Add(child);
        }

        public IEnumerable<IElementInstance> Children => _children;

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
}