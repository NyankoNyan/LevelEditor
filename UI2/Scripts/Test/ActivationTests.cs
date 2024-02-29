using System.Dynamic;

using UI2.Feature;

namespace UI2.Test
{
    //[UITest]
    public static class ActivationTests
    {
        [UITest]
        public static IElementSetupWrite Activation()
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

        [UITest]
        public static IElementSetupWrite GroupActivation()
        {
            return new PanelElement().Write()
                .State("Active", true)
                .Sub(
                    new ButtonElement(name: "Toggle").Write()
                        .Handle(Facade.Click, (_, ctx) => ctx.DrillUpSignal("TOGGLE")),
                    new PanelElement().Write()
                        .Id("Target")
                        .GroupHorizontal()
                        .Sub(
                            new ButtonElement("Click me").Write()
                                .Handle(Facade.Click, (_, ctx) => ctx.DrillUpSignal("CLICK")),
                            new InputElement().Write(),
                            new LabelElement().Write()
                                .Id("Status")
                        )
                        .Handle("CLICK", (_, ctx) =>
                            ctx.Find("Status").Feature<MainText>().SetText("clicked..."))
                )
                .Handle("TOGGLE", (_, ctx) => {
                    var active = ctx.Element.State("Active").Get<bool>();
                    active = !active;
                    ctx.Element.State("Active").Set(active);
                    ctx.Find("Target").Feature<Active>().Activate(active);
                });
        }
    }
}