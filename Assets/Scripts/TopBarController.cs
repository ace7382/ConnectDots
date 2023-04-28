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

    private VisualElement backButton;
    private VisualElement coinsButton;
    private EventCallback<PointerDownEvent> onBackClick;

    #endregion

    #region Public Properties

    public VisualElement BackButton { get { return backButton; } }
    public VisualElement CoinsButton { get { return coinsButton; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        backButton = uiDoc.rootVisualElement.Q<VisualElement>("BackButton");
        coinsButton = uiDoc.rootVisualElement.Q<VisualElement>("CoinsButton");

        VisualElement bar = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");
        bar.transform.position = new Vector3(bar.transform.position.x, -100f, bar.transform.position.z);
    }

    #endregion

    #region Public Functions

    public void ShowTopBar(bool show = true)
    {
        VisualElement bar = uiDoc.rootVisualElement.Q<VisualElement>("TopBar");

        Tween hideShow = DOTween.To(() => bar.transform.position,
                        x => bar.transform.position = x,
                        new Vector3(bar.transform.position.x, show ? 0f : -100f, bar.transform.position.z), .15f)
                        .SetEase(Ease.InOutQuad).Play();

        //uiDoc.rootVisualElement.Show(show);
    }

    public void UpdateBackButtonOnClick(EventCallback<PointerDownEvent> evt)
    {
        BackButton.UnregisterCallback<PointerDownEvent>(onBackClick);

        onBackClick = evt;

        BackButton.RegisterCallback<PointerDownEvent>(onBackClick);
    }

    #endregion
}
