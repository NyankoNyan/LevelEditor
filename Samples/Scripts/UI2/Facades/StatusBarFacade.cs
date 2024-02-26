using TMPro;

using UnityEngine;

namespace UI2
{
    public class StatusBarFacade : ElementInstanceFacade
    {
        [SerializeField] TextMeshProUGUI _textObj;
        protected override void OnInitFeatures()
        {
            AddFeature(new MainTextFeature(_textObj));
        }
    }
}