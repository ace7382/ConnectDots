using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GamePlayPage : Page
{
    #region Private Variables

    private Level currentLevel;
    private Board currentBoard;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   Level   -   The Level to load

        currentLevel = (Level)args[0];

        currentBoard = new Board(currentLevel, uiDoc.rootVisualElement);

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            if (!currentBoard.CanClick)
                return;

            object[] data = new object[1] { currentLevel.LevelCategory };

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);

        this.AddObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);
    }

    public override void HidePage()
    {
        currentBoard.UnregisterListeners();
        this.RemoveObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);
    }

    public override IEnumerator AnimateIn()
    {
        yield return currentBoard.AnimateBoardIn();
    }

    public override IEnumerator AnimateOut()
    {
        yield return currentBoard.AnimateBoardOut();
    }

    #endregion

    #region Private Functions

    private void BoardComplete(object sender, object info)
    {
        PageManager.instance.StartCoroutine(BoardComplete());
    }

    private IEnumerator BoardComplete()
    {
        yield return currentBoard.LevelCompleteAnimation();
    }

    #endregion

}
