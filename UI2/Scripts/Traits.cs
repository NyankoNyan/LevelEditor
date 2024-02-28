using UI2.Feature;

namespace UI2
{
    public static class Traits
    {
        public static readonly SetupDelegate Active = elem => {
            elem.Handle("ACTIVATE", (sig, ctx) => {
                    ctx.Element.Feature<Active>()?.Activate();
                })
                .Handle("DEACTIVATE", (sig, ctx) => {
                    ctx.Element.Feature<Active>()?.Deactivate();
                });
        };
    }
}