using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ShopItem_UnlockLevel", menuName = "New Shop Item - Unlock Category")]
public class ShopItem_UnlockLevelCategory : ShopItem
{
    #region Inspector Variables

    [Header("Level Category Unlock Variables")]
    [SerializeField] private LevelCategory levelCategoryToUnlock;

    #endregion

    #region Inherited Functions

    public override void OnPurchase()
    {
        base.OnPurchase();

        levelCategoryToUnlock.UnlockCategory();
    }

    public override VisualElement GetDisplayContent()
    {
        VisualElement container = new VisualElement();

        container.style.flexGrow= 1f;
        container.style
            .justifyContent     = Justify.Center;
        container.style
            .alignItems         = Align.Center;

        VisualElement catIcon   = UIManager.instance.LevelSelectButton.Instantiate();
        VisualElement icon      = catIcon.Q<VisualElement>("Icon");
        VisualElement bg        = catIcon.Q<VisualElement>("LevelSelectButton");

        bg.SetColor(levelCategoryToUnlock.Colors[0]);
        if (levelCategoryToUnlock.Colors.Count > 1) bg.SetShiftingBGColor(levelCategoryToUnlock.Colors);

        icon.style
            .backgroundImage    = levelCategoryToUnlock.LevelSelectImage;

        catIcon.Q<VisualElement>("CompletedIcon").RemoveFromHierarchy();
        bg.SetBorderColor(Color.clear);

        Label unlockText        = new Label();
        unlockText.text         = "Unlock Level Category:"; 
        unlockText.AddToClassList("ShopDescriptionText");
        unlockText.style
            .fontSize           = 45f;

        Label catName           = new Label();
        catName.text            = levelCategoryToUnlock.name; //TODO: is name the correct property?
        catName.AddToClassList("ShopDescriptionText");

        container.Add(catIcon);
        container.Add(unlockText);
        container.Add(catName);

        return container;
    }

    public override Color GetColor()
    {
        //TODO: Should these have BG shifts?
        return levelCategoryToUnlock.Colors[0];
    }

    public override Texture2D GetIcon()
    {
        return levelCategoryToUnlock.LevelSelectImage;
    }

    #endregion
}
