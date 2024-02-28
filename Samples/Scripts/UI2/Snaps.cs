using UnityEngine;

namespace UI2
{
    public static class Snaps
    {
        public static SetupDelegate HorizontalSnap(
            float? left = null,
            float? right = null,
            float? fixedSize = null,
            float? partSize = null) =>
            AxisSnap(0, left, right, fixedSize, partSize);

        public static SetupDelegate VerticalSnap(
            float? bottom = null,
            float? top = null,
            float? fixedSize = null,
            float? partSize = null) =>
            AxisSnap(1, bottom, top, fixedSize, partSize);

        public static SetupDelegate AxisSnap(
            int axisId,
            float? from = null,
            float? to = null,
            float? fixedSize = null,
            float? partSize = null)
        {
            Vector2 AxChange(Vector2 vec, float v)
            {
                vec[axisId] = v;
                return vec;
            }

            void SetAnchor(IElementSetupWrite elem, float vMin, float vMax)
            {
                (Vector2 min, Vector2 max) = elem.Read().Anchor;
                elem.SetAnchor(AxChange(min, vMin), AxChange(max, vMax));
            }
            
            // TODO У меня большие вопросы к этому коду, потому что он нарушает правило "Записывай не читая".
            // TODO Так оно не будет работать в шаблонах. 
            return (elemW) => {
                var elem = elemW.Read();
                if (from.HasValue) {
                    if (to.HasValue) {
                        SetAnchor(elemW, 0, 1);
                        elemW.SetPivot(AxChange(elem.Pivot, .5f));
                        elemW.SetSizeDelta(AxChange(elem.SizeDelta, -from.Value - to.Value));
                        elemW.SetAnchoredPosition(AxChange(elem.AnchoredPosition, -from.Value + to.Value));
                        if (fixedSize.HasValue) {
                            Debug.LogWarning("fixedSize will be ignored");
                        }

                        if (partSize.HasValue) {
                            Debug.LogWarning("partSize will be ignored");
                        }
                    } else {
                        elemW.SetPivot(AxChange(elem.Pivot, 0));
                        elemW.SetAnchoredPosition(AxChange(elem.AnchoredPosition, from.Value));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, 0, 0);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, 0, partSize.Value);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, -from.Value));
                        }
                    }
                } else {
                    if (to.HasValue) {
                        elemW.SetPivot(AxChange(elem.Pivot, 1));
                        elemW.SetAnchoredPosition(AxChange(elem.AnchoredPosition, -to.Value));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, 1, 1);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, partSize.Value, 1);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, -to.Value));
                        }
                    } else {
                        elemW.SetPivot(AxChange(elem.Pivot, .5f));
                        elemW.SetAnchoredPosition(AxChange(elem.AnchoredPosition, .5f));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, .5f, .5f);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, .5f - partSize.Value / 2f, .5f + partSize.Value / 2f);
                            elemW.SetSizeDelta(AxChange(elem.SizeDelta, 0));
                        } else {
                            Debug.LogWarning("missing size argument");
                        }
                    }
                }
            };
        }
    }
}