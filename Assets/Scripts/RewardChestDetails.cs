using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RewardChestDetails : Page
{
    #region Private Variables

    private bool            canClick;
    private RewardChest     chest;
    private VisualElement   modal;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   RewardChest     -   The RewardChest to be loaded on the details page

        chest                           = (RewardChest)args[0];
        modal                           = uiDoc.rootVisualElement.Q<VisualElement>("Container");

        VisualElement page              = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        VisualElement chestIcon         = uiDoc.rootVisualElement.Q<VisualElement>("HeaderIcon");
        VisualElement openButton        = uiDoc.rootVisualElement.Q<VisualElement>("OpenButton");
        VisualElement closeButton       = uiDoc.rootVisualElement.Q<VisualElement>("BackButton");
        Label headerLabel               = uiDoc.rootVisualElement.Q<Label>("Header");
        ScrollView chestContentsScroll  = uiDoc.rootVisualElement.Q<ScrollView>();

        chestIcon.style.backgroundImage = UIManager.instance.GetRewardChestIcon(chest.ChestType);
        headerLabel.text                = string.Format("Receive {0} of the following:"
                                          , chest.GetNumberOfRewards().ToString());

        chestContentsScroll
            .style.maxHeight            = Screen.height * .55f;

        List<RewardChest.Reward>
            potentialRewards            = chest.GetChestRewards();

        for (int i = 0; i < potentialRewards.Count; i++)
        {
            VisualElement rewardLine    = UIManager.instance.RewardLine.Instantiate();

            rewardLine.name             = "RewardLine";
            Label details               = rewardLine.Q<Label>("Details");
            Label chance                = rewardLine.Q<Label>("Chance");

            details.text                = potentialRewards[i].GetRewardLineText();
            chance.text                 = potentialRewards[i].Chance.ToString("0.00") + "%";

            rewardLine.SetMargins(10f, 0f, 10f, 0f);

            chestContentsScroll.Add(rewardLine);
        }

        openButton.RegisterCallback<ClickEvent>(OpenChest);

        EventCallback<ClickEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            canClick = false;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        };

        closeButton.RegisterCallback(backButtonAction);
        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        Color unpressedColor            = closeButton.resolvedStyle.backgroundColor;
        Color pressedColor              = new Color(
                                            unpressedColor.r - .2f
                                            , unpressedColor.g - .2f
                                            , unpressedColor.b - .2f
                                            , 1f
                                        );

        ButtonStateChanger closeBSC     = new ButtonStateChanger(closeButton, unpressedColor, pressedColor);
        ButtonStateChanger openBSC      = new ButtonStateChanger(openButton, unpressedColor, pressedColor);

        closeButton.RegisterCallback<PointerDownEvent>(closeBSC.OnPointerDown);
        openButton.RegisterCallback<PointerDownEvent>(openBSC.OnPointerDown);

        page.RegisterCallback<PointerUpEvent>(closeBSC.OnPointerUp);
        page.RegisterCallback<PointerUpEvent>(openBSC.OnPointerUp);
        page.RegisterCallback<PointerLeaveEvent>(closeBSC.OnPointerOff);
        page.RegisterCallback<PointerLeaveEvent>(openBSC.OnPointerOff);

        modal.transform.position        = new Vector3(0f, Screen.height, modal.transform.position.z);
    }

    public override IEnumerator AnimateIn()
    {
        canClick            = false;

        //This is needed because there is a bug with wrapping text with a % max width
        //https://forum.unity.com/threads/label-with-textwrap-not-adjusting-auto-height-appropriately-when-maxwidth-uses-a-percent.1482831/
        yield return null;

        ScrollView chestContentsScroll  = uiDoc.rootVisualElement.Q<ScrollView>();
        List<Label> detailLabels        = chestContentsScroll.Query<Label>("Details").ToList();

        for (int i = 0; i < detailLabels.Count; i++)
            detailLabels[i].style.maxWidth = new StyleLength(new Length(detailLabels[i].resolvedStyle.width, LengthUnit.Pixel));
        ////////

        yield return null;

        VisualElement topIndicator      = chestContentsScroll.Q<VisualElement>("TopArrow");
        VisualElement btmIndicator      = chestContentsScroll.Q<VisualElement>("BottomArrow");

        chestContentsScroll.SetBoundIndicators(topIndicator, btmIndicator);
        chestContentsScroll.verticalScroller.valueChanged += (evt) =>
        {
            chestContentsScroll.ShowHideVerticalBoundIndicators(topIndicator, btmIndicator);
        };
        chestContentsScroll.ShowHideVerticalBoundIndicators(
            topIndicator
            , btmIndicator
            , chestContentsScroll.contentContainer
            , chestContentsScroll.contentContainer.parent
        );


        //VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        //VisualElement page  = uiDoc.rootVisualElement.Q<VisualElement>("Container");
        //Tween flyIn         = DOTween.To(() => page.transform.position,
        //                        x => page.transform.position = x,
        //                        new Vector3(0f, 0f, page.transform.position.z), .65f)
        //                        .SetEase(Ease.OutQuart);

        //yield return flyIn.WaitForCompletion();

        yield return ModalInOut(true).WaitForCompletion();

        canClick            = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        yield return null;
    }

    public override void HidePage()
    {

    }

    #endregion

    #region Private Functions

    private void OpenChest(ClickEvent evt)
    {
        if (!canClick)
            return;

        PageManager.instance.StartCoroutine(OpenChestAnimation());
    }

    private IEnumerator OpenChestAnimation()
    {
        yield return ModalInOut(false).WaitForCompletion();

        ScrollView contentScroll        = modal.Q<ScrollView>();

        List<VisualElement> rewardLines = contentScroll.contentContainer
                                          .Query<VisualElement>("RewardLine").ToList();

        for (int i = 0; i < rewardLines.Count; i++)
            rewardLines[i].RemoveFromHierarchy();

        List<(RewardChest.RewardType type, int amount)> prizes = chest.GetPrizes();

        for (int i = 0; i < prizes.Count; i++)
        {
            Debug.Log(string.Format("{0} - {1}", prizes[i].type, prizes[i].amount.ToString()));
        }

        yield return ModalInOut(true).WaitForCompletion();

        canClick = true;
    }

    private Tween ModalInOut(bool flyIn)
    {
        if (flyIn)
        {
            return DOTween.To(
                    () => modal.transform.position
                    , x => modal.transform.position = x
                    , new Vector3(0f, 0f, modal.transform.position.z)
                    , .65f
                )
                .SetEase(Ease.OutQuart);
        }
        else
        {
            return DOTween.To(
                    () => modal.transform.position
                    , x => modal.transform.position = x
                    , new Vector3(0f, Screen.height, modal.transform.position.z)
                    , .65f
                )
                .SetEase(Ease.OutQuart);
        }
    }

    #endregion
}
