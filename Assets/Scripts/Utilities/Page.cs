using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class Page
{
    #region Protected Variables

    protected UIDocument   uiDoc;

    #endregion

    #region Abstract Functions

    //Function Execution Order is:
    //1) Show Page
    //2) Animate In
    //3) Animate Out
    //4) Hide Page

    public abstract void ShowPage(object[] args);
    public abstract void HidePage();

    public abstract IEnumerator AnimateOut();
    public abstract IEnumerator AnimateIn();

    #endregion

    #region Public Functions

    public void SetSortOrder(int so)
    {
        uiDoc.sortingOrder = so;
    }

    public void SetUIDoc(UIDocument u)
    {
        uiDoc = u;
    }

    public void SetSafeAreaMargins()
    {
        RectOffsetFloat safeMargins                     = uiDoc.rootVisualElement.panel.GetSafeArea();
        uiDoc.rootVisualElement.style.paddingBottom     = safeMargins.Bottom;
        uiDoc.rootVisualElement.style.paddingTop        = safeMargins.Top;
        uiDoc.rootVisualElement.style.paddingLeft       = safeMargins.Left;
        uiDoc.rootVisualElement.style.paddingRight      = safeMargins.Right;
    }

    public virtual void OnFocusReturnedToPage()
    {

    }

    #endregion
}
