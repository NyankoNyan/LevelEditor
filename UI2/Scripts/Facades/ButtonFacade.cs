
using UI2.Feature;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace UI2
{
    public class ButtonFacade : ElementInstanceFacade
    {
        [SerializeField] private Button _button;

        protected override void OnInitFeatures()
        {
            Assert.IsNotNull(_button);
            AddFeature(new Active(_button), new Click(_button));
        }
    }
}