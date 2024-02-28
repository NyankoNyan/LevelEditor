using UI2.Feature;

namespace UI2
{
    public class LabelElement : BaseElement
    {
        private readonly string _defaultText;

        public LabelElement(string defaultText = "")
        {
            _defaultText = defaultText ?? "";
            SetStyle("label");
            Feature<MainText>(f => f.SetText(_defaultText));
        }

        protected override BaseElement GetEmptyClone() => new LabelElement(_defaultText);
    }
}