using UI2.Feature;

namespace UI2.Test
{
    //[UITest]
    public static class TemplateTests
    {
        [UITest]
        public static IElementSetupWrite SimpleTemplateTest()
        {
            // Empty constructor
            var t1 = new ElementTemplate();
            t1.Feature<MainText>(f => f.SetText("Template applied correctly"));

            // Intake-style constructor
            var t2 = new ElementTemplate(t => t
                .Apply(Snaps.HorizontalSnap(0, 0),
                    Snaps.VerticalSnap(0, 0)));

            return new LabelElement().Write()
                .Apply(t1.All, t2.All);
        }
    }
}