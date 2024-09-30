namespace UI2
{
    public class PanelElement : BaseElement
    {
        public static IElementSetupWrite Create() => new PanelElement().Write();

        public PanelElement()
        {
            Write()
                .Style("panel");
        }

        protected override BaseElement GetEmptyClone() => new PanelElement();
    }
}