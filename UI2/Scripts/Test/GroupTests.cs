namespace UI2.Test
{
    [UITest]
    public static class GroupTests
    {
        [UITest]
        public static IElementSetupWrite NoLayout() =>
            new PanelElement().Write()
                .Apply(AddNElems(5));

        [UITest]
        public static IElementSetupWrite VerticalLayout() =>
            NoLayout().GroupVertical();

        [UITest]
        public static IElementSetupWrite HorizontalLayout() =>
            NoLayout().GroupHorizontal();

        [UITest]
        public static IElementSetupWrite GridLayout() =>
            new PanelElement().Write()
                .Apply(AddNElems(20))
                .Grid(cellSize: (20, 20),
                    padding: (5, 5, 5, 5));

        private static SetupDelegate AddNElems(int n)
        {
            SetupDelegate func = (setup) => {
                for (int i = 0; i < n; i++) {
                    setup.Sub(
                        new PanelElement().Write()
                            .Pivot(0, 0)
                            .Anchor(min: (0, 0), max: (0, 0))
                            .SizeDelta(20, 20)
                            .AnchoredPos(10 * i, 10 * i)
                    );
                }
            };
            return func;
        }
    }
}