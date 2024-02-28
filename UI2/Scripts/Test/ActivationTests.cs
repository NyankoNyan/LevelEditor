using UI2.Feature;

namespace UI2.Test
{
    //[UITest]
    public static class ActivationTests
    {
        [UITest]
        public static IElementSetupWrite ActivationTest()
        {
            return new PanelElement().Write().Sub(
                    new InputElement().Write()
                        .Id("First")
                        .Feature<MainText>(f => f.SetText("FIRST")),
                    new InputElement().Write()
                        .Id("Second")
                        .Feature<MainText>(f => f.SetText("SECOND"))
                )
                .GroupVertical()
                .Init(UpdateSubs)
                .Timer(.2f, ctx => {
                    var stageState = ctx.Element.State("Stage");
                    stageState.Set((stageState.Get<int>() + 1) % 2);
                    UpdateSubs(ctx);
                });

            void UpdateSubs(IElementRuntimeContext ctx)
            {
                int stage = ctx.Element.State("Stage").Get<int>();
                ctx.Find("First").Feature<Active>().Activate(stage == 0);
                ctx.Find("Second").Feature<Active>().Activate(stage == 1);
            }
        }
    }
}