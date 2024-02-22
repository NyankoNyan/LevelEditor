namespace UI2
{
    public class FieldFacade : ElementInstanceFacade
    {
        protected override void OnInitFeatures()
        {
            AddFeature(new ActivateFeature());
        }
    }
}