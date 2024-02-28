namespace UI2
{
    public class PanelElement : BaseElement
    {
        public PanelElement()
        {
            Write()
                .Style("panel");
        }
        protected override BaseElement GetEmptyClone() => new PanelElement();
    }
}