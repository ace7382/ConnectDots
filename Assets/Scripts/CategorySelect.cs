using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CategorySelect : Page
{
    #region Private Variables

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        ScrollView scroll                           = uiDoc.rootVisualElement.Q<ScrollView>();
        scroll.verticalScrollerVisibility           = ScrollerVisibility.Hidden;

        scroll.contentContainer.style.flexDirection = FlexDirection.Row;
        scroll.contentContainer.style.flexWrap = Wrap.Wrap;
        scroll.contentContainer.style
            .justifyContent                         = Justify.Center;
        scroll.contentContainer.style               .SetMargins(15f);

        scroll.style                                .SetBorderRadius(10f);
        scroll.style.backgroundColor                = new StyleColor(new Color(0f, 0f, 0f, .5f));

        List<LevelCategory> cats                    = Resources.LoadAll<LevelCategory>("Categories").ToList();

        for (int i = 0; i < cats.Count; i++)
        {
            VisualElement button = UIManager.instance.LevelSelectButton.Instantiate();
            LevelCategory lCat = cats[i];

            VisualElement icon = button.Q<VisualElement>("Icon");
            button.Q<VisualElement>("LevelSelectButton").style.backgroundColor = lCat.Color;
            icon.style.backgroundImage = lCat.LevelSelectImage;

            button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
            {
                object[] data   = new object[1];
                data[0]         = lCat;

                PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data);
            });

            scroll.contentContainer.Add(button);
        }
    }

    public override void HidePage()
    {
        
    }

    #endregion
}
