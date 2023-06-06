using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ShopItem_Powerup", menuName = "New Shop Item - Powerup")]
public class ShopItem_Powerup : ShopItem_MultiplePurchaseItem
{
    #region Inspector Variables

    [Header("Powerup Shop Item Variables")]
    [SerializeField] private PowerupType powerupType;

    #endregion

    #region Inherited Functions

    public override Color GetColor()
    {
        return Color.blue; //TODO: Set this
    }

    public override VisualElement GetDisplayContent(bool owned)
    {
        return new VisualElement();
    }

    public override Texture2D GetIcon()
    {
        return UIManager.instance.GetPowerupIcon(powerupType);
    }

    #endregion
}
