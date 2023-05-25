using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ShopItem_UnlockLevel", menuName = "New Shop Item - Unlock Category")]
public class ShopItem_UnlockLevelCategory : ShopItem
{
    #region Inherited Functions

    public override void OnPurchase()
    {

    }

    public override VisualElement GetDisplayContent()
    {


        return new VisualElement();
    }

    #endregion
}
