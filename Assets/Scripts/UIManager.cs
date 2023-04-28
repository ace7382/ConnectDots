using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private TopBarController topBarControl;
    [SerializeField] private VisualTreeAsset levelSelectButton;
    [SerializeField] private VisualTreeAsset objectiveCard;
    [SerializeField] private VisualTreeAsset coinDisplay;
    [SerializeField] private BackgroundScroll scrollingBG;
    [SerializeField] private Texture2D restrictedTileTexture;

    [Space]

    [Header("Colors")]
    [SerializeField] private List<Color> gameColors;

    #endregion

    #region Public Properties

    public TopBarController TopBar { get { return topBarControl; } }
    public VisualTreeAsset LevelSelectButton { get { return levelSelectButton; } }
    public VisualTreeAsset ObjectiveCard { get { return objectiveCard; } }
    public VisualTreeAsset CoinDisplay { get { return coinDisplay; } }
    public Texture2D RestrictedTile { get { return restrictedTileTexture; } }
    public int ColorCount { get { return gameColors.Count; } }

    #endregion

    #region Unity Functions

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    #endregion

    #region Public Functions

    public void SetBackground(Texture2D texture, Color color)
    {
        scrollingBG.Set(texture, color);
    }

    public Color GetColor(int index)
    {
        if (index > gameColors.Count - 1)
        {
            Debug.LogError("UIManager does not have enough colors for this request");
            return Color.clear;
        }

        return gameColors[index];
    }

    #endregion
}
