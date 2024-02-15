namespace UI2
{
    public class FieldFacade : ElementInstanceFacade
    {
        private void Awake()
        {
            AddFeature(new ActivateFeature());
        }
    }
}