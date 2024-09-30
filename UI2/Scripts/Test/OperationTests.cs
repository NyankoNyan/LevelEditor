
using UI2.Feature;

namespace UI2.Test
{
    //[UITest]
    public static class OperationTests
    {
        [UITest]
        public static IElementSetupWrite SimpleOperation()
        {
            return new LabelElement("ERROR ERROR").Write()
                .Init(ctx => {
                    ctx.Start(new Operation().Do(() => {
                        ctx.Element.Feature<MainText>().SetText("It's okay when you see this");
                    }));
                });
        }

        [UITest]
        public static IElementSetupWrite LoopOperationWithBreak()
        {
            return new LabelElement("ERROR ERROR").Write()
                .Init(ctx => {
                    ctx.Start(new Operation()
                        .Do(() => {
                            int counter = ctx.Element.State("Counter").Get<int>();
                            string text = ctx.Element.State("Text").Get<string>();
                            text += counter.ToString();
                            ctx.Element.State("Text").Set(text);
                            ctx.Element.Feature<MainText>().SetText(text);
                            counter++;
                            ctx.Element.State("Counter").Set(counter);
                        })
                        .Break(() => {
                            int counter = ctx.Element.State("Counter").Get<int>();
                            return counter > 9;
                        })
                        .CallSelf());
                });
        }
    }
}