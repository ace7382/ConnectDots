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

    public static void ShowHideVerticalBoundIndicators(this ScrollView scroll, VisualElement upperBoundIndicator, VisualElement lowerBoundIndicator)
    {
        if (scroll.mode == ScrollViewMode.Horizontal)
            Debug.LogWarning("Using ShowHideVerticalBoundIndicators on a horizontal scroll");

        lowerBoundIndicator.Show(
            !(float.IsNaN(scroll.verticalScroller.highValue)
            || scroll.verticalScroller.value == scroll.verticalScroller.highValue)
            );

        upperBoundIndicator.Show(scroll.verticalScroller.value != scroll.verticalScroller.lowValue);
    }

    public static void SetBoundIndicators(this ScrollView scroll, VisualElement indicator1, VisualElement indicator2)
    {
        scroll.Q<VisualElement>("unity-content-viewport").Add(indicator1);
        scroll.Q<VisualElement>("unity-content-viewport").Add(indicator2);
    }
}
