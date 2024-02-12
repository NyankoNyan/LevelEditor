using UnityEngine;

namespace UI2;

public static class Snaps
{
    public static SetupThenDelegate HorizontalSnap(
        float? left = null,
        float? right = null,
        float? fixedSize = null,
        float? partSize = null) =>
        AxisSnap(0, left, right, fixedSize, partSize);

    public static SetupThenDelegate VerticalSnap(
        float? bottom = null,
        float? top = null,
        float? fixedSize = null,
        float? partSize = null) =>
        AxisSnap(1, bottom, top, fixedSize, partSize);

    public static SetupThenDelegate AxisSnap(
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

        void SetAnchor(IElementSetup elem, float vMin, float vMax)
        {
            (Vector2 min, Vector2 max) = elem.Anchor;
            elem.SetAnchor(AxChange(min, vMin), AxChange(max, vMax));
        }

        return (elem) => {
            if (from.HasValue) {
                if (to.HasValue) {
                    SetAnchor(elem, 0, 1);
                    elem.SetPivot(AxChange(elem.Pivot, .5f));
                    elem.SetSizeDelta(AxChange(elem.SizeDelta, -from.Value - to.Value));
                    elem.SetAnchoredPosition(AxChange(elem.AnchoredPosition, -from.Value + to.Value));
                    if (fixedSize.HasValue) {
                        Debug.LogWarning("fixedSize will be ignored");
                    }

                    if (partSize.HasValue) {
                        Debug.LogWarning("partSize will be ignored");
                    }
                } else {
                    elem.SetPivot(AxChange(elem.Pivot, 0));
                    elem.SetAnchoredPosition(AxChange(elem.AnchoredPosition, from.Value));
                    if (fixedSize.HasValue) {
                        if (partSize.HasValue) {
                            Debug.LogWarning("partSize will be ignored");
                        }

                        SetAnchor(elem, 0, 0);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                    } else if (partSize.HasValue) {
                        SetAnchor(elem, 0, partSize.Value);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, -from.Value));
                    } 
                }
            } else {
                if (to.HasValue) {
                    elem.SetPivot(AxChange(elem.Pivot, 1));
                    elem.SetAnchoredPosition(AxChange(elem.AnchoredPosition, -to.Value));
                    if (fixedSize.HasValue) {
                        if (partSize.HasValue) {
                            Debug.LogWarning("partSize will be ignored");
                        }

                        SetAnchor(elem, 1, 1);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                    } else if (partSize.HasValue) {
                        SetAnchor(elem, partSize.Value, 1);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, -to.Value));
                    } 
                } else {
                    elem.SetPivot(AxChange(elem.Pivot, .5f));
                    elem.SetAnchoredPosition(AxChange(elem.AnchoredPosition, .5f));
                    if (fixedSize.HasValue) {
                        if (partSize.HasValue) {
                            Debug.LogWarning("partSize will be ignored");
                        }

                        SetAnchor(elem, .5f, .5f);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, fixedSize.Value));
                    } else if (partSize.HasValue) {
                        SetAnchor(elem, .5f - partSize.Value / 2f, .5f + partSize.Value / 2f);
                        elem.SetSizeDelta(AxChange(elem.SizeDelta, 0));
                    } else {
                        Debug.LogWarning("missing size argument");
                    }
                }
            }
        };
    }
}