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

        playButton.RegisterCallback<PointerUpEvent>(PlayButtonClicked);
        shopButton.RegisterCallback<PointerUpEvent>(OpenShop);
        settingsButton.RegisterCallback<PointerUpEvent>(OpenSettings);
        achievementsButton.RegisterCallback<PointerUpEvent>(OpenAchievements);
        profileCard.RegisterCallback<PointerUpEvent>(OpenProfile);

        canClick            = true;
    }

    public override void HidePage()
    {
        playButton.UnregisterCallback<PointerUpEvent>(PlayButtonClicked);
        shopButton.UnregisterCallback<PointerUpEvent>(OpenShop);
        settingsButton.UnregisterCallback<PointerUpEvent>(OpenSettings);
        achievementsButton.UnregisterCallback<PointerUpEvent>(OpenAchievements);
        profileCard.UnregisterCallback<PointerUpEvent>(OpenProfile);
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

    private void PlayButtonClicked(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<CategorySelect>());
    }

    private void OpenShop(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<Shop>());
    }

    private void OpenSettings(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<Settings>());
    }

    private void OpenAchievements(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<AchievementsPage>());
    }

    private void OpenProfile(PointerUpEvent evt)
    {
        if (!canClick)
            return;

        canClick = false;

        PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<ProfilePage>());
    }

    #endregion
}
