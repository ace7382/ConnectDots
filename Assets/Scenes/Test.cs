using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    public UIDocument uiDoc;
    public VisualElement map;
    public Label touchCounter;
    public Label scaleLabel;
    public Label positionLabel;

    public Vector2 dragOrigin;
    public float zoomSpeed = 3f;

    public int primaryTouchID;
    public int secondaryTouchID;
    public Vector2 primaryOrigin;
    public Vector2 secondaryOrigin;

    public Vector2 zoomBounds;

    float lastdistance;
    float currentscale;

    public Vector2 edgeBoundsInWorldSpaceX, edgeBoundsInWorldSpaceY;
    public Vector2 mapSize;

    public VisualElement screen;
    public Vector2 primaryOrigin_Screen;
    public Vector2 secondaryOrigin_Screen;

    public bool Dragging { get { return primaryTouchID != -99; } }
    public bool Zooming { get { return secondaryTouchID != -99; } }

    public void Start()
    {
        screen = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        map = uiDoc.rootVisualElement.Q<VisualElement>("Map");
        touchCounter = uiDoc.rootVisualElement.Q<Label>("TouchesLabel");
        scaleLabel = uiDoc.rootVisualElement.Q<Label>("ScaleLabel");
        positionLabel = uiDoc.rootVisualElement.Q<Label>("PositionLabel");

        map.RegisterCallback<PointerDownEvent>(OnPointerDown);
        map.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        map.RegisterCallback<PointerUpEvent>(OnPointerUp);
        map.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

        map.RegisterCallback<WheelEvent>(OnMouseScroll);

        primaryTouchID      = -99;
        secondaryTouchID    = -99;

        edgeBoundsInWorldSpaceX.y = Screen.width;
        edgeBoundsInWorldSpaceY.y = Screen.height;

        zoomBounds.x = Mathf.Max(zoomBounds.x, Screen.height / mapSize.y, Screen.width / mapSize.x); // + .01f;
    }

    public void Update()
    {
        touchCounter.text   = "Primary Touch ID: " + primaryTouchID.ToString() + "\n(" + primaryOrigin.x.ToString() + ", " + primaryOrigin.y.ToString() + ")" +
                        "\nSecondary Touch ID: " + secondaryTouchID.ToString() + "\n(" + secondaryOrigin.x.ToString() + ", " + secondaryOrigin.y.ToString() + ")";

        scaleLabel.text     = "Scale: " + map.transform.scale.x.ToString();
        positionLabel.text  = "Left: " + map.style.left.value.value.ToString() + ", Top: " + map.style.top.value.value.ToString();

        //Debug.Log(string.Format(
        //    "Top Left: {0}\n" +
        //    "Top Right: {1}\n" +
        //    "Bottom Right: {2}\n" +
        //    "Bottom Left: {3}"
        //    , "(" + map.worldBound.xMin + ", " + map.worldBound.yMax + ")"
        //    , "(" + map.worldBound.xMax + ", " + map.worldBound.yMax + ")"
        //    , "(" + map.worldBound.xMax + ", " + map.worldBound.yMin + ")"
        //    , "(" + map.worldBound.xMin + ", " + map.worldBound.yMin + ")"
        //));
    }

    public void OnPointerDown(PointerDownEvent evt)
    {
        if (primaryTouchID == -99)
        {
            //If there isn't another pointer, this is the first and we want to start dragging logic
            primaryTouchID          = evt.pointerId;
            primaryOrigin           = evt.localPosition;

            primaryOrigin_Screen    = screen.WorldToLocal(map.LocalToWorld(primaryOrigin));
        }
        else if (secondaryTouchID == -99)
        {
            //If there isn't a second pointer, use this pointer as the secondary pointer and start zoom logic
            secondaryTouchID        = evt.pointerId;
            secondaryOrigin         = evt.localPosition;

            secondaryOrigin_Screen  = screen.WorldToLocal(map.LocalToWorld(secondaryOrigin));

            lastdistance            = 0f;
        }
    }

    public void OnPointerMove(PointerMoveEvent evt)
    {
        if (Zooming)
        {
            if (evt.pointerId == primaryTouchID)
            {
                //primaryOrigin = evt.localPosition;
                primaryOrigin_Screen = screen.WorldToLocal(map.LocalToWorld(evt.localPosition));
            }
            else if (evt.pointerId == secondaryTouchID)
            {
                //secondaryOrigin = evt.localPosition;
                secondaryOrigin_Screen = screen.WorldToLocal(map.LocalToWorld(evt.localPosition));
            }
            else
                return;

            //float distance = Vector2.Distance(primaryOrigin, secondaryOrigin);
            float distance = Vector2.Distance(primaryOrigin_Screen, secondaryOrigin_Screen);

            if (lastdistance == 0f)
            {
                lastdistance = distance;
                currentscale = map.transform.scale.x;
            }
            else
            {
                float multiplier = distance / lastdistance;
                SetZoom(currentscale * multiplier);
            }
        }
        else if (Dragging)
        {
            if (evt.pointerId == primaryTouchID)
            {
                Vector2 delta   = (Vector2)evt.localPosition - primaryOrigin;

                delta *= map.transform.scale;

                Debug.Log(string.Format(
                    "Starting Delta: {0}"
                    , delta
                    ));

                if (map.worldBound.xMin + delta.x > edgeBoundsInWorldSpaceX.x)
                {
                    delta       = new Vector2(edgeBoundsInWorldSpaceX.x - map.worldBound.xMin, delta.y);
                    Debug.Log("fixed to left edge");
                }
                else if (map.worldBound.xMax + delta.x < edgeBoundsInWorldSpaceX.y)
                {
                    delta       = new Vector2(edgeBoundsInWorldSpaceX.y - map.worldBound.xMax, delta.y);
                    Debug.Log("fixed to right edge");
                }

                if (map.worldBound.yMin + delta.y > edgeBoundsInWorldSpaceY.x)
                {
                    delta       = new Vector2(delta.x, edgeBoundsInWorldSpaceY.x - map.worldBound.yMin);
                    Debug.Log("fixed to bottom edge");
                }
                else if (map.worldBound.yMax + delta.y < edgeBoundsInWorldSpaceY.y)
                {
                    delta       = new Vector2(delta.x, edgeBoundsInWorldSpaceY.y - map.worldBound.yMax);
                    Debug.Log("fixed to top edge");
                }

                Debug.Log(string.Format(
                    "Final Delta: {0}"
                    , delta
                    ));

                map.style.left  = map.layout.x + delta.x;
                map.style.top   = map.layout.y + delta.y;
            }
        }
    }

    public void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.pointerId == primaryTouchID)
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
        if (evt.pointerId == secondaryTouchID)
        {
            secondaryTouchID            = -99;
            secondaryOrigin             = Vector2.zero;

            secondaryOrigin_Screen      = Vector2.zero;
        }
    }

    public void OnPointerLeave(PointerLeaveEvent evt)
    {
        if (evt.pointerId == primaryTouchID)
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
        if (evt.pointerId == secondaryTouchID)
        {
            secondaryTouchID            = -99;
            secondaryOrigin             = Vector2.zero;

            secondaryOrigin_Screen      = Vector2.zero;
        }
    }

    public void OnMouseScroll(WheelEvent evt)
    {
        SetZoom(map.transform.scale.x - (Mathf.Sign(evt.delta.y) * zoomSpeed));
    }

    public void SetZoom(float newScale)
    {
        newScale            = Mathf.Clamp(newScale, zoomBounds.x, zoomBounds.y);

        if (map.transform.scale.x == newScale)
            return;

        //map.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(new Length(tempPivot.x), new Length(tempPivot.y)));

        map.transform.scale = new Vector3(newScale, newScale, 1f);

        Vector2 delta       = Vector2.zero;

        if (map.worldBound.xMin + delta.x > edgeBoundsInWorldSpaceX.x)
            delta           = new Vector2(edgeBoundsInWorldSpaceX.x - map.worldBound.xMin, delta.y);
        else if (map.worldBound.xMax + delta.x < edgeBoundsInWorldSpaceX.y)
            delta           = new Vector2(edgeBoundsInWorldSpaceX.y - map.worldBound.xMax, delta.y);

        if (map.worldBound.yMin + delta.y > edgeBoundsInWorldSpaceY.x)
            delta           = new Vector2(delta.x, edgeBoundsInWorldSpaceY.x - map.worldBound.yMin);
        else if (map.worldBound.yMax + delta.y < edgeBoundsInWorldSpaceY.y)
            delta           = new Vector2(delta.x, edgeBoundsInWorldSpaceY.y - map.worldBound.yMax);

        map.style.left      = map.layout.x + delta.x;
        map.style.top       = map.layout.y + delta.y;

        //map.style.transformOrigin = new StyleTransformOrigin(new TransformOrigin(0f, 0f));
    }
}