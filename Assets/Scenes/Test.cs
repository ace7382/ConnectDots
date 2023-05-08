using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    ////Chose an image
    ////layout the image with x spacing until the screen is filled +/- 3? cols
    ////offsetevery other row to make a diagonal pattern
    ////

    //public bool go = false;

    //[Space]

    //public int bufferX;
    //public int bufferY;

    //public Texture2D image;
    //public UIDocument uiDoc;
    //public float xSpacing;
    //public float ySpacing;
    //public float imageSize = 100f;

    //public VisualElement page;

    //public List<VisualElement> bgIcons;
    //public int iconCount = 0;

    //public Vector2 scrollSpeed;

    //public float    leftBound;
    //public float    rightBound;
    //public float    topBound;
    //public float    bottomBound;

    //public void Update()
    //{
    //    if (go)
    //    {
    //        CreatePattern();

    //        go = false;
    //    }

    //    for (int i = 0; i < bgIcons.Count; i++)
    //    {
    //        float newX = bgIcons[i].transform.position.x + (Time.deltaTime * scrollSpeed.x);

    //        if (newX < leftBound)
    //            newX = rightBound - (leftBound - newX);
    //        else if (newX > rightBound)
    //            newX = leftBound + (newX - rightBound);

    //        float newY = bgIcons[i].transform.position.y + (Time.deltaTime * scrollSpeed.y);

    //        if (newY < topBound)
    //            newY = bottomBound - (topBound - newY);
    //        else if (newY > bottomBound)
    //            newY = topBound + (newY - bottomBound);

    //        bgIcons[i].transform.position = new Vector3(
    //            newX//bgIcons[i].transform.position.x + (Time.deltaTime * scrollSpeed.x)
    //            , newY //bgIcons[i].transform.position.y
    //            , bgIcons[i].transform.position.z);
    //    }
    //}

    //public void Start()
    //{
    //    bgIcons = new List<VisualElement>();
    //    page = uiDoc.rootVisualElement.Q<VisualElement>("Page");
    //}

    //public void AddIcon(VisualElement i)
    //{
    //    bgIcons.Add(i);

    //    iconCount = bgIcons.Count;
    //}

    //public void CreatePattern()
    //{
    //    leftBound = xSpacing * -bufferX;
    //    rightBound = Screen.width + (xSpacing * bufferX);
    //    rightBound = rightBound + (xSpacing - (rightBound % xSpacing));
    //    topBound = ySpacing * -bufferY;
    //    bottomBound = Screen.height + (ySpacing * bufferY);
    //    bottomBound = bottomBound + (ySpacing - (bottomBound % ySpacing));

    //    float currentX = leftBound;
    //    float currentY = topBound;

    //    page.Clear();
    //    bgIcons.Clear();
    //    iconCount = 0;

    //    bool alternaterow = false;
    //    int numOfRows = 0;

    //    while (currentY < bottomBound || numOfRows % 2 != 0)
    //    {
    //        numOfRows++;

    //        if (currentY >= bottomBound && numOfRows % 2 == 0)
    //            topBound = ySpacing * -(bufferY + 1);

    //        while (currentX < rightBound)
    //        {
    //            VisualElement instance = new VisualElement();
    //            instance.AddToClassList("RepeatingBGIcon");
    //            instance.style.backgroundImage = image;

    //            page.Add(instance);

    //            instance.transform.position = new Vector3(currentX, currentY, page.transform.position.z);

    //            currentX += xSpacing;

    //            AddIcon(instance);
    //        }

    //        alternaterow = !alternaterow;

    //        currentX = leftBound + (alternaterow ? xSpacing / 2f : 0f);
    //        currentY += ySpacing;
    //    }
    //}

    public bool init, i, c, spin;
    public Texture2D image;
    public Color color;
    public int numToRotate;

    public void Update()
    {
        if (init)
        {
            GetComponent<ScrollingBackground>().Initialize(image);

            init = false;
        }

        if (i)
        {
            GetComponent<ScrollingBackground>().SetTexture(image);

            i = false;
        }

        if (c)
        {
            GetComponent<ScrollingBackground>().SetColor(color);
            c = false;
        }

        if (spin)
        {
            GetComponent<ScrollingBackground>().RotateRandom(numToRotate);

            spin = false;
        }
    }
}