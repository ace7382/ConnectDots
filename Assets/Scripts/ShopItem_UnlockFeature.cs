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
    }

    #endregion

    #region Inspector Variables

    [Header("Unlock Feature Variables")]
    [SerializeField] private Feature    feature;
    [SerializeField] private int        colorIndex;
    [SerializeField] private Texture2D  icon;

    #endregion

    #region Public Properties

    public Feature Feat { get { return feature; } }

    #endregion

    #region Inherited Functions

    public override VisualElement GetDisplayContent()
    {
        return new VisualElement();
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
