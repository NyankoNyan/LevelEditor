using TMPro;

using UI2.Feature;

using UnityEngine;

namespace UI2
{
    public class LabelFacade : ElementInstanceFacade
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        protected override void OnInitFeatures()
        {
            AddFeature(new MainText(_textMesh));
        }
    }
}