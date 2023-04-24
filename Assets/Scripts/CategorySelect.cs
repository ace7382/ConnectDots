using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CategorySelect : Page
{
    #region Private Variables

    VisualElement bigCategoryIcon, selectACategory, detailsPanel;
    Label categoryTitle, levelsCompleted, categoryObjectivesMet;
    VisualElement selectedButton;
    List<VisualElement> categoryButtons;

    #endregion

    #region Private Properties

    private VisualElement SelectedCategoryButton
    {
        get { return selectedButton; }
        set
        {
            if (selectedButton == value)
                return;

            if (selectedButton != null)
                selectedButton.style.SetBorderColor(Color.clear);

            value.style.SetBorderColor(Color.green);

            selectedButton = value;

            ShowCategoryDetails();
        }
    }

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
        categoryButtons                             = new List<VisualElement>();

        detailsPanel                                = uiDoc.rootVisualElement.Q<VisualElement>("DetailsPanel");
        selectACategory                             = uiDoc.rootVisualElement.Q<VisualElement>("InstructionsLabel");
        bigCategoryIcon                             = uiDoc.rootVisualElement.Q<VisualElement>("CategoryIcon_Big");
        categoryTitle                               = uiDoc.rootVisualElement.Q<Label>("CategoryName");
        levelsCompleted                             = uiDoc.rootVisualElement.Q<Label>("LevelsCompleteCount");
        categoryObjectivesMet                       = uiDoc.rootVisualElement.Q<Label>("ObjectivesCompleteCount");

        //StyleFloat opacity0                         = new StyleFloat(0f);

        for (int i = 0; i < cats.Count; i++)
        {
            for (int test = 0; test < 50; test++)
            {
                VisualElement button = UIManager.instance.LevelSelectButton.Instantiate();
                LevelCategory lCat = cats[i];

                button.userData = lCat;

                VisualElement icon = button.Q<VisualElement>("Icon");
                button.Q<VisualElement>("LevelSelectButton").style.backgroundColor = lCat.Color;
                icon.style.backgroundImage = lCat.LevelSelectImage;

                button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
                {
                    if (SelectedCategoryButton == button)
                    {
                        object[] data = new object[1];
                        data[0] = lCat;

                        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
                    }
                    else
                    {
                        SelectedCategoryButton = button;
                    }
                });

                scroll.contentContainer.Add(button);

                //button.style.opacity = opacity0;
                categoryButtons.Add(button);
            }
        }

        SelectedCategoryButton = null;
    }

    public override void HidePage()
    {
        //TODO: Remove ability to click on animate in and animate out
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(0f);

        Tween fadein = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                1f, .33f);

        yield return fadein.Play().WaitForCompletion();

        //System.Random r = new System.Random();
        //categoryButtons = categoryButtons.OrderBy(x => r.Next()).ToList();

        //WaitForSeconds w = new WaitForSeconds(.005f);
        //List<Tween> waitFor = new List<Tween>();

        //for (int i = 0; i < categoryButtons.Count; i++)
        //{
        //    VisualElement v = categoryButtons[i];

        //    Tween a = DOTween.To(() => v.style.opacity.value,
        //        x => v.style.opacity = new StyleFloat(x),
        //        1f, .025f);

        //    waitFor.Add(a);

        //    a.Play();

        //    a.onKill += () => waitFor.Remove(a);

        //    yield return w;
        //}

        //while (waitFor.Count > 0)
        //    yield return null;
    }

    public override IEnumerator AnimateOut()
    {
        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(1f);

        Tween fadeout = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                0f, .33f);

        yield return fadeout.Play().WaitForCompletion();
    }

    #endregion

    #region Private Functions

    public void ShowCategoryDetails()
    {
        if (selectedButton == null)
        {
            selectACategory.style.Show();

            detailsPanel.style.Hide();
            bigCategoryIcon.style.Hide();
            categoryTitle.style.Hide();
            categoryObjectivesMet.style.Hide();
            levelsCompleted.style.Hide();
        }
        else
        {
            selectACategory.style.Hide();
            
            bigCategoryIcon.Clear();

            VisualElement buttonBG = new VisualElement();
            VisualElement icon = new VisualElement();

            buttonBG.style.alignItems = Align.Center;
            buttonBG.style.justifyContent = Justify.Center;
            buttonBG.style.backgroundColor = (SelectedCategoryButton.userData as LevelCategory).Color;
            buttonBG.style.SetHeight(200f);
            buttonBG.style.SetWidth(200f);
            buttonBG.style.SetBorderRadius(15f);

            icon.style.SetWidth(80f);
            icon.style.SetHeight(80f);
            icon.style.backgroundImage = (selectedButton.userData as LevelCategory).LevelSelectImage;

            buttonBG.Add(icon);
            bigCategoryIcon.Add(buttonBG);

            detailsPanel.style.Show();
            bigCategoryIcon.style.Show();
            categoryTitle.style.Show();
            categoryObjectivesMet.style.Show();
            levelsCompleted.style.Show();
        }
    }

    #endregion
}
