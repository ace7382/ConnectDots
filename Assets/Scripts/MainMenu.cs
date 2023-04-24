using DG.Tweening;
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
        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(1f);

        Tween fadeout = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                0f, .33f);

        yield return fadeout.Play().WaitForCompletion();
    }

    #endregion

    #region Private Functions

    private void PlayButtonClicked(PointerDownEvent evt)
    {
        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
    }

    #endregion
}
