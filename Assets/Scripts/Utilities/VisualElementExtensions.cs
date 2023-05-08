using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class VisualElementExtensions
{
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
        return ve.style.display == DisplayStyle.Flex;
    }

    public static void SetColor(this VisualElement ve, Color color)
    {
        ve.style.backgroundColor = color;
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
}
