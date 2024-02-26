namespace UI2
{
    public class ButtonElement : BaseElement
    {
        private readonly string _name;

        public ButtonElement(string id = null, string name = null)
        {
            _name = name;
            if (!string.IsNullOrWhiteSpace(id)) {
                SetId(id);
            }

            SetStyle("button");
            if (!string.IsNullOrWhiteSpace(name)) {
                Feature<MainTextFeature>(f => f.SetText(_name));
            } else {
                Feature<MainTextFeature>(f => f.SetText(""));
            }
        }

        protected override BaseElement GetEmptyClone() => new ButtonElement(Id, _name);
    }
}