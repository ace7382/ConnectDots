using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CategorySelect : Page
{
    #region Private Variables

    private VisualElement bigCategoryIcon, instructionsVE, levelsCompletedPanel, objectivesPanel, playUnlockButton;
    private Label categoryTitle, levelsCompleted, categoryObjectivesMet;
    private VisualElement selectedButton;
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

                //old.Q<VisualElement>("LevelSelectButton").style.SetBorderColor(Color.clear);
                old.Q<VisualElement>("SelectionBorder").Hide();

                Tween scaleDown = DOTween.To(() => old.transform.scale,
                                    x => old.transform.scale = x,
                                    Vector3.one, .02f).SetEase(Ease.OutQuad).Play();
            }

            selectedButton = value;

            if (selectedButton != null)
            {
                //selectedButton.Q<VisualElement>("LevelSelectButton").style.SetBorderColor(Color.green);
                selectedButton.Q<VisualElement>("SelectionBorder").Show();

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

        instructionsVE                              = uiDoc.rootVisualElement.Q<VisualElement>("InstructionsLabel");
        bigCategoryIcon                             = uiDoc.rootVisualElement.Q<VisualElement>("CategoryIcon_Big");
        categoryTitle                               = uiDoc.rootVisualElement.Q<Label>("CategoryName");
        levelsCompleted                             = uiDoc.rootVisualElement.Q<Label>("LevelsCompleteCount");
        levelsCompletedPanel                        = uiDoc.rootVisualElement.Q<VisualElement>("LevelsCompletePanel");
        categoryObjectivesMet                       = uiDoc.rootVisualElement.Q<Label>("ObjectivesCompleteCount");
        objectivesPanel                             = uiDoc.rootVisualElement.Q<VisualElement>("ObjectivesCompletePanel");
        playUnlockButton                            = uiDoc.rootVisualElement.Q<VisualElement>("PlayUnlockButton");

        playUnlockButton.RegisterCallback<PointerUpEvent>((evt) =>
        {
            if (!canClick)
                return;

            if ((SelectedCategoryButton.userData as LevelCategory).Unlocked)
            {
                PlayButtonClicked();
            }
            else
            {
                UnlockButtonClicked();
            }
        });

        for (int i = 0; i < cats.Count; i++)
        {
            for (int test = 0; test < 20; test++)
            {
                VisualElement button = UIManager.instance.LevelSelectButton.Instantiate();
                LevelCategory lCat = cats[i];

                button.userData = lCat;

                VisualElement icon = button.Q<VisualElement>("Icon");
                VisualElement bg = button.Q<VisualElement>("LevelSelectButton");
                bg.SetColor(lCat.Color);
                icon.style.backgroundImage = lCat.LevelSelectImage;

                VisualElement completedIcon = button.Q<VisualElement>("CompletedIcon");
                completedIcon.Show(lCat.IsComplete);
                bg.SetBorderColor(lCat.IsComplete ? Color.yellow : Color.clear);

                button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
                {
                    if (!canClick)
                        return;

                    canClick = false;

                    if (SelectedCategoryButton != button)
                    {
                        SelectedCategoryButton = button;
                    }

                    canClick = true;
                });

                scrollContent.Add(button);
            }
        }

        canClick = true;
        SelectedCategoryButton = null;
        ShowCategoryDetails();

        this.AddObserver(ShowCategoryDetails, Notifications.CATEGORY_UNLOCKED);
    }

    public override void HidePage()
    {
        this.RemoveObserver(ShowCategoryDetails, Notifications.CATEGORY_UNLOCKED);
    }

    public override IEnumerator AnimateIn()
    {
        canClick = false;

        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(0f);

        Tween fadein = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                1f, .33f);

        if (!UIManager.instance.TopBar.IsShowing)
            UIManager.instance.TopBar.ShowTopBar();

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

    private void ShowCategoryDetails(object sender, object info)
    {
        ShowCategoryDetails();
    }

    private void ShowCategoryDetails()
    {
        if (selectedButton == null)
        {
            instructionsVE.style.Show();

            bigCategoryIcon.Hide();
            categoryTitle.Hide();
            levelsCompletedPanel.Hide();
            objectivesPanel.Hide();
            playUnlockButton.Hide();
        }
        else
        {
            LevelCategory cat = SelectedCategoryButton.userData as LevelCategory;

            instructionsVE.Hide();

            bigCategoryIcon.Clear();

            //VisualElement buttonBG          = new VisualElement();
            //VisualElement icon              = new VisualElement();

            //buttonBG.style.alignItems       = Align.Center;
            //buttonBG.style.justifyContent   = Justify.Center;
            //buttonBG.style.backgroundColor  = cat.Color;
            //buttonBG.style.SetHeight(150f);
            //buttonBG.style.SetWidth(150f);
            //buttonBG.style.SetBorderRadius(15f);

            //icon.style.SetWidth(60f);
            //icon.style.SetHeight(60f);
            //icon.style.backgroundImage = cat.LevelSelectImage;

            //buttonBG.Add(icon);
            //bigCategoryIcon.Add(buttonBG);

            VisualElement c                 = UIManager.instance.LevelSelectButton.Instantiate();
            VisualElement button            = c.Q<VisualElement>("LevelSelectButton");
            VisualElement icon              = button.Q<VisualElement>("Icon");
            VisualElement completeIcon      = button.Q<VisualElement>("CompletedIcon");
            c.Q<VisualElement>("SelectionBorder").RemoveFromHierarchy();

            icon.style.backgroundImage      = cat.LevelSelectImage;
            button.SetColor(cat.Color);

            button.transform.scale          = new Vector3(2f / 1.5f, 2f / 1.5f, 1f);
            completeIcon.Show(cat.IsComplete);
            button.SetBorderColor(cat.IsComplete ? Color.yellow : Color.clear);

            bigCategoryIcon.Add(button);
            bigCategoryIcon.SetHeight(150f, false, false);
            bigCategoryIcon.SetWidth(150f);

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

            if (cat.Unlocked)
            {
                playUnlockButton.Q<Label>().text = "Play         ";
                playUnlockButton.SetColor(Color.green);
            }
            else
            {
                playUnlockButton.Q<Label>().text = "Unlock Category";
                playUnlockButton.SetColor(Color.grey);
            }

            playUnlockButton.Show();
        }
    }

    private void PlayButtonClicked()
    {
        object[] data = new object[1];
        data[0] = SelectedCategoryButton.userData as LevelCategory;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
    }

    private void UnlockButtonClicked()
    {
        object[] data = new object[1];
        data[0] = SelectedCategoryButton.userData as LevelCategory;

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<CategoryUnlockPopup>(data));
    }

    #endregion
}
