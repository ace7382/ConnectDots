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
        public int                  ColorIndex;
        public Vector2Int           StartPosition;
        public bool                 NodeUnlocked;
        public VisualElement        StartNode;
        public VisualElement        EndNode;
        public List<UIToolkitLine>  Lines;

        public ProductLine(int colorIndex, Vector2Int startPosition, bool unlocked)
        {
            ColorIndex          = colorIndex;
            StartPosition       = startPosition;
            NodeUnlocked        = unlocked;
            StartNode           = null;
            EndNode             = null;
            Lines               = new List<UIToolkitLine>();
        }
    }

    #endregion

    #region Private Consts

    private const float     DETAILS_LEFT_WIDTH  = 702f;
    private const float     DETAILS_RIGHT_WIDTH = 378f;
    private const float     SPACING             = 25f;
    private const float     GRID_SIZE           = 150f;
    private const int       GRID_X_MIN          = -10;
    private const int       GRID_X_MAX          = 10;
    private const int       GRID_Y_MIN          = -10;
    private const int       GRID_Y_MAX          = 10;
    private const float     PL_TAB_SHOWING      = 420f;
    private const float     PL_TAB_HIDDEN       = 420f - 75f; //75 is pl tab height

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
    private Dictionary<Vector2Int, bool> visibleNodes;

    private EventCallback<PointerUpEvent> purchaseButtonAction;

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
        visibleNodes        = new Dictionary<Vector2Int, bool>();

        for (int i = GRID_X_MIN; i <= GRID_X_MAX; i++)
        {
            for (int j = GRID_Y_MIN; j <= GRID_Y_MAX; j++)
            {
                visibleNodes.Add(new Vector2Int(i, j), false);
            }
        }

        visibleNodes[Vector2Int.zero] = true;

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

        this.RemoveObserver(OnItemPurchase, Notifications.ITEM_PURCHASED);
    }

    #endregion

    #region Private Functions

    private void SetupShop()
    {
        Vector2 zeroLocation            = new Vector2(shopBoard.resolvedStyle.width / 2f, shopBoard.resolvedStyle.height / 2f);

        List<ShopItem> shopItems        = Resources.LoadAll<ShopItem>("ShopItems").ToList();

        List<Vector2Int> ownedNodes     = ShopManager.instance.OwnedNodes();

        detailsButton
            .transform.position         = new Vector3(
                                            -1f * detailsButton.localBound.width
                                            , detailsButton.transform.position.y
                                            , detailsButton.transform.position.z
                                        );

        VisualElement detailsPLTab      = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");
        detailsPLTab.style.bottom       = PL_TAB_HIDDEN;
        detailsPLTab.transform.position = new Vector3(
                                            -1f * Screen.width
                                            , detailsPLTab.transform.position.y
                                            , detailsPLTab.transform.position.z
                                        );

        for (int i = 0; i < ownedNodes.Count; i++)
        {
            SetVisibleNodes(ownedNodes[i]);
        }

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

            if (visibleNodes.ContainsKey(shopItem.Position))
            {
                if (visibleNodes[shopItem.Position])
                {
                    buttonBG.SetColor(UIManager.instance.GetColor(0));
                    icon.style
                        .backgroundImage = shopItem.GetIcon();
                }
                else
                {
                    buttonBG.SetColor(Color.grey); //TODO: This better lol
                    icon.style
                        .backgroundImage = null;
                }
            }
            else
            {
                Debug.Log("ShopItem " + shopItem.name + " has a Position outside of the shop grid size");

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

        this.AddObserver(OnItemPurchase, Notifications.ITEM_PURCHASED);

        SetupProductLines();
    }

    private void OnItemPurchase(object sender, object info)
    {
        //sender    -   ShopItem    -   The ShopItem that was purchased

        //Get the VE of the purchased item
        //  Spin and recolor the purchased item's node
        //Find all nodes next to the purchased node
        //  if the node is not revealed, spin and reveal the node
        //Draw the correct prodcut line to the purchased item's node

        ShopItem boughtItem                     = (ShopItem)sender;

        VisualElement boughtItemNode            = null;
        List<VisualElement> newlyOpenedNodes    = new List<VisualElement>();
        List<ProductLine> newlyOpenedPLs        = new List<ProductLine>();

        SetVisibleNodes(boughtItem.Position);

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
                        ProductLine p = productLines[i];

                        SetVisibleNodes(p.StartPosition);

                        newlyOpenedPLs.Add(p);

                        count++;
                    }

                    if (count == 4)
                        break;
                }
            }
            else if (featItem.Feat == ShopItem_UnlockFeature.Feature.PRODUCT_LINE_TAB)
            {
                VisualElement productLineTab = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");
                Label label = productLineTab.Q<Label>();

                ShopItem s = SelectedShopNode.userData as ShopItem;

                //TODO: Get the color's name once that functionality is built
                label.text = s.ProductLine.ToString() + " Product Line";
                productLineTab.SetColor(UIManager.instance.GetColor(s.ProductLine));
                productLineTab.SetBorderColor(s.GetColor());

                Tween tabUp = DOTween.To(
                                    () => productLineTab.style.bottom.value.value
                                    , x => productLineTab.style.bottom = x
                                    , PL_TAB_SHOWING
                                    , .25f)
                                .SetEase(Ease.OutQuart);

                tabUp.Play();
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            ShopItem currentSI      = (ShopItem)nodes[i].userData;
            VisualElement buttonBG  = nodes[i].Q<VisualElement>("LevelSelectButton");

            if (visibleNodes.ContainsKey(currentSI.Position))
            {
                if (visibleNodes[currentSI.Position] && buttonBG.style.backgroundColor.value == Color.grey)
                {
                    newlyOpenedNodes.Add(nodes[i]);
                }
            }

            if (currentSI == boughtItem)
                boughtItemNode = nodes[i];
        }

        //------
        
        Sequence seq                = DOTween.Sequence();
        float animationLength       = .5f;

        Tween shrinkPurchased       = DOTween.To(
                                        () => boughtItemNode.transform.scale
                                        , x => boughtItemNode.style.scale = x
                                        , Vector3.zero
                                        , animationLength / 2f)
                                    .SetEase(Ease.InQuad)
                                    .OnComplete(() =>
                                        {
                                            VisualElement bg    = boughtItemNode.Q<VisualElement>("LevelSelectButton");
                                            VisualElement icon  = boughtItemNode.Q<VisualElement>("Icon");
                                            Color c             = UIManager.instance.GetColor(boughtItem.ProductLine);

                                            icon.Hide();
                                            bg.SetColor(new Color(
                                                c.r + (1f - c.r) * .8f
                                                , c.g + (1f - c.g) * .8f
                                                , c.b + (1f - c.b) * .8f
                                                , 1f));
                                        })
                                    .Pause();

        Tween resizePurchased       = DOTween.To(
                                        () => boughtItemNode.transform.scale
                                        , x => boughtItemNode.style.scale = x
                                        , Vector3.one
                                        , animationLength / 2f)
                                    .SetEase(Ease.OutQuad)
                                    .Pause();

        seq.Append(shrinkPurchased);
        seq.Append(resizePurchased);

        for (int i = 0; i < newlyOpenedNodes.Count; i++)
        {
            VisualElement node      = newlyOpenedNodes[i];
            Tween shrinkNode        = DOTween.To(
                                            () => node.transform.scale
                                            , x => node.style.scale = x
                                            , Vector3.zero
                                            , animationLength / 2f)
                                        .SetEase(Ease.InQuad)
                                        .OnComplete(() =>
                                        {
                                            VisualElement bg    = node.Q<VisualElement>("LevelSelectButton");
                                            VisualElement icon  = node.Q<VisualElement>("Icon");
                                            Color c             = UIManager.instance.GetColor(0);

                                            icon.style
                                                .backgroundImage= ((ShopItem)node.userData).GetIcon();
                                            bg.SetColor(c);
                                        })
                                        .Pause();

            if (i == 0)
                seq.Append(shrinkNode);
            else
                seq.Join(shrinkNode);
        }

        for (int i = 0; i < newlyOpenedPLs.Count; i++)
        {
            ProductLine pl          = newlyOpenedPLs[i];
            
            Tween shrinkNode        = DOTween.To(
                                            () => pl.StartNode.transform.scale
                                            , x => pl.StartNode.style.scale = x
                                            , Vector3.zero
                                            , animationLength / 2f)
                                        .SetEase(Ease.InQuad)
                                        .OnComplete(() =>
                                        {
                                            VisualElement bg = pl.StartNode.Q<VisualElement>("LevelSelectButton");
                                            Color c = UIManager.instance.GetColor(pl.ColorIndex);

                                            bg.SetColor(new Color(
                                            c.r + (1f - c.r) * .8f
                                            , c.g + (1f - c.g) * .8f
                                            , c.b + (1f - c.b) * .8f
                                            , 1f));
                                        })
                                        .Pause();

            if (i == 0 && newlyOpenedNodes.Count == 0)
                seq.Append(shrinkNode);
            else
                seq.Join(shrinkNode);
        }

        for (int i = 0; i < newlyOpenedNodes.Count; i++)
        {
            VisualElement node      = newlyOpenedNodes[i];

            Tween resizeNode        = DOTween.To(
                                            () => node.transform.scale
                                            , x => node.style.scale = x
                                            , Vector3.one
                                            , animationLength / 2f)
                                        .SetEase(Ease.OutQuad)
                                        .Pause();

            if (i == 0)
                seq.Append(resizeNode);
            else
                seq.Join(resizeNode);
        }

        for (int i = 0; i < newlyOpenedPLs.Count; i++)
        {
            ProductLine pl          = newlyOpenedPLs[i];
            Tween resizeNode        = DOTween.To(
                                            () => pl.StartNode.transform.scale
                                            , x => pl.StartNode.style.scale = x
                                            , Vector3.one
                                            , animationLength / 2f)
                                        .SetEase(Ease.OutQuad)
                                        .OnStart(() => DrawProductLineStartPoint(pl))
                                        .Pause();

            if (i == 0 && newlyOpenedNodes.Count == 0)
                seq.Append(resizeNode);
            else
                seq.Join(resizeNode);
        }

        seq.Play();

        if (boughtItem.ProductLine != 0)
        {
            VisualElement buttonBG  = boughtItemNode.Q<VisualElement>("LevelSelectButton");
            VisualElement icon      = boughtItemNode.Q<VisualElement>("Icon");
            Vector2 destination     = shopBoard.WorldToLocal(buttonBG.worldBound.center);

            canClick                = false;

            Tween draw              = productLines.Find(x => boughtItem.ProductLine == x.ColorIndex).Lines[0]
                                        .DrawTowardNewPoint_Tween(destination, .3f)
                                        .OnComplete(() => canClick = true)
                                        .Play();
        }
    }

    private void SetupProductLines()
    {
        List<ProductLine> newProductLines  = new List<ProductLine>()
        {
            new     ProductLine(1   , new Vector2Int(1, 0)  , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(2   , new Vector2Int(-1, 0) , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(3   , new Vector2Int(0, 1)  , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(4   , new Vector2Int(0, -1) , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
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
            shopButton.style.left       = zeroLocation.x + ((GRID_SIZE + SPACING) * productLine.StartPosition.x);
            shopButton.style.top        = zeroLocation.y + ((GRID_SIZE + SPACING) * productLine.StartPosition.y);
            
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
                
                DrawProductLineStartPoint(productLine);
            }
            else
            {
                buttonBG.SetColor(Color.grey);
            }
        }
    }

    private void DrawProductLineStartPoint(ProductLine productLine)
    {
        Vector2 origin = shopBoard.WorldToLocal(productLine.StartNode.Q<VisualElement>("LevelSelectButton").worldBound.center);

        UIToolkitCircle endPoint = new UIToolkitCircle(
                                origin
                                , GRID_SIZE / 4f
                                , UIManager.instance.GetColor(productLine.ColorIndex)
                            ); ;

        shopBoard.Add(endPoint);
        endPoint.BringToFront();

        UIToolkitLine line = new UIToolkitLine(
                                new List<Vector2>() { endPoint.Center }
                                , GRID_SIZE / 3f * .75f
                                , UIManager.instance.GetColor(productLine.ColorIndex)
                                , LineCap.Round
                            );

        shopBoard.Add(line);
        line.BringToFront();

        productLine.Lines.Add(line);
    }

    private void SetVisibleNodes(Vector2Int position)
    {
        Vector2Int up       = new Vector2Int(position.x, position.y + 1);
        Vector2Int right    = new Vector2Int(position.x + 1, position.y);
        Vector2Int down     = new Vector2Int(position.x, position.y - 1);
        Vector2Int left     = new Vector2Int(position.x - 1, position.y);

        if (visibleNodes.ContainsKey(up))
            visibleNodes[up] = true;
        if (visibleNodes.ContainsKey(right))
            visibleNodes[right] = true;
        if (visibleNodes.ContainsKey(down))
            visibleNodes[down] = true;
        if (visibleNodes.ContainsKey(left))
            visibleNodes[left] = true;
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
        //TODO: Dragging/Zooming should cancel out opening a node

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
        VisualElement productLineTab        = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");
        Label productLineLabel              = productLineTab.Q<Label>();

        container.Clear();
        costScrollView.ClearWithChildBoundIndicators(topIndicator, bottomIndicator);

        ShopItem item                       = content.userData as ShopItem;

        productLineLabel.text               = item.ProductLine.ToString() + " Product Line";
        productLineTab.SetColor(UIManager.instance.GetColor(item.ProductLine));
        productLineTab.SetBorderColor(item.GetColor());

        if (!visibleNodes[item.Position])
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

                purchaseButton.UnregisterCallback<PointerUpEvent>(purchaseButtonAction);

                purchaseButton.SetColor(Color.clear);

                if (CurrencyManager.instance.CanAfford(item))
                {
                    purchaseButton.SetBorderColor(Color.green);
                    purchaseButtonLabel
                        .style.color = Color.green;
                    purchaseButtonLabel.text = "Purchase";

                    purchaseButtonAction = (evt) => { OnPurchase(item, evt); };
                    purchaseButton.RegisterCallback<PointerUpEvent>(purchaseButtonAction);
                }
                else
                {
                    purchaseButton.SetBorderColor(Color.grey);
                    purchaseButtonLabel
                        .style.color = Color.grey;
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

        VisualElement productLineTab = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");

        Sequence seq = DOTween.Sequence();

        Tween panelIn = DOTween.To(
                            () => detailsPanel.transform.position
                            , x => detailsPanel.transform.position = x
                            , new Vector3(
                                0f
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        Tween tabIn =   DOTween.To(
                            () => productLineTab.transform.position
                            , x => productLineTab.transform.position = x
                            , new Vector3(
                                0f
                                , productLineTab.transform.position.y
                                , productLineTab.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        seq.Append(panelIn);
        seq.Join(tabIn);

        yield return seq.WaitForCompletion();

        if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.PRODUCT_LINE_TAB))
        {
            Label label                     = productLineTab.Q<Label>();

            ShopItem s                      = SelectedShopNode.userData as ShopItem;

            //TODO: Get the color's name once that functionality is built
            label.text                      = s.ProductLine.ToString() + " Product Line";
            productLineTab.SetColor(UIManager.instance.GetColor(s.ProductLine));
            productLineTab.SetBorderColor(s.GetColor());

            Tween tabUp =   DOTween.To(
                                () => productLineTab.style.bottom.value.value
                                , x => productLineTab.style.bottom = x
                                , PL_TAB_SHOWING
                                , .25f)
                            .SetEase(Ease.OutQuart);

            yield return tabUp.WaitForCompletion();
        }
    }

    private IEnumerator DetailsPanelOut()
    {
        if (detailsPanel.transform.position.x == -1f * Screen.width)
            yield break;

        VisualElement productLineTab = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");

        if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.PRODUCT_LINE_TAB))
        {
            Label label                     = productLineTab.Q<Label>();

            ShopItem s                      = SelectedShopNode.userData as ShopItem;

            //TODO: Get the color's name once that functionality is built
            label.text                      = s.ProductLine.ToString() + " Product Line";
            productLineTab.SetColor(UIManager.instance.GetColor(s.ProductLine));

            Tween tabDown = DOTween.To(
                                () => productLineTab.style.bottom.value.value
                                , x => productLineTab.style.bottom = x
                                , PL_TAB_HIDDEN
                                , .25f)
                            .SetEase(Ease.OutQuart)
                            .Play();
        }

        Sequence seq    = DOTween.Sequence();

        Tween tabOut    = DOTween.To(
                            () => productLineTab.transform.position
                            , x => productLineTab.transform.position = x
                            , new Vector3(
                                -1f * Screen.width
                                , productLineTab.transform.position.y
                                , productLineTab.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        Tween panelOut  = DOTween.To(
                            () => detailsPanel.transform.position
                            , x => detailsPanel.transform.position = x
                            , new Vector3(
                                -1f * Screen.width
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        seq.Append(tabOut);
        seq.Join(panelOut);

        yield return seq.WaitForCompletion();
    }

    #endregion
}
