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
    [SerializeField] private VisualTreeAsset requirementDisplay;
    [SerializeField] private VisualTreeAsset timeAttackButtonPrefab;
    [SerializeField] private ScrollingBackground scrollingBackground;
    [SerializeField] private Texture2D restrictedTileTexture;
    [SerializeField] private Texture2D trophyTexture;

    [Space]

    [Header("Board Variables")]
    [SerializeField] private VisualTreeAsset tilePrefab;
    [SerializeField] private float boardPadding;
    [SerializeField] private float hardBorderSize;
    [SerializeField] private Color hardBorderColor;
    [SerializeField] private float softBorderSize;
    [SerializeField] private Color softBorderColor;

    [Space]

    [Header("Colors")]
    [SerializeField] private List<Color> gameColors;

    [Space]
    [SerializeField] private Sprite directionalEndSprite;
    [SerializeField] private Sprite omnidirectionalEndSprite;
    [SerializeField] private Sprite lineSprite;
    [SerializeField] private Sprite cornerSprite;

    [Space]

    [SerializeField] private Texture2D bronzeMedal;
    [SerializeField] private Texture2D silverMedal;
    [SerializeField] private Texture2D goldMedal;
    [SerializeField] private Texture2D starMedal;

    [Space]

    [SerializeField] private List<Texture2D> powerupIcons;

    #endregion

    #region Public Properties

    public VisualTreeAsset      TilePrefab          { get { return tilePrefab; } }
    public float                BoardPadding        { get { return boardPadding; } }
    public float                HardBorderSize      { get { return hardBorderSize; } }
    public float                SoftBorderSize      { get { return softBorderSize; } }
    public Color                HardBorderColor     { get { return hardBorderColor; } }
    public Color                SoftBorderColor     { get { return softBorderColor; } }
    public TopBarController     TopBar              { get { return topBarControl; } }
    public VisualTreeAsset      LevelSelectButton   { get { return levelSelectButton; } }
    public VisualTreeAsset      ObjectiveCard       { get { return objectiveCard; } }
    public VisualTreeAsset      CoinDisplay         { get { return coinDisplay; } }
    public VisualTreeAsset      RequirementDisplay  { get { return requirementDisplay; } }
    public VisualTreeAsset      TimeAttackButton    { get { return timeAttackButtonPrefab; } }
    public Texture2D            RestrictedTile      { get { return restrictedTileTexture; } }
    public Texture2D            TrophyTexture       { get { return trophyTexture; } }
    public Texture2D            BronzeMedal         { get { return bronzeMedal; } }
    public Texture2D            SilverMedal         { get { return silverMedal; } }
    public Texture2D            GoldMedal           { get { return goldMedal; } }
    public Texture2D            StarMedal           { get { return starMedal; } }
    public int                  ColorCount          { get { return gameColors.Count; } }

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
        scrollingBackground.SetColor(color);
        scrollingBackground.SetTexture(texture);
    }

    public void SetBackground(Color color)
    {
        scrollingBackground.SetColor(color);
    }

    public void SetBackgroundShift(List<Color> colors)
    {
        scrollingBackground.Page.SetShiftingBGColor(colors);
    }

    public Color GetBackgroundColor()
    {
        return scrollingBackground.Color;
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

    public Sprite GetTileStateTexture(TileState state, EndTileRotation endPieceRotation)
    {
        if (state == TileState.END)
        {
            if (endPieceRotation == EndTileRotation.OPEN)
                return omnidirectionalEndSprite;

            return directionalEndSprite;
        }
        else if (state == TileState.LINE)
        {
            return lineSprite;
        }
        else if (state == TileState.CORNER)
        {
            return cornerSprite;
        }
        else if (state == TileState.HEAD)
        {
            return directionalEndSprite;
        }

        return null;
    }

    public Texture2D GetPowerupIcon(PowerupType type)
    {
        switch (type)
        {
            case PowerupType.HINT:
                return powerupIcons[0];
            case PowerupType.REMOVE_SPECIAL_TILE:
                return powerupIcons[1];
            case PowerupType.FILL_EMPTY:
                return powerupIcons[2];
        }

        Debug.LogError("Can't find powerup Icon");

        return null;
    }
    #endregion
}
