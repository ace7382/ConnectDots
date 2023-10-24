using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RewardChestDetails : Page
{
    #region Private Variables

    private bool                canClick;
    private List<RewardChest>   chests;
    private VisualElement       modal;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   RewardChest     -   The RewardChest to be loaded on the details page
        //args[0]   -   List<RewardChest>   -   The RewardChest(s) to be loaded on the details page

        chests                          = (List<RewardChest>)args[0];
        modal                           = uiDoc.rootVisualElement.Q<VisualElement>("Container");

        VisualElement page              = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        VisualElement chestIcon         = uiDoc.rootVisualElement.Q<VisualElement>("HeaderIcon");
        VisualElement openButton        = uiDoc.rootVisualElement.Q<VisualElement>("OpenButton");
        VisualElement closeButton       = uiDoc.rootVisualElement.Q<VisualElement>("BackButton");
        Label headerLabel               = uiDoc.rootVisualElement.Q<Label>("Header");
        ScrollView chestContentsScroll  = uiDoc.rootVisualElement.Q<ScrollView>();

        chestContentsScroll
            .style.maxHeight            = Screen.height * .55f;

        if (chests.Count == 1)
        {
            chestIcon.style
                .backgroundImage        = UIManager.instance.GetRewardChestIcon(chests[0].ChestType);
            headerLabel.text            = string.Format("Receive {0} of the following:"
                                          , chests[0].GetNumberOfRewards().ToString());

            List<RewardChest.Reward>
                potentialRewards        = chests[0].GetChestRewards();

            for (int i = 0; i < potentialRewards.Count; i++)
            {
                VisualElement rewardLine= UIManager.instance.RewardLine.Instantiate();

                rewardLine.name         = "RewardLine";
                Label details           = rewardLine.Q<Label>("Details");
                Label chance            = rewardLine.Q<Label>("Chance");

                details.text            = potentialRewards[i].GetPotentialRewardLineText();
                chance.text             = potentialRewards[i].Chance.ToString("0.00") + "%";

                rewardLine.SetMargins(10f, 0f, 10f, 0f);

                chestContentsScroll.Add(rewardLine);
            }
        }
        else //If opening more than 1 chest
        {
            //TODO: Set top icon

            headerLabel.text            = string.Format("Open {0}", chests.Count);

            //TODO: Show counts of each chests
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

        uiDoc.rootVisualElement.SetPadding(0f);

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

        List<(RewardChest.Reward reward, int amount)>
            prizesWithAmounts = new List<(RewardChest.Reward reward, int amount)>();

        if (chests.Count == 1)
        {
            prizesWithAmounts           = CurrencyManager.instance.OpenRewardChest(chests[0]);
        }
        else
        {
            for (int i = 0; i < chests.Count; i++)
            {
                List<(RewardChest.Reward reward, int amount)> tempForChest
                    = CurrencyManager.instance.OpenRewardChest(chests[i]);

                for (int j = 0; j < tempForChest.Count; j++)
                {
                    //If the reward from this chest exists in the final rewards, add the totals
                    //  else add the reward to the final list
                    int index = prizesWithAmounts.FindIndex(x => x.reward.Type == tempForChest[j].reward.Type);

                    if (index != -1)
                    {
                        (RewardChest.Reward r, int a) 
                            temp                    = prizesWithAmounts[index];
                        temp.a                      += tempForChest[j].amount;
                        prizesWithAmounts[index]    = temp;
                    }
                    else
                    {
                        (RewardChest.Reward r, int a) 
                            temp                    = (tempForChest[j].reward, tempForChest[j].amount);

                        prizesWithAmounts.Add(temp);
                    }
                }
            }
        }    

        for (int i = 0; i < prizesWithAmounts.Count; i++)
        {
            VisualElement rewardLine    = UIManager.instance.RewardLine.Instantiate();

            rewardLine.name             = "RewardLine";
            Label details               = rewardLine.Q<Label>("Details");
            Label chance                = rewardLine.Q<Label>("Chance");

            details.text                = prizesWithAmounts[i].reward.GetPrizeLineText();

            if (prizesWithAmounts[i].amount == -1)
            {
                chance.RemoveFromHierarchy();
            }
            else
            {
                chance.text             = "x" + prizesWithAmounts[i].amount.ToString();
            }

            rewardLine.SetMargins(10f, 0f, 10f, 0f);

            contentScroll.Add(rewardLine);
        }

        modal.Q<VisualElement>("BackButton").RemoveFromHierarchy();

        modal.Q<Label>("Header").text   = "Chest Contents";
        VisualElement claimButton       = modal.Q<VisualElement>("OpenButton");
        claimButton.Q<Label>().text     = "Claim";

        claimButton.UnregisterCallback<ClickEvent>(OpenChest);
        claimButton.RegisterCallback<ClickEvent>((evt) => SpawnRewards(evt, prizesWithAmounts));

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

    private void SpawnRewards(ClickEvent evt, List<(RewardChest.Reward reward, int amount)> prizes)
    {
        if (!canClick)
            return;

        canClick                                = false;

        VisualElement modalBG                   = modal.Q<VisualElement>("BG");
        Vector2 xLimits                         = new Vector2(
                                                    modalBG.worldBound.center.x - ((modalBG.worldBound.width / 2f) * .8f)
                                                    , modalBG.worldBound.center.x + ((modalBG.worldBound.width / 2f) * .8f)
                                                );

        Vector2 yLimits                         = new Vector2(
                                                    modalBG.worldBound.center.y - ((modalBG.worldBound.height / 2f) * .8f)
                                                    , modalBG.worldBound.center.y + ((modalBG.worldBound.height / 2f) * .8f)
                                                );

        //TODO: Add exp/categories/etc as spawns
        int coinsToSpawn                        = 0;
        int powerupsToSpawn                     = 0;
        List<ColorCategory> colorsCoinsToSpawn  = new List<ColorCategory>();
        List<PowerupType> powerupTypesToSpawn   = new List<PowerupType>();

        for (int i = 0; i < prizes.Count; i++)
        {
            switch (prizes[i].reward.Type)
            {
                case RewardChest.RewardType.BW_SEGMENTS:
                    coinsToSpawn                += prizes[i].amount;
                    colorsCoinsToSpawn.Add(ColorCategory.BLACK_AND_WHITE);
                    break;
                case RewardChest.RewardType.POWERUP_FILLEMPTY:
                    powerupsToSpawn             += prizes[i].amount;
                    powerupTypesToSpawn.Add(PowerupType.FILL_EMPTY);
                    break;
                case RewardChest.RewardType.POWERUP_HINT:
                    powerupsToSpawn             += prizes[i].amount;
                    powerupTypesToSpawn.Add(PowerupType.FILL_EMPTY);
                    break;
                case RewardChest.RewardType.POWERUP_REMOVESPECIALTILE:
                    powerupsToSpawn             += prizes[i].amount;
                    powerupTypesToSpawn.Add(PowerupType.REMOVE_SPECIAL_TILE);
                    break;
                default:
                    break;
            }
        }

        coinsToSpawn                            = Mathf.Min(30, coinsToSpawn);
        powerupsToSpawn                         = Mathf.Min(15, powerupsToSpawn);

        for (int i = 0; i < coinsToSpawn; i++)
        {
            Vector2 origin                      = new Vector2(
                                                    Random.Range(xLimits.x, xLimits.y)
                                                    , Random.Range(yLimits.x, yLimits.y)
                                                );

            CurrencyManager.instance.SpawnCoin(
                colorsCoinsToSpawn[Random.Range(0, colorsCoinsToSpawn.Count)]
                , origin
                , new Vector2(Screen.width / 2f, -30f)
            ); ;
        }

        for (int i = 0; i < powerupsToSpawn; i++)
        {
            Vector2 origin                      = new Vector2(
                                                    Random.Range(xLimits.x, xLimits.y)
                                                    , Random.Range(yLimits.x, yLimits.y)
                                                );

            CurrencyManager.instance.SpawnPowerups(
                powerupTypesToSpawn[Random.Range(0, powerupTypesToSpawn.Count)]
                , origin
                , new Vector2(Screen.width / 2f, -30f)
            ); ;
        }

        ModalInOut(false)
            .OnComplete(() =>
            {
                PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
            })
            .SetDelay(.9f) //powerup/coin fly animation is between .75 and 1 if not specified
            .Play();
    }

    #endregion
}
