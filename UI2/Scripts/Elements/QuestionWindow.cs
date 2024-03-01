namespace UI2
{
    public class QuestionWindow : BaseElement
    {
        private readonly string _text;
        private readonly string[] _buttons;
        private readonly string _outSignal;

        public QuestionWindow(string outSignal, string text, params string[] buttons)
        {
            _outSignal = outSignal;
            _text = text;
            _buttons = buttons;

            var buttonPanel = new PanelElement().Write()
                .GroupHorizontal();

            // ButtonSetup[] buttonSetups = new ButtonSetup[_buttons.Length];
            for (int i = 0; i < _buttons.Length; i++) {
                var i1 = i + 1;
                buttonPanel.Sub(
                    new ButtonElement(null, _buttons[i]).Write()
                        .Handle(Facade.Click, (_, ctx) => {
                            ctx.DrillUpSignal("CHOOSE", i1);
                        })
                );
            }

            Write()
                .Sub(
                    new LabelElement(_text).Write()
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(100, 0)),
                    buttonPanel
                        .Apply(Snaps.HorizontalSnap(0, 0),
                            Snaps.VerticalSnap(bottom: 0, fixedSize: 100))
                )
                .SignalBlock()
                .Handle("CHOOSE", (sig, ctx) => {
                    ctx.Element.Hide();
                    ctx.DrillUpSignal(_outSignal, sig.Data);
                });
        }

        protected override BaseElement GetEmptyClone() => new QuestionWindow(_outSignal, _text, _buttons);
    }
}