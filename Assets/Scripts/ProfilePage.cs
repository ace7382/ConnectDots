using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfilePage : Page
{
    #region Private Variables

    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        EventCallback<ClickEvent> backbuttonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>());
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
        UIManager.instance.TopBar.ShowCoinButton(false);
    }

    public override IEnumerator AnimateIn()
    {
        if (!UIManager.instance.TopBar.IsShowing)
            UIManager.instance.TopBar.ShowTopBar();

        yield return null;

        DrawEXPBars();
        ShowOwnedSegments();

        yield return null;

        //Set the viewport height so that the content can scroll in the "full" screen area that it should be using
        VisualElement viewport      = uiDoc.rootVisualElement.Q<ScrollView>().Q<VisualElement>("unity-content-viewport");
        RectOffsetFloat safeArea    = viewport.panel.GetSafeArea();
        viewport.SetHeight(Screen.height - (uiDoc.rootVisualElement.Q<VisualElement>("Page").resolvedStyle.paddingTop + safeArea.Bottom + safeArea.Top));

        yield return null;

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick            = false;

        VisualElement page  = uiDoc.rootVisualElement;

        page.style.opacity  = new StyleFloat(1f);

        Tween fadeout       = DOTween.To(() => page.style.opacity.value,
                                x => page.style.opacity = new StyleFloat(x),
                                0f, .33f);

        UIManager.instance.TopBar.ShowTopBar(false);
        yield return fadeout.Play().WaitForCompletion();
    }

    public override void HidePage()
    {

    }

    #endregion

    #region Private Functions

    private void DrawEXPBars()
    {
        //TODO - Turn this into a loop lollllllll

        List<VisualElement> expBars = new List<VisualElement>()
        {
            uiDoc.rootVisualElement.Q<VisualElement>("BlackAndWhiteEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("RedEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("PurpleEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("BlueEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("GreenEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("YellowEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("OrangeEXP")
        };

        for (int i = 0; i < expBars.Count; i++)
        {
            VisualElement currentBar        = expBars[i];
            ColorCategory currentCC         = (ColorCategory)i;

            Label label                     = currentBar.Q<Label>("Label");
            Label currentLabel              = currentBar.Q<Label>("CurrentLevel");
            Label nextLabel                 = currentBar.Q<Label>("NextLevel");
            Label progressLabel             = currentBar.Q<Label>("CurrentProgress");
            VisualElement parentVE          = currentBar.Q<VisualElement>("BG");

            Vector2 leftOrigin              = new Vector2(parentVE.WorldToLocal(currentLabel.worldBound.center).x, 0f);
            Vector2 rightOrigin             = new Vector2(parentVE.WorldToLocal(nextLabel.worldBound.center).x, 0f);

            label.text                      = currentCC.Name();
            currentLabel.text               = ProfileManager.instance.GetEXPLevel(currentCC).ToString();
            nextLabel.text                  = ProfileManager.instance.GetNextEXPLevel(currentCC).ToString();
            progressLabel.text              = ProfileManager.instance.GetCurrentEXP(currentCC).ToString() + " / "
                                            + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(currentCC)).ToString();

            Color progressBarColor          = currentCC == ColorCategory.BLACK_AND_WHITE ?
                                                Color.black : UIManager.instance.GetColor(currentCC);

            UIToolkitCircle leftDot         = new UIToolkitCircle(leftOrigin, 35f, progressBarColor);
            UIToolkitCircle rightDot        = new UIToolkitCircle(rightOrigin, 35f, progressBarColor);

            float dotDistance               = rightOrigin.x - leftOrigin.x;
            Vector2 progressStop            = new Vector2(
                                                leftOrigin.x + (
                                                    (float)ProfileManager.instance.GetCurrentEXP(currentCC) /
                                                    (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(currentCC))
                                                    * dotDistance)
                                                , leftOrigin.y);

            List<Vector2> progPoints        = new List<Vector2>()
                                                { leftOrigin, progressStop };

            UIToolkitLine progLine          = new UIToolkitLine(progPoints, 30f, progressBarColor, LineCap.Round);

            parentVE.Add(leftDot);
            parentVE.Add(rightDot);
            parentVE.Add(progLine);
        }
    }

    public void ShowOwnedSegments()
    {
        VisualElement[] coinDisplays = new VisualElement[7]
        {
            uiDoc.rootVisualElement.Q<VisualElement>("WhiteCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("RedCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("PurpleCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("BlueCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("GreenCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("YellowCoinDisplay")
            , uiDoc.rootVisualElement.Q<VisualElement>("OrangeCoinDisplay")
        };

        for (int i = 0; i < coinDisplays.Length; i++)
        {
            VisualElement current = coinDisplays[i];

            current.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor((ColorCategory)i));
            current.Q<Label>("AmountLabel").text = CurrencyManager.instance.GetCoinsForColorIndex((ColorCategory)i).ToString();
        }
        
    }

    #endregion
}
