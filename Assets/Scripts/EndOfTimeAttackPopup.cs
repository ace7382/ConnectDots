using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EndOfTimeAttackPopup : Page
{
    #region Private Variables

    private LevelCategory cat;
    private int difficultyIndex;

    private VisualElement homeButton;
    private VisualElement replayButton;
    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   -   The category to play in timed mode
        //args[1]   -   int             -   The index of the TimeAttackStats in the category

        cat = (LevelCategory)args[0];
        difficultyIndex = (int)args[1];

        homeButton = uiDoc.rootVisualElement.Q<VisualElement>("HomeButton");
        replayButton = uiDoc.rootVisualElement.Q<VisualElement>("ReplayButton");

        homeButton.RegisterCallback<PointerUpEvent>(GoHome);
        replayButton.RegisterCallback<PointerUpEvent>(Restart);
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyIn = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, 0f, page.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        VisualElement page = uiDoc.rootVisualElement.Q<VisualElement>("Page");

        Tween flyOut = DOTween.To(() => page.transform.position,
                                x => page.transform.position = x,
                                new Vector3(0f, Screen.height, page.transform.position.z), .45f)
                                .SetEase(Ease.OutQuart);

        UIManager.instance.TopBar.ShowTopBar();

        yield return flyOut.WaitForCompletion();
    }

    public override void HidePage()
    {
        homeButton.UnregisterCallback<PointerUpEvent>(GoHome);
        replayButton.UnregisterCallback<PointerUpEvent>(Restart);
    }

    #endregion

    #region Private Functions

    private void GoHome(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>());
    }

    private void Restart(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        object[] data = new object[2];
        data[0] = cat;
        data[1] = difficultyIndex;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<TimedModePage>(data));
    }

    #endregion
}
