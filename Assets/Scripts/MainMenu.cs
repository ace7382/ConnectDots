using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : Page
{
    #region Private Variables

    private VisualElement playButton;
    private VisualElement shopButton;
    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        DOTween.SetTweensCapacity(1000, 250); //TODO: Find a more appropriate place for this

        UIManager.instance.TopBar.ShowTopBar(false);

        playButton  = uiDoc.rootVisualElement.Q<VisualElement>("PlayButton");
        shopButton  = uiDoc.rootVisualElement.Q<VisualElement>("ShopButton");

        playButton.RegisterCallback<PointerUpEvent>(PlayButtonClicked);
        shopButton.RegisterCallback<PointerUpEvent>(OpenShop);

        canClick    = true;
    }

    public override void HidePage()
    {
        playButton.UnregisterCallback<PointerUpEvent>(PlayButtonClicked);
        shopButton.UnregisterCallback<PointerUpEvent>(OpenShop);
    }

    public override IEnumerator AnimateIn()
    {
        return null;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        VisualElement page = uiDoc.rootVisualElement;

        page.style.opacity = new StyleFloat(1f);

        Tween fadeout = DOTween.To(() => page.style.opacity.value,
                x => page.style.opacity = new StyleFloat(x),
                0f, .33f);

        yield return fadeout.Play().WaitForCompletion();
    }

    #endregion

    #region Private Functions

    private void PlayButtonClicked(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
    }

    private void OpenShop(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<Shop>());
    }

    #endregion
}
