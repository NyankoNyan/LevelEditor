namespace UI2
{
    public class ButtonFacade : ElementInstanceFacade
    {
        private void Awake()
        {
            AddFeature(new ActivateFeature(), new ClickFeature());
        }
    }
}