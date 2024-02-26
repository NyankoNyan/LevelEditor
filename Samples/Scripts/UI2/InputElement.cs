namespace UI2
{
    public class InputElement : BaseElement
    {
        public InputElement()
        {
            SetStyle("field");
        }

        protected override BaseElement GetEmptyClone() => new InputElement();
    }
}