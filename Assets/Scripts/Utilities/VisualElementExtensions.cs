using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
    private static Dictionary<VisualElement, Tween> shifts = new Dictionary<VisualElement, Tween>();

    public static void Show(this VisualElement ve, bool show = true)
    {
        ve.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public static void Hide(this VisualElement ve)
    {
        ve.Show(false);
    }

    public static bool IsShowing(this VisualElement ve)
    {
        //return ve.style.display == DisplayStyle.Flex;
        return ve.resolvedStyle.display == DisplayStyle.Flex;
    }

    public static void SetColor(this VisualElement ve, Color color)
    {
        if (shifts.ContainsKey(ve))
        {
            shifts[ve].Kill();
            shifts.Remove(ve);
        }

        ve.style.backgroundColor = color;
    }

    public static void SetOpacity(this VisualElement ve, float opacity0to100)
    {
        ve.style.opacity = new StyleFloat(opacity0to100);
    }

    public static void SetImage(this VisualElement ve, Texture2D image)
    {
        ve.style.backgroundImage = image;
    }

    public static void SetHeight(this VisualElement ve, StyleLength h, bool setMax = true, bool setMin = true)
    {
        ve.style.height = h;

        if (setMax) ve.style.maxHeight = h;
        if (setMin) ve.style.minHeight = h;
    }

    public static void SetWidth(this VisualElement ve, StyleLength w, bool setMax = true, bool setMin = true)
    {
        ve.style.width = w;
        if (setMax) ve.style.maxWidth = w;
        if (setMin) ve.style.minWidth = w;
    }

    public static void SetMargins(this VisualElement ve, float marginSize, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if (top)    ve.style.marginTop      = marginSize;
        if (right)  ve.style.marginRight    = marginSize;
        if (left)   ve.style.marginLeft     = marginSize;
        if (bottom) ve.style.marginBottom   = marginSize;
    }

    public static void SetMargins(this VisualElement ve, float top, float right, float bottom, float left)
    {
        ve.style.marginTop      = top;
        ve.style.marginRight    = right;
        ve.style.marginBottom   = bottom;
        ve.style.marginLeft     = left;
    }

    public static void SetPadding(this VisualElement ve, float paddingSize, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if (top)    ve.style.paddingTop         = paddingSize;
        if (right)  ve.style.paddingRight       = paddingSize;
        if (left)   ve.style.paddingLeft        = paddingSize;
        if (bottom) ve.style.paddingBottom      = paddingSize;
    }

    public static void SetBorderWidth(this VisualElement value, float borderWidth, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if(top)     value.style.borderTopWidth        = borderWidth;
        if(right)   value.style.borderRightWidth      = borderWidth;
        if(left)    value.style.borderLeftWidth       = borderWidth;
        if (bottom) value.style.borderBottomWidth     = borderWidth;
    }

    public static void SetBorderColor(this VisualElement value, Color borderColor, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if(left)    value.style.borderLeftColor       = borderColor;
        if(right)   value.style.borderRightColor      = borderColor;
        if(top)     value.style.borderTopColor        = borderColor;
        if(bottom)  value.style.borderBottomColor     = borderColor;
    }

    public static void SetBorderRadius(this VisualElement value, StyleLength r, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        if(topLeft)     value.style.borderTopLeftRadius       = r;
        if(topRight)    value.style.borderTopRightRadius      = r;
        if(bottomLeft)  value.style.borderBottomLeftRadius    = r;
        if(bottomRight) value.style.borderBottomRightRadius   = r;
    }

    public static void ScaleToFit(this VisualElement ve)
    {
        ve.style.backgroundPositionX        = new BackgroundPosition(BackgroundPositionKeyword.Center);
        ve.style.backgroundPositionY        = new BackgroundPosition(BackgroundPositionKeyword.Center);
        ve.style.backgroundRepeat           = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
        ve.style.backgroundSize             = new BackgroundSize(BackgroundSizeType.Contain);
    }

    public static void SetShiftingBGColor(this VisualElement ve, List<Color> colors, float shiftTime = 5f)
    {
        if (shifts.ContainsKey(ve))
        {
            shifts[ve].Kill();
            shifts.Remove(ve);
        }

        Color current = ve.style.backgroundColor.value;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < colors.Count; i++)
        {
            if (i == 0 && current == colors[i])
                continue;

            int a = i;

            Tween shift = DOTween.To(() => ve.style.backgroundColor.value,
                x => ve.style.backgroundColor = new StyleColor(x),
                colors[a]
                , shiftTime)
                .SetEase(Ease.InQuart);

            seq.Append(shift);
        }

        Tween shiftToStart = DOTween.To(() => ve.style.backgroundColor.value,
            x => ve.style.backgroundColor = new StyleColor(x),
            current
            , shiftTime)
            .SetEase(Ease.Linear);

        seq.Append(shiftToStart)
        .SetLoops(-1)
        .Play();

        shifts.Add(ve, seq);
    }


    //TODO: finish writing this so it can be used.
    //Not high priority at all, so not finishing it rn

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="ve"></param>
    ///// <param name="side">0 top, 1 right, 2 bottom, 3 left</param>
    ///// <param name=""></param>
    ///// <returns></returns>
    //public static Tween SlideInOutFromSide(this VisualElement ve, int side, bool slideIn, float animationLength)
    //{
    //    float destinationX, destinationY;

    //    switch (side)
    //    {
    //        case 0:
    //            if (slideIn)
    //                destinationY    = 0f;
    //            else
    //                destinationY    = -1 * ve.localBound.height;

    //            destinationX        = ve.transform.position.x;
    //            break;
    //        case 1:
    //            if (slideIn)
    //                destinationX    = -1 * ve.localBound.width;
    //            else
    //                destinationX    = 0f;

    //            destinationY        = ve.transform.position.y;
    //            break;
    //        case 2:
    //            if (slideIn)
    //                destinationX    = 0f;
    //            else
    //                destinationX    = ve.localBound.width;

    //            destinationY        = ve.transform.position.y;
    //            break;
    //        default
    //            if (slideIn)
    //                destinationX = 0f;
    //            else
    //                destinationX = -1 * ve.localBound.width;

    //            destinationY = ve.transform.position.y;
    //            break;

    //    }


    //    return  
    //        DOTween.To(
    //            () => ve.transform.position
    //            , x => ve.transform.position = x
    //            , new Vector3(
    //                destinationX
    //                , destinationY
    //                , ve.transform.position.z)
    //            , animationLength
    //        )
    //        .Pause()
    //        .SetEase(Ease.OutQuart);
    //}
}
