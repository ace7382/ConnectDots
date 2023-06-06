using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    #region Singleton

    public static ShopManager           instance;

    #endregion

    #region Inspector Variables

    #endregion

    #region Private Variables

    private Dictionary<ShopItem, int>   purchasedItems;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        purchasedItems = new Dictionary<ShopItem, int>();
    }

    #endregion

    #region Public Functions

    public void ItemPurchased(ShopItem item)
    {
        if (purchasedItems.ContainsKey(item))
            purchasedItems[item]++;
        else
            purchasedItems.Add(item, 1);

        item.PostNotification(Notifications.ITEM_PURCHASED);
    }

    public bool IsItemPurchased(ShopItem item)
    {
        return purchasedItems.ContainsKey(item);
    }

    public bool FeatureUnlocked(ShopItem_UnlockFeature.Feature feature)
    {
        return purchasedItems.Where(x =>
            x.Key is ShopItem_UnlockFeature
            && ((ShopItem_UnlockFeature)x.Key).Feat == feature
        ).Count() > 0;
    }

    public List<Vector2Int> OwnedNodes()
    {
        List<Vector2Int> ret = new List<Vector2Int>();

        foreach (ShopItem s in purchasedItems.Keys)
        {
            //TODO: Account for different sized nodes
            ret.Add(s.Position);
        }

        return ret;
    }

    public int GetNumPurchased(ShopItem item)
    {
        return IsItemPurchased(item) ? purchasedItems[item]: 0;
    }

    #endregion
}
