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

    private VisualElement       homeButton;
    private VisualElement       replayButton;
    private VisualElement       nextLevelButton;
    private VisualElement       showButton;
    private Level               nextLevel;
    private bool                canClick;

    private Dictionary
        <ColorCategory, int>    baseCoinsWon;

    private VisualElement[]     expDisplays;
        
    //Normal Mode
    private Level               level;

    //Timed Mode
    private LevelCategory       levelCat;
    private int                 difficultyIndex;

    #endregion

    #region Private Properties

    private bool PanelShowing { get { return showButton.transform.position.y > 50f; } }

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

        baseCoinsWon                        = (Dictionary<ColorCategory, int>)args[0];

        VisualElement container             = uiDoc.rootVisualElement.Q<VisualElement>("Container");

        homeButton                          = container.Q<VisualElement>("HomeButton");
        replayButton                        = container.Q<VisualElement>("ReplayButton");
        nextLevelButton                     = container.Q<VisualElement>("NextLevelButton");
        showButton                          = uiDoc.rootVisualElement.Q<VisualElement>("ShowEndScreenButton");

        Label ribbonLabel                   = container.Q<Label>("RibbonLabel");
        VisualElement normalModeDetails     = container.Q<VisualElement>("NormalMode");
        VisualElement timedModeDetails      = container.Q<VisualElement>("TimedMode");

        VisualElement[] segmentDisplays     = new VisualElement[7]
        {
            container.Q<VisualElement>("BWSegmentsEarned")
            , container.Q<VisualElement>("RedSegmentsEarned")
            , container.Q<VisualElement>("PurpleSegmentsEarned")
            , container.Q<VisualElement>("BlueSegmentsEarned")
            , container.Q<VisualElement>("GreenSegmentsEarned")
            , container.Q<VisualElement>("YellowSegmentsEarned")
            , container.Q<VisualElement>("OrangeSegmentsEarned")
        };

        for (int i = 0; i < CurrencyManager.instance.SegmentColorCount; i++)
        {
            ColorCategory current           = (ColorCategory)i;

            if (baseCoinsWon.ContainsKey(current))
            {
                segmentDisplays[i].Show();

                //Segment Award
                segmentDisplays[i].Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(current));
                segmentDisplays[i].Q<Label>("TilesColored").text    = baseCoinsWon[current].ToString();

                int bonus                   = 0;

                segmentDisplays[i].Q<Label>("BonusMultiplier").text = bonus.ToString();
                segmentDisplays[i].Q<Label>("TotalEarned").text     = (bonus + baseCoinsWon[current]).ToString();
            }
            else
            {
                segmentDisplays[i].Hide();
            }
        }

        if (args[1] != null) //Post normal mode
        {
            level = (Level)args[1];
            List<Level> levels              = Resources.LoadAll<Level>("Levels/" + level.LevelCategory.FilePath).ToList();
            int levelIndex                  = levels.FindIndex(x => x == level);
            Label timeLabel                 = normalModeDetails.Q<Label>("TimeLabel");
            Label objectivesLabel           = normalModeDetails.Q<Label>("ObjectivesLabel");

            timedModeDetails.RemoveFromHierarchy();
            normalModeDetails.Show();

            ribbonLabel.text                = "COMPLETE";
            timeLabel.text                  = ((TimeSpan)args[2]).ToString("mm\\:ss\\.fff");

            objectivesLabel.text            = string.Format("{0} / {1}",
                                                ObjectiveManager.instance.GetCompletedObjectivesForCategory(level.LevelCategory).Count.ToString("000"),
                                                ObjectiveManager.instance.GetObjectivesForCategory(level.LevelCategory).Count.ToString("000"));

            if (levelIndex == -1 || levelIndex == levels.Count - 1)
            {
                nextLevelButton.Hide();
                nextLevel = null;
            }
            else
                nextLevel = levels[levelIndex + 1];

            replayButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(level, evt));
            nextLevelButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(nextLevel, evt));
        }
        else //Post-Timed Mode
        {
            levelCat                        = (LevelCategory)args[2];
            difficultyIndex                 = (int)args[3];
            bool won                        = (bool)args[4];
            TimeSpan remain                 = (TimeSpan)args[5];
            int completed                   = (int)args[6];
            Label bestLabel                 = timedModeDetails.Q<Label>("BestLabel");

            normalModeDetails.RemoveFromHierarchy();
            timedModeDetails.Show();
            nextLevelButton.Hide();

            if (won)
            {
                ribbonLabel.text            = remain.ToString("mm\\:ss\\.fff");
                bestLabel.text              = "Best - " + TimeSpan.FromSeconds(levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds)
                                                .ToString("mm\\:ss\\.fff");

                if (remain.TotalSeconds > levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds)
                {
                    levelCat.TimeAttacks[difficultyIndex].bestTimeInSeconds = remain.TotalSeconds;
                    bestLabel.text          = "NEW BEST!";
                }
            }
            else
            {
                ribbonLabel.text            = "Nice Try";
                bestLabel.text              = completed.ToString("000") + " / "
                                                + levelCat.TimeAttacks[difficultyIndex].numberOfPuzzles.ToString("000")
                                                + " Completed";
            }

            replayButton.RegisterCallback<PointerUpEvent>(RestartTimedMode);
        }

        showButton.transform.position       = new Vector3(
                                                showButton.transform.position.x
                                                , 150f
                                                , showButton.transform.position.z);
        VisualElement page                  = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        page.transform.position             = new Vector3(0f, Screen.height, page.transform.position.z);

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
        yield return null; //The coinscroll's bounds need to be set

        ShowEXPBars();

        yield return PanelIn();

        FillEXPBars();

        canClick            = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick            = false;

        if (PanelShowing)
            yield return PanelOut();
        else
            yield return ButtonOut();
    }

    #endregion

    #region Private Functions

    private void ShowEXPBars()
    {
        VisualElement container     = uiDoc.rootVisualElement.Q<VisualElement>("Container");
        expDisplays                 = new VisualElement[7]
        {
            container.Q<VisualElement>("BlackAndWhiteEXP")
            , container.Q<VisualElement>("RedEXP")
            , container.Q<VisualElement>("PurpleEXP")
            , container.Q<VisualElement>("BlueEXP")
            , container.Q<VisualElement>("GreenEXP")
            , container.Q<VisualElement>("YellowEXP")
            , container.Q<VisualElement>("OrangeEXP")
        };

        int[] expEarned             = new int[7] {0,0,0,0,0,0,0};

        foreach (KeyValuePair<ColorCategory, int> a in baseCoinsWon)
        {
            expEarned[(int)a.Key]   = a.Value;
        }

        for (int i = 0; i < expEarned.Length; i++)
        {
            if (expEarned[i] <= 0)
            {
                expDisplays[i].Hide();
                continue;
            }

            expDisplays[i].Show();
            ColorCategory current       = (ColorCategory)i;

            Label expBarLabel           = expDisplays[i].Q<Label>("Label");
            Label currentLabel          = expDisplays[i].Q<Label>("CurrentLevel");
            Label nextLabel             = expDisplays[i].Q<Label>("NextLevel");
            Label earnedEXPLabel        = expDisplays[i].Q<Label>("CurrentProgress");
            VisualElement parentVE      = expDisplays[i].Q<VisualElement>("BG");

            expBarLabel.text            = current.Name();
            currentLabel.text           = ProfileManager.instance.GetEXPLevel(current).ToString();
            nextLabel.text              = ProfileManager.instance.GetNextEXPLevel(current).ToString();
            earnedEXPLabel.text         = "+" + expEarned[i] + " EXP"; //TODO: Determine if EXP should be calculated fully in profile manager or here etc.

            Vector2 barLeftOrigin       = new Vector2(parentVE.WorldToLocal(currentLabel.worldBound.center).x, 0f);
            Vector2 barRightOrigin      = new Vector2(parentVE.WorldToLocal(nextLabel.worldBound.center).x, 0f);

            Debug.Log(expBarLabel.text + ":\nleft origin: " + barLeftOrigin + "\nRight origin: " + barRightOrigin);

            Color progressBarColor      = current == ColorCategory.BLACK_AND_WHITE ?
                                                Color.black : UIManager.instance.GetColor(current);

            UIToolkitCircle leftDot     = new UIToolkitCircle(barLeftOrigin, 35f, progressBarColor);
            UIToolkitCircle rightDot    = new UIToolkitCircle(barRightOrigin, 35f, progressBarColor);

            float dotDistance           = barRightOrigin.x - barLeftOrigin.x;

            Vector2 progressBarStop     = new Vector2(
                                            barLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(current) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(current))
                                                * dotDistance)
                                            , barLeftOrigin.y
                                            );

            List<Vector2> barPoints     = new List<Vector2>() { barLeftOrigin, progressBarStop };
            UIToolkitLine progLine      = new UIToolkitLine(barPoints, 20f, progressBarColor, LineCap.Round);

            parentVE.Add(leftDot);
            parentVE.Add(rightDot);
            parentVE.Add(progLine);
        }
    }

    private void FillEXPBars()
    {
        for (int i = 0; i < expDisplays.Length; i++)
        {
            if (!expDisplays[i].IsShowing())
                continue;

            ColorCategory currentCC     = (ColorCategory)i;
            UIToolkitLine currentLine   = expDisplays[i].Q<UIToolkitLine>();
            Label currentLabel          = expDisplays[i].Q<Label>("CurrentLevel");
            Label nextLabel             = expDisplays[i].Q<Label>("NextLevel");
            Label earnedEXPLabel        = expDisplays[i].Q<Label>("CurrentProgress");
            VisualElement parentVE      = expDisplays[i].Q<VisualElement>("BG");

            //Sequence fillSeq            = DOTween.Sequence();
            float animTime              = 1f;

            //TODO: Determine if EXP should be calculated fully in profile manager or here etc.
            if (ProfileManager.instance.AddEXP(currentCC, baseCoinsWon[currentCC]))
            {
                Vector2 levelUpDest     = new Vector2(
                                            parentVE.WorldToLocal(nextLabel.worldBound.center).x
                                            , 0f
                                        );

                currentLine.DrawTowardNewPoint_Tween(levelUpDest, animTime)
                    .OnComplete(() =>
                        {
                            currentLine.RemoveFromHierarchy();
                            currentLine             = null;

                            currentLabel.text       = ProfileManager.instance.GetEXPLevel(currentCC).ToString();
                            nextLabel.text          = ProfileManager.instance.GetNextEXPLevel(currentCC).ToString();

                            Vector2 newLineStart    = new Vector2(
                                                        parentVE.WorldToLocal(currentLabel.worldBound.center).x
                                                        , 0f
                                                    );

                            UIToolkitLine newLine   = new UIToolkitLine(
                                                        new List<Vector2>() { newLineStart }
                                                        , 20f
                                                        , currentCC == ColorCategory.BLACK_AND_WHITE ?
                                                            Color.black : UIManager.instance.GetColor(currentCC)
                                                        , LineCap.Round
                                                    );

                            parentVE.Add(newLine);

                            float dotDistance       = parentVE.WorldToLocal(nextLabel.worldBound.center).x
                                                        - parentVE.WorldToLocal(currentLabel.worldBound.center).x;

                            Vector2 destination     = new Vector2(
                                                        parentVE.WorldToLocal(currentLabel.worldBound.center).x
                                                        + (
                                                            (float)ProfileManager.instance.GetCurrentEXP(currentCC)
                                                            / (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(currentCC))
                                                            * dotDistance)
                                                        , 0f
                                                    );

                            newLine.DrawTowardNewPoint_Tween(destination, animTime).SetEase(Ease.OutQuart).Play();
                        }
                    )
                    .Play();
            }
            else //This is 2 fully separate sections (that repeat each other) bc I can't seem to get an OnComplete to run correctly mid-sequence
            {
                float dotDistance       = parentVE.WorldToLocal(nextLabel.worldBound.center).x
                                            - parentVE.WorldToLocal(currentLabel.worldBound.center).x;

                Vector2 destination     = new Vector2(
                                            parentVE.WorldToLocal(currentLabel.worldBound.center).x
                                            + (
                                                (float)ProfileManager.instance.GetCurrentEXP(currentCC)
                                                / (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(currentCC))
                                                * dotDistance)
                                            , 0f
                                        );

                currentLine.DrawTowardNewPoint_Tween(destination, animTime).SetEase(Ease.OutQuart).Play();
            }
        }
    }

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

        object[] data       = new object[1];
        data[0]             = l;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
    }

    private void RestartTimedMode(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        object[] data       = new object[2];
        data[0]             = levelCat;
        data[1]             = difficultyIndex;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<TimedModePage>(data));
    }

    private IEnumerator HideEndOfLevelScreen()
    {
        canClick            = false;

        yield return PanelOut();

        yield return ButtonIn();

        canClick            = true;
    }

    private void ShowEndOfLevelScreen(PointerUpEvent evt)
    {
        PageManager.instance.StartCoroutine(ShowEndOfLevelScreen());
    }

    private IEnumerator ShowEndOfLevelScreen()
    {
        canClick            = false;

        yield return ButtonOut();

        yield return PanelIn();

        canClick            = true;
    }

    private IEnumerator ButtonIn()
    {
        Tween buttonIn      = DOTween.To(() => showButton.transform.position,
                                x => showButton.transform.position = x,
                                new Vector3(
                                    showButton.transform.position.x
                                    , 0f
                                    , showButton.transform.position.z)
                                , .65f)
                                .SetEase(Ease.OutQuart);

        yield return buttonIn.WaitForCompletion();
    }

    private IEnumerator ButtonOut()
    {
        Tween buttonOut     = DOTween.To(() => showButton.transform.position,
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
        VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        Tween flyIn         = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, 0f, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();
    }

    private IEnumerator PanelOut()
    {
        VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        Tween flyOut        = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, Screen.height, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyOut.WaitForCompletion();
    }

    #endregion
}