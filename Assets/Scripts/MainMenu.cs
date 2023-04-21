using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : Page
{
    #region Private Variables

    VisualElement playButton;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        playButton = uiDoc.rootVisualElement.Q<VisualElement>("PlayButton");

        playButton.RegisterCallback<PointerDownEvent>(PlayButtonClicked);
    }

    public override void HidePage()
    {
        playButton.UnregisterCallback<PointerDownEvent>(PlayButtonClicked);
    }

    public override IEnumerator AnimateIn()
    {
        return null;
    }

    public override IEnumerator AnimateOut()
    {
        return null;
    }

    #endregion

    #region Private Functions

    private void PlayButtonClicked(PointerDownEvent evt)
    {
        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
    }

    #endregion
}
