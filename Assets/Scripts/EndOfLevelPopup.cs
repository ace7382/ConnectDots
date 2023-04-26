using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EndOfLevelPopup : Page
{
    #region Private Variables

    VisualElement   homeButton;
    VisualElement   replayButton;
    VisualElement   nextLevelButton;
    Level           nextLevel;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   Dictionary<int, int>    -   The coins awarded from the level

        Dictionary<int, int> coinsWon = (Dictionary<int, int>)args[0];

        homeButton = uiDoc.rootVisualElement.Q<VisualElement>("HomeButton");
        replayButton = uiDoc.rootVisualElement.Q<VisualElement>("ReplayButton");
        nextLevelButton = uiDoc.rootVisualElement.Q<VisualElement>("NextLevelButton");

        List<Level> levels = Resources.LoadAll<Level>("Levels/" + BoardCreator.instance.CurrentLevel.LevelCategory.FilePath).ToList();

        int levelIndex = levels.FindIndex(x => x == BoardCreator.instance.CurrentLevel);

        if (levelIndex == -1 || levelIndex == levels.Count - 1)
        {
            nextLevelButton.style.Hide();
            nextLevel = null;
        }
        else
            nextLevel = levels[levelIndex + 1];

        foreach (KeyValuePair<int, int> coins in coinsWon)
        {
            CurrencyManager.instance.AddCurrency(coins.Key, coins.Value);
        }

        homeButton.RegisterCallback<PointerDownEvent>(GoHome);
        replayButton.RegisterCallback<PointerDownEvent>((evt) => LoadLevel(BoardCreator.instance.CurrentLevel, evt));
        nextLevelButton.RegisterCallback<PointerDownEvent>((evt) => LoadLevel(nextLevel, evt));
    }

    public override void HidePage()
    {
        homeButton.UnregisterCallback<PointerDownEvent>(GoHome);
        replayButton.UnregisterCallback<PointerDownEvent>((evt) => LoadLevel(BoardCreator.instance.CurrentLevel, evt));
        nextLevelButton.UnregisterCallback<PointerDownEvent>((evt) => LoadLevel(nextLevel, evt));
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyIn = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, 0f, page.transform.position.z), .75f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();
    }

    public override IEnumerator AnimateOut()
    {
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        
        Tween flyOut = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, Screen.height, page.transform.position.z), .45f)
                                .SetEase(Ease.OutQuart);

        yield return flyOut.WaitForCompletion();
    }

    #endregion

    #region Private Functions

    private void GoHome(PointerDownEvent evt)
    {
        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>());
    }

    private void LoadLevel(Level l, PointerDownEvent evt)
    {
        object[] data = new object[1];
        data[0] = l;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<GamePlayPage>(data));
    }

    #endregion
}
