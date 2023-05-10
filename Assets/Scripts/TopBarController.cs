using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TopBarController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private UIDocument uiDoc;

    #endregion

    #region Private Variables

    private bool isShowing;
    private bool canClick;
    private bool coinDrawerOpen;
    private VisualElement backButton;
    private VisualElement coinsButton;
    private EventCallback<PointerDownEvent> onBackClick;

    #endregion

    #region Public Properties

    public VisualElement BackButton { get { return backButton; } }
    public VisualElement CoinsButton { get { return coinsButton; } }
    public bool IsShowing { get { return isShowing; } }
    public bool CanClick { get { return canClick; } set { canClick = value; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        backButton = uiDoc.rootVisualElement.Q<VisualElement>("BackButton");
        coinsButton = uiDoc.rootVisualElement.Q<VisualElement>("CoinsButton");

        VisualElement bar = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");
        //bar.transform.position = new Vector3(bar.transform.position.x, -100f, bar.transform.position.z);
        bar.transform.position = new Vector3(bar.transform.position.x,
                                -(uiDoc.rootVisualElement.style.paddingTop.value.value + 100f), //100 is topbar height
                                bar.transform.position.z);

        RectOffsetFloat safeMargins = uiDoc.rootVisualElement.panel.GetSafeArea();
        uiDoc.rootVisualElement.style.paddingTop = safeMargins.Top;
        uiDoc.rootVisualElement.style.paddingLeft = safeMargins.Left;
        uiDoc.rootVisualElement.style.paddingRight = safeMargins.Right;

        coinDrawerOpen = false;
        CanClick = true;

        CoinsButton.RegisterCallback<PointerDownEvent>(CoinButtonClicked);
    }

    #endregion

    #region Public Functions

    public void ShowTopBar(bool show = true)
    {
        VisualElement bar = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");

        Tween hideShow = DOTween.To(() => bar.transform.position,
                        x => bar.transform.position = x,
                        //new Vector3(bar.transform.position.x, show ? 0f : -100f, bar.transform.position.z), .15f)
                        new Vector3(bar.transform.position.x,
                            show ? 0f : -(uiDoc.rootVisualElement.style.paddingTop.value.value + 100f), //100 is topbar height
                            bar.transform.position.z), .15f)
                        .SetEase(Ease.InOutQuad).Play().OnComplete(() => { isShowing = show; });

        //uiDoc.rootVisualElement.Show(show);
    }

    public void UpdateBackButtonOnClick(EventCallback<PointerDownEvent> evt)
    {
        BackButton.UnregisterCallback<PointerDownEvent>(onBackClick);

        onBackClick = (x) => 
        {
            if (!CanClick)
                return;

            evt.Invoke(x);
        };

        BackButton.RegisterCallback<PointerDownEvent>(onBackClick);
    }

    public EventCallback<PointerDownEvent> GetCurrentBackButtonEvent()
    {
        return onBackClick;
    }

    public void CoinButtonClicked(PointerDownEvent evt)
    {
        if (!canClick)
            return;

        if (coinDrawerOpen)
        {
            PageManager.instance.StartCoroutine(PageManager.instance.CloseTopPage());

            coinDrawerOpen = false;
        }
        else
        {
            PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<CoinDrawer>());

            coinDrawerOpen = true;
        }
    }

    #endregion

    #region Private Functions

    #endregion
}
