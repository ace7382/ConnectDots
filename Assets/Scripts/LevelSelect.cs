using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelect : Page
{
    #region Private Variables

    private ScrollView              levelScroll;
    private ScrollView              timeAttackScroll;
    private VisualElement           levelsButton;
    private VisualElement           objectivesButton;
    private VisualElement           timeAttackButton;

    private bool                    canClick;
    private LevelCategory           cat;

    private VisualElement           levelTimedScrollPanel;
    private VisualElement           goalsScrollPanel;
    private ScrollView              goalsScroll;

    private List<VisualElement>     goalCards;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   - The level category to show levels for

        cat                                     = (LevelCategory)args[0];
        List<Level> levels                      = cat.GetLevels();
        goalCards                               = new List<VisualElement>();

        levelTimedScrollPanel                   = uiDoc.rootVisualElement.Q<VisualElement>("ScrollPanel");
        levelsButton                            = uiDoc.rootVisualElement.Q<VisualElement>("LevelsButton");
        objectivesButton                        = uiDoc.rootVisualElement.Q<VisualElement>("ObjectivesButton");
        timeAttackButton                        = uiDoc.rootVisualElement.Q<VisualElement>("TimeAttackButton");
        levelScroll                             = uiDoc.rootVisualElement.Q<ScrollView>("LevelScroll");
        levelScroll.contentContainer
            .style.flexGrow                     = 1f;

        //----------LEVELS----------
        VisualElement levelScrollContent        = levelScroll.contentContainer.Q<VisualElement>("LevelScrollContent");

        for (int i = 0; i < levels.Count; i++)
        {
            VisualElement button                = UIManager.instance.LevelSelectButton.Instantiate();
            Level lev                           = levels[i];

            button.Q<VisualElement>("Icon").RemoveFromHierarchy();
            Label num                           = button.Q<Label>("Number");
            num.style.Show();
            num.text                            = lev.LevelNumber;

            VisualElement completedIcon         = button.Q<VisualElement>("CompletedIcon");
            completedIcon.Show(levels[i].IsComplete);
            button.Q<VisualElement>("LevelSelectButton").SetBorderColor(levels[i].IsComplete ? Color.yellow : Color.clear);

            button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
            {
                if (!canClick)
                    return;

                canClick        = false;

                object[] data   = new object[1];
                data[0]         = lev;

                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
            });

            levelScrollContent.Add(button);
        }
        //----------END LEVELS----------

        //----------Goals----------
        List<Objective> objectives              = ObjectiveManager.instance.GetObjectivesForCategory(cat);

        goalsScrollPanel                        = uiDoc.rootVisualElement.Q<VisualElement>("GoalsScrollPanel");
        goalsScroll                             = goalsScrollPanel.Q<ScrollView>();

        goalsScroll.contentContainer.SetPadding(0f, 8f, 8f, 0f); //8 is the dropshadow size for objective cards

        Toggle hideCompleted                    = goalsScrollPanel.Q<Toggle>("HideCompletedToggle");

        hideCompleted.value                     = false;
        hideCompleted.focusable                 = false;
        hideCompleted.labelElement.AddToClassList("HeaderLabel");
        hideCompleted.labelElement.style.fontSize = 45f;
        hideCompleted.SetMargins(25f, 0f, 25f, 0f);

        VisualElement check                     = hideCompleted.Q<VisualElement>("unity-checkmark");
        check.parent.style.justifyContent       = Justify.FlexEnd;
        check.SetColor(new Color(.8f, .8f, .8f, 1f));
        check.SetBorderWidth(0f);
        check.SetBorderRadius(20f);
        check.ScaleToFit();
        check.SetWidth(70f);
        check.SetHeight(check.style.width);

        hideCompleted.RegisterValueChangedCallback<bool>((evt) => ShowHideCompleted(evt.newValue));

        for (int i = 0; i < objectives.Count; i++)
        {
            Objective objective                 = objectives[i];
            VisualElement goalCard              = UIManager.instance.ObjectiveCard.Instantiate();
            goalCard.name                       = objective.ID + " GOAL CARD";
            goalCard.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            goalCard.SetMargins(10f, i != 0, false, i != objectives.Count - 1, false);

            ObjectiveCard controller            = new ObjectiveCard(objective, goalCard);
            goalCard.userData                   = controller;

            goalsScroll.Add(goalCard);
            goalCards.Add(goalCard);
        }

        SetNotificationBubble(null, null);
        this.AddObserver(SetNotificationBubble, Notifications.OBJECTIVE_COMPLETE);
        this.AddObserver(SetNotificationBubble, Notifications.OBJECTIVE_REWARD_CLAIMED);
        //----------END Goals----------

        //----------TIME ATTACK----------
        timeAttackScroll = uiDoc.rootVisualElement.Q<ScrollView>("TimeAttackScroll");
        timeAttackScroll.contentContainer
            .style.flexGrow                     = 1f;
        VisualElement timeAttackScrollContent   = timeAttackScroll.contentContainer.Q<VisualElement>("TimeAttackScrollContent");
        
        for (int i = 0; i < cat.TimeAttacks.Count; i++)
        {
            VisualElement card                  = UIManager.instance.TimeAttackButton.Instantiate();
            card.style.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            card.style.SetMargins(10f, i != 0, false, i != objectives.Count - 1, false);

            TimedModeCard controller            = new TimedModeCard(cat.TimeAttacks[i], card);
            card.userData                       = controller;

            int index                           = i;

            card.RegisterCallback<PointerUpEvent>((evt) =>
            {
                if (!canClick)
                    return;

                canClick                        = false;

                object[] data                   = new object[2];
                data[0]                         = cat;
                data[1]                         = index;

                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<TimedModePage>(data));
            });

            timeAttackScrollContent.Add(card);

            if (i != cat.TimeAttacks.Count - 1)
            {
                VisualElement spacer            = new VisualElement();
                spacer.SetWidth(new StyleLength(new Length(90f, LengthUnit.Percent)));
                spacer.SetHeight(7f);
                spacer.SetColor(new Color(.8f, .8f, .8f, 1f));

                timeAttackScrollContent.Add(spacer);
            }
        }
        //----------END TIME ATTACK----------

        levelsButton.RegisterCallback<PointerUpEvent>(ShowLevels);
        objectivesButton.RegisterCallback<PointerUpEvent>(ShowGoals);
        timeAttackButton.RegisterCallback<PointerUpEvent>(ShowTimeAttacks);

        //UIManager.instance.SetBackground(cat.LevelSelectImage, cat.Color);
        UIManager.instance.SetBackground(cat.LevelSelectImage, cat.Colors[0]);
        if (cat.Colors.Count > 1) UIManager.instance.SetBackgroundShift(cat.Colors);

        canClick = true;

        ShowLevels(null);

        EventCallback<ClickEvent> backbuttonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
    }

    public override void HidePage()
    {
        //Not sure this is needed? if it is tho then
        //TODO: Unregister the level and time attack buttons
        levelsButton.UnregisterCallback<PointerUpEvent>(ShowLevels);
        objectivesButton.UnregisterCallback<PointerUpEvent>(ShowGoals);
        timeAttackButton.UnregisterCallback<PointerUpEvent>(ShowTimeAttacks);

        this.RemoveObserver(SetNotificationBubble, Notifications.OBJECTIVE_COMPLETE);
        this.RemoveObserver(SetNotificationBubble, Notifications.OBJECTIVE_REWARD_CLAIMED);
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(0f);

        Tween fadein = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                1f, .33f);

        yield return fadein.Play().WaitForCompletion();
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

    private void ShowLevels(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        goalsScrollPanel.Hide();
        levelTimedScrollPanel.Show();

        levelScroll.Show();
        timeAttackScroll.Hide();

        levelScroll.GoToTop();

        canClick = true;
    }

    private void ShowGoals(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        goalsScrollPanel.Show();
        levelTimedScrollPanel.Hide();

        goalsScroll.GoToTop();

        canClick = true;
    }

    private void ShowTimeAttacks(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        goalsScrollPanel.Hide();
        levelTimedScrollPanel.Show();

        levelScroll.Hide();
        timeAttackScroll.Show();

        timeAttackScroll.GoToTop();
    }

    private void ShowHideCompleted(bool hideCompleted)
    {
        for (int i = 0; i < goalCards.Count; i++)
        {
            VisualElement goalCard  = goalCards[i];
            Objective goal          = (goalCard.userData as ObjectiveCard).Objective;

            if (goal.IsComplete)
                goalCard.Show(!hideCompleted);
        }
    }

    private void SetNotificationBubble(object sender, object info)
    {
        int goalsToClaim                    = ObjectiveManager.instance.GetNumberOfUnclaimedAndCompleteObjectives(cat);
        VisualElement claimableNotDot       = objectivesButton.Q<VisualElement>("Counter");
        claimableNotDot.Q<Label>().text     = goalsToClaim.ToString();
        claimableNotDot.Show(goalsToClaim > 0);
    }

    #endregion
}
