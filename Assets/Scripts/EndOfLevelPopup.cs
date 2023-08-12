using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EndOfLevelPopup : Page
{
    #region Private Variables

    private VisualElement   homeButton;
    private VisualElement   replayButton;
    private VisualElement   nextLevelButton;
    private VisualElement   showButton;
    private ScrollView      coinScroll;
    private Level           nextLevel;
    private bool            canClick;

    //Normal Mode
    private Level           level;

    //Timed Mode
    private LevelCategory   levelCat;
    private int             difficultyIndex;

    #endregion

    #region Private Properties

    private bool            PanelShowing { get { return showButton.transform.position.y > 50f; } }

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //Normal Mode
        //args[0]   -   Dictionary<ColorCategory, int>  -   The coins awarded from the level
        //args[1]   -   Level                           -   The level that was just completed
        //args[2]   -   TimeSpan                        -   The time it took to finish the level

        //Timed Mode
        //args[0]   -   Dictionary<int, int>            -   The coins awarded from the level
        //args[1]   -   null                            -   This null signifies that it was timed mode in the logic below
        //args[2]   -   LevelCategory                   -   The category to play in timed mode
        //args[3]   -   int                             -   The index of the TimeAttackStats in the category
        //args[4]   -   bool                            -   Indicats if Timed Mode was won or not
        //args[5]   -   TimeSpan                        -   The TimeRemaining object from the played timed mode
        //args[6]   -   int                             -   The number of levels completed in timed mode

        Dictionary<ColorCategory, int> 
            coinsWon                    = (Dictionary<ColorCategory, int>)args[0];

        homeButton                      = uiDoc.rootVisualElement.Q<VisualElement>("HomeButton");
        replayButton                    = uiDoc.rootVisualElement.Q<VisualElement>("ReplayButton");
        nextLevelButton                 = uiDoc.rootVisualElement.Q<VisualElement>("NextLevelButton");
        showButton                      = uiDoc.rootVisualElement.Q<VisualElement>("ShowEndScreenButton");

        Label ribbonLabel               = uiDoc.rootVisualElement.Q<Label>("RibbonLabel");
        VisualElement normalModeDetails = uiDoc.rootVisualElement.Q<VisualElement>("NormalMode");
        VisualElement timedModeDetails  = uiDoc.rootVisualElement.Q<VisualElement>("TimedMode");
        VisualElement topIcon           = uiDoc.rootVisualElement.Q<VisualElement>("IconContainerBG").Q<VisualElement>("Icon");

        coinScroll                      = uiDoc.rootVisualElement.Q<ScrollView>("CoinAwardScroll");

        if (coinsWon.Count == 0)
            coinScroll.Hide();
        else
            foreach (KeyValuePair<ColorCategory, int> award in coinsWon)
            {
                VisualElement display = UIManager.instance.CoinDisplay.Instantiate();
                display.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(award.Key));
                display.Q<Label>("AmountLabel").text = award.Value.ToString();

                coinScroll.Add(display);
            }

        if (args[1] != null) //Post normal mode
        {
            level                       = (Level)args[1];
            List<Level> levels          = Resources.LoadAll<Level>("Levels/" + level.LevelCategory.FilePath).ToList();
            int levelIndex              = levels.FindIndex(x => x == level);
            Label timeLabel             = normalModeDetails.Q<Label>("TimeLabel");
            Label objectivesLabel       = normalModeDetails.Q<Label>("ObjectivesLabel");

            timedModeDetails.RemoveFromHierarchy();
            normalModeDetails.Show();

            ribbonLabel.text            = "COMPLETE";
            timeLabel.text              = ((TimeSpan)args[2]).ToString("mm\\:ss\\.fff");

            objectivesLabel.text        = string.Format("{0} / {1}",
                                            ObjectiveManager.instance.GetCompletedObjectivesForCategory(level.LevelCategory).Count.ToString("000"),
                                            ObjectiveManager.instance.GetObjectivesForCategory(level.LevelCategory).Count.ToString("000"));

            if (levelIndex == -1 || levelIndex == levels.Count - 1)
            {
                nextLevelButton.Hide();
                nextLevel               = null;
            }
            else
                nextLevel               = levels[levelIndex + 1];
  
            replayButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(level, evt));
            nextLevelButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(nextLevel, evt));   
        }
        else //Post-Timed Mode
        {
            levelCat                    = (LevelCategory)args[2];
            difficultyIndex             = (int)args[3];
            bool won                    = (bool)args[4];
            TimeSpan remain             = (TimeSpan)args[5];
            int completed               = (int)args[6];
            Label bestLabel             = timedModeDetails.Q<Label>("BestLabel");

            normalModeDetails.RemoveFromHierarchy();
            timedModeDetails.Show();
            nextLevelButton.Hide();

            if (won)
            {
                ribbonLabel.text        = remain.ToString("mm\\:ss\\.fff");
                bestLabel.text          = "Best - " + TimeSpan.FromSeconds(levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds)
                                            .ToString("mm\\:ss\\.fff");

                if (remain.TotalSeconds > levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds)
                {
                    levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds = remain.TotalSeconds;
                    bestLabel.text      = "NEW BEST!";
                }
            }
            else
            {
                ribbonLabel.text        = "Nice Try";
                bestLabel.text          = completed.ToString("000") + " / "
                                            + levelCat.TimeAttacks[difficultyIndex].numberOfPuzzles.ToString("000")
                                            + " Completed";
            }
            
            replayButton.RegisterCallback<PointerUpEvent>(RestartTimedMode);
        }

        showButton.transform.position   = new Vector3(
                                            showButton.transform.position.x
                                            , 150f
                                            , showButton.transform.position.z);
        VisualElement page              = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        page.transform.position         = new Vector3(0f, Screen.height, page.transform.position.z);

        homeButton.RegisterCallback<PointerUpEvent>(GoHome);

        EventCallback<ClickEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            if (!PanelShowing)
            {
                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>
                    (new object[1] { level == null ? levelCat : level.LevelCategory }));
            }
            else
                PageManager.instance.StartCoroutine(HideEndOfLevelScreen());
        };

        showButton.RegisterCallback<PointerUpEvent>(ShowEndOfLevelScreen);

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        canClick = false;
    }

    public override void HidePage()
    {
        homeButton.UnregisterCallback<PointerUpEvent>(GoHome);
        replayButton.UnregisterCallback<PointerUpEvent>((evt) => LoadLevel(level, evt));
        replayButton.UnregisterCallback<PointerUpEvent>(RestartTimedMode);
        nextLevelButton.UnregisterCallback<PointerUpEvent>((evt) => LoadLevel(nextLevel, evt));
        showButton.UnregisterCallback<PointerUpEvent>(ShowEndOfLevelScreen);
    }

    public override IEnumerator AnimateIn()
    {
        //TODO: these can be coinscrol.Q<toparrow> etc i think. Save a few tre traversal steps
        VisualElement topIndicator      = uiDoc.rootVisualElement.Q<VisualElement>("TopArrow");
        VisualElement bottomIndicator   = uiDoc.rootVisualElement.Q<VisualElement>("BottomArrow");
        
        coinScroll.SetBoundIndicators(topIndicator, bottomIndicator);
        coinScroll.verticalScroller.valueChanged += (evt) => {
            coinScroll.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator);
        };

        yield return null; //The coinscroll's bounds need to be set

        coinScroll.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator);

        yield return PanelIn();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        if (PanelShowing)
            yield return PanelOut();
        else
            yield return ButtonOut();
    }

    #endregion

    #region Private Functions

    private void GoHome(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>());
    }

    private void LoadLevel(Level l, PointerUpEvent evt)
    {
        if (!canClick)
            return;

        object[] data = new object[1];
        data[0] = l;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
    }

    private void RestartTimedMode(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        object[] data = new object[2];
        data[0] = levelCat;
        data[1] = difficultyIndex;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<TimedModePage>(data));
    }

    private IEnumerator HideEndOfLevelScreen()
    {
        canClick = false;

        yield return PanelOut();

        yield return ButtonIn();

        canClick = true;
    }

    private void ShowEndOfLevelScreen(PointerUpEvent evt)
    {
        PageManager.instance.StartCoroutine(ShowEndOfLevelScreen());
    }

    private IEnumerator ShowEndOfLevelScreen()
    {
        canClick = false;

        yield return ButtonOut();

        yield return PanelIn();

        canClick = true;
    }

    private IEnumerator ButtonIn()
    {
        Tween buttonIn = DOTween.To(() => showButton.transform.position,
                x => showButton.transform.position = x,
                new Vector3(showButton.transform.position.x, 0f, showButton.transform.position.z), .65f)
                .SetEase(Ease.OutQuart);

        yield return buttonIn.WaitForCompletion();
    }

    private IEnumerator ButtonOut()
    {
        Tween buttonOut = DOTween.To(() => showButton.transform.position,
                x => showButton.transform.position = x,
                new Vector3(
                    showButton.transform.position.x
                    , 150f
                    , showButton.transform.position.z), .65f)
                .SetEase(Ease.OutQuart);

        yield return buttonOut.WaitForCompletion();
    }

    private IEnumerator PanelIn()
    {
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyIn = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, 0f, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();
    }

    private IEnumerator PanelOut()
    {
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyOut = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, Screen.height, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyOut.WaitForCompletion();
    }

    #endregion
}
