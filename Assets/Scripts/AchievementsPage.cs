using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AchievementsPage : Page
{
    #region Private Variables

    private VisualElement achievementsButton;
    private VisualElement goalsButton;

    private ScrollView achievementsScroll;
    private ScrollView goalsScroll;

    private List<VisualElement> cards;
    private List<Foldout> goalGroups;

    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        canClick                = false;
        cards                   = new List<VisualElement>();
        goalGroups              = new List<Foldout>();

        achievementsButton      = uiDoc.rootVisualElement.Q<VisualElement>("AchievementsButton");
        goalsButton             = uiDoc.rootVisualElement.Q<VisualElement>("GoalsButton");
        achievementsScroll      = uiDoc.rootVisualElement.Q<ScrollView>("AchievementsScroll");
        goalsScroll             = uiDoc.rootVisualElement.Q<ScrollView>("GoalsScroll");
        
        Toggle hideCompleted    = uiDoc.rootVisualElement.Q<Toggle>("HideCompletedToggle");

        hideCompleted.value     = false;
        hideCompleted.focusable = false;
        hideCompleted.labelElement.AddToClassList("HeaderLabel");
        hideCompleted.labelElement.style.fontSize = 45f;
        hideCompleted.SetMargins(25f, 0f, 25f, 0f);

        VisualElement check = hideCompleted.Q<VisualElement>("unity-checkmark");
        check.parent.style.justifyContent = Justify.FlexEnd;
        check.SetColor(new Color(.8f, .8f, .8f, 1f));
        check.SetBorderWidth(0f);
        check.SetBorderRadius(20f);
        check.ScaleToFit();
        check.SetWidth(70f);
        check.SetHeight(check.style.width);

        achievementsScroll.contentContainer.SetPadding(0f, 8f, 8f, 0f); //8 is the dropshadow size for objective cards
        goalsScroll.contentContainer.SetPadding(0f, 8f, 8f, 0f);

        hideCompleted.RegisterValueChangedCallback<bool>((evt) => ShowHideCompleted(evt.newValue));
        achievementsButton.RegisterCallback<PointerUpEvent>(ShowAchievementsList);
        goalsButton.RegisterCallback<PointerUpEvent>(ShowGoalsList);

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>(null, false));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
    }

    public override IEnumerator AnimateIn()
    {
        yield return null;

        VisualElement sc    = uiDoc.rootVisualElement.Q<VisualElement>("ScrollPanel");
        sc.style.maxHeight  = new StyleLength(sc.resolvedStyle.height);

        SetupAchievementsList();
        SetupGoalsList();

        ShowGoalsList(null);

        if (!UIManager.instance.TopBar.IsShowing)
            UIManager.instance.TopBar.ShowTopBar();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        throw new System.NotImplementedException();
    }

    public override void HidePage()
    {
        this.RemoveObserver(SetNotificationBubble, Notifications.OBJECTIVE_REWARD_CLAIMED);
        this.RemoveObserver(SetNotificationBubble, Notifications.OBJECTIVE_COMPLETE);
        return;
    }

    #endregion

    #region Private Functions

    private void SetupAchievementsList()
    {
        List<Objective> tempList                    = new List<Objective>(ObjectiveManager.instance.GetAllAchievements());

        for (int i = 0; i < tempList.Count; i++)
        {
            Objective objective                     = tempList[i];
            VisualElement achievementCard           = UIManager.instance.ObjectiveCard.Instantiate();
            achievementCard.name                    = objective.ID + " AchivementCard Card";
            achievementCard.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            achievementCard.SetMargins(10f, i != 0, false, i != tempList.Count - 1, false);

            ObjectiveCard controller                = new ObjectiveCard(objective, achievementCard);
            achievementCard.userData                = controller;

            cards.Add(achievementCard);
            achievementsScroll.Add(achievementCard);
        }
    }

    private void SetupGoalsList()
    {
        List<Objective> tempList                    = new List<Objective>(ObjectiveManager.instance.GetAllObjectives());

        //TODO: Need to make sure they're sorted by level category. Probably will sort them this
        //      way in the definitions, but just in case i dont, sort them here

        LevelCategory currCat                       = null;
        Foldout currFold                            = null;

        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i].LevelCategory != currCat)
            {
                currCat                             = tempList[i].LevelCategory;
                currFold                            = new Foldout {text = "a"};
                currFold.contentContainer.name      = currCat.name + " foldout content container";
                currFold.focusable                  = false;
                currFold.contentContainer.SetMargins(0f, 0f, 0f, 0f); //Remove the tab in

                int completed                       = ObjectiveManager.instance.GetCompletedObjectivesForCategory(currCat).Count;
                int total                           = ObjectiveManager.instance.GetObjectivesForCategory(currCat).Count;

                Label foldLabel                     = currFold.Q<Label>();

                foldLabel.AddToClassList("HeaderLabel");
                foldLabel.SetMargins(15f, 0f, 25f, 0f);
                foldLabel.SetBorderColor(Color.black);
                foldLabel.SetBorderWidth(5f, false, false, true, false);

                foldLabel.style.unityTextAlign      = TextAnchor.MiddleLeft;
                foldLabel.text                      = currCat.name;
                foldLabel.focusable                 = false;

                Toggle tog                          = currFold.Q<Toggle>();
                tog.focusable                       = false;

                VisualElement foldArrow             = tog.Q<VisualElement>("unity-checkmark");
                foldArrow.SetWidth(60f);
                foldArrow.SetHeight(foldArrow.style.width);
                foldArrow.BringToFront();

                foldArrow.parent
                    .style.justifyContent           = Justify.SpaceBetween;

                Label completeCount                 = new Label();
                completeCount.AddToClassList("HeaderLabel");
                completeCount.SetMargins(0f);

                completeCount.style.unityTextAlign  = TextAnchor.MiddleRight;
                completeCount.text                  = completed.ToString() + " / " + total.ToString();
                completeCount.focusable             = false;
                completeCount.style.position        = Position.Absolute;
                completeCount.style.right           = 10f + 60f; //10 margin + foldarrow's width
                completeCount.style.alignSelf       = Align.Center;

                VisualElement completeStar          = new VisualElement();
                completeStar.name                   = "CompleteStar";
                completeStar.style.alignSelf        = Align.Center;
                completeStar.SetWidth(60f);
                completeStar.SetHeight(completeStar.style.width);
                completeStar.SetImage(UIManager.instance.StarMedal);
                completeStar.ScaleToFit();

                completeStar.Show(total == completed);
                foldArrow.Show(!completeStar.IsShowing());

                foldArrow.parent.Add(completeStar);
                foldArrow.parent.Add(completeCount);

                goalsScroll.Add(currFold);
            }

            VisualElement goalCard                  = UIManager.instance.ObjectiveCard.Instantiate();
            goalCard.name                           = tempList[i].ID + " GOAL CARD";
            goalCard.SetWidth(new StyleLength(new Length(100f, LengthUnit.Percent)));
            goalCard.SetMargins(10f, i != 0, false, i != tempList.Count - 1, false);

            Objective objective                     = tempList[i];
            ObjectiveCard controller                = new ObjectiveCard(objective, goalCard);
            goalCard.userData                       = controller;

            currFold.Add(goalCard);
            cards.Add(goalCard);
            goalGroups.Add(currFold);
        }

        SetNotificationBubble(null, null);
        this.AddObserver(SetNotificationBubble, Notifications.OBJECTIVE_REWARD_CLAIMED);
        this.AddObserver(SetNotificationBubble, Notifications.OBJECTIVE_COMPLETE);
    }

    private void ShowAchievementsList(PointerUpEvent evt)
    {
        achievementsScroll.Show();
        goalsScroll.Hide();
    }

    private void ShowGoalsList(PointerUpEvent evt)
    {
        achievementsScroll.Hide();
        goalsScroll.Show();
    }

    private void ShowHideCompleted(bool hideCompleted)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            VisualElement goalCard  = cards[i];
            Objective goal          = (goalCard.userData as ObjectiveCard).Objective;

            if (goal.IsComplete)
                goalCard.Show(!hideCompleted);
        }

        if (hideCompleted)
        {
            for (int i = 0; i < goalGroups.Count; i++)
            {
                if (goalGroups[i].contentContainer.Children().FirstOrDefault(x => x.IsShowing()) == null)
                {
                    goalGroups[i].Hide();
                }
            }
        }
        else
        {
            for (int i = 0; i < goalGroups.Count; i++)
            {
                goalGroups[i].Show();
            }
        }
    }

    private void SetNotificationBubble(object sender, object info)
    {
        int goalsToClaim                = ObjectiveManager.instance.GetNumberOfUnclaimedAndCompleteObjectives();
        VisualElement claimableNotDot   = goalsButton.Q<VisualElement>("Counter");
        claimableNotDot.Q<Label>().text = goalsToClaim.ToString();
        claimableNotDot.Show(goalsToClaim > 0);
    }

    #endregion
}
