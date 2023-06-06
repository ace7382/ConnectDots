using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ShopItem_UnlockLevel", menuName = "New Shop Item - Unlock Feature")]
public class ShopItem_UnlockFeature : ShopItem
{
    #region Enum

    public enum Feature
    {
        UNLOCK_SHOP,
        PRODUCT_LINE_TAB,
        PRODUCT_LINE_NUMBER,
    }

    #endregion

    #region Inspector Variables

    [Header("Unlock Feature Variables")]
    [SerializeField] private Feature    feature;
    [SerializeField] private string     desc;
    [SerializeField] private int        colorIndex;
    [SerializeField] private Texture2D  icon;

    #endregion

    #region Public Properties

    public Feature Feat { get { return feature; } }

    #endregion

    #region Inherited Functions

    public override VisualElement GetDisplayContent(bool owned)
    {
        VisualElement container         = new VisualElement();

        container.style.flexGrow        = 1f;
        container.style.flexDirection   = FlexDirection.Column;
        container.style.alignItems      = owned ? Align.Center : Align.FlexStart;
        container.style.justifyContent  = owned ? Justify.Center : Justify.FlexStart;
        container.SetMargins(30f, true, false, false, true);

        Label unlockText                = new Label();
        unlockText.text                 = owned ? "Unlocked:" : "Unlock"; 
        unlockText.AddToClassList("ShopDescriptionText");
        unlockText.style.fontSize       = 35f;

        Label descLabel                 = new Label();
        descLabel.text                  = desc;
        descLabel.AddToClassList("ShopDescriptionText");
        descLabel.style.fontSize        = 70f;

        container.Add(unlockText);
        container.Add(descLabel);

        return container;
    }

    public override Color GetColor()
    {
        return UIManager.instance.GetColor(colorIndex);
    }

    public override Texture2D GetIcon()
    {
        return icon;
    }

    #endregion
}
