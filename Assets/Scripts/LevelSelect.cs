using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelSelect : Page
{
    #region Private Variables

    ScrollView levelScroll, objectiveScroll;
    VisualElement levelsButton, objectivesButton;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   - The level category to show levels for

        LevelCategory cat = (LevelCategory)args[0];

        List<Level> levels = cat.GetLevels(); //Resources.LoadAll<Level>("Levels/" + cat.FilePath).ToList();

        levelScroll = uiDoc.rootVisualElement.Q<ScrollView>("LevelScroll");
        levelScroll.contentContainer.style.flexGrow = 1f;

        VisualElement levelScrollContent = levelScroll.contentContainer.Q<VisualElement>("LevelScrollContent");

        for (int i = 0; i < levels.Count; i++)
        {
            for (int test = 0; test < 130; test++)
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

                levelScrollContent.Add(button);
            }
        }

        objectiveScroll = uiDoc.rootVisualElement.Q<ScrollView>("ObjectiveScroll");
        objectiveScroll.contentContainer.style.flexGrow = 1f;
        VisualElement objectiveScrollContent = objectiveScroll.contentContainer.Q<VisualElement>("ObjectiveScrollContent");
        List<Objective> objectives = ObjectiveManager.instance.GetObjectivesForCategory(cat);

        Debug.Log("Found " + objectives.Count + " objectives for " + cat.name);

        for (int i = 0; i < objectives.Count; i++)
        {
            VisualElement card = UIManager.instance.ObjectiveCard.Instantiate();
            card.style.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            card.style.SetMargins(10f, i != 0, false, i != objectives.Count - 1, false);

            ObjectiveCard controller = new ObjectiveCard(objectives[i], card);
            card.userData = controller;

            objectiveScrollContent.Add(card);
        }

        levelsButton = uiDoc.rootVisualElement.Q<VisualElement>("LevelsButton");
        objectivesButton = uiDoc.rootVisualElement.Q<VisualElement>("ObjectivesButton");

        levelsButton.RegisterCallback<PointerDownEvent>(ShowLevels);
        objectivesButton.RegisterCallback<PointerDownEvent>(ShowObjectives);

        UIManager.instance.SetBackground(cat.BackgroundImage, cat.Color);

        ShowLevels(null);
    }

    public override void HidePage()
    {
        levelsButton.UnregisterCallback<PointerDownEvent>(ShowLevels);
        objectivesButton.UnregisterCallback<PointerDownEvent>(ShowObjectives);
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
        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(1f);

        Tween fadeout = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                0f, .33f);

        yield return fadeout.Play().WaitForCompletion();
    }

    #endregion

    #region Private Functions

    private void ShowLevels(PointerDownEvent evt)
    {
        (objectiveScroll as VisualElement).Show(false);
        (levelScroll as VisualElement).Show();

        levelScroll.GoToTop();
    }

    private void ShowObjectives(PointerDownEvent evt)
    {
        (objectiveScroll as VisualElement).Show();
        (levelScroll as VisualElement).Show(false);

        objectiveScroll.GoToTop();
    }

    #endregion
}
