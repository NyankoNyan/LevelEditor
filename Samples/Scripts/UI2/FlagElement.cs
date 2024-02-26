namespace UI2
{
    public class FlagElement : BaseElement
    {
        public FlagElement()
        {
            SetStyle("flag");
        }

        protected override BaseElement GetEmptyClone() => new FlagElement();
    }
}