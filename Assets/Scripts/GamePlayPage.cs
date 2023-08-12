using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GamePlayPage : Page
{
    #region Private Variables

    private Level currentLevel;
    private Board currentBoard;
    private PowerupController powerups;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   Level   -   The Level to load

        currentLevel = (Level)args[0];

        currentBoard = new Board(currentLevel, uiDoc.rootVisualElement);

        EventCallback<ClickEvent> backbuttonAction = (evt) =>
        {
            if (!currentBoard.CanClick)
                return;

            object[] data = new object[1] { currentLevel.LevelCategory };

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);

        powerups = new PowerupController(uiDoc.rootVisualElement.Q<VisualElement>("PowerupUI"), false, currentBoard);

        this.AddObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);
    }

    public override void HidePage()
    {
        currentBoard.UnregisterListeners();
        this.RemoveObserver(BoardComplete, Notifications.BOARD_COMPLETE, currentBoard);
        powerups.Unregister();
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
        powerups.SlideOut().Play();

        yield return currentBoard.LevelCompleteAnimation();
    }

    #endregion

}
