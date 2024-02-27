using UnityEngine;

namespace UI2.Tests
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
            new PanelElement().Write()
                .Apply(AddNElems(5))
                .GroupVertical();

        [UITest]
        public static IElementSetupWrite HorizontalLayout() =>
            new PanelElement().Write()
                .Apply(AddNElems(5))
                .GroupHorizontal();

        [UITest]
        public static IElementSetupWrite GridLayout() =>
            new PanelElement().Write()
                .Apply(AddNElems(20))
                .Grid(new Vector2(20, 20),
                    new Vector2(5, 5));

        private static SetupThenDelegate AddNElems(int n)
        {
            SetupThenDelegate func = (setup) => {
                for (int i = 0; i < n; i++) {
                    setup.Sub(
                        new PanelElement().Write()
                            .SetPivot(new Vector2(0, 0))
                            .SetAnchor(new Vector2(0, 0), new Vector2(0, 0))
                            .SetSizeDelta(new Vector2(20, 20))
                            .SetAnchoredPosition(new Vector2(10 * i, 10 * i))
                    );
                }
            };
            return func;
        }
    }
}