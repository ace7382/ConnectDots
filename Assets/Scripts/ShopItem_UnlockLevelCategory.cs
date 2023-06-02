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

    public override VisualElement GetDisplayContent(bool owned)
    {
        VisualElement container                 = new VisualElement();

        container.style.flexGrow                = 1f;
        container.style.flexDirection           = FlexDirection.Row;
        container.style.justifyContent          = owned ? Justify.Center : Justify.FlexStart;

        VisualElement catIcon                   = UIManager.instance.LevelSelectButton.Instantiate();
        VisualElement icon                      = catIcon.Q<VisualElement>("Icon");
        VisualElement bg                        = catIcon.Q<VisualElement>("LevelSelectButton");

        bg.style.alignSelf                      = Align.Center;
        bg.SetColor(levelCategoryToUnlock.Colors[0]);
        if (levelCategoryToUnlock.Colors.Count > 1) bg.SetShiftingBGColor(levelCategoryToUnlock.Colors);

        icon.style
            .backgroundImage    = levelCategoryToUnlock.LevelSelectImage;

        catIcon.Q<VisualElement>("CompletedIcon").RemoveFromHierarchy();
        bg.SetBorderColor(Color.clear);

        VisualElement rightContainer            = new VisualElement();
        rightContainer.style.flexDirection      = FlexDirection.Column;
        rightContainer.style.alignItems         = Align.FlexStart;
        rightContainer.style.justifyContent     = Justify.Center;

        Label unlockText                        = new Label();
        unlockText.text                         = "Unlock Level Category:"; 
        unlockText.AddToClassList("ShopDescriptionText");
        unlockText.style.fontSize               = 35f;

        Label catName                           = new Label();
        catName.text                            = levelCategoryToUnlock.name; //TODO: is name the correct property?
        catName.AddToClassList("ShopDescriptionText");
        catName.style.fontSize                  = 70f;

        rightContainer.Add(unlockText);
        rightContainer.Add(catName);

        container.Add(bg);
        container.Add(rightContainer);

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
