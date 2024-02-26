using TMPro;

using UnityEngine;

namespace UI2
{
    public class FieldFacade : ElementInstanceFacade
    {
        [SerializeField] private TMP_InputField _inputField;

        protected override void OnInitFeatures()
        {
            AddFeature(new ActivateFeature(), new InputFeature(_inputField));
        }
    }
}