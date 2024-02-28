namespace UI2.Test
{
    [UITest]
    public static class SnapTests
    {
        [UITest]
        public static IElementSetupWrite SnapTop() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, right: 10),
                            Snaps.VerticalSnap(top: 10, partSize: .5f))
                );

        [UITest]
        public static IElementSetupWrite SnapBottom() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, right: 10),
                            Snaps.VerticalSnap(bottom: 10, partSize: .5f))
                );

        [UITest]
        public static IElementSetupWrite SnapLeft() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, partSize: .5f),
                            Snaps.VerticalSnap(top: 10, bottom: 10))
                );

        [UITest]
        public static IElementSetupWrite SnapRight() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(right: 10, partSize: .5f),
                            Snaps.VerticalSnap(top: 10, bottom: 10))
                );

        [UITest]
        public static IElementSetupWrite SnapTopAbs() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, right: 10),
                            Snaps.VerticalSnap(top: 10, fixedSize: 100f))
                );

        [UITest]
        public static IElementSetupWrite SnapBottomAbs() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, right: 10),
                            Snaps.VerticalSnap(bottom: 10, fixedSize: 100f))
                );

        [UITest]
        public static IElementSetupWrite SnapLeftAbs() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(left: 10, fixedSize: 100f),
                            Snaps.VerticalSnap(top: 10, bottom: 10))
                );

        [UITest]
        public static IElementSetupWrite SnapRightAbs() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(right: 10, fixedSize: 100f),
                            Snaps.VerticalSnap(top: 10, bottom: 10))
                );

        [UITest]
        public static IElementSetupWrite SnapCorner() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(right: 10, partSize: .5f),
                            Snaps.VerticalSnap(top: 10, partSize: .5f))
                );

        [UITest]
        public static IElementSetupWrite SnapCornerAbs() =>
            new PanelElement().Write()
                .Sub(
                    new PanelElement().Write()
                        .Apply(Snaps.HorizontalSnap(right: 10, fixedSize: 100f),
                            Snaps.VerticalSnap(top: 10, fixedSize: 100f))
                );
    }
}