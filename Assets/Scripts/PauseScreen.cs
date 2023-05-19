using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseScreen : Page
{
    #region Private Variables

    private bool canClick;

    private EventCallback<PointerDownEvent> previousBackButtonAction;

    #endregion

    #region Inheritted Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   -   The LevelCategory currently being played in Timed Mode

        LevelCategory category      = (LevelCategory)args[0];
        VisualElement returnButton  = uiDoc.rootVisualElement.Q<VisualElement>("ReturnButton");
        VisualElement exitButton    = uiDoc.rootVisualElement.Q<VisualElement>("ExitButton");

        EventCallback<PointerUpEvent> exitButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            object[] data = new object[1] { category };

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<LevelSelect>(data));
        };

        exitButton.RegisterCallback<PointerUpEvent>(exitButtonAction);

        previousBackButtonAction    = UIManager.instance.TopBar.GetCurrentBackButtonEvent();

        EventCallback<PointerDownEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);
        returnButton.RegisterCallback<PointerDownEvent>(backButtonAction);

        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        page.transform.position = new Vector3(0f, Screen.height, page.transform.position.z);

        this.PostNotification(Notifications.PAUSE_GAME);
    }

    public override IEnumerator AnimateIn()
    {
        canClick            = false;

        VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyIn         = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, 0f, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();

        canClick            = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick            = false;

        VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyOut        = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, Screen.height, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyOut.WaitForCompletion();
    }

    public override void HidePage()
    {
        UIManager.instance.TopBar.UpdateBackButtonOnClick(previousBackButtonAction);

        this.PostNotification(Notifications.UNPAUSE_GAME);
    }

    #endregion

    #region Private Functions


    #endregion
}
