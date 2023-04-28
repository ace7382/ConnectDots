using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CoinDrawer : Page
{
    #region Private Functions

    private EventCallback<PointerDownEvent> previousBackButtonAction;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        previousBackButtonAction = UIManager.instance.TopBar.GetCurrentBackButtonEvent();

        EventCallback<PointerDownEvent> backButtonAction = (evt) =>
        {
            UIManager.instance.TopBar.CoinButtonClicked(evt);
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        ScrollView coinsScroll = uiDoc.rootVisualElement.Q<ScrollView>("CoinsScroll");
        coinsScroll.contentContainer.style.flexGrow = 1f;

        VisualElement coinsScrollContent = coinsScroll.contentContainer.Q<VisualElement>("CoinsScrollContent");
        Label emptyLabel = coinsScrollContent.Q<Label>("EmptyLabel");

        if (CurrencyManager.instance.TotalTokens > 0)
        {
            emptyLabel.Hide();

            for (int i = 0; i < UIManager.instance.ColorCount; i++)
            {
                int amount = CurrencyManager.instance.GetCoinsForColorIndex(i);

                if (amount != 0)
                {
                    VisualElement display = UIManager.instance.CoinDisplay.Instantiate();

                    display.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(i));
                    display.Q<Label>("AmountLabel").text = amount.ToString();

                    coinsScrollContent.Add(display);
                }
            }
        }
        else
        {
            emptyLabel.Show();
        }
    }

    public override void HidePage()
    {
        UIManager.instance.TopBar.UpdateBackButtonOnClick(previousBackButtonAction);
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

}
