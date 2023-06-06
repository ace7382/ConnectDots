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

    public static void ShowHideVerticalBoundIndicators(this ScrollView scroll
        , VisualElement upperBoundIndicator, VisualElement lowerBoundIndicator
        , VisualElement contentContainer = null, VisualElement viewport = null)
    {
        if (scroll.mode == ScrollViewMode.Horizontal)
            Debug.LogWarning("Using ShowHideVerticalBoundIndicators on a horizontal scroll");

        if (contentContainer != null && contentContainer.resolvedStyle.height <= viewport.resolvedStyle.height)
        {
            lowerBoundIndicator.Hide();
            upperBoundIndicator.Hide();
            return;
        }

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

    public static void ClearWithChildBoundIndicators(this ScrollView scroll, VisualElement indicator1, VisualElement indicator2)
    {
        VisualElement temp = new VisualElement();
        temp.Add(indicator1);
        temp.Add(indicator2);
        scroll.Clear();
        scroll.Add(indicator1);
        scroll.Add(indicator2);
    }
}
