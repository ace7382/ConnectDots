using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    #region Singleton

    public static ShopManager           instance;

    #endregion

    #region Inspector Variables

    #endregion

    #region Private Variables

    private HashSet<ShopItem>           purchasedItems;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        purchasedItems = new HashSet<ShopItem>();
    }

    #endregion

    #region Public Functions

    public void ItemPurchased(ShopItem item)
    {
        if (purchasedItems.Add(item))
        {
            item.PostNotification(Notifications.ITEM_PURCHASED);
        }
        //TODO: Idk if i need to handle a purchase fail?
        //      if something can be bought multiple times though, something
        //      besides a HashSet (or beside this function) will need to be used
    }

    public bool IsItemPurchased(ShopItem item)
    {
        return purchasedItems.Contains(item);
    }

    #endregion
}
