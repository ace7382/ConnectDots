using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ScrollingBackground : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Texture2D startingImage;
    [SerializeField] private Color startingColor;
    [SerializeField] private Vector2 scrollSpeed;
    [SerializeField] private Vector2 spacing;

    [SerializeField] private List<Texture2D> multiImagesTest;
    [SerializeField] private List<Color> testCOlors;
    [SerializeField] private bool multitest;
    [SerializeField] private bool multiTestRandom;
    [SerializeField] private bool spinTest;
    [SerializeField] private bool fadeTest;
    [SerializeField] private bool changecolorsbgtest;
    [SerializeField] private bool testdeletepage;

    #endregion

    #region Private Variables

    private UIDocument uiDoc;
    private List<VisualElement> icons;
    private VisualElement page;

    private Vector2Int buffer;

    private float leftBound;
    private float rightBound;
    private float topBound;
    private float bottomBound;

    private HashSet<VisualElement> spins;
    private HashSet<VisualElement> fades;

    #endregion

    #region Public Properties

    public Color Color { get { return page.style.backgroundColor.value; } }
    public VisualElement Page { get { return page; } }

    #endregion

    #region Unity Functions

    private void Start()
    {
        uiDoc   = GetComponent<UIDocument>();
        page    = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        icons   = new List<VisualElement>();
        spins   = new HashSet<VisualElement>();
        fades   = new HashSet<VisualElement>();

        Initialize(startingImage);
    }

    private void Update()
    {
        if (multitest)
        {
            SetMultipleImages(multiImagesTest, multiTestRandom);
            multitest = false;
        }

        if (spinTest)
        {
            RotateRandom(20);
            spinTest = false;
        }

        if (fadeTest)
        {
            FadeRandom(20);
            fadeTest = false;
        }
        
        if (changecolorsbgtest)
        {
            page.SetShiftingBGColor(testCOlors);

            changecolorsbgtest = false;
        }

        if (testdeletepage)
        {
            icons.Clear();
            page.RemoveFromHierarchy();
            testdeletepage = false;
        }

        for (int i = 0; i < icons.Count; i++)
        {
            float newX = icons[i].transform.position.x + (Time.deltaTime * scrollSpeed.x);

            if (newX < leftBound)
                newX = rightBound - (leftBound - newX);
            else if (newX > rightBound)
                newX = leftBound + (newX - rightBound);

            float newY = icons[i].transform.position.y + (Time.deltaTime * scrollSpeed.y);

            if (newY < topBound)
                newY = bottomBound - (topBound - newY);
            else if (newY > bottomBound)
                newY = topBound + (newY - bottomBound);

            icons[i].transform.position = new Vector3(
                newX
                , newY
                , icons[i].transform.position.z);
        }
    }

    #endregion

    #region Public Functions

    public void Initialize(Texture2D image)
    {
        buffer              = new Vector2Int(1, 1);

        leftBound           = spacing.x * -buffer.x;
        rightBound          = Screen.width + (spacing.x * buffer.x);
        rightBound          = rightBound + (spacing.x - (rightBound % spacing.x));
        topBound            = spacing.y * -buffer.y;
        bottomBound         = Screen.height + (spacing.y * buffer.y);
        bottomBound         = bottomBound + (spacing.y - (bottomBound % spacing.y));

        Vector2 current     = new Vector2(leftBound, topBound);

        bool alternateRow   = false;
        bool needsExtraRow  = false;

        while (current.y < bottomBound || needsExtraRow)
        {
            needsExtraRow = !needsExtraRow;

            if (current.y >= bottomBound && !needsExtraRow)
                topBound = spacing.y * -(buffer.y + 1);

            while (current.x < rightBound)
            {
                VisualElement instance = new VisualElement();
                instance.name = current.ToString();
                instance.AddToClassList("RepeatingBGIcon");
                instance.style.backgroundImage = image;

                page.Add(instance);

                instance.transform.position = new Vector3(current.x, current.y, page.transform.position.z);

                current.x += spacing.x;

                icons.Add(instance);
            }

            alternateRow = !alternateRow;

            current.x = leftBound + (alternateRow ? spacing.x / 2f : 0f);
            current.y += spacing.y;
        }

        SetColor(startingColor);
    }

    public void SetTexture(Texture2D texture)
    {
        for (int i = 0; i < icons.Count; i++)
            icons[i].SetImage(texture);
    }

    public void SetColor(Color color)
    {
        page.SetColor(color);
    }

    public void RotateRandom(int num)
    {
        icons.Shuffle();

        num = Mathf.Min(num, icons.Count);

        for (int i = 0; i < num; i++)
        {
            int j = i;

            VisualElement icon = icons[j];

            if (spins.Contains(icon))
                continue;

            spins.Add(icon);

            Tween t = DOTween.To(() => icon.transform.rotation.eulerAngles,
                x => icon.transform.rotation = Quaternion.Euler(x),
                new Vector3(0f, 0f, 360f), 2f).SetEase(Ease.InOutBack)
                .OnKill(() => spins.Remove(icon))
                .Play();
        }
    }

    private void FadeRandom(int num)
    {
        //TODO: use icon.style.opacity vs color. Might be more performant bc
        //1 value is vhanging vs an entire color struct

        icons.Shuffle();

        num = Mathf.Min(num, icons.Count);

        for (int i = 0; i < num; i++)
        {
            int j = i;

            VisualElement icon = icons[j];

            if (fades.Contains(icon))
                continue;

            fades.Add(icon);

            icon.style.unityBackgroundImageTintColor = new StyleColor(new Color(1f, 1f, 1f, 1f));

            Debug.Log(icon.name + ": " + icon.style.unityBackgroundImageTintColor.value);

            Color retColor = icon.style.unityBackgroundImageTintColor.value;

            Tween back = DOTween.To(() => icon.style.unityBackgroundImageTintColor.value,
                x => icon.style.unityBackgroundImageTintColor = new StyleColor(x),
                retColor
                , 2f)
                .OnPlay(() => Debug.Log(icon.name + " fading in"))
                .SetEase(Ease.InOutBack)
                .OnKill(() => fades.Remove(icon))
                .Pause();

            Tween t = DOTween.To(() => icon.style.unityBackgroundImageTintColor.value.a,
                x => icon.style.unityBackgroundImageTintColor = new StyleColor(
                            new Color(icon.style.unityBackgroundImageTintColor.value.r,
                                icon.style.unityBackgroundImageTintColor.value.g,
                                icon.style.unityBackgroundImageTintColor.value.b,
                                x)),
                0f
                , 2f)
                .SetEase(Ease.InOutBack)
                .OnComplete(() => back.Play())
                .Play();
        }
    }

    public void SetMultipleImages(List<Texture2D> images, bool random = true)
    {
        if (random) icons.Shuffle();
        else
        {
            icons.Clear();
            icons = page.Children().ToList();
        }

        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetImage(images[i % images.Count]);
        }
    }

    #endregion

    #region Private Functions

    #endregion
}
