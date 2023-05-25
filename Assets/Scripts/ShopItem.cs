using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ShopItem : ScriptableObject
{
    #region Inspector Variables

    [SerializeField] protected int          colorIndex;
    [SerializeField] protected Texture2D    icon;
    [SerializeField] protected Vector2      shopGridPosition;
    [SerializeField] protected Vector2      gridSize            = new Vector2(1, 1);

    #endregion

    #region Abstract Functions

    public abstract void            OnPurchase();
    public abstract VisualElement   GetDisplayContent();

    #endregion
}
