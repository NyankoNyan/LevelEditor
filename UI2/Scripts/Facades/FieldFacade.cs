using TMPro;

using UI2.Feature;

using UnityEngine;

namespace UI2
{
    public class FieldFacade : ElementInstanceFacade
    {
        [SerializeField] private TMP_InputField _inputField;

        protected override void OnInitFeatures()
        {
            AddFeature(new Active(), new Feature.Input(_inputField));
        }
    }
}