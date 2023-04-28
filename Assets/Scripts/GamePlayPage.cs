using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GamePlayPage : Page
{
    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   Level   -   The Level to load

        Level level = (Level)args[0];

        BoardCreator.instance.Setup(uiDoc, level);

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            if (!InputController.instance.CanAcceptClick)
                return;

            object[] data = new object[1] { level.LevelCategory };

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
    }

    public override void HidePage()
    {
        
    }

    public override IEnumerator AnimateIn()
    {
        InputController.instance.CanClick(false);

        yield return BoardCreator.instance.AnimateBoardIn();

        InputController.instance.CanClick();
    }

    public override IEnumerator AnimateOut()
    {
        InputController.instance.CanClick(false);

        yield return BoardCreator.instance.AnimateBoardOut();
    }

    #endregion
}
