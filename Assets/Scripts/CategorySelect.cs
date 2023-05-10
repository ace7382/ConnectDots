using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class CategorySelect : Page
{
    #region Private Consts

    //TODO: Put these in UIManager or a Style Class
    private Color BUTTONACTIVE = new Color(0.3490196f, 0.8117647f, 0.5490196f, 1f);
    private Color BUTTONINACTIVE = Color.grey;

    #endregion

    #region Private Variables

    private VisualElement header_Instructions;
    private VisualElement header_CategoryIcon;
    private VisualElement header_LeftPanel;
    private VisualElement header_RightPanel;
    private VisualElement header_PlayUnlockButton;

    private Label header_CategoryTitle;
    private Label header_LevelsCompletedLabel;
    private Label header_ObjectivesCompletedLabel;
    private Label header_BronzeTTLabel;
    private Label header_SilverTTLabel;
    private Label header_GoldTTLabel;
    private Label header_StarTTLabel;

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

                old.Q<VisualElement>("SelectionBorder").Hide();

                Tween scaleDown = DOTween.To(() => old.transform.scale,
                                    x => old.transform.scale = x,
                                    Vector3.one
                                    , .07f)
                                    .SetEase(Ease.Linear)
                                    .Play();
            }

            selectedButton = value;

            if (selectedButton != null)
            {
                selectedButton.Q<VisualElement>("SelectionBorder").Show();

                Tween scaleUp = DOTween.To(() => selectedButton.transform.scale,
                        x => selectedButton.transform.scale = x,
                        new Vector3(1.15f, 1.15f, 1f), .07f).SetEase(Ease.Linear).Play();
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

        header_Instructions                         = uiDoc.rootVisualElement.Q<VisualElement>("InstructionsLabel");
        header_CategoryIcon                         = uiDoc.rootVisualElement.Q<VisualElement>("CategoryIconBG");
        header_LeftPanel                            = uiDoc.rootVisualElement.Q<VisualElement>("LeftPanel");
        header_RightPanel                           = uiDoc.rootVisualElement.Q<VisualElement>("RightPanel");
        header_PlayUnlockButton                     = uiDoc.rootVisualElement.Q<VisualElement>("PlayUnlockButtonLabel");

        header_CategoryTitle                        = uiDoc.rootVisualElement.Q<Label>("CategoryName");
        header_LevelsCompletedLabel                 = uiDoc.rootVisualElement.Q<Label>("LevelsCompleteCount");
        header_ObjectivesCompletedLabel             = uiDoc.rootVisualElement.Q<Label>("ObjectivesCompleteCount");
        header_BronzeTTLabel                        = uiDoc.rootVisualElement.Q<VisualElement>("BronzeAwardContainer").Q<Label>("Count");
        header_SilverTTLabel                        = uiDoc.rootVisualElement.Q<VisualElement>("SilverAwardContainer").Q<Label>("Count");
        header_GoldTTLabel                          = uiDoc.rootVisualElement.Q<VisualElement>("GoldAwardContainer").Q<Label>("Count");
        header_StarTTLabel                          = uiDoc.rootVisualElement.Q<VisualElement>("StarAwardContainer").Q<Label>("Count");

        header_PlayUnlockButton.RegisterCallback<PointerUpEvent>((evt) =>
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
            VisualElement button    = UIManager.instance.LevelSelectButton.Instantiate();
            LevelCategory lCat      = cats[i];

            button.userData         = lCat;

            VisualElement icon      = button.Q<VisualElement>("Icon");
            VisualElement bg        = button.Q<VisualElement>("LevelSelectButton");

            bg.SetColor(lCat.Colors[0]);
            if (lCat.Colors.Count > 1) bg.SetShiftingBGColor(lCat.Colors);

            icon.style
                .backgroundImage    = lCat.LevelSelectImage;

            VisualElement completedIcon = button.Q<VisualElement>("CompletedIcon");
            completedIcon.Show(lCat.IsComplete);
            bg.SetBorderColor(lCat.IsComplete ? Color.yellow : Color.clear);

            button.RegisterCallback<PointerUpEvent>((PointerUpEvent evt) =>
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
            header_Instructions.Show();

            header_LeftPanel.Hide();
            header_RightPanel.Hide();
            header_CategoryIcon.Hide();
            header_CategoryTitle.Hide();
        }
        else
        {
            LevelCategory cat = SelectedCategoryButton.userData as LevelCategory;

            header_Instructions.Hide();

            header_CategoryIcon.Show();
            header_CategoryIcon
                .Q<VisualElement>("CategoryIcon")
                .style.backgroundImage              = cat.LevelSelectImage;
            header_CategoryIcon.SetColor(cat.Colors[0]);
            if (cat.Colors.Count > 1) header_CategoryIcon.SetShiftingBGColor(cat.Colors);

            header_CategoryTitle.Show();
            header_CategoryTitle.text               = cat.name;

            header_LeftPanel.Show();
            header_RightPanel.Show();

            List<Objective> catObjectives           = ObjectiveManager.instance.GetObjectivesForCategory(cat);
            header_ObjectivesCompletedLabel.text    = string.Format("{0} / {1}",
                                                        catObjectives.FindAll(x => x.IsComplete).Count.ToString("000"),
                                                        catObjectives.Count.ToString("000"));

            List<Level> catLevels                   = cat.GetLevels();
            header_LevelsCompletedLabel.text        = string.Format("{0} / {1}",
                                                        catLevels.FindAll(x => x.IsComplete).Count.ToString("000"),
                                                        catLevels.Count.ToString("000"));

            header_BronzeTTLabel.text               = "x " + cat.BronzeTimeMedals.ToString("000");
            header_SilverTTLabel.text               = "x " + cat.SilverTimeMedals.ToString("000");
            header_GoldTTLabel.text                 = "x " + cat.GoldTimeMedals.ToString("000");
            header_StarTTLabel.text                 = "x " + cat.StarTimeMedals.ToString("000");

            if (cat.Unlocked)
            {
                ((Label)header_PlayUnlockButton).text = "Play";
                header_PlayUnlockButton.SetColor(BUTTONACTIVE);
            }
            else
            {
                ((Label)header_PlayUnlockButton).text = "Unlock Category";
                header_PlayUnlockButton.SetColor(BUTTONINACTIVE);
            }
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
