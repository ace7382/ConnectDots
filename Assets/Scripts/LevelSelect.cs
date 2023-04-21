using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelect : Page
{
    #region Private Variables

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   - The level category to show levels for

        LevelCategory cat = (LevelCategory)args[0];

        List<Level> levels = Resources.LoadAll<Level>("Levels/" + cat.FilePath).ToList();

        ScrollView scroll = uiDoc.rootVisualElement.Q<ScrollView>();
        scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;

        scroll.contentContainer.style.flexDirection = FlexDirection.Row;
        scroll.contentContainer.style.flexWrap = Wrap.Wrap;
        scroll.contentContainer.style
            .justifyContent = Justify.Center;
        scroll.contentContainer.style.SetMargins(15f);

        scroll.style.SetBorderRadius(10f);
        scroll.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, .5f));

        for (int i = 0; i < levels.Count; i++)
        {
            for (int test = 0; test < 30; test++)
            {
                VisualElement button = UIManager.instance.LevelSelectButton.Instantiate();
                Level lev = levels[i];

                button.Q<VisualElement>("Icon").RemoveFromHierarchy();
                Label num = button.Q<Label>("Number");
                num.style.Show();
                num.text = lev.LevelNumber;

                button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
                {
                    object[] data = new object[1];
                    data[0] = lev;

                    PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
                });

                scroll.contentContainer.Add(button);
            }
        }

        UIManager.instance.SetBackground(cat.BackgroundImage, cat.Color);
    }

    public override void HidePage()
    {
        
    }

    public override IEnumerator AnimateIn()
    {
        return null;
    }

    public override IEnumerator AnimateOut()
    {
        return null;
    }

    #endregion
}
