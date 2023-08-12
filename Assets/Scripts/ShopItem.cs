using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ShopItem : ScriptableObject
{
    #region Classes

    [System.Serializable]
    public class PurchaseCost
    {
        public ColorCategory    colorCategory;
        public int              amount;
    }

    #endregion

    #region Inspector Variables

    [Header("General Shop Item Variables")]
    [SerializeField] protected Vector2Int           shopGridPosition;
    [SerializeField] protected Vector2Int           gridSize                    = new Vector2Int(1, 1);
    [SerializeField] protected int                  productLine;
    [SerializeField] protected ShopItem             previousProductLineItem;
    [SerializeField] protected List<PurchaseCost>   cost;

    [SerializeField] protected bool                 drawLinesToOutlineOnPurchase;
    [SerializeField] protected Vector2Int           lineInPosition;
    [SerializeField] protected Vector2Int           lineOutPosition;

    #endregion

    #region Public Properties

    public Vector2Int                               Position                    { get { return shopGridPosition; } }
    public Vector2Int                               Size                        { get { return gridSize; } }
    public bool                                     Purchased                   { get { return ShopManager.instance.IsItemPurchased(this); } }
    public List<PurchaseCost>                       Costs                       { get { return cost; } }
    public int                                      ProductLine                 { get { return productLine; } }
    public ShopItem                                 PreviousItem                { get { return previousProductLineItem; } }
    public int                                      ProductLineNumber           { get { return GetProductLineNumber(); } }
    public bool                                     DrawLinesToOutline          { get { return drawLinesToOutlineOnPurchase; } }
    public Vector2Int                               LineInPosition              { get { return lineInPosition; } }
    public Vector2Int                               LineOutPosition             { get { return lineOutPosition; } }

    #endregion

    #region Abstract Functions

    public abstract VisualElement   GetDisplayContent(bool owned);
    public abstract Color           GetColor();
    public abstract Texture2D       GetIcon();

    #endregion

    #region Public Functions

    public virtual void OnPurchase()
    {
        for (int i = 0; i < cost.Count; i++)
        {
            CurrencyManager.instance.SpendCurrency(cost[i].colorCategory, cost[i].amount);
        }

        ShopManager.instance.ItemPurchased(this);
    }

    #endregion

    #region Private Functions

    private int GetProductLineNumber()
    {
        int productLineNumber   = 1;

        ShopItem tempCheck      = this;

        while (tempCheck.PreviousItem != null)
        {
            productLineNumber++;
            tempCheck           = tempCheck.PreviousItem;
        }

        return productLineNumber;
    }

    #endregion
}
