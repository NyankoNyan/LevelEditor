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

        public static SetupDelegate Fill(float left = 0, float right = 0, float top = 0, float bottom = 0) =>
            (elem) => elem.Apply(
                HorizontalSnap(left, right),
                VerticalSnap(bottom, top)
            );

        public static SetupDelegate Center(float? width = null, float? height = null) =>
            (elem) => {
                elem
                    .Anchor((.5f, .5f), (.5f, .5f))
                    .Pivot(.5f, .5f);
                Vector2 newSize = new(
                    width ?? elem.Read().SizeDelta.x,
                    height ?? elem.Read().SizeDelta.y);

                elem.SizeDelta(newSize);
            };

        public static SetupDelegate AxisSnap(
            int axisId,
            float? from = null,
            float? to = null,
            float? fixedSize = null,
            float? partSize = null)
        {
            // TODO У меня большие вопросы к этому коду, потому что он нарушает правило "Записывай не читая".
            // TODO Так оно не будет работать в шаблонах. 
            return (elemW) => {
                var elem = elemW.Read();
                if (from.HasValue) {
                    if (to.HasValue) {
                        SetAnchor(elemW, 0, 1);
                        elemW.Pivot(AxChange(elem.Pivot, .5f));
                        elemW.SizeDelta(AxChange(elem.SizeDelta, -from.Value - to.Value));
                        elemW.AnchoredPos(AxChange(elem.AnchoredPosition, -from.Value + to.Value));
                        if (fixedSize.HasValue) {
                            Debug.LogWarning("fixedSize will be ignored");
                        }

                        if (partSize.HasValue) {
                            Debug.LogWarning("partSize will be ignored");
                        }
                    } else {
                        elemW.Pivot(AxChange(elem.Pivot, 0));
                        elemW.AnchoredPos(AxChange(elem.AnchoredPosition, from.Value));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, 0, 0);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, 0, partSize.Value);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, -from.Value));
                        }
                    }
                } else {
                    if (to.HasValue) {
                        elemW.Pivot(AxChange(elem.Pivot, 1));
                        elemW.AnchoredPos(AxChange(elem.AnchoredPosition, -to.Value));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, 1, 1);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, partSize.Value, 1);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, -to.Value));
                        }
                    } else {
                        elemW.Pivot(AxChange(elem.Pivot, .5f));
                        elemW.AnchoredPos(AxChange(elem.AnchoredPosition, .5f));
                        if (fixedSize.HasValue) {
                            if (partSize.HasValue) {
                                Debug.LogWarning("partSize will be ignored");
                            }

                            SetAnchor(elemW, .5f, .5f);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                        } else if (partSize.HasValue) {
                            SetAnchor(elemW, .5f - partSize.Value / 2f, .5f + partSize.Value / 2f);
                            elemW.SizeDelta(AxChange(elem.SizeDelta, 0));
                        } else {
                            Debug.LogWarning("missing size argument");
                        }
                    }
                }
            };

            Vector2 AxChange(Vector2 vec, float v)
            {
                vec[axisId] = v;
                return vec;
            }

            void SetAnchor(IElementSetupWrite elem, float vMin, float vMax)
            {
                (Vector2 min, Vector2 max) = elem.Read().Anchor;
                elem.Anchor(AxChange(min, vMin), AxChange(max, vMax));
            }
        }
    }
}