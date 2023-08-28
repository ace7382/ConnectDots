using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfilePage : Page
{
    #region Private Variables

    private bool                        canClick;
    private VisualElement               profileCard;
    private EventCallback<ClickEvent>   backbuttonAction;
    private VisualElement[]             coinDisplays;
    private VisualElement[]             expBars;
    List<VisualElement>                 rewardChestButtons;

    #endregion

    #region Inherited Functions

    public override void OnFocusReturnedToPage()
    {
        canClick = true;
        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
    }

    public override void ShowPage(object[] args)
    {
        backbuttonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>());
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
        UIManager.instance.TopBar.ShowCoinButton(false);

        profileCard                     = uiDoc.rootVisualElement.Q<VisualElement>("ProfileCard");
        ProfileCardController pcControl = new ProfileCardController(profileCard);
        profileCard.userData            = pcControl;
    }

    public override IEnumerator AnimateIn()
    {
        if (!UIManager.instance.TopBar.IsShowing)
            UIManager.instance.TopBar.ShowTopBar();

        yield return null;

        DrawEXPBars();
        ShowOwnedSegments();
        ShowRewardChests();

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
        this.RemoveObserver(UpdateSegmentDisplays, Notifications.SEGMENTS_RECEIVED);
        this.RemoveObserver(UpdateEXPBars, Notifications.EXP_RECEIVED);
        this.RemoveObserver(RemoveOpenedChest, Notifications.REWARD_CHEST_OPENED);
    }

    #endregion

    #region Private Functions

    private void DrawEXPBars()
    {
        expBars = new VisualElement[7]
        {
            uiDoc.rootVisualElement.Q<VisualElement>("BlackAndWhiteEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("RedEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("PurpleEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("BlueEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("GreenEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("YellowEXP")
            , uiDoc.rootVisualElement.Q<VisualElement>("OrangeEXP")
        };

        for (int i = 0; i < expBars.Length; i++)
        {
            VisualElement currentBar        = expBars[i];
            ColorCategory currentCC         = (ColorCategory)i;

            currentBar.userData             = currentCC;

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

        this.AddObserver(UpdateEXPBars, Notifications.EXP_RECEIVED);
    }

    private void ShowOwnedSegments()
    {
        coinDisplays = new VisualElement[7]
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
            VisualElement current   = coinDisplays[i];
            ColorCategory category  = (ColorCategory)i;

            current.userData        = category;

            current.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(category));
            current.Q<Label>("AmountLabel").text = CurrencyManager.instance.GetCoinsForColorIndex(category).ToString();
        }

        this.AddObserver(UpdateSegmentDisplays, Notifications.SEGMENTS_RECEIVED);
    }

    private void UpdateSegmentDisplays(object sender, object info)
    {
        //info      -   object[2]
        //info[0]   -   ColorCategory   -   the ColorCategory that needs to be updated
        //info[1]   -   int             -   the number of segments earned

        object[] data               = (object[])info;
        ColorCategory category      = (ColorCategory)data[0];

        VisualElement veToUpdate    = System.Array.Find<VisualElement>(coinDisplays
                                    , x => (ColorCategory)x.userData == category);

        Label display               = veToUpdate.Q<Label>("AmountLabel");
        int currentValue            = System.Int32.Parse(display.text);
        int endValue                = CurrencyManager.instance.GetCoinsForColorIndex(category);

        Tween countUp               = DOTween.To(
                                        () => currentValue
                                        , x => currentValue = x
                                        , endValue
                                        , 1f
                                    ).OnUpdate(() =>
                                        display.text = currentValue.ToString()
                                    ).SetEase(Ease.OutQuad)
                                    .Play();
    }

    private void UpdateEXPBars(object sender, object info)
    {
        //TODO: to match end of level popup's functionality, might want these to fill on level up
        //      and then fill forward. Currently the end point of the progress line is just set to
        //      the new location (so the bar may shrink instead of filling and then partially filling)

        //info      -   object[2]
        //info[0]   -   ColorCategory   -   the ColorCategory that needs to be updated
        //info[1]   -   int             -   the number of EXP earned

        object[] data               = (object[])info;
        ColorCategory category      = (ColorCategory)data[0];

        VisualElement veToUpdate    = System.Array.Find<VisualElement>(expBars
                                    , x => (ColorCategory)x.userData == category);

        Label currentLabel          = veToUpdate.Q<Label>("CurrentLevel");
        Label nextLabel             = veToUpdate.Q<Label>("NextLevel");
        Label progressLabel         = veToUpdate.Q<Label>("CurrentProgress");
        VisualElement parentVE      = veToUpdate.Q<VisualElement>("BG");

        currentLabel.text           = ProfileManager.instance.GetEXPLevel(category).ToString();
        nextLabel.text              = ProfileManager.instance.GetNextEXPLevel(category).ToString();
        progressLabel.text          = ProfileManager.instance.GetCurrentEXP(category).ToString() + " / "
                                    + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(category)).ToString();
        
        Vector2 leftOrigin          = new Vector2(parentVE.WorldToLocal(currentLabel.worldBound.center).x, 0f);
        Vector2 rightOrigin         = new Vector2(parentVE.WorldToLocal(nextLabel.worldBound.center).x, 0f);
        float dotDistance           = rightOrigin.x - leftOrigin.x;
        Vector2 progressStop        = new Vector2(
                                        leftOrigin.x + (
                                            (float)ProfileManager.instance.GetCurrentEXP(category) /
                                            (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(category))
                                            * dotDistance)
                                        , leftOrigin.y);

        UIToolkitLine currentLine   = parentVE.Q<UIToolkitLine>();
        Debug.Log("CurrentLine points: " + currentLine.Points.Count);
        Tween moveBarEnd            = DOTween.To(
                                        () => currentLine.LastPoint
                                        , x => currentLine.UpdateLastPoint(x)
                                        , progressStop
                                        , 1f
                                    )
                                    .OnUpdate(() => Debug.Log("Current last point: " + currentLine.LastPoint + " || Goal: " + progressStop))
                                    .SetEase(Ease.OutQuad)
                                    .Play();
    }

    private void ShowRewardChests()
    {
        rewardChestButtons                      = new List<VisualElement>();
        VisualElement page                      = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        VisualElement rewardChestContainer      = uiDoc.rootVisualElement
                                                .Q<VisualElement>("RewardChestsContainer")
                                                .Q<VisualElement>("BG");

        for (int i = 0; i < CurrencyManager.instance.TotalRewardChests; i++)
        {
            VisualElement rewardChestButton     = UIManager.instance.RewardChestButton.Instantiate();
            rewardChestButton.name              = "Chest " + i.ToString();
            VisualElement bg                    = rewardChestButton.Q<VisualElement>("BG");
            RewardChest currentChest            = CurrencyManager.instance.GetRewardChest(i);
            rewardChestButton.userData          = currentChest;

            StyleBackground chestImage          = new StyleBackground(UIManager.instance.GetRewardChestIcon(currentChest.ChestType));
            rewardChestButton.Q<VisualElement>("Shadow")
                .style.backgroundImage          = chestImage;
            bg.style.backgroundImage            = chestImage;

            rewardChestButton.SetMargins(15f);

            rewardChestContainer.Add(rewardChestButton);

            rewardChestButton.RegisterCallback<ClickEvent>((evt) =>
            {
                if (!canClick)
                    return;

                canClick                        = false;

                object[] data                   = new object[1];
                data[0]                         = currentChest;

                PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<RewardChestDetails>(data));
            });

            ButtonStateChanger chestBSC         = new ButtonStateChanger(bg, false);

            rewardChestButton.RegisterCallback<PointerDownEvent>(chestBSC.OnPointerDown);
            page.RegisterCallback<PointerUpEvent>(chestBSC.OnPointerUp);
            page.RegisterCallback<PointerLeaveEvent>(chestBSC.OnPointerOff);

            rewardChestButtons.Add(rewardChestButton);
        }

        rewardChestContainer.Q<Label>().Show(rewardChestButtons.Count <= 0);

        this.AddObserver(RemoveOpenedChest, Notifications.REWARD_CHEST_OPENED);
    }

    private void RemoveOpenedChest(object sender, object info)
    {
        //info  -   RewardChest -   The RewardChest that was opened

        RewardChest chest = (RewardChest)info;

        VisualElement chestButton = rewardChestButtons.Find(x => (RewardChest)x.userData == chest);
        rewardChestButtons.Remove(chestButton);
        chestButton.RemoveFromHierarchy();

        uiDoc.rootVisualElement.Q<VisualElement>("RewardChestsContainer")
            .Q<VisualElement>("BG").Q<Label>().Show(rewardChestButtons.Count <= 0);
    }

    #endregion
}
