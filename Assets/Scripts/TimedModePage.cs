using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TimedModePage : Page
{
    #region Private Variables

    private LevelCategory currentCategory;
    private int settingsIndex;
    private LevelCategory.TimeAttackStats settings;

    private Board currentBoard;
    private List<Level> levelsRemaining;
    private List<Level> completedLevels;

    private Label boardCounter;
    private Label timerLabel;
    private TimeSpan timeRemaining;
    private IEnumerator timerCoroutine;

    private Color originalColor;
    private Tween bgFlash;
    private Tween timerFlash;

    private double lowTimeThreshold;

    private Dictionary<int, int> coinsWon;

    #endregion

    #region Public Properties

    public bool LowTime
    { 
        //get { return lowTime; }
        get { return timeRemaining.TotalSeconds < lowTimeThreshold; }
        private set
        {
            if (value) //if starting LowTime
            {
                if (bgFlash == null)
                    StartBGFlash();
            }
            else //if ending LowTime
            {
                if (bgFlash != null)
                {
                    StopBGFlash();
                }
            }
        }
    }

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   -   The category to play in timed mode
        //args[1]   -   int             -   The index of the TimeAttackStats in the category

        currentCategory = (LevelCategory)args[0];
        settingsIndex = (int)args[1];

        originalColor = UIManager.instance.GetBackgroundColor();

        settings = currentCategory.TimeAttacks[settingsIndex];

        List<Level> temp = new List<Level>(currentCategory.GetLevels());
        temp.Shuffle();

        levelsRemaining = new List<Level>();
        completedLevels = new List<Level>();

        for (int i = 0; i < settings.numberOfPuzzles; i++)
            levelsRemaining.Add(temp[i]);

        currentBoard = new Board(levelsRemaining[levelsRemaining.Count - 1], uiDoc.rootVisualElement);
        this.AddObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            //TODO: Pause screen

            if (!currentBoard.CanClick)
                return;

            object[] data = new object[1] { currentCategory };

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);

        boardCounter = uiDoc.rootVisualElement.Q<Label>("BoardCounter");
        SetBoardCounter();

        timeRemaining = TimeSpan.FromSeconds(settings.totalTimeInSeconds);
        lowTimeThreshold = settings.totalTimeInSeconds <= 30f ? 10f : 30f;

        timerLabel = uiDoc.rootVisualElement.Q<Label>("Timer");
        int totalminutes = timeRemaining.Hours * 60 + timeRemaining.Minutes;
        timerLabel.text = string.Format("{0}:{1}", totalminutes.ToString("00"), timeRemaining.Seconds.ToString("00"));

        coinsWon = new Dictionary<int, int>();

        //TODO: Remove this
        bool test = true;
        timerLabel.RegisterCallback<PointerDownEvent>((e) =>
        {
            //AddTime(test ? 10f : -5f);
            AddTime(-20f);
            test = !test;
        });
    }

    public override void HidePage()
    {
        this.RemoveObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);
    }

    public override IEnumerator AnimateIn()
    {
        boardCounter.Hide();
        timerLabel.Hide();

        yield return CountdownToStart();

        boardCounter.Show();
        timerLabel.Show();
        currentBoard.BoardInWithoutAnimation();

        timerCoroutine = UpdateTimer();
        PageManager.instance.StartCoroutine(timerCoroutine);
    }

    public override IEnumerator AnimateOut()
    {
        //Kill timer update coroutine
        PageManager.instance.StopCoroutine(timerCoroutine);
        timerCoroutine = null;

        //set bg to original color
        if (bgFlash != null)
            bgFlash.Kill();
        UIManager.instance.SetBackground(originalColor);

        yield return null;
    }

    #endregion

    #region Private Functions

    private IEnumerator CountdownToStart()
    {
        VisualElement countdownContainer = uiDoc.rootVisualElement.Q<VisualElement>("CountdownContainer");
        Label a     = countdownContainer.Q<Label>("3");
        Label a1    = countdownContainer.Q<Label>("31");
        Label a2    = countdownContainer.Q<Label>("32");
        Label a3    = countdownContainer.Q<Label>("33");
        Label b     = countdownContainer.Q<Label>("2");
        Label b1    = countdownContainer.Q<Label>("21");
        Label b2    = countdownContainer.Q<Label>("22");
        Label b3    = countdownContainer.Q<Label>("23");
        Label c     = countdownContainer.Q<Label>("1");
        Label c1    = countdownContainer.Q<Label>("11");
        Label c2    = countdownContainer.Q<Label>("12");
        Label c3    = countdownContainer.Q<Label>("13");
        Label start = countdownContainer.Q<Label>("Start");

        start.Hide();
        WaitForSeconds w = new WaitForSeconds(.25f);
        WaitForSeconds w4 = new WaitForSeconds(1f);

        yield return w4;
        a.style.color = Color.green;
        yield return w;
        a1.style.color = Color.green;
        yield return w;
        a2.style.color = Color.green;
        yield return w;
        a3.style.color = Color.green;
        yield return w;
        b.style.color = Color.green;
        yield return w;
        b1.style.color = Color.green;
        yield return w;
        b2.style.color = Color.green;
        yield return w;
        b3.style.color = Color.green;
        yield return w;
        c.style.color = Color.green;
        yield return w;
        c1.style.color = Color.green;
        yield return w;
        c2.style.color = Color.green;
        yield return w;
        c3.style.color = Color.green;
        yield return w;

        a           .Hide();
        a1          .Hide();
        a2          .Hide();
        a3          .Hide();
        b           .Hide();
        b1          .Hide();
        b2          .Hide();
        b3          .Hide();
        c           .Hide();
        c1          .Hide();
        c2          .Hide();
        c3          .Hide();
        start       .Hide();

        start.style.color = Color.green;
        start.Show();

        yield return w4;

        countdownContainer.Hide();
    }

    private IEnumerator UpdateTimer()
    {
        while (timeRemaining.Ticks > 0)
        {
            //TODO: Add a cap either directly or just through design. Don't feel like formatting
            //      this for every amount of time

            timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(Time.deltaTime));

            int totalminutes = timeRemaining.Hours * 60 + timeRemaining.Minutes; 

            timerLabel.text = string.Format("{0}:{1}", totalminutes.ToString("00"), timeRemaining.Seconds.ToString("00"));

            LowTime = timeRemaining.TotalSeconds < lowTimeThreshold;

            if (bgFlash != null)
            {
                bgFlash.timeScale = (float)(2d - (timeRemaining.TotalSeconds / lowTimeThreshold));

                Debug.Log(bgFlash.timeScale);
            }

            yield return null;
        }

        RoundOver(false);
    }

    private void SetupNewBoard()
    {
        this.RemoveObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);

        currentBoard = new Board(levelsRemaining[levelsRemaining.Count - 1], uiDoc.rootVisualElement);

        this.AddObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);

        currentBoard.BoardInWithoutAnimation();
    }

    private void BoardComplete(object sender, object info)
    {
        PageManager.instance.StartCoroutine(BoardComplete());
    }

    private IEnumerator BoardComplete()
    {
        foreach(KeyValuePair<int, int> award in currentBoard.SpawnCoinsOnBoardComplete())
        {
            if (coinsWon.ContainsKey(award.Key))
                coinsWon[award.Key] += award.Value;
            else
                coinsWon.Add(award.Key, award.Value);
        }

        VisualElement boardVE = uiDoc.rootVisualElement.Q<VisualElement>("Board");

        Tween shrinkBoard = DOTween.To(() => boardVE.transform.scale,
            x => boardVE.transform.scale = x,
            Vector3.zero, .3f).SetEase(Ease.InQuad);

        yield return shrinkBoard.WaitForCompletion();

        completedLevels.Add(levelsRemaining[levelsRemaining.Count - 1]);
        levelsRemaining.RemoveAt(levelsRemaining.Count - 1);

        if (levelsRemaining.Count > 0)
        {
            SetBoardCounter();

            AddTime(settings.timeAddedOnCompletePuzzle);

            boardVE.Clear();
            boardVE.transform.scale = Vector3.one;
            currentBoard.UnregisterListeners();

            SetupNewBoard();
        }
        else
        {
            RoundOver(true);
        }
    }
    
    private void SetBoardCounter()
    {
        boardCounter.text = String.Format("{0} / {1}", (completedLevels.Count + 1).ToString("000")
            , settings.numberOfPuzzles.ToString("000"));
    }

    private void StartBGFlash()
    {
        bgFlash = DOTween.To(() => UIManager.instance.GetBackgroundColor(),
            x => UIManager.instance.SetBackground(x),
            Color.red, .6f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo)
            .OnKill(() => UIManager.instance.SetBackground(originalColor))
            .Play();
    }

    private void StopBGFlash()
    {
        bgFlash.Kill();

        //bgFlash = DOTween.To(() => UIManager.instance.GetBackgroundColor(),
        //    x => UIManager.instance.SetBackground(x),
        //    originalColor, .4f).SetEase(Ease.Linear).Play().OnComplete(() => { bgFlash = null; });
    }

    private void AddTime(float timeToAdd)
    {
        FlashTimer(timeToAdd > 0f);

        timeRemaining = timeRemaining.Add(TimeSpan.FromSeconds(timeToAdd));
    }

    private void FlashTimer(bool timeIncreasing = true)
    {
        if (timerFlash != null)
            timerFlash.Kill();

        timerFlash = DOTween.To(() => timerLabel.style.color.value,
            x => timerLabel.style.color = new StyleColor(x),
            timeIncreasing ? Color.green : Color.red, .15f)
            .SetEase(Ease.Flash)
            .SetLoops(4)
            .OnKill(() => { timerLabel.style.color = new StyleColor(Color.black);  timerFlash = null; })
            .Play();
    }

    private void RoundOver(bool won)
    {
        StopBGFlash();
        PageManager.instance.StopCoroutine(timerCoroutine);

        object[] data = new object[4];
        data[0] = coinsWon;
        data[1] = null; //This indicates that it's a post a timed mode round
        data[2] = currentCategory;
        data[3] = settingsIndex;

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<EndOfLevelPopup>(data));
    }

    #endregion
}
