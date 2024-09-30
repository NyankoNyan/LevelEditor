using UI2.Feature;

using UnityEngine;

namespace UI2.Test
{
    //[UITest]
    public static class TimerTests
    {
        [UITest]
        public static IElementSetupWrite OperationTimer() =>
            new PanelElement().Write().Sub(
                new LabelElement().Write()
                    .State("Counter", 0)
                    .Init(ctx =>
                        ctx.Start(new Operation()
                            .Do(() => {
                                var counterState = ctx.Element.State("Counter");
                                int counter = counterState.Get<int>();
                                counter++;
                                counterState.Set(counter);
                                ctx.Element.Feature<MainText>().SetText(counter.ToString());
                            })
                            .Wait(new WaitForSeconds(1))
                            .CallSelf()
                        )
                    )
            );

        [UITest]
        public static IElementSetupWrite SugarTimer() =>
            new PanelElement().Write().Sub(
                new LabelElement().Write()
                    .UseState("Counter")
                    .Timer(1, ctx => {
                        var counterState = ctx.Element.State("Counter");
                        counterState.Set(counterState.Get<int>() + 1);
                    })
            );
    }
}