using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class IStyleExtentions
{
    public static void SetMargins(this IStyle value, float marginSize, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if(top)     value.marginTop             = marginSize;
        if(right)   value.marginRight           = marginSize;
        if(left)    value.marginLeft            = marginSize;
        if(bottom)  value.marginBottom          = marginSize;
    }

    public static void SetPadding(this IStyle value, float marginSize)
    {
        value.paddingTop            = marginSize;
        value.paddingRight          = marginSize;
        value.paddingLeft           = marginSize;
        value.paddingBottom         = marginSize;
    }

    public static void SetBorderWidth(this IStyle value, float borderWidth, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if(top)     value.borderTopWidth        = borderWidth;
        if(right)   value.borderRightWidth      = borderWidth;
        if(left)    value.borderLeftWidth       = borderWidth;
        if (bottom) value.borderBottomWidth     = borderWidth;
    }

    public static void SetBorderColor(this IStyle value, Color borderColor, bool top = true, bool right = true, bool bottom = true, bool left = true)
    {
        if(left)    value.borderLeftColor       = borderColor;
        if(right)   value.borderRightColor      = borderColor;
        if(top)     value.borderTopColor        = borderColor;
        if(bottom)  value.borderBottomColor     = borderColor;
    }

    public static void SetBorderRadius(this IStyle value, StyleLength r, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        if(topLeft)     value.borderTopLeftRadius       = r;
        if(topRight)    value.borderTopRightRadius      = r;
        if(bottomLeft)  value.borderBottomLeftRadius    = r;
        if(bottomRight) value.borderBottomRightRadius   = r;
    }

    public static void SetHeight(this IStyle value, StyleLength h, bool setMax = true, bool setMin = true)
    {
        value.height                = h;

        if (setMax) value.maxHeight = h;
        if (setMin) value.minHeight = h;
    }

    public static void SetWidth(this IStyle value, StyleLength w, bool setMax = true, bool setMin = true)
    {
        value.width                 = w;
        if (setMax) value.maxWidth  = w;
        if (setMin) value.minWidth  = w;
    }

    public static void Show(this IStyle value)
    {
        value.display               = DisplayStyle.Flex;
    }

    public static void Hide(this IStyle value)
    {
        value.display               = DisplayStyle.None;
    }
}
