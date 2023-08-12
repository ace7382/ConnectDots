using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectiveCard
{
    #region Private Variables

    private Objective       objective;
    private VisualElement   card;

    #endregion

    #region Public Properties

    public Objective        Objective { get { return objective; } }

    #endregion

    #region Constructor

    public ObjectiveCard(Objective objective, VisualElement root)
    {
        this.objective  = objective;
        card            = root;

        Setup();
    }

    #endregion

    #region Private Functions

    private void Setup()
    {
        Label description                       = card.Q<Label>("Description");
        VisualElement icon                      = card.Q<VisualElement>("Icon");
        VisualElement progressBarWidth          = card.Q<VisualElement>("Mask");
        VisualElement progressBarFill           = progressBarWidth.Q<VisualElement>("Fill");
        Label progressBarLabel                  = card.Q<Label>("ProgressBarLabel");
        VisualElement awardPanel                = card.Q<VisualElement>("AwardPanel");
        VisualElement isCompleteIcon            = card.Q<VisualElement>("CompletedIcon");

        description.text                        = objective.Description;
        icon.style.backgroundImage              = new StyleBackground(objective.Icon);
        icon.parent.SetColor(objective.ProgressBarColor);
        isCompleteIcon.Q<VisualElement>("CheckIcon")
            .style.unityBackgroundImageTintColor = objective.ProgressBarColor;

        progressBarFill.SetColor(objective.ProgressBarColor);
        progressBarWidth.SetWidth(new StyleLength(new Length(objective.GetProgressAsPercentage(), LengthUnit.Percent)));
        progressBarLabel.text                   = string.Format(objective.GetProgressAsString());

        if (objective.IsAchievement)
        {
            awardPanel.Hide();

            isCompleteIcon.Show(objective.IsComplete);
            card.Q<VisualElement>("ProgressBarBG").SetMargins(50f, false, objective.IsComplete, false, false);

            //TODO: On click syncs achievement to whichever platform service it's on
        }
        else
        {
            VisualElement awardIcon         = awardPanel.Q<VisualElement>("AwardIcon");
            Label awardAmount               = awardPanel.Q<Label>("AwardAmount");

            if (objective.RewardClaimed)
            {
                MarkAsClaimed(awardPanel, awardIcon, awardAmount, isCompleteIcon);
            }
            else
            {
                if (objective.PowerupRewardType != PowerupType.none)
                {
                    awardIcon.SetImage(UIManager.instance.GetPowerupIcon(objective.PowerupRewardType));
                    awardIcon.ScaleToFit();
                    awardIcon.SetBorderWidth(0f);
                }
                else //if (objective.RewardColor != ColorCategory.NONE)
                {
                    awardIcon.SetColor(UIManager.instance.GetColor(objective.RewardColor));

                    //TODO: if settings.show color numbers, display numbers
                }

                awardAmount.text = "x " + objective.RewardAmount.ToString();

                if (Objective.IsComplete)
                {
                    VisualElement notBub    = awardPanel.Q<VisualElement>("NotificationBubble");
                    notBub.SetColor(objective.ProgressBarColor);
                    notBub.Show();

                    card.RegisterCallback<PointerUpEvent>(ClaimReward);
                }
            }
        }
    }

    private void MarkAsClaimed(VisualElement awardPanel, VisualElement awardIcon, Label awardAmount, VisualElement isCompleteIcon)
    {
        awardAmount.text            = "Claimed";
        awardIcon.RemoveFromHierarchy();

        isCompleteIcon.Show();

        awardPanel.style.height     = new StyleLength(StyleKeyword.Auto);
        awardPanel.style.maxHeight  = new StyleLength(StyleKeyword.None);
        awardPanel.style.minHeight  = awardPanel.style.maxHeight;

        awardPanel.Q<VisualElement>("NotificationBubble").RemoveFromHierarchy();

        awardAmount.SetMargins(25f, 0f, 25f, 0f);
    }

    private void ClaimReward(PointerUpEvent evt)
    {
        VisualElement awardPanel        = card.Q<VisualElement>("AwardPanel");
        VisualElement isCompleteIcon    = card.Q<VisualElement>("CompletedIcon");
        VisualElement awardIcon         = awardPanel.Q<VisualElement>("AwardIcon");
        Label awardAmount               = awardPanel.Q<Label>("AwardAmount");

        MarkAsClaimed(awardPanel, awardIcon, awardAmount, isCompleteIcon);

        objective.ClaimReward();

        Vector2 spawnBoundsX            = new Vector2(card.worldBound.center.x - ((card.worldBound.width / 2f) * .8f)
                                                    , card.worldBound.center.x + ((card.worldBound.width / 2f) * .8f));

        Vector2 spawnBoundsY            = new Vector2(card.worldBound.center.y - ((card.worldBound.height / 2f) * .8f)
                                                    , card.worldBound.center.y + ((card.worldBound.height / 2f) * .8f));
        
        if (objective.PowerupRewardType != PowerupType.none)
        {
            //TODO: powerup spawn
            int maxCoins                = Mathf.Min(30, objective.RewardAmount);

            for (int i = 0; i < maxCoins; i++)
            {
                Vector2 origin          = new Vector2(Random.Range(spawnBoundsX.x, spawnBoundsX.y)
                                                    , Random.Range(spawnBoundsY.x, spawnBoundsY.y));

                CurrencyManager.instance.SpawnPowerups(
                                        objective.PowerupRewardType
                                        , origin
                                        , UIManager.instance.TopBar.CoinsButton.worldBound.center
                                        );
            }

            CurrencyManager.instance.AddCurrency(objective.PowerupRewardType, objective.RewardAmount);
        }
        else //if (objective.RewardColor != ColorCategory.NONE)
        {
            int maxCoins                = Mathf.Min(100, objective.RewardAmount);

            for (int i = 0; i < maxCoins; i++)
            {
                Vector2 origin          = new Vector2(Random.Range(spawnBoundsX.x, spawnBoundsX.y)
                                                    , Random.Range(spawnBoundsY.x, spawnBoundsY.y));

                CurrencyManager.instance.SpawnCoin(
                                        objective.RewardColor
                                        , origin
                                        , UIManager.instance.TopBar.CoinsButton.worldBound.center
                                        );
            }

            CurrencyManager.instance.AddCurrency(objective.RewardColor, objective.RewardAmount);
        }

        card.UnregisterCallback<PointerUpEvent>(ClaimReward);

        this.PostNotification(Notifications.OBJECTIVE_REWARD_CLAIMED);
    }

    #endregion
}
