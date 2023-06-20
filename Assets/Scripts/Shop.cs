using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Shop : Page
{
    #region Private Structs
    
    private class ProductLine
    {
        public int                  ColorIndex;
        public Vector2Int           StartPosition;
        public bool                 NodeUnlocked;
        public VisualElement        StartNode;
        public VisualElement        EndNode;
        public List<UIToolkitLine>  Lines;

        public UIToolkitLine        LastLine { get { return Lines[Lines.Count - 1]; } }

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

    private const float     SPACING             = 25f;
    private const float     GRID_SIZE           = 150f;
    private const float     SELECTION_BORDER_ADD= 8f;   //This is the extra size set on the seleciton border. It's hard coded into the LevelSelectButton's UI file
    private const int       GRID_X_MIN          = -10;
    private const int       GRID_X_MAX          = 10;
    private const int       GRID_Y_MIN          = -10;
    private const int       GRID_Y_MAX          = 10;

    #endregion

    #region Private Variables

    private bool            canClick;

    private bool            canClickButtons;

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

    private List<ProductLine>   productLines;

    private Dictionary<Vector2Int, VisualElement> VEbyPosition;
    private Dictionary<VisualElement, bool> VEsRevealed;

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
                selectedShopNode.Q<VisualElement>("SelectionBorder").Hide();
                selectedShopNode = null;

                //TODO: This doesn't set canClick to false/true at the end.
                //      dont think it causes any issues but might want to review eventually
                PageManager.instance.StartCoroutine(DetailsPanelOut());
                PageManager.instance.StartCoroutine(DetailsButtonOut());

                return;
            }
            
            if (selectedShopNode != null)
            {
                VisualElement old = selectedShopNode;

                old.Q<VisualElement>("SelectionBorder").Hide();
            }

            selectedShopNode = value;

            if (selectedShopNode != null)
            {
                selectedShopNode.Q<VisualElement>("SelectionBorder").Show();
            }

            PageManager.instance.StartCoroutine(SetDetailsPanel(selectedShopNode));
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

        productLines        = new List<ProductLine>();

        VEbyPosition        = new Dictionary<Vector2Int, VisualElement>();
        VEsRevealed         = new Dictionary<VisualElement, bool>();

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
            //if (!canClick)
            //    return;

            if (!canClickButtons)
                return;

            PageManager.instance.StartCoroutine(ShowDetailsPanel());
        });

        closeDetails.RegisterCallback<PointerUpEvent>((evt) =>
        {
            //if (!canClick)
            //    return;

            if (!canClickButtons)
                return;

            PageManager.instance.StartCoroutine(CloseDetailsPanel());
        });

        EventCallback<PointerDownEvent> backbuttonAction = (evt) =>
        {
            //if (!canClick)
            //    return;

            if (!canClickButtons)
                return;

            PageManager.instance.StartCoroutine(PageManager.instance.OpenPageOnAnEmptyStack<MainMenu>(null, false));
        };

        UIManager.instance.TopBar.UpdateBackButtonOnClick(backbuttonAction);
    }

    public override IEnumerator AnimateIn()
    {
        VisualElement page  = uiDoc.rootVisualElement;
        canClick            = false;
        canClickButtons     = false;

        page.SetOpacity(0f);

        if (!UIManager.instance.TopBar.IsShowing)
            UIManager.instance.TopBar.ShowTopBar();

        yield return null; //wait a frame to be able to reference the board's size

        //TODO: Handle device safe area
        //      Will need to handle it on bounds check and on zoom size setup
        //TODO: re calculate this on screen size change
        zoomBounds          = new Vector2(
                                Mathf.Max(
                                    .5f
                                    , Screen.width / shopBoard.resolvedStyle.width
                                    , Screen.height / shopBoard.resolvedStyle.height
                                )
                                , 2.5f
                            );

        SetupShop();

        yield return SetupProductLines();

        Tween fadein        = DOTween.To(
                                () => page.style.opacity.value
                                , x => page.style.opacity = new StyleFloat(x)
                                , 1f
                                , .33f);

        yield return fadein.Play().WaitForCompletion();

        canClick            = true;
        canClickButtons     = true;
    }

    public override IEnumerator AnimateOut()
    {
        canClick            = false;
        canClickButtons     = false;

        VisualElement page  = uiDoc.rootVisualElement;
        Tween fadeout       = DOTween.To(
                                () => page.style.opacity.value
                                , x => page.style.opacity = new StyleFloat(x)
                                , 0f
                                , .33f);

        yield return fadeout.Play().WaitForCompletion();
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
        detailsPLTab.style.top          = 0f;

        for (int i = 0; i < shopItems.Count; i++)
        {
            ShopItem shopItem           = shopItems[i];
            VisualElement shopButton    = UIManager.instance.LevelSelectButton.Instantiate();
            VisualElement buttonBG      = shopButton.Q<VisualElement>("LevelSelectButton");
            VisualElement icon          = shopButton.Q<VisualElement>("Icon");
            VisualElement purchasedIcon = shopButton.Q<VisualElement>("CompletedIcon");
            VisualElement selBorder     = shopButton.Q<VisualElement>("SelectionBorder");

            shopButton.name             = shopItem.name;

            for (int width = 0; width < shopItem.Size.x; width++)
            {
                for (int height = 0; height < shopItem.Size.y; height++)
                {
                    Vector2Int pos = new Vector2Int(shopItem.Position.x + width
                                            , shopItem.Position.y + height);

                    VEbyPosition.Add(pos, shopButton);
                }
            }

            VEsRevealed.Add(shopButton, ShopManager.instance.IsItemPurchased(shopItem));
            VEsRevealed[VEbyPosition[Vector2Int.zero]] = true;

            buttonBG.SetMargins(0f);
            selBorder.SetMargins(0f);

            shopButton.style.position   = Position.Absolute;
            shopButton.style.left       = zeroLocation.x + ((GRID_SIZE + SPACING) * shopItem.Position.x);// + (((GRID_SIZE + SPACING) / 2f) * (shopItem.Size.x - 1));
            shopButton.style.top        = zeroLocation.y + ((GRID_SIZE + SPACING) * shopItem.Position.y);// + (((GRID_SIZE + SPACING) / 2f) * (shopItem.Size.y - 1));

            shopButton.userData         = shopItem;

            buttonBG.SetWidth((GRID_SIZE * shopItem.Size.x) + (SPACING * (shopItem.Size.x - 1)), false, false);
            buttonBG.SetHeight((GRID_SIZE * shopItem.Size.y) + (SPACING * (shopItem.Size.y - 1)), false, false);
            selBorder.SetWidth((GRID_SIZE * shopItem.Size.x) + (SPACING * (shopItem.Size.x - 1)) + SELECTION_BORDER_ADD, false, false);
            selBorder.SetHeight((GRID_SIZE * shopItem.Size.y) + (SPACING * (shopItem.Size.y - 1)) + SELECTION_BORDER_ADD, false, false);

            shopButton.RegisterCallback<PointerUpEvent>((evt) =>
            {
                //if (!canClick)
                //    return;

                if (!canClickButtons)
                    return;

                SelectedShopNode = shopButton;
            });

            purchasedIcon.RemoveFromHierarchy();
            buttonBG.SetBorderColor(Color.clear);

            shopButton.Q<Label>().RemoveFromHierarchy();
            shopBoard.Add(shopButton);
        }

        for (int i = 0; i < VEsRevealed.Count; i++)
        {
            ShopItem s = VEsRevealed.ElementAt(i).Key.userData as ShopItem;

            if (ShopManager.instance.IsItemPurchased(s))
                SetVisibiltyOfNeighboringTiles(VEsRevealed.ElementAt(i).Key);
        }

        foreach (KeyValuePair<VisualElement, bool> ve in VEsRevealed)
        {
            VisualElement buttonBG = ve.Key.Q<VisualElement>("LevelSelectButton");
            VisualElement icon = ve.Key.Q<VisualElement>("Icon");

            if (ve.Value)
            {
                buttonBG.SetColor(UIManager.instance.GetColor(0));
                icon.style
                    .backgroundImage = ((ShopItem)ve.Key.userData).GetIcon();
            }
            else
            {
                buttonBG.SetColor(Color.grey);
                icon.style
                    .backgroundImage = null;
            }
        }

        this.AddObserver(OnItemPurchase, Notifications.ITEM_PURCHASED);
    }

    private List<VisualElement> SetVisibiltyOfNeighboringTiles(VisualElement ve)
    {
        List<Vector2Int> posToCheck = VEbyPosition.Where(x => x.Value == ve).Select(x => x.Key).ToList();
        List<VisualElement> ret     = new List<VisualElement>();

        for (int i = 0; i < posToCheck.Count; i++)
        {
            Vector2Int up           = new Vector2Int(posToCheck[i].x        , posToCheck[i].y - 1);
            Vector2Int right        = new Vector2Int(posToCheck[i].x + 1    , posToCheck[i].y);
            Vector2Int down         = new Vector2Int(posToCheck[i].x        , posToCheck[i].y + 1);
            Vector2Int left         = new Vector2Int(posToCheck[i].x - 1    , posToCheck[i].y);

            if (VEbyPosition.ContainsKey(up))
                if (!VEsRevealed[VEbyPosition[up]])
                {
                    VEsRevealed[VEbyPosition[up]] = true;
                    ret.Add(VEbyPosition[up]);
                }
            if (VEbyPosition.ContainsKey(right))
                if (!VEsRevealed[VEbyPosition[right]])
                {
                    VEsRevealed[VEbyPosition[right]] = true;
                    ret.Add(VEbyPosition[right]);
                }
            if (VEbyPosition.ContainsKey(down))
                if (!VEsRevealed[VEbyPosition[down]])
                {
                    VEsRevealed[VEbyPosition[down]] = true;
                    ret.Add(VEbyPosition[down]);
                }
            if (VEbyPosition.ContainsKey(left))
                if (!VEsRevealed[VEbyPosition[left]])
                {
                    VEsRevealed[VEbyPosition[left]] = true;
                    ret.Add(VEbyPosition[left]);
                }
        }

        return ret;
    }

    private void OnItemPurchase(object sender, object info)
    {
        //sender    -   ShopItem    -   The ShopItem that was purchased

        canClickButtons                         = false;

        ShopItem boughtItem                     = (ShopItem)sender;

        VisualElement boughtItemNode            = VEbyPosition[boughtItem.Position];
        List<VisualElement> newlyOpenedNodes    = new List<VisualElement>();
        List<ProductLine> newlyOpenedPLs        = new List<ProductLine>();

        newlyOpenedNodes.AddRange(SetVisibiltyOfNeighboringTiles(boughtItemNode));

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

                        p.StartNode.userData = p;

                        newlyOpenedNodes.AddRange(SetVisibiltyOfNeighboringTiles(p.StartNode));

                        newlyOpenedPLs.Add(p);

                        count++;
                    }

                    if (count == 4)
                        break;
                }
            }
            else if (featItem.Feat == ShopItem_UnlockFeature.Feature.PRODUCT_LINE_TAB)
            {
                VisualElement productLineTab    = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");
                Label label                     = productLineTab.Q<Label>();

                ShopItem s                      = SelectedShopNode.userData as ShopItem;

                productLineTab.SetBorderColor(s.GetColor());

                Tween tabUp                     =   DOTween.To(
                                                        () => productLineTab.style.top.value.value
                                                        , x => productLineTab.style.top = x
                                                        , -1 * (productLineTab.resolvedStyle.height - productLineTab.resolvedStyle.borderBottomWidth)
                                                        , .25f)
                                                    .SetEase(Ease.OutQuart);

                tabUp.Play();
            }
            else if (featItem.Feat == ShopItem_UnlockFeature.Feature.PRODUCT_LINE_NUMBER)
            {
                VisualElement productLineTab    = uiDoc.rootVisualElement.Q<VisualElement>("ProductLineTab");
                Label label                     = productLineTab.Q<Label>();
                label.text                      = boughtItem.ProductLine.ToString() + " Product Line - #" 
                                                    + boughtItem.ProductLineNumber.ToString();
            }
        }

        float animationLength       = .5f;
        Sequence seq                = DOTween.Sequence();

        int numOfCoinsToSpawn       = Mathf.Min(100, boughtItem.Costs.Sum(x => x.amount));
        Vector2 spawnBoundsX        = new Vector2(detailsPanel.worldBound.center.x - ((detailsPanel.worldBound.width / 2f) * .8f)
                                                , detailsPanel.worldBound.center.x + ((detailsPanel.worldBound.width / 2f) * .8f));

        Vector2 spawnBoundsY        = new Vector2(detailsPanel.worldBound.center.y - ((detailsPanel.worldBound.height / 2f) * .8f)
                                                , detailsPanel.worldBound.center.y + ((detailsPanel.worldBound.height / 2f) * .8f));

        for (int i = 0; i < numOfCoinsToSpawn; i++)
        {
            Vector2 origin          = new Vector2(Random.Range(spawnBoundsX.x, spawnBoundsX.y)
                                    , Random.Range(spawnBoundsY.x, spawnBoundsY.y));

            int colIndex            = boughtItem.Costs[Random.Range(0, boughtItem.Costs.Count - 1)].colorIndex;

            CurrencyManager.instance.SpawnCoin(
                                    colIndex
                                    , origin
                                    , boughtItemNode.worldBound.center
                                    , animationLength * 1.3f);
        }
        //------
        
        if (!(boughtItem is ShopItem_MultiplePurchaseItem))
        {
            Tween shrinkPurchased   = DOTween.To(
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

            Tween resizePurchased   = DOTween.To(
                                        () => boughtItemNode.transform.scale
                                        , x => boughtItemNode.style.scale = x
                                        , Vector3.one
                                        , animationLength / 2f)
                                    .SetEase(Ease.OutQuad)
                                    .Pause();

            seq.Append(shrinkPurchased);
            seq.Append(resizePurchased);
        }
        else
        {
            VisualElement bg    = boughtItemNode.Q<VisualElement>("LevelSelectButton");
            Color c             = UIManager.instance.GetColor(boughtItem.ProductLine);

            bg.SetColor(new Color(
                c.r + (1f - c.r) * .8f
                , c.g + (1f - c.g) * .8f
                , c.b + (1f - c.b) * .8f
                , 1f));

            //TODO: CurrencyMan.spawn powerup coin
        }

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
                                            VisualElement bg        = node.Q<VisualElement>("LevelSelectButton");

                                            if (node.userData is ShopItem)
                                            { 
                                                VisualElement icon  = node.Q<VisualElement>("Icon");
                                                Color c             = UIManager.instance.GetColor(0);

                                                icon.style
                                                    .backgroundImage= ((ShopItem)node.userData).GetIcon();
                                                bg.SetColor(c);
                                            }
                                            else //Product Line node
                                            {
                                                Color c = UIManager.instance.GetColor((((ProductLine)node.userData).ColorIndex));

                                                bg.SetColor(new Color(
                                                    c.r + (1f - c.r) * .8f
                                                    , c.g + (1f - c.g) * .8f
                                                    , c.b + (1f - c.b) * .8f
                                                    , 1f));
                                            }
                                        })
                                        .Pause();

            if (i == 0)
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
                                        .OnStart(() =>
                                        {
                                            if (node.userData is ProductLine)
                                            {
                                                DrawProductLineStartPoint(((ProductLine)node.userData));
                                            }
                                        })
                                        .Pause();

            if (i == 0)
                seq.Append(resizeNode);
            else
                seq.Join(resizeNode);
        }

        if (boughtItem.ProductLine != 0 && ShopManager.instance.GetNumPurchased(boughtItem) == 1)
        {
            //canClick = false;

            seq.Join(GetLineDrawingTween(boughtItemNode, animationLength)
                .Pause());
                //.Play()
                //.onComplete += () => canClick = true;
        }

        seq.Play()
            .onComplete += () => canClickButtons = true;
    }

    private Tween GetLineDrawingTween(VisualElement destinationNode, float animationLength)
    {
        ShopItem destinationItem    = destinationNode.userData as ShopItem;
        VisualElement buttonBG      = destinationNode.Q<VisualElement>("LevelSelectButton");
        ProductLine productLine     = productLines.Find(x => destinationItem.ProductLine == x.ColorIndex);

        Vector2 destination         = AddToProductLineLines(destinationItem);

        Tween draw                  = productLine.LastLine
                                        .DrawTowardNewPoint_Tween(destination, animationLength)
                                        .OnComplete(() => {

                                        if (destinationItem.DrawLinesToOutline)
                                        {
                                            buttonBG.SetBorderWidth(20f);
                                            buttonBG.SetBorderColor(UIManager.instance.GetColor(productLine.ColorIndex));
                                        }
                                    });

        return draw;
    }

    private Vector2 AddToProductLineLines(ShopItem destinationItem)
    {
        VisualElement buttonBG      = VEbyPosition[destinationItem.Position].Q<VisualElement>("LevelSelectButton");
        ProductLine productLine     = productLines.Find(x => destinationItem.ProductLine == x.ColorIndex);

        Vector2 destination         = Vector2.zero;
        Vector2 origin              = Vector2.zero;
        ShopItem startingItem       = destinationItem.PreviousItem;
        Vector2 zeroLocation        = new Vector2(shopBoard.resolvedStyle.width / 2f
                                        , shopBoard.resolvedStyle.height / 2f)
                                        + new Vector2(GRID_SIZE / 2f, GRID_SIZE / 2f);

        Vector2 outComparer         = startingItem == null ?
                                        productLines.Find(x => x.ColorIndex == destinationItem.ProductLine).StartPosition
                                        : (startingItem.DrawLinesToOutline ?
                                            startingItem.LineOutPosition
                                            : startingItem.Position);

        Vector2 inComparer          = destinationItem.DrawLinesToOutline ?
                                        destinationItem.LineInPosition
                                        : destinationItem.Position;

        if (startingItem == null || !startingItem.DrawLinesToOutline)
            origin = productLine.LastLine.LastPoint;
        else
        {
            if (outComparer.y < inComparer.y)
            {
                origin              = new Vector2(
                                        (GRID_SIZE + SPACING) * outComparer.x
                                        , ((GRID_SIZE + SPACING) * outComparer.y) + (GRID_SIZE / 2f)
                                    );
            }
            else if (outComparer.y > inComparer.y)
            {
                origin              = new Vector2(
                                        (GRID_SIZE + SPACING) * outComparer.x
                                        , ((GRID_SIZE + SPACING) * outComparer.y) - (GRID_SIZE / 2f)
                                    );
            }
            else if (outComparer.x < inComparer.x)
            {
                origin              = new Vector2(
                                        ((GRID_SIZE + SPACING) * outComparer.x) + (GRID_SIZE / 2f)
                                        , (GRID_SIZE + SPACING) * outComparer.y
                                    );
            }
            else
            {
                origin              = new Vector2(
                                        ((GRID_SIZE + SPACING) * outComparer.x) - (GRID_SIZE / 2f)
                                        , (GRID_SIZE + SPACING) * outComparer.y
                                    );
            }

            origin                  += zeroLocation;
        }

        if (!destinationItem.DrawLinesToOutline)
            destination = shopBoard.WorldToLocal(buttonBG.worldBound.center);
        else
        {
            if (outComparer.y < inComparer.y)
            {
                destination             = new Vector2(
                                            (GRID_SIZE + SPACING) * inComparer.x
                                            , ((GRID_SIZE + SPACING) * inComparer.y) - (GRID_SIZE / 2f)
                                        );
            }
            else if (outComparer.y > inComparer.y)
            {
                destination             = new Vector2(
                                            (GRID_SIZE + SPACING) * inComparer.x
                                            , ((GRID_SIZE + SPACING) * inComparer.y) + (GRID_SIZE / 2f)
                                        );
            }
            else if (outComparer.x < inComparer.x)
            {
                destination             = new Vector2(
                                            ((GRID_SIZE + SPACING) * inComparer.x) - (GRID_SIZE / 2f)
                                            , (GRID_SIZE + SPACING) * inComparer.y
                                        );
            }
            else
            {
                destination             = new Vector2(
                                            ((GRID_SIZE + SPACING) * inComparer.x) + (GRID_SIZE / 2f)
                                            , (GRID_SIZE + SPACING) * inComparer.y
                                        );
            }

            destination                 += zeroLocation;
        }

        if (origin != productLine.LastLine.LastPoint)
        {
            UIToolkitLine l             = new UIToolkitLine(
                                            new List<Vector2>() { origin }
                                            , GRID_SIZE / 3f * .75f
                                            , UIManager.instance.GetColor(productLine.ColorIndex)
                                            , LineCap.Round
                                        );

            shopBoard.Add(l);
            l.BringToFront();
            productLine.Lines.Add(l);
        }

        return destination;
    }

    private IEnumerator SetupProductLines()
    {
        List<ProductLine> newProductLines  = new List<ProductLine>()
        {
            new     ProductLine(1   , new Vector2Int(1, 0)  , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(2   , new Vector2Int(-1, 0) , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(3   , new Vector2Int(0, 1)  , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
            , new   ProductLine(4   , new Vector2Int(0, -1) , ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.UNLOCK_SHOP))
        };

        Vector2 zeroLocation            = new Vector2(shopBoard.resolvedStyle.width / 2f, shopBoard.resolvedStyle.height / 2f);
        List<VisualElement> adjNodes    = new List<VisualElement>();

        for (int i = 0; i < newProductLines.Count; i++)
        {
            ProductLine productLine     = newProductLines[i];

            VisualElement shopButton    = UIManager.instance.LevelSelectButton.Instantiate();
            VisualElement buttonBG      = shopButton.Q<VisualElement>("LevelSelectButton");
            VisualElement icon          = shopButton.Q<VisualElement>("Icon");
            VisualElement purchasedIcon = shopButton.Q<VisualElement>("CompletedIcon");
            VisualElement selBorder     = shopButton.Q<VisualElement>("SelectionBorder");

            shopButton.name             = "Product Line: " + productLine.ColorIndex;
            
            buttonBG.SetMargins(0f);
            selBorder.SetMargins(0f);

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

            VEbyPosition.Add(productLine.StartPosition, productLine.StartNode);
            VEsRevealed.Add(productLine.StartNode, productLine.NodeUnlocked);

            shopButton.RegisterCallback<PointerUpEvent>((evt) =>
            {
                //if (!canClick)
                //    return;

                if (!canClickButtons)
                    return;

                SelectedShopNode = shopButton;
            });

            if (productLine.NodeUnlocked)
            {
                yield return null;

                Color c                 = UIManager.instance.GetColor(productLine.ColorIndex);
                Color bgTint            = new Color(
                                            c.r + (1f - c.r) * .8f
                                            , c.g + (1f - c.g) * .8f
                                            , c.b + (1f - c.b) * .8f
                                            , 1f);
                buttonBG.SetColor(bgTint);

                DrawProductLineStartPoint(productLine);

                adjNodes.AddRange(SetVisibiltyOfNeighboringTiles(productLine.StartNode));

                List<ShopItem> purchasedOnPL = ShopManager.instance.GetPurchasedProductLineItems(productLine.ColorIndex);

                while (purchasedOnPL.Count > 0)
                {
                    Vector2 newPoint = AddToProductLineLines(purchasedOnPL[0]);

                    productLine.LastLine.AddNewPoint(newPoint);

                    VEbyPosition[purchasedOnPL[0].Position].Q<VisualElement>("Icon").Show(purchasedOnPL[0] is ShopItem_MultiplePurchaseItem);
                    VisualElement bg = VEbyPosition[purchasedOnPL[0].Position].Q<VisualElement>("LevelSelectButton");

                    icon.Show(purchasedOnPL[0] is ShopItem_MultiplePurchaseItem);
                    bg.SetColor(bgTint);

                    if (purchasedOnPL[0].DrawLinesToOutline)
                    {
                        bg.SetBorderWidth(20f);
                        bg.SetBorderColor(c);
                    }

                    purchasedOnPL.RemoveAt(0);
                }
            }
            else
            {
                buttonBG.SetColor(Color.grey);
            }
        }

        for (int i = 0; i < adjNodes.Count; i++)
        {
            VisualElement buttonBG      = adjNodes[i].Q<VisualElement>("LevelSelectButton");
            VisualElement icon          = adjNodes[i].Q<VisualElement>("Icon");

            buttonBG.SetColor(UIManager.instance.GetColor(0));
            icon.style
                .backgroundImage = ((ShopItem)adjNodes[i].userData).GetIcon();
        }
    }

    private void DrawProductLineStartPoint(ProductLine productLine)
    {
        Vector2 origin              = shopBoard.WorldToLocal(productLine.StartNode
                                        .Q<VisualElement>("LevelSelectButton").worldBound.center);

        Debug.Log(productLine.ColorIndex + " " + origin);

        UIToolkitCircle endPoint    = new UIToolkitCircle(
                                        origin
                                        , GRID_SIZE / 4f
                                        , UIManager.instance.GetColor(productLine.ColorIndex)
                                    ); ;

        shopBoard.Add(endPoint);
        endPoint.BringToFront();

        UIToolkitLine line          = new UIToolkitLine(
                                        new List<Vector2>() { endPoint.Center }
                                        , GRID_SIZE / 3f * .75f
                                        , UIManager.instance.GetColor(productLine.ColorIndex)
                                        , LineCap.Round
                                    );

        shopBoard.Add(line);
        line.BringToFront();

        productLine.Lines.Add(line);
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
            canClickButtons = false;

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
                canClickButtons         = false;

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

                canClickButtons         = true;
            }
        }
        if (pointerID == secondaryTouchID)
        {
            secondaryTouchID            = -99;
            secondaryOrigin             = Vector2.zero;

            secondaryOrigin_Screen      = Vector2.zero;
        }
    }

    //TODO: might want to make the panel fly out if the selected node changes
    //      in order to get the cost items onto the scroll it needs to flash for a second
    //      flying out -> updating -> flying in would fix that but its overall a longer wait
    private IEnumerator SetDetailsPanel(VisualElement content)
    {
        //canClick                            = false;
        canClickButtons                     = false;

        VisualElement container             = detailsPanel.Q<VisualElement>("Content");
        VisualElement topIndicator          = costScrollView.Q<VisualElement>("TopArrow");
        VisualElement bottomIndicator       = costScrollView.Q<VisualElement>("BottomArrow");
        VisualElement productLineTab        = detailsPanel.Q<VisualElement>("ProductLineTab");
        Label productLineLabel              = productLineTab.Q<Label>();

        container.Clear();
        costScrollView.ClearWithChildBoundIndicators(topIndicator, bottomIndicator);

        ShopItem item                       = content.userData is ShopItem ? (ShopItem)(content.userData) : null;
        ProductLine productLine             = content.userData is ProductLine ? (ProductLine)(content.userData) : null;

        if (item != null)
        {
            if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.PRODUCT_LINE_NUMBER))
                productLineLabel.text = item.ProductLine.ToString() + " Product Line - #" + item.ProductLineNumber.ToString();
            else
                productLineLabel.text = item.ProductLine.ToString() + " Product Line";

            productLineTab.SetBorderColor(item.GetColor());
        }
        else
        {
            if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.PRODUCT_LINE_NUMBER))
                productLineLabel.text = productLine.ToString() + " Product Line - #0";
            else
                productLineLabel.text = productLine.ToString() + " Product Line";

            productLineTab.SetBorderColor(UIManager.instance.GetColor(productLine.ColorIndex));
        }

        if ((item != null && !VEsRevealed[VEbyPosition[item.Position]]) ||  //TODO: Might be able to just use content here
            (productLine != null && !VEsRevealed[VEbyPosition[productLine.StartPosition]]))
        {
            productLineTab.SetBorderColor(Color.black);
            costScrollView.Hide();
            SetDetailsMystery();
        }
        else if (item == null)
        {
            productLineTab.SetBorderColor(UIManager.instance.GetColor(productLine.ColorIndex));
            costScrollView.Hide();
            SetDetailsProductLineEnd(productLine);
        }
        else
        {
            container.Add(item.GetDisplayContent(item.Purchased));
            costScrollView.Show();
            detailsPanel.Q<VisualElement>("BG").SetBorderColor(item.GetColor());

            if (item.Purchased)
            {
                SetDetailsOwned(item);
            }

            if (!item.Purchased || item is ShopItem_MultiplePurchaseItem)
            {
                detailsPanel.Q<VisualElement>("PurchasedIcon").Show(item is ShopItem_MultiplePurchaseItem);

                costScrollView.Show();
                container.Show();
                purchaseButton.Show();
                
                costScrollView.SetOpacity(0f);

                List<VisualElement> costObjs = new List<VisualElement>();

                for (int i = 0; i < item.Costs.Count; i++)
                {
                    VisualElement obj           = UIManager.instance.CoinDisplay.Instantiate();
                    VisualElement costLine      = obj.Q<VisualElement>("Container");

                    costLine.style.flexGrow     = 0f;
                    costLine.style.flexShrink   = 1f;
                    costLine.style.flexDirection= FlexDirection.Row;
                    costLine.style.alignItems   = Align.Center;
                    costLine.style.alignSelf    = Align.Auto;
                    costLine.style
                        .justifyContent         = Justify.FlexStart;

                    costLine.style.height       = new StyleLength(StyleKeyword.Auto);
                    costLine.style.minHeight    = costLine.style.height;
                    costLine.style.maxHeight    = new StyleLength(StyleKeyword.None);

                    costLine.style.width        = costLine.style.height;
                    costLine.style.minWidth     = costLine.style.height;
                    costLine.style.maxWidth     = costLine.style.maxHeight;

                    costLine.SetMargins(25f, false, true, false, true);
                    costLine.Q<VisualElement>("X").SetMargins(15f, false, true, false, true);

                    Label amountLabel           = costLine.Q<Label>("AmountLabel");
                    amountLabel.style.position  = Position.Relative;
                    amountLabel.text            = CurrencyManager.instance.GetCoinsForColorIndex(item.Costs[i].colorIndex).ToString()
                                                            + " / " + item.Costs[i].amount.ToString();
                    costLine.Q<VisualElement>("CoinSquare").SetColor(UIManager.instance.GetColor(item.Costs[i].colorIndex));

                    costObjs.Add(costLine);
                }

                Label purchaseButtonLabel = purchaseButton.Q<Label>();

                purchaseButton.UnregisterCallback<PointerUpEvent>(purchaseButtonAction);

                purchaseButton.SetColor(Color.clear);

                if (CurrencyManager.instance.CanAfford(item))
                {
                    if (item.PreviousItem == null || ShopManager.instance.IsItemPurchased(item.PreviousItem))
                    {
                        purchaseButton.SetBorderColor(item.GetColor());
                        purchaseButton.SetColor(item.GetColor());
                        purchaseButtonLabel
                            .style.color            = Color.black;
                        purchaseButtonLabel.text    = "Purchase";

                        purchaseButtonAction = (evt) => { OnPurchase(item, evt); };
                        purchaseButton.RegisterCallback<PointerUpEvent>(purchaseButtonAction);
                    }
                    else
                    {
                        purchaseButton.SetBorderColor(Color.grey);
                        purchaseButton.SetColor(Color.grey);
                        purchaseButtonLabel
                            .style.color            = Color.black;
                        purchaseButtonLabel.text    = "Purchase earlier Product Line items";
                    }
                }
                else
                {
                    purchaseButton.SetBorderColor(Color.grey);
                    purchaseButton.SetColor(Color.grey);
                    purchaseButtonLabel
                        .style.color                = Color.black;
                    purchaseButtonLabel.text        = "Need More Segments";
                }

                yield return null;

                yield return CreateCostRows(costObjs);

                costScrollView.SetOpacity(100f);

                costScrollView.SetBoundIndicators(topIndicator, bottomIndicator);
                costScrollView.verticalScroller.valueChanged += (evt) =>
                {
                    costScrollView.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator);
                };

                costScrollView.ShowHideVerticalBoundIndicators(topIndicator, bottomIndicator
                                , costScrollView.contentContainer, costScrollView.contentContainer.parent);
            }
        }

        PageManager.instance.StartCoroutine(ShowDetailsPanel());
    }

    private IEnumerator CreateCostRows(List<VisualElement> costEntries)
    {
        VisualElement row               = new VisualElement();

        row.style.flexDirection         = FlexDirection.Row;
        row.style.flexGrow              = 0f;
        row.style.flexShrink            = 1f;
        row.style.justifyContent        = Justify.Center;
        row.style.alignItems            = Align.Stretch;
        row.style.alignSelf             = Align.Center;

        row.SetMargins(10f, true, false, true, false);

        costScrollView.Add(row);

        float maxWidth      = costScrollView.resolvedStyle.width * .9f;

        for (int i = 0; i < costEntries.Count; i++)
        {
            row.Add(costEntries[i]);

            yield return null;

            if (row.resolvedStyle.width > maxWidth)
            {
                row                     = new VisualElement();

                row.style.flexDirection = FlexDirection.Row;
                row.style.flexGrow      = 0f;
                row.style.flexShrink    = 1f;
                row.style.justifyContent= Justify.Center;
                row.style.alignItems    = Align.Stretch;
                row.style.alignSelf     = Align.Center;

                row.SetMargins(10f, true, false, true, false);

                row.Add(costEntries[i]);

                costScrollView.Add(row);
            }
        }
    }

    private void SetDetailsOwned(ShopItem item)
    {
        VisualElement purchasedIcon = detailsPanel.Q<VisualElement>("PurchasedIcon");
        VisualElement icon          = purchasedIcon.Q<VisualElement>("Icon");
        Label purchaseCounter       = purchasedIcon.Q<Label>();

        purchasedIcon.SetColor(item.GetColor());

        if (!(item is ShopItem_MultiplePurchaseItem))
        {
            purchasedIcon.SetWidth(100f);
            
            costScrollView.Hide();
            purchaseButton.Hide();

            icon.Show();
            purchaseCounter.Hide();
        }
        else
        {
            purchasedIcon.SetWidth(new StyleLength(StyleKeyword.None), true, false);
            purchasedIcon.SetWidth(new StyleLength(StyleKeyword.Auto), false);

            costScrollView.Show();
            purchaseButton.Show();

            icon.Hide();
            purchaseCounter.Show();
            purchaseCounter.text    = ShopManager.instance.GetNumPurchased(item).ToString();
            //TODO: Set text to black/white based onbg color (utility function to be added from brain game utilities)
        }

        purchasedIcon.Show();
    }

    private void SetDetailsMystery()
    {
        detailsPanel.Q<VisualElement>("BG").SetBorderColor(Color.black);

        VisualElement purchasedIcon         = detailsPanel.Q<VisualElement>("PurchasedIcon");
        VisualElement container             = detailsPanel.Q<VisualElement>("Content");

        Label questionMarks                 = new Label();
        questionMarks.text                  = "?????";

        questionMarks.AddToClassList("ShopDescriptionText");

        questionMarks.style.flexGrow        = 1f;
        questionMarks.style.alignSelf       = Align.Center;
        questionMarks.style.unityTextAlign  = TextAnchor.MiddleCenter;

        container.Add(questionMarks);

        purchasedIcon.Hide();
        purchaseButton.Hide();
    }

    private void SetDetailsProductLineEnd(ProductLine prod)
    {
        detailsPanel.Q<VisualElement>("BG").SetBorderColor(UIManager.instance.GetColor(prod.ColorIndex));

        VisualElement purchasedIcon         = detailsPanel.Q<VisualElement>("PurchasedIcon");
        VisualElement container             = detailsPanel.Q<VisualElement>("Content");

        Label label                         = new Label();
        label.text                          = "Product Line - Start";

        label.AddToClassList("ShopDescriptionText");

        label.style.flexGrow                = 1f;
        label.style.alignSelf               = Align.Center;
        label.style.unityTextAlign          = TextAnchor.MiddleCenter;
        label.style.color                   = UIManager.instance.GetColor(prod.ColorIndex);

        container.Add(label);

        purchasedIcon.Hide();
        purchaseButton.Hide();
    }

    private void OnPurchase(ShopItem item, PointerUpEvent evt)
    {
        item.OnPurchase();

        PageManager.instance.StartCoroutine(SetDetailsPanel(SelectedShopNode));
        //VisualElement container = detailsPanel.Q<VisualElement>("Content");
        //container.Clear();
        //container.Add(item.GetDisplayContent(item.Purchased));

        //SetDetailsOwned(item);
    }

    private IEnumerator ShowDetailsPanel()
    {
        //canClick = false;
        canClickButtons = false;

        yield return DetailsButtonOut();

        yield return DetailsPanelIn();

        //canClick = true;
        canClickButtons = true;
    }

    private IEnumerator CloseDetailsPanel()
    {
        //canClick = false;
        canClickButtons = false;

        yield return DetailsPanelOut();

        yield return DetailsButtonIn();

        //canClick = true;
        canClickButtons = true;
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

        Tween panelIn = DOTween.To(
                            () => detailsPanel.transform.position
                            , x => detailsPanel.transform.position = x
                            , new Vector3(
                                0f
                                , detailsPanel.transform.position.y
                                , detailsPanel.transform.position.z)
                            , .65f)
                        .SetEase(Ease.OutQuart);

        yield return panelIn.WaitForCompletion();

        if (ShopManager.instance.FeatureUnlocked(ShopItem_UnlockFeature.Feature.PRODUCT_LINE_TAB))
        {
            Tween tabUp                     =   DOTween.To(
                                                    () => productLineTab.style.top.value.value
                                                    , x => productLineTab.style.top = x
                                                    , -1 * (productLineTab.resolvedStyle.height - productLineTab.resolvedStyle.borderBottomWidth)
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
            Tween tabDown                   =   DOTween.To(
                                                    () => productLineTab.style.top.value.value
                                                    , x => productLineTab.style.top = x
                                                    , 0f
                                                    , .25f)
                                                .SetEase(Ease.OutQuart)
                                                .Play();
        }

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
    }

    #endregion
}
