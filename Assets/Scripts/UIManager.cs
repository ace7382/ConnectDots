using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager instance;

    #endregion

    #region Private Consts

    private const int BW_INDEX          = 0;
    private const int RED_INDEX         = 5;
    private const int PURPLE_INDEX      = 13;
    private const int BLUE_INDEX        = 20;
    private const int GREEN_INDEX       = 29;
    private const int YELLOW_INDEX      = 37;
    private const int ORANGE_INDEX      = 43;

    #endregion

    #region Inspector Variables

    [SerializeField] private TopBarController       topBarControl;
    [SerializeField] private VisualTreeAsset        levelSelectButton;
    [SerializeField] private VisualTreeAsset        objectiveCard;
    [SerializeField] private VisualTreeAsset        coinDisplay;
    [SerializeField] private VisualTreeAsset        requirementDisplay;
    [SerializeField] private VisualTreeAsset        timeAttackButtonPrefab;
    [SerializeField] private ScrollingBackground    scrollingBackground;
    [SerializeField] private Texture2D              restrictedTileTexture;
    [SerializeField] private Texture2D              trophyTexture;
    [SerializeField] private VisualTreeAsset        powerupIcon;

    [Space]

    [Header("Board Variables")]
    [SerializeField] private float                  boardPadding;

    [SerializeField] private VisualTreeAsset        newTilePrefab;
    [SerializeField] private VisualTreeAsset        newRowPrefab;

    [Space]

    [Header("Colors")]
    [SerializeField] private List<GameColor>        gameColors;

    [Space]

    [SerializeField] private Texture2D              bronzeMedal;
    [SerializeField] private Texture2D              silverMedal;
    [SerializeField] private Texture2D              goldMedal;
    [SerializeField] private Texture2D              starMedal;

    [Space]

    [SerializeField] private List<Texture2D>        powerupIcons;

    [Space]

    [Header("Settings Screen Templates")]
    [SerializeField] private VisualTreeAsset        settings_ColorList;
    [SerializeField] private VisualTreeAsset        settings_ColorLine;
    [SerializeField] private VisualTreeAsset        settings_ColorSetter;

    [Space]

    [Header("Reward Chests")]
    [SerializeField] private VisualTreeAsset        rewardChestButton;
    [SerializeField] private VisualTreeAsset        rewardChestDetails_RewardLine;
    [SerializeField] private List<Texture2D>        rewardChestSprites;

    #endregion

    #region Public Properties

    public float                Board_SpaceOnEdge   { get { return boardPadding; } }
    public TopBarController     TopBar              { get { return topBarControl; } }
    public VisualTreeAsset      LevelSelectButton   { get { return levelSelectButton; } }
    public VisualTreeAsset      ObjectiveCard       { get { return objectiveCard; } }
    public VisualTreeAsset      CoinDisplay         { get { return coinDisplay; } }
    public VisualTreeAsset      RequirementDisplay  { get { return requirementDisplay; } }
    public VisualTreeAsset      TimeAttackButton    { get { return timeAttackButtonPrefab; } }
    public VisualTreeAsset      PowerupButton       { get { return powerupIcon; } }
    public Texture2D            RestrictedTile      { get { return restrictedTileTexture; } }
    public Texture2D            TrophyTexture       { get { return trophyTexture; } }
    public Texture2D            BronzeMedal         { get { return bronzeMedal; } }
    public Texture2D            SilverMedal         { get { return silverMedal; } }
    public Texture2D            GoldMedal           { get { return goldMedal; } }
    public Texture2D            StarMedal           { get { return starMedal; } }
    public int                  ColorCount          { get { return gameColors.Count; } }

    public VisualTreeAsset      TilePrefab_New      { get { return newTilePrefab; } }
    public VisualTreeAsset      RowPrefab_New       { get { return newRowPrefab; } }

    public VisualTreeAsset      Settings_ColorList  { get { return settings_ColorList; } }
    public VisualTreeAsset      Settings_ColorLine  { get { return settings_ColorLine; } }
    public VisualTreeAsset      Settings_ColorSetter{ get { return settings_ColorSetter; } }

    public VisualTreeAsset      RewardChestButton   { get { return rewardChestButton; } }
    public VisualTreeAsset      RewardLine          { get { return rewardChestDetails_RewardLine; } }

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

    public GameColor GetGameColor(int index)
    {
        if (index > gameColors.Count - 1)
        {
            Debug.LogError("UIManager does not have enough colors for this request");
            return null;
        }

        return gameColors[index];
    }

    public Color GetColor(int index)
    {
        if (index > gameColors.Count - 1)
        {
            Debug.LogError("UIManager does not have enough colors for this request");
            return Color.clear;
        }

        return gameColors[index].color;
    }

    public Color GetColor(ColorCategory colorCategory)
    {
        //TODO: Set these

        switch (colorCategory)
        {
            case ColorCategory.BLACK_AND_WHITE:
                return gameColors[BW_INDEX].color;
            case ColorCategory.RED:
                return gameColors[RED_INDEX].color;
            case ColorCategory.PURPLE:
                return gameColors[PURPLE_INDEX].color;
            case ColorCategory.BLUE:
                return gameColors[BLUE_INDEX].color;
            case ColorCategory.GREEN:
                return gameColors[GREEN_INDEX].color;
            case ColorCategory.YELLOW:
                return gameColors[YELLOW_INDEX].color;
            default: //Orange
                return gameColors[ORANGE_INDEX].color;
        }
    }
    
    public string GetColorName(int index)
    {
        if (index > gameColors.Count - 1)
        {
            Debug.LogError("UIManager does not have enough colors for this request");
            return "ERROR";
        }

        return gameColors[index].name;
    }

    //public string GetColorName(ColorCategory colorCategory)
    //{
    //    switch (colorCategory)
    //    {
    //        case ColorCategory.BLACK_AND_WHITE:
    //            return "Black and White";
    //            //return gameColors[BW_INDEX].name;
    //        case ColorCategory.RED:
    //            return gameColors[RED_INDEX].name;
    //        case ColorCategory.PURPLE:
    //            return gameColors[PURPLE_INDEX].name;
    //        case ColorCategory.BLUE:
    //            return gameColors[BLUE_INDEX].name;
    //        case ColorCategory.GREEN:
    //            return gameColors[GREEN_INDEX].name;
    //        case ColorCategory.YELLOW:
    //            return gameColors[YELLOW_INDEX].name;
    //        default: //Orange
    //            return gameColors[ORANGE_INDEX].name;
    //    }
    //}

    public void UpdateColor(int index, string newName, Color newColor)
    {
        if (index > gameColors.Count - 1)
        {
            Debug.Log("UIManager does not have enough colors for this request");
            return;
        }

        gameColors[index].name = newName;
        gameColors[index].color = newColor;
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

    public Texture2D GetRewardChestIcon(RewardChestType type)
    {
        switch (type)
        {
            case RewardChestType.LEVELUP: return rewardChestSprites[0];
            case RewardChestType.POWERUP: return rewardChestSprites[1];
        }

        return null;
    }

    #endregion
}
