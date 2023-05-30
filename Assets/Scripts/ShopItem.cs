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
        public int colorIndex;
        public int amount;
    }

    #endregion

    #region Inspector Variables

    [Header("General Shop Item Variables")]
    [SerializeField] protected List<ShopItem>       previousPurchasesNeeded;
    [SerializeField] protected Vector2              shopGridPosition;
    [SerializeField] protected Vector2              gridSize                    = new Vector2(1, 1);

    [SerializeField] protected List<PurchaseCost>   cost;

    #endregion

    #region Public Properties

    public Vector2                                  Position                    { get { return shopGridPosition; } }
    public Vector2                                  Size                        { get { return gridSize; } }
    public bool                                     Purchased                   { get { return ShopManager.instance.IsItemPurchased(this); } } //{ get { return purchased; } }
    public bool                                     NodeUnlocked                { get { return previousPurchasesNeeded.FindIndex(x => !ShopManager.instance.IsItemPurchased(x)) == -1; } }
    public List<PurchaseCost>                       Costs                       { get { return cost; } }
    public List<ShopItem>                           PrePurchases                { get { return previousPurchasesNeeded; } }

    #endregion

    #region Abstract Functions

    public abstract VisualElement   GetDisplayContent();
    public abstract Color           GetColor();
    public abstract Texture2D       GetIcon();

    #endregion

    #region Public Functions

    public virtual void OnPurchase()
    {
        for (int i = 0; i < cost.Count; i++)
        {
            CurrencyManager.instance.SpendCurrency(cost[i].colorIndex, cost[i].amount);
        }

        ShopManager.instance.ItemPurchased(this);
    }

    #endregion
}
