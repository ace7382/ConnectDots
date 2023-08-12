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
    private VisualElement settingsButton;
    private VisualElement achievementsButton;
    private VisualElement profileCard;
    private bool canClick;

    private VisualElement dailyTrackerBox1;

    #endregion

    #region Inherited Functions

    public override void OnFocusReturnedToPage()
    {
        canClick = true;
    }

    public override void ShowPage(object[] args)
    {
        DOTween.SetTweensCapacity(1000, 250); //TODO: Find a more appropriate place for this

        UIManager.instance.TopBar.ShowTopBar(false);

        playButton          = uiDoc.rootVisualElement.Q<VisualElement>("PlayButton");
        shopButton          = uiDoc.rootVisualElement.Q<VisualElement>("ShopButton");
        settingsButton      = uiDoc.rootVisualElement.Q<VisualElement>("SettingsButton");
        achievementsButton  = uiDoc.rootVisualElement.Q<VisualElement>("AchievementsButton");
        profileCard         = uiDoc.rootVisualElement.Q<VisualElement>("ProfileCard");

        dailyTrackerBox1    = uiDoc.rootVisualElement.Q<VisualElement>("DailyTrackerBox1");

        playButton.RegisterCallback<ClickEvent>(PlayButtonClicked);
        shopButton.RegisterCallback<ClickEvent>(OpenShop);
        settingsButton.RegisterCallback<ClickEvent>(OpenSettings);
        achievementsButton.RegisterCallback<ClickEvent>(OpenAchievements);
        profileCard.RegisterCallback<ClickEvent>(OpenProfile);

        ButtonStateChanger playBSC      = new ButtonStateChanger(playButton.Q<VisualElement>("BG"));
        ButtonStateChanger shopBSC      = new ButtonStateChanger(shopButton.Q<VisualElement>("BG"));
        ButtonStateChanger achieveBSC   = new ButtonStateChanger(achievementsButton.Q<VisualElement>("BG"));
        ButtonStateChanger settingsBSC  = new ButtonStateChanger(settingsButton.Q<VisualElement>("BG"));

        playButton.RegisterCallback<PointerDownEvent>(playBSC.OnPointerDown);
        shopButton.RegisterCallback<PointerDownEvent>(shopBSC.OnPointerDown);
        achievementsButton.RegisterCallback<PointerDownEvent>(achieveBSC.OnPointerDown);
        settingsButton.RegisterCallback<PointerDownEvent>(settingsBSC.OnPointerDown);

        VisualElement p                 = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        p.RegisterCallback<PointerUpEvent>(playBSC.OnPointerUp);
        p.RegisterCallback<PointerUpEvent>(shopBSC.OnPointerUp);
        p.RegisterCallback<PointerUpEvent>(achieveBSC.OnPointerUp);
        p.RegisterCallback<PointerUpEvent>(settingsBSC.OnPointerUp);
        p.RegisterCallback<PointerLeaveEvent>(playBSC.OnPointerOff);
        p.RegisterCallback<PointerLeaveEvent>(shopBSC.OnPointerOff);
        p.RegisterCallback<PointerLeaveEvent>(achieveBSC.OnPointerOff);
        p.RegisterCallback<PointerLeaveEvent>(settingsBSC.OnPointerOff);

        canClick                        = true;
    }

    public override void HidePage()
    {
        playButton.UnregisterCallback<ClickEvent>(PlayButtonClicked);
        shopButton.UnregisterCallback<ClickEvent>(OpenShop);
        settingsButton.UnregisterCallback<ClickEvent>(OpenSettings);
        achievementsButton.UnregisterCallback<ClickEvent>(OpenAchievements);
        profileCard.UnregisterCallback<ClickEvent>(OpenProfile);
    }

    public override IEnumerator AnimateIn()
    {
        Vector2 origin                              = new Vector2(0f, 50f);//, dailyTrackerBox1.resolvedStyle.height / 2f);
        UIToolkitCircle dailyTrackerZeroEndPoint    = new UIToolkitCircle(origin, 30f, Color.green);
        dailyTrackerZeroEndPoint.name               = "DAILY TRACKER END POINT";

        dailyTrackerBox1.Add(dailyTrackerZeroEndPoint);

        return null;
    }

    public override IEnumerator AnimateOut()
    {
        canClick            = false;

        VisualElement page  = uiDoc.rootVisualElement;

        page.style.opacity  = new StyleFloat(1f);

        Tween fadeout       = DOTween.To(() => page.style.opacity.value,
                                x => page.style.opacity = new StyleFloat(x),
                                0f, .33f);

        yield return fadeout.Play().WaitForCompletion();
    }

    #endregion

    #region Private Functions

    private void PlayButtonClicked(ClickEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
    }

    private void OpenShop(ClickEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<Shop>());
    }

    private void OpenSettings(ClickEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<Settings>());
    }

    private void OpenAchievements(ClickEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<AchievementsPage>());
    }

    private void OpenProfile(ClickEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<ProfilePage>());
    }

    #endregion
}
