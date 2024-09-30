using UI2.Feature;

namespace UI2
{
    public class ButtonElement : BaseElement
    {
        private readonly string _name;
        private readonly bool _sendSignal;

        public ButtonElement(string id = null, string name = null, bool sendSignal = true)
        {
            _name = name;
            if (!string.IsNullOrWhiteSpace(id)) {
                SetId(id);
            }

            SetStyle("button");
            if (!string.IsNullOrWhiteSpace(name)) {
                Feature<MainText>(f => f.SetText(_name));
            } else {
                Feature<MainText>(f => f.SetText(""));
            }

            _sendSignal = sendSignal;

            if (sendSignal) {
                if (id != null) {
                    Handle(Facade.Click, (_, ctx) => {
                        ctx.DrillUpSignal(id);
                    });
                } else {
                    Handle(Facade.Click, (_, ctx) => {
                        ctx.DrillUpSignal(Facade.Click);
                    });
                }
            }
        }

        protected override BaseElement GetEmptyClone() => new ButtonElement(Id, _name, _sendSignal);
    }
}