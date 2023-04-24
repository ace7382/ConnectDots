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
}
