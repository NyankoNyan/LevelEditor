namespace UI2
{
    public static class Traits
    {
        public static readonly SetupThenDelegate Active = elem => {
            elem.Handle("ACTIVATE", (sig, ctx) => {
                    ctx.Element.GetFacadeFeature<ActivateFeature>()?.Activate();
                })
                .Handle("DEACTIVATE", (sig, ctx) => {
                    ctx.Element.GetFacadeFeature<ActivateFeature>()?.Deactivate();
                });
        };
    }
}