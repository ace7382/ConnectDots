using DG.Tweening;
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
        //args[0]   -   Dictionary<int, int>    -   The coins awarded from the level
        //args[1]   -   Level                   -   The level that was just completed
        //args[2]   -   LevelCategory           -   The category to play in timed mode
        //args[3]   -   int                     -   The index of the TimeAttackStats in the category


        Dictionary<int, int> coinsWon = (Dictionary<int, int>)args[0];

        homeButton      = uiDoc.rootVisualElement.Q<VisualElement>("HomeButton");
        replayButton    = uiDoc.rootVisualElement.Q<VisualElement>("ReplayButton");
        nextLevelButton = uiDoc.rootVisualElement.Q<VisualElement>("NextLevelButton");
        showButton      = uiDoc.rootVisualElement.Q<VisualElement>("ShowEndScreenButton");

        ScrollView coinScroll = uiDoc.rootVisualElement.Q<ScrollView>("CoinAwardScroll");

        foreach (KeyValuePair<int, int> award in coinsWon)
        {
            VisualElement display = UIManager.instance.CoinDisplay.Instantiate();
            display.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(award.Key));
            display.Q<Label>("AmountLabel").text = award.Value.ToString();

            coinScroll.Add(display);
        }

        if (args[1] != null) //Post normal mode
        {
            level = (Level)args[1];

            List<Level> levels = Resources.LoadAll<Level>("Levels/" + level.LevelCategory.FilePath).ToList();

            int levelIndex = levels.FindIndex(x => x == level);

            if (levelIndex == -1 || levelIndex == levels.Count - 1)
            {
                nextLevelButton.style.Hide();
                nextLevel = null;
            }
            else
                nextLevel = levels[levelIndex + 1];
  
            replayButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(level, evt));
            nextLevelButton.RegisterCallback<PointerUpEvent>((evt) => LoadLevel(nextLevel, evt));
        }
        else //Post-Timed Mode
        {
            levelCat = (LevelCategory)args[2];
            difficultyIndex = (int)args[3];

            nextLevelButton.Hide();

            replayButton.RegisterCallback<PointerUpEvent>(RestartTimedMode);
        }

        showButton.transform.position = new Vector3(showButton.transform.position.x, 100f, showButton.transform.position.z);
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        page.transform.position = new Vector3(0f, Screen.height, page.transform.position.z);

        homeButton.RegisterCallback<PointerUpEvent>(GoHome);

        EventCallback<PointerDownEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            if (!PanelShowing) //(showButton.transform.position.y < 50f)
            {
                PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>
                    (new object[1] { level == null ? levelCat : level.LevelCategory }));
            }
            else
                PageManager.instance.StartCoroutine(HideEndOfLevelScreen());
        };

        showButton.RegisterCallback<PointerUpEvent>(ShowEndOfLevelScreen);

        VisualElement topIndicator = uiDoc.rootVisualElement.Q<VisualElement>("TopArrow");
        VisualElement bottomIndicator = uiDoc.rootVisualElement.Q<VisualElement>("BottomArrow");

        coinScroll.SetBoundIndicators(topIndicator, bottomIndicator);

        coinScroll.verticalScroller.valueChanged += (evt) => {
            coinScroll.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator); 
        };

        //TODO: This better. it needs 1 frame to get it's bounds. I think execute later
        //      will at minimum be the next frame, but i'm not positive. Probably can
        //      move this into animate in's coroutine
        coinScroll.schedule.Execute(() => 
            coinScroll.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator)
        ).ExecuteLater(1); //This is needed bc the bounds of the scroll aren't set yet

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        canClick = false;

        //UIManager.instance.TopBar.ShowTopBar(false);
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
                new Vector3(showButton.transform.position.x, 100f, showButton.transform.position.z), .65f)
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
