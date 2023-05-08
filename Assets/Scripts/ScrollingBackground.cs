using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ScrollingBackground : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Texture2D startingImage;

    #endregion

    #region Private Variables

    private UIDocument uiDoc;
    private List<VisualElement> icons;
    private VisualElement page;

    private Vector2Int buffer;
    [SerializeField] private Vector2 scrollSpeed;
    [SerializeField] private Vector2 spacing;

    private float leftBound;
    private float rightBound;
    private float topBound;
    private float bottomBound;
    private HashSet<int> spins;

    #endregion

    #region Public Properties

    public Color Color { get { return page.style.backgroundColor.value; } }

    #endregion

    #region Unity Functions

    private void Start()
    {
        uiDoc   = GetComponent<UIDocument>();
        page    = uiDoc.rootVisualElement.Q<VisualElement>("Page");
        icons   = new List<VisualElement>();
        spins   = new HashSet<int>();

        Initialize(startingImage);
    }

    private void Update()
    {
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
        scrollSpeed         = new Vector2(-80f, -80f);
        spacing             = new Vector2(175f, 175f);

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

        SetColor(UIManager.instance.GetColor(1));
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

            if (spins.Contains(j))
                continue;

            spins.Add(j);

            VisualElement icon = icons[j];

            Tween t = DOTween.To(() => icon.transform.rotation.eulerAngles,
                x => icon.transform.rotation = Quaternion.Euler(x),
                new Vector3(0f, 0f, 360f), 2f).SetEase(Ease.InOutBack)
                .OnKill(() => spins.Remove(j))
                .Play();
        }
    }

    #endregion

    #region Private Functions

    #endregion
}
