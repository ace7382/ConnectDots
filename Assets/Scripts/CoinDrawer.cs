using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CoinDrawer : Page
{
    #region Private Varuables

    private EventCallback<PointerDownEvent> previousBackButtonAction;
    private bool canClick;

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        previousBackButtonAction = UIManager.instance.TopBar.GetCurrentBackButtonEvent();

        EventCallback<PointerDownEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            UIManager.instance.TopBar.CoinButtonClicked(evt);
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        ScrollView coinsScroll = uiDoc.rootVisualElement.Q<ScrollView>("CoinsScroll");
        coinsScroll.contentContainer.style.flexGrow = 1f;

        VisualElement coinsScrollContent = coinsScroll.contentContainer.Q<VisualElement>("CoinsScrollContent");
        Label emptyLabel = coinsScrollContent.Q<Label>("EmptyLabel");

        if (CurrencyManager.instance.TotalSegments > 0)
        {
            emptyLabel.Hide();

            for (int i = 0; i < CurrencyManager.instance.SegmentColorCount; i++)
            {
                int amount = CurrencyManager.instance.GetCoinsForColorIndex((ColorCategory)i);

                if (amount != 0)
                {
                    VisualElement display = UIManager.instance.CoinDisplay.Instantiate();

                    display.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor((ColorCategory)i));
                    display.Q<Label>("AmountLabel").text = amount.ToString();

                    coinsScrollContent.Add(display);
                }
            }
        }
        else
        {
            emptyLabel.Show();
        }

        this.PostNotification(Notifications.PAUSE_GAME);
    }

    public override void HidePage()
    {
        UIManager.instance.TopBar.UpdateBackButtonOnClick(previousBackButtonAction);

        this.PostNotification(Notifications.UNPAUSE_GAME);
    }

    public override IEnumerator AnimateIn()
    {
        canClick = false;

        uiDoc.rootVisualElement.style.translate = new StyleTranslate(new Translate(new Length(100f, LengthUnit.Percent), 0f));

        Tween flyIn = DOTween.To(() => uiDoc.rootVisualElement.transform.position,
                                x => uiDoc.rootVisualElement.transform.position = x,
                                new Vector3(0f, 0f, uiDoc.rootVisualElement.transform.position.z), .65f)
                                .SetEase(Ease.OutQuart);

        yield return flyIn.WaitForCompletion();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        Tween flyout = DOTween.To(() => uiDoc.rootVisualElement.transform.position,
                                x => uiDoc.rootVisualElement.transform.position = x,
                                new Vector3(Screen.width, 0f, uiDoc.rootVisualElement.transform.position.z), .65f)
                                .SetEase(Ease.InQuart);

        yield return flyout.WaitForCompletion();
    }

    #endregion

}
