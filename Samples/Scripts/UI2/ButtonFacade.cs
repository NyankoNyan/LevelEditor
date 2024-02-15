using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI2
{
    public class ButtonFacade : ElementInstanceFacade
    {
        [SerializeField] Button _button;
        private void Awake()
        {
            Assert.IsNotNull(_button);
            AddFeature(new ActivateFeature(_button), new ClickFeature(_button));
        }
    }
}