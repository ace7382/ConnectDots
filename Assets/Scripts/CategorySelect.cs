using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CategorySelect : Page
{
    #region Private Variables

    private VisualElement bigCategoryIcon, instructionsVE, levelsCompletedPanel, objectivesPanel;
    private Label categoryTitle, levelsCompleted, categoryObjectivesMet;
    private VisualElement selectedButton;
    private List<VisualElement> categoryButtons;
    private bool canClick;

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
            {
                VisualElement old = selectedButton;

                old.Q<VisualElement>("LevelSelectButton").style.SetBorderColor(Color.clear);

                Tween scaleDown = DOTween.To(() => old.transform.scale,
                                    x => old.transform.scale = x,
                                    Vector3.one, .02f).SetEase(Ease.OutQuad).Play();
            }

            selectedButton = value;

            if (selectedButton != null)
            {
                selectedButton.Q<VisualElement>("LevelSelectButton").style.SetBorderColor(Color.green);

                Tween scaleUp = DOTween.To(() => selectedButton.transform.scale,
                        x => selectedButton.transform.scale = x,
                        new Vector3(1.15f, 1.15f, 1f), .02f).SetEase(Ease.OutQuad).Play();
            }

            ShowCategoryDetails();
        }
    }

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>(null, false));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);

        ScrollView scroll                           = uiDoc.rootVisualElement.Q<ScrollView>("CategoryScroll");
        scroll.contentContainer.style.flexGrow      = 1f;
        VisualElement scrollContent                 = scroll.contentContainer.Q<VisualElement>("CategoryScrollContent");

        List<LevelCategory> cats                    = Resources.LoadAll<LevelCategory>("Categories").ToList();
        categoryButtons                             = new List<VisualElement>();

        instructionsVE                              = uiDoc.rootVisualElement.Q<VisualElement>("InstructionsLabel");
        bigCategoryIcon                             = uiDoc.rootVisualElement.Q<VisualElement>("CategoryIcon_Big");
        categoryTitle                               = uiDoc.rootVisualElement.Q<Label>("CategoryName");
        levelsCompleted                             = uiDoc.rootVisualElement.Q<Label>("LevelsCompleteCount");
        levelsCompletedPanel                        = uiDoc.rootVisualElement.Q<VisualElement>("LevelsCompletePanel");
        categoryObjectivesMet                       = uiDoc.rootVisualElement.Q<Label>("ObjectivesCompleteCount");
        objectivesPanel                             = uiDoc.rootVisualElement.Q<VisualElement>("ObjectivesCompletePanel");

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
                    if (!canClick)
                        return;

                    canClick = false;

                    if (SelectedCategoryButton == button)
                    {
                        object[] data = new object[1];
                        data[0] = lCat;

                        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
                    }
                    else
                    {
                        SelectedCategoryButton = button;
                        canClick = true;
                    }
                });

                scrollContent.Add(button);

                categoryButtons.Add(button);
            }
        }

        canClick = true;
        SelectedCategoryButton = null;
        ShowCategoryDetails();
    }

    public override void HidePage()
    {

    }

    public override IEnumerator AnimateIn()
    {
        canClick = false;

        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(0f);

        Tween fadein = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                1f, .33f);

        yield return fadein.Play().WaitForCompletion();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

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
            instructionsVE.style.Show();

            bigCategoryIcon.Hide();
            categoryTitle.Hide();
            levelsCompletedPanel.Hide();
            objectivesPanel.Hide();
        }
        else
        {
            LevelCategory cat = SelectedCategoryButton.userData as LevelCategory;

            instructionsVE.Hide();

            bigCategoryIcon.Clear();

            VisualElement buttonBG          = new VisualElement();
            VisualElement icon              = new VisualElement();

            buttonBG.style.alignItems       = Align.Center;
            buttonBG.style.justifyContent   = Justify.Center;
            buttonBG.style.backgroundColor  = cat.Color;
            buttonBG.style.SetHeight(150f);
            buttonBG.style.SetWidth(150f);
            buttonBG.style.SetBorderRadius(15f);

            icon.style.SetWidth(60f);
            icon.style.SetHeight(60f);
            icon.style.backgroundImage = cat.LevelSelectImage;

            buttonBG.Add(icon);
            bigCategoryIcon.Add(buttonBG);

            bigCategoryIcon.Show();

            categoryTitle.text = cat.name;
            categoryTitle.Show();

            List<Objective> catObjectives = ObjectiveManager.instance.GetObjectivesForCategory(cat);

            categoryObjectivesMet.text = string.Format("{0} / {1}",
                                        catObjectives.FindAll(x => x.IsComplete).Count.ToString("000"),
                                        catObjectives.Count.ToString("000"));

            List<Level> catLevels = cat.GetLevels();

            levelsCompleted.text = string.Format("{0} / {1}",
                                    catLevels.FindAll(x => x.IsComplete).Count.ToString("000"),
                                    catLevels.Count.ToString("000"));
                                            
            levelsCompletedPanel.Show();
            objectivesPanel.Show();
        }
    }

    #endregion
}
