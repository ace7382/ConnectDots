using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum PowerupType
{
    [EnumName("")] 
    [Instructions("")]
    none,

    [EnumName("Hint")]
    [Instructions("")]
    HINT,

    [EnumName("Normalizer")]
    [Instructions("Select a Special Tile to Remove")]
    REMOVE_SPECIAL_TILE,

    [EnumName("Hole Filler")]
    [Instructions("Select a Blank Tile to Fill in")]
    FILL_EMPTY,
}

public class PowerupButtonController
{
    #region Consts

    private Color havePowerups = new Color(0.2588235f, 0.8980392f, 0.4823529f, 1f);
    private Color needPowerups = new Color(1f, 0.3764706f, 0.372549f, 1f);

    #endregion

    #region Private Variables

    private VisualElement root;
    private PowerupType type;

    #endregion

    #region Constructor

    public PowerupButtonController(PowerupType type, VisualElement root)
    {
        this.root = root;
        this.type = type;

        root.Q<VisualElement>("Icon").style.backgroundImage = UIManager.instance.GetPowerupIcon(this.type);

        SetCounter();

        this.AddObserver(PowerupUsed, Notifications.POWERUP_USED);
    }

    #endregion

    #region Public Functions

    public void UnregisterListeners()
    {
        this.RemoveObserver(PowerupUsed, Notifications.POWERUP_USED);
    }

    #endregion

    #region Private Functions

    private void PowerupUsed(object sender, object info)
    {
        //info  -   PowerupType -   The Type of powerup used

        if ((PowerupType)info == type)
            SetCounter();
    }

    private void SetCounter()
    {
        int owned = CurrencyManager.instance.GetPowerupsOwned(this.type);

        if (owned == 0)
        {
            root.Q<VisualElement>("Counter").SetColor(needPowerups);
            root.Q<Label>("Count").text = "+";
        }
        else
        {
            root.Q<VisualElement>("Counter").SetColor(havePowerups);
            root.Q<Label>("Count").text = owned > 99 ? "99+" : owned.ToString();
        }
    }
    #endregion
}
