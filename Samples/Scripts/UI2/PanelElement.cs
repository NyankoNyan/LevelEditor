namespace UI2
{
    public class PanelElement : BaseElement
    {
        public PanelElement()
        {
            Write()
                .SetStyle("panel");
        }
        protected override BaseElement GetEmptyClone() => new PanelElement();
    }
}