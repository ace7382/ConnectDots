using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Shop : Page
{
    #region Private Structs
    
    private struct ProductLine
    {
        public int              ColorIndex;
        public Vector2          GridOrigin;
        public bool             NodeUnlocked;
        public VisualElement    StartNode;
        public VisualElement    EndNode;

        public ProductLine(int colorIndex, Vector2 gridOrigin, bool unlocked)
        {
            ColorIndex          = colorIndex;
            GridOrigin          = gridOrigin;
            NodeUnlocked        = unlocked;
            StartNode           = null;
            EndNode             = null;
        }
    }

    #endregion

    #region Private Consts

    private const float     DETAILS_LEFT_WIDTH  = 702f;
    private const float     DETAILS_RIGHT_WIDTH = 378f;
    private const float     SPACING             = 25f;
    private const float     GRID_SIZE           = 150f;

    #endregion

    #region Private Variables

    private bool            canClick;

    private VisualElement   screen;
    private VisualElement   shopBoard;
    private VisualElement   detailsPanel;
    private VisualElement   detailsButton;
    private VisualElement   closeDetails;
    private ScrollView      costScrollView;
    private VisualElement   purchaseButton;

    private int             primaryTouchID;
    private int             secondaryTouchID;

    private Vector2         primaryOrigin;
    private Vector2         secondaryOrigin;
    private Vector2         primaryOrigin_Screen;
    private Vector2         secondaryOrigin_Screen;

    private Vector2         zoomBounds;
    private float           zoomSpeed;

    private float           zoomStartingDistance;
    private float           zoomStartingScale;

    private VisualElement   selectedShopNode;

    private List<VisualElement> nodes;
    private List<ProductLine>   productLines;

    #endregion

    #region Private Properties

    private bool            Dragging            { get { return primaryTouchID != -99; } }
    private bool            Zooming             { get { return secondaryTouchID != -99; } }

    private VisualElement   SelectedShopNode
    {
        get { return selectedShopNode; }
        set
        {
            if (selectedShopNode == value)
            {
                //Unselect and hide details panel

                return;
            }
            
            if (selectedShopNode != null)
            {
                VisualElement old = selectedShopNode;

                old.Q<VisualElement>("SelectionBorder").Hide();
                
                //TODO: I resize the category select buttons on select.
                //Not sure if i want to do that here though
            }

            selectedShopNode = value;

            if (selectedShopNode != null)
            {
                selectedShopNode.Q<VisualElement>("SelectionBorder").Show();
            }

            SetDetailsPanel(selectedShopNode);
        }
    }

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        screen              = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        shopBoard           = uiDoc.rootVisualElement.Q<VisualElement>("ShopBoard");
        detailsPanel        = uiDoc.rootVisualElement.Q<VisualElement>("DetailsPanel");
        detailsButton       = uiDoc.rootVisualElement.Q<VisualElement>("DetailsButton");
        closeDetails        = detailsPanel.Q<VisualElement>("CloseButton");
        costScrollView      = detailsPanel.Q<ScrollView>("CostScroll");
        purchaseButton      = detailsPanel.Q<VisualElement>("PurchaseButton");

        primaryTouchID      = -99;
        secondaryTouchID    = -99;
        
        zoomSpeed           = .02f;

        nodes               = new List<VisualElement>();
        productLines        = new List<ProductLine>();

        shopBoard.RegisterCallback<PointerDownEvent>(OnPointerDownOnShopBoard);
        shopBoard.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnShopBoard);
        shopBoard.RegisterCallback<PointerUpEvent>(OnPointerUpOnShopBoard);
        shopBoard.RegisterCallback<WheelEvent>(OnMouseScroll);
        screen.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveScreen);


        detailsPanel.transform
            .position       = new Vector3(
                                -1 * Screen.width
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z
                            );

        detailsButton.RegisterCallback<PointerUpEvent>((evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(ShowDetailsPanel());
        });

        closeDetails.RegisterCallback<PointerUpEvent>((evt) =>
        {
            if (!canClick)
                return;

            PageManager.instance.StartCoroutine(CloseDetailsPanel());
        });
    }

    public override IEnumerator AnimateIn()
    {
        canClick            = false;

        yield return null; //wait a frame to be able to reference the board's size

        //TODO: Handle device safe area
        //      Will need to handle it on bounds check and on zoom size setup
        //TODO: re calculate this on screen size change
        zoomBounds = new Vector2(
                                Mathf.Max(
                                    .5f
                                    , Screen.width / shopBoard.resolvedStyle.width
                                    , Screen.height / shopBoard.resolvedStyle.height
                                )
                                , 2.5f
                            );

        SetupShop();

        canClick = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick = false;

        return null;
    }

    public override void HidePage()
    {
        shopBoard.UnregisterCallback<PointerDownEvent>(OnPointerDownOnShopBoard);
        shopBoard.UnregisterCallback<PointerMoveEvent>(OnPointerMoveOnShopBoard);
        shopBoard.UnregisterCallback<PointerUpEvent>(OnPointerUpOnShopBoard);
        shopBoard.UnregisterCallback<WheelEvent>(OnMouseScroll);

        screen.UnregisterCallback<PointerLeaveEvent>(OnPointerLeaveScreen);

        this.RemoveObserver(CheckNodeUnlocked, Notifications.ITEM_PURCHASED);
    }

    #endregion

    #region Private Functions

    private void SetupShop()
    {
        Vector2 zeroLocation            = new Vector2(shopBoard.resolvedStyle.width / 2f, shopBoard.resolvedStyle.height / 2f);

        List<ShopItem> shopItems        = Resources.LoadAll<ShopItem>("ShopItems").ToList();

        for (int i = 0; i < shopItems.Count; i++)
        {
            ShopItem shopItem           = shopItems[i];
            VisualElement shopButton    = UIManager.instance.LevelSelectButton.Instantiate();
            VisualElement buttonBG      = shopButton.Q<VisualElement>("LevelSelectButton");
            VisualElement icon          = shopButton.Q<VisualElement>("Icon");
            VisualElement purchasedIcon = shopButton.Q<VisualElement>("CompletedIcon");

            shopButton.style.position   = Position.Absolute;
            shopButton.style.left       = zeroLocation.x + ((GRID_SIZE + SPACING) * shopItem.Position.x);
            shopButton.style.top        = zeroLocation.y + ((GRID_SIZE + SPACING) * shopItem.Position.y);

            shopButton.userData         = shopItem;

            nodes.Add(shopButton);

            if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP)
                || (shopItem is ShopItem_UnlockFeature 
                    && ((ShopItem_UnlockFeature)shopItem).Feat == ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            {
                if (shopItem.NodeUnlocked)
                {
                    //buttonBG.SetColor(shopItem.GetColor());
                    buttonBG.SetColor(UIManager.instance.GetColor(0));
                    icon.style
                        .backgroundImage = shopItem.GetIcon();
                }
                else
                {
                    //Locked and hidden icon
                    buttonBG.SetColor(Color.grey); //TODO: This better lol
                    icon.style
                        .backgroundImage = null;
                }
            }
            else
            {
                //Locked and hidden icon
                buttonBG.SetColor(Color.grey); //TODO: This better lol
                icon.style
                    .backgroundImage = null;
            }

            shopButton.RegisterCallback<PointerUpEvent>((evt) =>
            {
                if (!canClick)
                    return;

                SelectedShopNode = shopButton;
            });

            purchasedIcon.RemoveFromHierarchy();
            buttonBG.SetBorderColor(Color.clear);

            shopButton.Q<Label>().RemoveFromHierarchy();
            shopBoard.Add(shopButton);
        }

        this.AddObserver(CheckNodeUnlocked, Notifications.ITEM_PURCHASED);

        SetupProductLines();
    }

    private void CheckNodeUnlocked(object sender, object info)
    {
        //sender    -   ShopItem    -   The ShopItem that was purchased

        ShopItem boughtItem         = (ShopItem)sender;

        //If the bought item unlocks a feature
        //  if the feature is shop unlocked
        //      draw blue line at point x,y
        //  if feature is  blue product line
        //      drwa at abc
        
        if (boughtItem is ShopItem_UnlockFeature)
        {
            ShopItem_UnlockFeature featItem = (ShopItem_UnlockFeature)boughtItem;

            if (featItem.Feat == ShopItem_UnlockFeature.Feature.UNLOCK_SHOP)
            {
                int count = 0;

                for (int i = 0; i < productLines.Count; i++)
                {
                    if (productLines[i].ColorIndex == 1 || productLines[i].ColorIndex == 2
                            || productLines[i].ColorIndex == 3 || productLines[i].ColorIndex == 4)
                    {
                        productLines[i].StartNode.Q<VisualElement>("LevelSelectButton").SetColor(UIManager.instance.GetColor(0));

                        DrawProductLineEndPoints(productLines[i]);

                        count++;
                    }

                    if (count == 4)
                        break;
                }
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            ShopItem currentSI      = (ShopItem)nodes[i].userData;

            if (!currentSI.PrePurchases.Contains(boughtItem))
                continue;

            VisualElement buttonBG  = nodes[i].Q<VisualElement>("LevelSelectButton");
            VisualElement icon      = nodes[i].Q<VisualElement>("Icon");

            if (currentSI.NodeUnlocked)
            {
                buttonBG.SetColor(UIManager.instance.GetColor(0));
                icon.style
                    .backgroundImage = currentSI.GetIcon();
            }
            else
            {
                //Locked and hidden icon
                buttonBG.SetColor(Color.grey); //TODO: This better lol
                icon.style
                    .backgroundImage = null;
            }
        }
    }

    private void SetupProductLines()
    {
        List<ProductLine> newProductLines  = new List<ProductLine>()
        {
            new     ProductLine(1   , new Vector2(1, 0)     , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(2   , new Vector2(-1, 0)    , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(3   , new Vector2(0, 1)     , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(4   , new Vector2(0, -1)    , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
        };

        Vector2 zeroLocation            = new Vector2(shopBoard.resolvedStyle.width / 2f, shopBoard.resolvedStyle.height / 2f);

        for (int i = 0; i < newProductLines.Count; i++)
        {
            ProductLine productLine     = newProductLines[i];

            VisualElement shopButton    = UIManager.instance.LevelSelectButton.Instantiate();
            VisualElement buttonBG      = shopButton.Q<VisualElement>("LevelSelectButton");
            VisualElement icon          = shopButton.Q<VisualElement>("Icon");
            VisualElement purchasedIcon = shopButton.Q<VisualElement>("CompletedIcon");

            shopButton.style.position   = Position.Absolute;
            shopButton.style.left       = zeroLocation.x + ((GRID_SIZE + SPACING) * productLine.GridOrigin.x);
            shopButton.style.top        = zeroLocation.y + ((GRID_SIZE + SPACING) * productLine.GridOrigin.y);
            
            purchasedIcon.RemoveFromHierarchy();
            icon.RemoveFromHierarchy();
            buttonBG.SetBorderColor(Color.clear);

            shopButton.userData         = productLine;
            productLine.StartNode       = shopButton;

            shopBoard.Add(shopButton);
            productLines.Add(productLine);

            if (productLine.NodeUnlocked)
            {
                buttonBG.SetColor(UIManager.instance.GetColor(0));
                
                DrawProductLineEndPoints(productLine);
            }
            else
            {
                buttonBG.SetColor(Color.grey);
            }
        }
    }

    private void DrawProductLineEndPoints(ProductLine productLine)
    {
        Vector2 origin = shopBoard.WorldToLocal(productLine.StartNode.Q<VisualElement>("LevelSelectButton").worldBound.center);

        UIToolkitCircle endPoint = new UIToolkitCircle(
                                origin//productLine.StartNode.worldBound.center
                                , GRID_SIZE / 4f
                                , UIManager.instance.GetColor(productLine.ColorIndex)
                            ); ;

        shopBoard.Add(endPoint);
        endPoint.BringToFront();
    }

    private void OnPointerDownOnShopBoard(PointerDownEvent evt)
    {
        //TODO: Opening details panel etc stops you from
        //      dragging/zooming for the length of the animation
        //      might want to change that bc it feels like i should be able to pan/zoom
        //      before the panel is fully in out

        if (!canClick)
            return;

        if (primaryTouchID == -99)
        {
            //If there isn't another pointer, this is the first and we want to start dragging logic
            primaryTouchID          = evt.pointerId;
            primaryOrigin           = evt.localPosition;

            primaryOrigin_Screen    = screen.WorldToLocal(shopBoard.LocalToWorld(primaryOrigin));
        }
        else if (secondaryTouchID == -99)
        {
            //If there isn't a second pointer, use this pointer as the secondary pointer and start zoom logic
            secondaryTouchID        = evt.pointerId;
            secondaryOrigin         = evt.localPosition;

            secondaryOrigin_Screen  = screen.WorldToLocal(shopBoard.LocalToWorld(secondaryOrigin));

            zoomStartingDistance    = 0f;
        }
    }

    private void OnPointerMoveOnShopBoard(PointerMoveEvent evt)
    {
        if (Zooming)
        {
            if (evt.pointerId == primaryTouchID)
                primaryOrigin_Screen    = screen.WorldToLocal(shopBoard.LocalToWorld(evt.localPosition));
            else if (evt.pointerId == secondaryTouchID)
                secondaryOrigin_Screen  = screen.WorldToLocal(shopBoard.LocalToWorld(evt.localPosition));
            else
                return;

            float distance              = Vector2.Distance(primaryOrigin_Screen, secondaryOrigin_Screen);

            if (zoomStartingDistance == 0f)
            {
                zoomStartingDistance    = distance;
                zoomStartingScale       = shopBoard.transform.scale.x;
            }
            else
            {
                float multiplier = distance / zoomStartingDistance;
                SetZoom(zoomStartingScale * multiplier);
            }
        }
        else if (Dragging)
        {
            if (evt.pointerId == primaryTouchID)
            {
                Vector2 delta           = (Vector2)evt.localPosition - primaryOrigin;
                delta                   *= shopBoard.transform.scale;

                if (shopBoard.worldBound.xMin + delta.x > 0f)
                    delta               = new Vector2(-1 * shopBoard.worldBound.xMin, delta.y);
                else if (shopBoard.worldBound.xMax + delta.x < Screen.width)
                    delta               = new Vector2(Screen.width - shopBoard.worldBound.xMax, delta.y);

                if (shopBoard.worldBound.yMin + delta.y > 0f)
                    delta               = new Vector2(delta.x, -1 * shopBoard.worldBound.yMin);
                else if (shopBoard.worldBound.yMax + delta.y < Screen.height)
                    delta               = new Vector2(delta.x, Screen.height - shopBoard.worldBound.yMax);

                shopBoard.style.left    = shopBoard.layout.x + delta.x;
                shopBoard.style.top     = shopBoard.layout.y + delta.y;
            }
        }
    }

    private void OnPointerUpOnShopBoard(PointerUpEvent evt)
    {
        RemoveTouch(evt.pointerId);
    }

    private void OnPointerLeaveScreen(PointerLeaveEvent evt)
    {
        RemoveTouch(evt.pointerId);
    }

    private void OnMouseScroll(WheelEvent evt)
    {
        SetZoom(shopBoard.transform.scale.x - (Mathf.Sign(evt.delta.y) * zoomSpeed));
    }

    private void SetZoom(float newScale)
    {
        newScale                    = Mathf.Clamp(newScale, zoomBounds.x, zoomBounds.y);

        if (shopBoard.transform.scale.x == newScale)
            return;

        //TODO: Something like this is needed to zoom into a specific spot
        //      otherwise it just scales at the center of the shopBoard
        //      it gets jittery with just the code below though.
        //map.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(tempPivot.x), new Length(tempPivot.y)));

        shopBoard.transform.scale   = new Vector3(newScale, newScale, 1f);

        Vector2 delta = Vector2.zero;

        if (shopBoard.worldBound.xMin + delta.x > 0f)
            delta                   = new Vector2(-1 * shopBoard.worldBound.xMin, delta.y);
        else if (shopBoard.worldBound.xMax + delta.x < Screen.width)
            delta                   = new Vector2(Screen.width - shopBoard.worldBound.xMax, delta.y);

        if (shopBoard.worldBound.yMin + delta.y > 0f)
            delta                   = new Vector2(delta.x, -1 * shopBoard.worldBound.yMin);
        else if (shopBoard.worldBound.yMax + delta.y < Screen.height)
            delta                   = new Vector2(delta.x, Screen.height - shopBoard.worldBound.yMax);

        shopBoard.style.left        = shopBoard.layout.x + delta.x;
        shopBoard.style.top         = shopBoard.layout.y + delta.y;
    }

    private void RemoveTouch(int pointerID)
    {
        if (pointerID == primaryTouchID)
        {
            if (secondaryTouchID != -99)
            {
                primaryTouchID          = secondaryTouchID;
                primaryOrigin           = secondaryOrigin;

                primaryOrigin_Screen    = secondaryOrigin_Screen;

                secondaryTouchID        = -99;
                secondaryOrigin         = Vector2.zero;

                secondaryOrigin_Screen  = Vector2.zero;
            }
            else
            {
                primaryTouchID          = -99;
                primaryOrigin           = Vector2.zero;

                primaryOrigin_Screen    = Vector2.zero;
            }
        }
        if (pointerID == secondaryTouchID)
        {
            secondaryTouchID            = -99;
            secondaryOrigin             = Vector2.zero;

            secondaryOrigin_Screen      = Vector2.zero;
        }
    }

    private void SetDetailsPanel(VisualElement content)
    {
        VisualElement container             = detailsPanel.Q<VisualElement>("LeftPanel");
        VisualElement topIndicator          = costScrollView.Q<VisualElement>("TopArrow");
        VisualElement bottomIndicator       = costScrollView.Q<VisualElement>("BottomArrow");

        container.Clear();
        costScrollView.ClearWithChildBoundIndicators(topIndicator, bottomIndicator);

        ShopItem item                       = content.userData as ShopItem;

        if (!item.NodeUnlocked)
        {
            SetDetailsMystery(); 
        }
        else
        {
            container.Add(item.GetDisplayContent());
            detailsPanel.SetBorderColor(item.GetColor());

            if (item.Purchased)
            {
                SetDetailsOwned(item);
            }
            else
            {
                detailsPanel.Q<VisualElement>("RightPanel").Show();
                detailsPanel.Q<VisualElement>("LeftPanel").SetWidth(DETAILS_LEFT_WIDTH);
                detailsPanel.Q<VisualElement>("PurchasedIcon").Hide();

                for (int i = 0; i < item.Costs.Count; i++)
                {
                    VisualElement costLine = UIManager.instance.CoinDisplay.Instantiate();

                    costLine.style.height = new StyleLength(StyleKeyword.Auto);
                    costLine.style.minHeight = costLine.style.height;
                    costLine.style.maxHeight = new StyleLength(StyleKeyword.None);

                    costLine.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(item.Costs[i].colorIndex));
                    costLine.Q<Label>("AmountLabel").text = CurrencyManager.instance.GetCoinsForColorIndex(item.Costs[i].colorIndex).ToString()
                                                            + " / " + item.Costs[i].amount.ToString();

                    costScrollView.Add(costLine);
                }

                costScrollView.SetBoundIndicators(topIndicator, bottomIndicator);
                costScrollView.verticalScroller.valueChanged += (evt) =>
                {
                    costScrollView.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator);
                };

                Label purchaseButtonLabel = purchaseButton.Q<Label>();

                purchaseButton.UnregisterCallback<PointerUpEvent>((evt) => OnPurchase(item, evt));
                purchaseButton.SetColor(Color.clear);

                if (!item.NodeUnlocked)
                {
                    //TODO: Don't think i'm going to use this.
                    //      I think i'm going to add tile border barriers with "remove" requirements

                    purchaseButton.SetBorderColor(Color.grey);
                    purchaseButtonLabel
                        .style.color = Color.grey;
                    purchaseButtonLabel.text = "Unlock Requirements";
                }
                else if (CurrencyManager.instance.CanAfford(item))
                {
                    purchaseButton.SetBorderColor(Color.green);
                    purchaseButtonLabel
                        .style.color = Color.green;
                    purchaseButtonLabel.text = "Purchase";

                    purchaseButton.RegisterCallback<PointerUpEvent>((evt) => OnPurchase(item, evt));
                }
                else
                {
                    purchaseButton.SetBorderColor(Color.red);
                    purchaseButton.SetColor(Color.grey);
                    purchaseButtonLabel
                        .style.color = Color.red;
                    purchaseButtonLabel.text = "Need More";
                }

                //TODO: when a cost list with few enough lines that it doesn't require scrolling, the bottom indicator
                //      still shows. Probably will be resolved with the TODO below
                //TODO: This should probably be moved into a coroutine by reworking this function. Just need it set on a seperate frame
                //      bc the scrollview's size is set at the begining of the functions, so this can't access it yet
                costScrollView.schedule.Execute(() => costScrollView.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator));
            }
        }
        PageManager.instance.StartCoroutine(ShowDetailsPanel());
    }

    private void SetDetailsOwned(ShopItem item)
    {
        VisualElement purchasedIcon = detailsPanel.Q<VisualElement>("PurchasedIcon");

        detailsPanel.Q<VisualElement>("RightPanel").Hide();
        detailsPanel.Q<VisualElement>("LeftPanel").SetWidth(DETAILS_LEFT_WIDTH + DETAILS_RIGHT_WIDTH);
        purchasedIcon.SetColor(item.GetColor());
        purchasedIcon.Show();
    }

    private void SetDetailsMystery()
    {
        VisualElement purchasedIcon = detailsPanel.Q<VisualElement>("PurchasedIcon");

        detailsPanel.Q<VisualElement>("RightPanel").Hide();
        VisualElement leftPanel = detailsPanel.Q<VisualElement>("LeftPanel");
        leftPanel.SetWidth(DETAILS_LEFT_WIDTH + DETAILS_RIGHT_WIDTH);

        Label questionMarks = new Label();
        questionMarks.text  = "?????";
        questionMarks.AddToClassList("ShopDescriptionText");
        questionMarks.style.flexGrow = 1f;
        questionMarks.style.alignSelf = Align.Center;
        questionMarks.style.unityTextAlign = TextAnchor.MiddleCenter;
        leftPanel.Add(questionMarks);

        purchasedIcon.Hide();
    }

    private void OnPurchase(ShopItem item, PointerUpEvent evt)
    {
        item.OnPurchase();

        SetDetailsOwned(item);
    }

    private IEnumerator ShowDetailsPanel()
    {
        canClick = false;

        yield return DetailsButtonOut();

        yield return DetailsPanelIn();

        canClick = true;
    }

    private IEnumerator CloseDetailsPanel()
    {
        canClick = false;

        yield return DetailsPanelOut();

        yield return DetailsButtonIn();

        canClick = true;
    }

    private IEnumerator DetailsButtonIn()
    {
        if (detailsButton.transform.position.x == 0f)
            yield break;

        Tween buttonIn = DOTween.To(
                            () => detailsButton.transform.position
                            , x => detailsButton.transform.position = x
                            , new Vector3(
                                0f
                                , detailsButton.transform.position.y
                                , detailsButton.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        yield return buttonIn.WaitForCompletion();
    }

    private IEnumerator DetailsButtonOut()
    {
        if (detailsButton.transform.position.x == -1f * detailsButton.localBound.width)
            yield break;

        Tween buttonOut = DOTween.To(
                            () => detailsButton.transform.position
                            , x => detailsButton.transform.position = x
                            , new Vector3(
                                -1f * detailsButton.localBound.width
                                , detailsButton.transform.position.y
                                , detailsButton.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        yield return buttonOut.WaitForCompletion();
    }

    private IEnumerator DetailsPanelIn()
    {
        if (detailsPanel.transform.position.x == 0f)
            yield break;

        Tween panelIn   = DOTween.To(
                            () => detailsPanel.transform.position
                            , x => detailsPanel.transform.position = x
                            , new Vector3(
                                0f
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        yield return panelIn.WaitForCompletion();
    }

    private IEnumerator DetailsPanelOut()
    {
        if (detailsPanel.transform.position.x == -1f * Screen.width)
            yield break;

        Tween panelOut  = DOTween.To(
                            () => detailsPanel.transform.position
                            , x => detailsPanel.transform.position = x
                            , new Vector3(
                                -1f * Screen.width
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);
        
        yield return panelOut.WaitForCompletion();

        detailsPanel.SetBorderColor(Color.black);
    }

    #endregion
}
