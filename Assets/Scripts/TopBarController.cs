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

    private bool                            isShowing;
    private bool                            canClick;
    private bool                            coinDrawerOpen;
    private VisualElement                   backButton;
    private VisualElement                   coinsButton;
    private EventCallback<ClickEvent>       onBackClick;
    private ButtonStateChanger              backBSC;
    private ButtonStateChanger              coinsBSC;

    #endregion

    #region Public Properties

    public VisualElement                    BackButton          { get { return backButton; } }
    public VisualElement                    CoinsButton         { get { return coinsButton; } }
    public bool                             IsShowing           { get { return isShowing; } }
    public bool                             CanClick            { get { return canClick; } set { canClick = value; } }
    public ButtonStateChanger               BackBSC             { get { return backBSC; } }
    public ButtonStateChanger               CoinsBSC            { get { return coinsBSC; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        backButton                                  = uiDoc.rootVisualElement.Q<VisualElement>("BackButton");
        coinsButton                                 = uiDoc.rootVisualElement.Q<VisualElement>("CoinsButton");

        VisualElement bar                           = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");
        bar.transform.position                      = new Vector3(bar.transform.position.x
                                                        , -(uiDoc.rootVisualElement.style.paddingTop.value.value + 160f) //160 is topbar height
                                                        //, -(uiDoc.rootVisualElement.style.paddingTop.value.value + bar.resolvedStyle.height) //160 is topbar height
                                                        , bar.transform.position.z);

        RectOffsetFloat safeMargins                 = uiDoc.rootVisualElement.panel.GetSafeArea();
        uiDoc.rootVisualElement.style.paddingTop    = safeMargins.Top;
        uiDoc.rootVisualElement.style.paddingLeft   = safeMargins.Left;
        uiDoc.rootVisualElement.style.paddingRight  = safeMargins.Right;

        coinDrawerOpen                              = false;
        CanClick                                    = true;

        CoinsButton.RegisterCallback<ClickEvent>(CoinButtonClicked);

        backBSC                                     = new ButtonStateChanger(backButton.Q<VisualElement>("BG"));
        coinsBSC                                    = new ButtonStateChanger(coinsButton.Q<VisualElement>("BG"));

        backButton.RegisterCallback<PointerDownEvent>(backBSC.OnPointerDown);
        coinsButton.RegisterCallback<PointerDownEvent>(coinsBSC.OnPointerDown);

        uiDoc.rootVisualElement.RegisterCallback<PointerUpEvent>(backBSC.OnPointerUp);
        uiDoc.rootVisualElement.RegisterCallback<PointerLeaveEvent>(backBSC.OnPointerOff);
        uiDoc.rootVisualElement.RegisterCallback<PointerUpEvent>(coinsBSC.OnPointerUp);
        uiDoc.rootVisualElement.RegisterCallback<PointerLeaveEvent>(coinsBSC.OnPointerOff);
    }

    #endregion

    #region Public Functions

    public void ShowTopBar(bool show = true)
    {
        VisualElement bar = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");

        Tween hideShow = DOTween.To(() => bar.transform.position,
                        x => bar.transform.position = x,
                        new Vector3(bar.transform.position.x
                            , show ? 0f : -(uiDoc.rootVisualElement.style.paddingTop.value.value + 160f) //160 is topbar height
                            //, show ? 0f : -(uiDoc.rootVisualElement.style.paddingTop.value.value + bar.resolvedStyle.height) //160 is topbar height
                            , bar.transform.position.z), .15f)
                        .SetEase(Ease.InOutQuad).Play().OnComplete(() => { isShowing = show; });

        //uiDoc.rootVisualElement.Show(show);
    }

    public void UpdateBackButtonOnClick(EventCallback<ClickEvent> evt)
    {
        BackButton.UnregisterCallback<ClickEvent>(onBackClick);

        onBackClick = (x) => 
        {
            if (!CanClick)
                return;

            evt.Invoke(x);
        };

        BackButton.RegisterCallback<ClickEvent>(onBackClick);
    }

    public EventCallback<ClickEvent> GetCurrentBackButtonEvent()
    {
        return onBackClick;
    }

    public void CoinButtonClicked(ClickEvent evt)
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

    public void ShowCoinButton(bool show)
    {
        coinsButton.Show(show);
    }

    #endregion

    #region Private Functions

    #endregion
}
