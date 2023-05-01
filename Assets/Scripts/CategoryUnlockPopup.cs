using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CategoryUnlockPopup : Page
{
    #region Private Variables

    private bool canClick;

    #endregion

    #region Inherited Functions

    public override IEnumerator AnimateIn()
    {
        canClick = false;

        uiDoc.rootVisualElement.style.translate = new StyleTranslate(new Translate(0f, new Length(100f, LengthUnit.Percent)));

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

        Tween flyOut = DOTween.To(() => uiDoc.rootVisualElement.transform.position,
                        x => uiDoc.rootVisualElement.transform.position = x,
                        new Vector3(0f, Screen.height, uiDoc.rootVisualElement.transform.position.z), .65f)
                        .SetEase(Ease.InQuart);

        yield return flyOut.WaitForCompletion();

        yield return null;
    }

    public override void HidePage()
    {

    }

    public override void ShowPage(object[] args)
    {
        //args[0]   -   LevelCategory   - The level category to show requirements for

        LevelCategory cat = (LevelCategory)args[0];

        VisualElement unlockButton = uiDoc.rootVisualElement.Q<VisualElement>("UnlockButton");
        ScrollView reqScroll = uiDoc.rootVisualElement.Q<ScrollView>();

        bool canUnlock = true;

        for (int i = 0; i < cat.REQS_Purchase.Count; i++)
        {
            VisualElement req = UIManager.instance.RequirementDisplay.Instantiate();

            VisualElement completeIcon = req.Q<VisualElement>("CompleteIcon");
            VisualElement incompleteIcon = req.Q<VisualElement>("IncompleteIcon");
            VisualElement reqIcon = req.Q<VisualElement>("ReqIcon");
            Label details = req.Q<Label>();
            int coins = CurrencyManager.instance.GetCoinsForColorIndex(cat.REQS_Purchase[i].colorIndex);

            details.text = string.Format("{0} / {1}"
                , coins
                , cat.REQS_Purchase[i].amount);

            reqIcon.SetColor(UIManager.instance.GetColor(cat.REQS_Purchase[i].colorIndex));
            reqIcon.SetBorderColor(Color.black);
            reqIcon.SetBorderWidth(2f);

            completeIcon.Show(coins >= cat.REQS_Purchase[i].amount);
            incompleteIcon.Show(!completeIcon.IsShowing());

            if (canUnlock)
                canUnlock = coins >= cat.REQS_Purchase[i].amount;

            reqScroll.contentContainer.Add(req);
        }

        for (int i = 0; i < cat.REQS_Category.Count; i++)
        {
            VisualElement req = UIManager.instance.RequirementDisplay.Instantiate();

            VisualElement completeIcon = req.Q<VisualElement>("CompleteIcon");
            VisualElement incompleteIcon = req.Q<VisualElement>("IncompleteIcon");
            VisualElement reqIcon = req.Q<VisualElement>("ReqIcon");
            Label details = req.Q<Label>();

            //VisualElement categoryIcon = UIManager.instance.LevelSelectButton.Instantiate();
            //categoryIcon.Q<VisualElement>("Icon").style.backgroundImage = cat.REQS_Category[i].LevelSelectImage;
            //categoryIcon.Q<VisualElement>("LevelSelectButton").SetColor(cat.REQS_Category[i].Color);
            //reqIcon.Add(categoryIcon);

            reqIcon.SetBorderRadius(15f);
            reqIcon.SetColor(cat.REQS_Category[i].Color);

            VisualElement icon = new VisualElement();
            icon.style.backgroundImage = cat.REQS_Category[i].LevelSelectImage;
            icon.SetWidth(25f);
            icon.SetHeight(25f);
            icon.style.position = Position.Absolute;
            icon.style.alignSelf = Align.Center;
            icon.style.top = new StyleLength(new Length(60f, LengthUnit.Percent));
            reqIcon.Add(icon);

            details.text = "Complete " + cat.REQS_Category[i].name;

            completeIcon.Show(cat.REQS_Category[i].IsComplete);
            incompleteIcon.Show(!completeIcon.IsShowing());

            reqScroll.contentContainer.Add(req);
        }

        if (cat.REQS_NumberOfObjectives > 0)
        {
            VisualElement req = UIManager.instance.RequirementDisplay.Instantiate();

            VisualElement completeIcon = req.Q<VisualElement>("CompleteIcon");
            VisualElement incompleteIcon = req.Q<VisualElement>("IncompleteIcon");
            VisualElement reqIcon = req.Q<VisualElement>("ReqIcon");
            Label details = req.Q<Label>();

            details.text = string.Format("{0} / {1}"
                            , ObjectiveManager.instance.CompletedObjectivesCount.ToString()
                            , cat.REQS_NumberOfObjectives.ToString());

            reqIcon.style.backgroundImage = UIManager.instance.TrophyTexture;
            reqIcon.style.unityBackgroundImageTintColor = Color.yellow;
            completeIcon.Show(ObjectiveManager.instance.CompletedObjectivesCount >= cat.REQS_NumberOfObjectives);
            incompleteIcon.Show(!completeIcon.IsShowing());

            reqScroll.contentContainer.Add(req);
        }

        for (int i = 0; i < cat.REQS_Objective.Count; i++)
        {
            VisualElement req = UIManager.instance.RequirementDisplay.Instantiate();

            VisualElement completeIcon = req.Q<VisualElement>("CompleteIcon");
            VisualElement incompleteIcon = req.Q<VisualElement>("IncompleteIcon");
            VisualElement reqIcon = req.Q<VisualElement>("ReqIcon");
            Label details = req.Q<Label>();

            details.text = cat.REQS_Objective[i].ID;
            reqIcon.style.backgroundImage = UIManager.instance.TrophyTexture;
            reqIcon.style.unityBackgroundImageTintColor = Color.yellow;
            completeIcon.Show(cat.REQS_Objective[i].IsComplete);
            incompleteIcon.Show(!completeIcon.IsShowing());

            reqScroll.contentContainer.Add(req);
        }

        EventCallback<PointerDownEvent> backButtonAction = (evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backButtonAction);

        uiDoc.rootVisualElement.Q<VisualElement>("Page").RegisterCallback<PointerUpEvent>((evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
        });

        if (canUnlock)
        {
            unlockButton.Q<Label>().text = "Unlock";
            unlockButton.SetColor(Color.green);
            unlockButton.RegisterCallback<PointerUpEvent>((evt) =>
            {
                cat.UnlockCategory();

                PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());
            });
        }
        else
        {
            unlockButton.Q<Label>().text = "Complete Requirements";
            unlockButton.SetColor(Color.grey);
        }
    }

    #endregion.
}
