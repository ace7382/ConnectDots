using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelect : Page
{
    #region Private Variables

    ScrollView levelScroll, objectiveScroll, timeAttackScroll;
    VisualElement levelsButton, objectivesButton, timeAttackButton;
    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   - The level category to show levels for

        LevelCategory cat = (LevelCategory)args[0];

        List<Level> levels = cat.GetLevels();

        levelScroll = uiDoc.rootVisualElement.Q<ScrollView>("LevelScroll");
        levelScroll.contentContainer.style.flexGrow = 1f;

        VisualElement levelScrollContent = levelScroll.contentContainer.Q<VisualElement>("LevelScrollContent");

        for (int i = 0; i < levels.Count; i++)
        {
            VisualElement button = UIManager.instance.LevelSelectButton.Instantiate();
            Level lev = levels[i];

            button.Q<VisualElement>("Icon").RemoveFromHierarchy();
            Label num = button.Q<Label>("Number");
            num.style.Show();
            num.text = lev.LevelNumber;

            VisualElement completedIcon = button.Q<VisualElement>("CompletedIcon");
            completedIcon.Show(levels[i].IsComplete);
            button.Q<VisualElement>("LevelSelectButton").SetBorderColor(levels[i].IsComplete ? Color.yellow : Color.clear);

            button.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) =>
            {
                if (!canClick)
                    return;

                canClick = false;

                object[] data = new object[1];
                data[0] = lev;

                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
            });

            levelScrollContent.Add(button);
        }

        objectiveScroll = uiDoc.rootVisualElement.Q<ScrollView>("ObjectiveScroll");
        objectiveScroll.contentContainer.style.flexGrow = 1f;
        VisualElement objectiveScrollContent = objectiveScroll.contentContainer.Q<VisualElement>("ObjectiveScrollContent");
        List<Objective> objectives = ObjectiveManager.instance.GetObjectivesForCategory(cat);

        for (int i = 0; i < objectives.Count; i++)
        {
            VisualElement card = UIManager.instance.ObjectiveCard.Instantiate();
            card.style.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            card.style.SetMargins(10f, i != 0, false, i != objectives.Count - 1, false);

            ObjectiveCard controller = new ObjectiveCard(objectives[i], card);
            card.userData = controller;

            objectiveScrollContent.Add(card);
        }

        timeAttackScroll = uiDoc.rootVisualElement.Q<ScrollView>("TimeAttackScroll");
        timeAttackScroll.contentContainer.style.flexGrow = 1f;
        VisualElement timeAttackScrollContent = timeAttackScroll.contentContainer.Q<VisualElement>("TimeAttackScrollContent");
        
        for (int i = 0; i < cat.TimeAttacks.Count; i++)
        {
            VisualElement card = UIManager.instance.TimeAttackButton.Instantiate();
            card.style.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            card.style.SetMargins(10f, i != 0, false, i != objectives.Count - 1, false);

            //TODO: Make a card controller probably
            card.Q<Label>("DifficultyLabel").text = cat.TimeAttacks[i].difficulty;
            card.Q<Label>("PuzzleCount").text = cat.TimeAttacks[i].numberOfPuzzles.ToString() + " Puzzles";
            card.Q<Label>("StartingTime").text = cat.TimeAttacks[i].totalTimeInSeconds.ToString(); //TODO: time format
            card.Q<Label>("CompletionBonus").text = "+" + cat.TimeAttacks[i].timeAddedOnCompletePuzzle.ToString() + "s per completion";

            int index = i;

            card.RegisterCallback<PointerUpEvent>((evt) =>
            {
                if (!canClick)
                    return;

                canClick = false;

                object[] data = new object[2];
                data[0] = cat;
                data[1] = index;

                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<TimedModePage>(data));
            });

            timeAttackScrollContent.Add(card);
        }

        levelsButton = uiDoc.rootVisualElement.Q<VisualElement>("LevelsButton");
        objectivesButton = uiDoc.rootVisualElement.Q<VisualElement>("ObjectivesButton");
        timeAttackButton = uiDoc.rootVisualElement.Q<VisualElement>("TimeAttackButton");

        levelsButton.RegisterCallback<PointerUpEvent>(ShowLevels);
        objectivesButton.RegisterCallback<PointerUpEvent>(ShowObjectives);
        timeAttackButton.RegisterCallback<PointerUpEvent>(ShowTimeAttacks);

        //UIManager.instance.SetBackground(cat.LevelSelectImage, cat.Color);
        UIManager.instance.SetBackground(cat.LevelSelectImage, cat.Colors[0]);
        if (cat.Colors.Count > 1) UIManager.instance.SetBackgroundShift(cat.Colors);

        canClick = true;

        ShowLevels(null);

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
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
        objectivesButton.UnregisterCallback<PointerUpEvent>(ShowObjectives);
        timeAttackButton.UnregisterCallback<PointerUpEvent>(ShowTimeAttacks);
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

        objectiveScroll.Hide();
        levelScroll.Show();
        timeAttackScroll.Hide();

        levelScroll.GoToTop();

        canClick = true;
    }

    private void ShowObjectives(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        objectiveScroll.Show();
        levelScroll.Hide();
        timeAttackScroll.Hide();

        objectiveScroll.GoToTop();

        canClick = true;
    }

    private void ShowTimeAttacks(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        objectiveScroll.Hide();
        levelScroll.Hide();
        timeAttackScroll.Show();

        timeAttackScroll.GoToTop();
    }

    #endregion
}
