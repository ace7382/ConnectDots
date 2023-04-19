using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class ScrollViewExtentions
{
    public static void GoToTop(this ScrollView scroll, bool resetHorizontalScroll = true)
    {
        if (scroll.mode == ScrollViewMode.Horizontal)
            Debug.LogWarning("Using GoToTop on a horizontal scroll");

        scroll.scrollOffset = new Vector2(resetHorizontalScroll ? 0f : scroll.scrollOffset.x, 0f);
    }
}
