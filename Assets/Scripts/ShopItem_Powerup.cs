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

    #region Public Properties

    public PowerupType PowerupType { get { return powerupType; } }

    #endregion

    #region Overridden Functions

    public override void OnPurchase()
    {
        //TODO: if there are issues with the order of the addition of the powerup
        //      and the spending of coins/updating of shop, bring base code into
        //      here and rearange it vs calling base

        //TODO: Account for more than 1 Powerup purchased

        CurrencyManager.instance.AddCurrency(PowerupType, 1);

        base.OnPurchase();
    }

    #endregion

    #region Inherited Functions

    public override Color GetColor()
    {
        return Color.blue; //TODO: Set this
    }

    public override VisualElement GetDisplayContent(bool owned)
    {
        VisualElement container                 = new VisualElement();

        container.style.flexGrow                = 1f;
        container.style.flexDirection           = FlexDirection.Row;
        container.style.justifyContent          = Justify.FlexStart;

        VisualElement powerupIcon               = UIManager.instance.PowerupButton.Instantiate().Q<VisualElement>("Container");
        powerupIcon.Q<VisualElement>("Icon")
            .style.backgroundImage              = UIManager.instance.GetPowerupIcon(powerupType);

        powerupIcon.Q<Label>().text             = CurrencyManager.instance.GetPowerupsOwned(powerupType) >= 100 ?
                                                    "99+" : CurrencyManager.instance.GetPowerupsOwned(powerupType).ToString();

        powerupIcon.SetWidth(200f);
        powerupIcon.SetHeight(150f);
        powerupIcon.SetMargins(15f, false, true, false, false);
        powerupIcon.style.alignSelf             = Align.Center;
        
        VisualElement bg                        = powerupIcon.Q<VisualElement>("BG");
        bg.SetWidth(powerupIcon.style.height);
        bg.SetHeight(powerupIcon.style.height);

        VisualElement counter                   = powerupIcon.Q<VisualElement>("Counter");
        counter.style.bottom                    = 0f;
        counter.style.right                     = 15f;

        VisualElement rightContainer            = new VisualElement();
        rightContainer.style.flexDirection      = FlexDirection.Column;
        rightContainer.style.alignItems         = Align.FlexStart;
        rightContainer.style.justifyContent     = Justify.Center;

        Label smallText                         = new Label();
        smallText.text                          = "+1 Powerup:"; 
        smallText.AddToClassList("ShopDescriptionText");
        smallText.style.fontSize                = 35f;

        Label powName                           = new Label();
        powName.text                            = powerupType.Name();
        powName.AddToClassList("ShopDescriptionText");
        powName.style.fontSize                  = 70f;

        rightContainer.Add(smallText);
        rightContainer.Add(powName);

        container.Add(powerupIcon);
        container.Add(rightContainer);

        return container;
    }

    public override Texture2D GetIcon()
    {
        return UIManager.instance.GetPowerupIcon(powerupType);
    }

    #endregion
}
