using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Shop : Page
{
    //#region Consts

    ////We set these here because the map reference isn't loaded
    ////  until a frame after ShowPage is run
    //private const float     MAP_SIZE_WIDTH      = 4000f;
    //private const float     MAP_SIZE_HEIGHT     = 6000f;

    //#endregion

    #region Private Variables

    private bool            canClick;

    private VisualElement   screen;
    private VisualElement   shopBoard;

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

    #endregion

    #region Private Properties

    private bool            Dragging            { get { return primaryTouchID != -99; } }
    private bool            Zooming             { get { return secondaryTouchID != -99; } }

    #endregion

    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        screen              = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        shopBoard           = uiDoc.rootVisualElement.Q<VisualElement>("ShopBoard");

        primaryTouchID      = -99;
        secondaryTouchID    = -99;
        
        zoomSpeed           = .02f;

        //TODO: Recalculate this if the screen size changes (if switched to landscape etc)
        RectOffsetFloat safe= uiDoc.rootVisualElement.panel.GetSafeArea();

        //zoomBounds          = new Vector2(
        //                        Mathf.Max(
        //                            .5f
        //                            , Screen.width / MAP_SIZE_WIDTH
        //                            , Screen.height / MAP_SIZE_HEIGHT
        //                        )
        //                        , 2.5f
        //                    );

        shopBoard.RegisterCallback<PointerDownEvent>(OnPointerDownOnShopBoard);
        shopBoard.RegisterCallback<PointerMoveEvent>(OnPointerMoveOnShopBoard);
        shopBoard.RegisterCallback<PointerUpEvent>(OnPointerUpOnShopBoard);
        shopBoard.RegisterCallback<WheelEvent>(OnMouseScroll);
        screen.RegisterCallback<PointerLeaveEvent>(OnPointerLeaveScreen);
    }

    public override IEnumerator AnimateIn()
    {
        canClick            = false;

        yield return null; //wait a frame to be able to reference the board's size

        //TODO: Handle device safe area
        //      Will need to handle it on bounds check and on zoom size setup
        zoomBounds = new Vector2(
                                Mathf.Max(
                                    .5f
                                    , Screen.width / shopBoard.resolvedStyle.width
                                    , Screen.height / shopBoard.resolvedStyle.height
                                )
                                , 2.5f
                            );
        

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
    }

    #endregion

    #region Private Functions

    private void SetupShop()
    {
        
    }

    private void OnPointerDownOnShopBoard(PointerDownEvent evt)
    {
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

    #endregion
}
