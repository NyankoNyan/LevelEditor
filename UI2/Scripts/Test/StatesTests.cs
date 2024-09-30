using System.Xml;

using UI2.Feature;

namespace UI2.Test
{
    //[UITest]
    public static class StatesTests
    {
        [UITest]
        public static IElementSetupWrite StatesLinking()
        {
            return PanelElement.Create()
                .Id("Main")
                .State("Status", "It's okay when you see this")
                .Sub(
                    new LabelElement().Write()
                        .StateFrom("Main", "Status", "StatusRef")
                        .UseState("StatusRef")
                        .Init(ctx => ctx.Element.Feature<MainText>()
                            .SetText("Of course it's not okay when you see initial text in non-writable field"))
                );
        }

        [UITest]
        public static IElementSetupWrite SharedStates()
        {
            return PanelElement.Create()
                .State("Status", "You can change this")
                .Sub(
                    new InputElement().Write()
                        .StateFrom("Main", "Status", "StatusRef")
                        .UseState("StatusRef"),
                    new LabelElement().Write()
                        .StateFrom("Main", "Status", "StatusRef")
                        .UseState("StatusRef")
                )
                .GroupVertical();
        }

        [UITest]
        public static IElementSetupWrite LinkStateFromChild()
        {
            return PanelElement.Create()
                .StateFrom("WithState", "Status")
                .Sub(
                    new LabelElement("This element contains state and another show state").Write()
                        .Id("WithState")
                        .State("Status", "It's okay when you see this"),
                    new LabelElement().Write()
                        .Id("WithoutState")
                )
                .GroupVertical()
                .Init(ctx => {
                    ctx.Find("WithoutState")
                        .Feature<MainText>()
                        .SetText(ctx.Element.State("Status").Get<string>());
                });
        }

        [UITest]
        public static IElementSetupWrite LinkStateFromParent()
        {
            return PanelElement.Create()
                .Id("Main")
                .State("Status", "It's okay when you see this")
                .Sub(new LabelElement().Write()
                    .StateFrom("Main", "Status", "StatusRef")
                    .Init(ctx => {
                        string status = ctx.Element.State("Status").Get<string>();
                        ctx.Element.Feature<MainText>().SetText(status);
                    }));
        }

        [UITest]
        public static IElementSetupWrite LinkStateFromAnotherBranch()
        {
            return PanelElement.Create()
                .Sub(
                    new LabelElement("This element contains state and another show state").Write()
                        .Id("WithState")
                        .State("Status", "It's okay when you see this"),
                    new LabelElement().Write()
                        .Id("WithoutState")
                        .StateFrom("WithState", "Status")
                        .Init(ctx => {
                            string status = ctx.Element.State("Status").Get<string>();
                            ctx.Element.Feature<MainText>().SetText(status);
                        })
                )
                .GroupVertical();
        }

        [UITest]
        public static IElementSetupWrite ContextLayer()
        {
            return PanelElement.Create()
                .Sub(
                    new LabelElement("This element contains state and another show state").Write()
                        .Id("WithState")
                        .State("Status", "It's okay when you see this"),
                    new LabelElement().Write()
                        .Id("WithoutState")
                        .StateFrom("Context", "Status")
                        .Init(ctx => {
                            string status = ctx.Element.State("Status").Get<string>();
                            ctx.Element.Feature<MainText>().SetText(status);
                        })
                )
                .StateContext("Context")
                .GroupVertical();
        }
    }
}