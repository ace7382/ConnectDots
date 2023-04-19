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

    public abstract void ShowPage(object[] args);
    public abstract void HidePage();

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

    #endregion
}
