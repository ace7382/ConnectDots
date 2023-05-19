using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrencyManager : MonoBehaviour
{
    #region Singleton

    public static CurrencyManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private UIDocument uiDoc;

    #endregion

    #region Private Variables

    private VisualElement coinDisplayContainer;

    private Dictionary<int, int> ownedColors;
    private Vector2 coinFlyDestination; //TODO: Make this the UI element's origin? not sure it matters though
    private Dictionary<PowerupType, int> ownedPowerups;

    #endregion

    #region Public Properties

    public int TotalTokens { get { return ownedColors.Sum(x => x.Value); } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        ownedColors = new Dictionary<int, int>();
        ownedPowerups = new Dictionary<PowerupType, int>();

        CurrencyManager.instance.AddCurrency(PowerupType.HINT, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.REMOVE_SPECIAL_TILE, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.FILL_EMPTY, 20);
    }

    private void Start()
    {
        coinDisplayContainer = uiDoc.rootVisualElement.Q<VisualElement>("Container");
    }

    #endregion

    #region Public Functions

    public void AddCurrency(int colorIndex, int amount)
    {
        if (!ownedColors.ContainsKey(colorIndex))
            ownedColors.Add(colorIndex, 0);

        ownedColors[colorIndex] += amount;
    }

    public void SpendCurrency(int colorIndex, int amount)
    {
        if (!ownedColors.ContainsKey(colorIndex))
        {
            Debug.Log("Trying to spend a currency that you do not have");
        }

        if (amount > ownedColors[colorIndex])
        {
            Debug.Log(string.Format("Trying to spend {0} of color {1}. Only have {2} though"
                , amount.ToString(), colorIndex.ToString(), ownedColors[colorIndex].ToString()));

            return;
        }

        ownedColors[colorIndex] -= amount;
    }

    public override string ToString()
    {
        string ret = "***Current Currency***";

        foreach (KeyValuePair<int, int> color in ownedColors)
            ret += "\n" + color.Key.ToString() + ": " + color.Value.ToString();

        return ret;
    }

    public int GetCoinsForColorIndex(int index)
    {
        if (!ownedColors.ContainsKey(index))
            return 0;

        return ownedColors[index];
    }

    public Dictionary<int, int> AwardCoins(List<Tile> tiles)
    {
        //Return is Dictionary<ColorIndex, NumberAwarded>

        Dictionary<int, int> ret = new Dictionary<int, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile t = tiles[i];

            if (t.State == TileState.BLANK || t.LineCancelled)
                continue;

            if (t.Line != null)
            {
                if (ret.ContainsKey(t.Line.colorIndex))
                    ret[t.Line.colorIndex] += t.Multiplier;
                else
                    ret.Add(t.Line.colorIndex, t.Multiplier);
            }
            else
            {
                if (ret.ContainsKey(0)) //White Tiles
                    ret[0] += t.Multiplier;
                else
                    ret.Add(0, t.Multiplier);
            }
        }

        foreach (KeyValuePair<int,int> award in ret)
            AddCurrency(award.Key, award.Value);

        return ret;
    }

    public int AwardCoins(Tile tile)
    {
        if (tile.State == TileState.BLANK || tile.LineCancelled)
            return 0;

        AddCurrency(tile.Line == null ? 0 : tile.Line.colorIndex, tile.Multiplier);

        return tile.Multiplier;
    }

    public void SpawnCoin_Newt(int colorIndex, Vector3 origin)
    {
        coinFlyDestination                  = UIManager.instance.TopBar.CoinsButton.worldBound.center; //TODO: Set this outside of the function

        VisualElement coin                  = new VisualElement();
        coin.SetWidth(25f);
        coin.SetHeight(coin.style.width);
        coin.SetColor(UIManager.instance.GetColor(colorIndex));
        coin.SetBorderColor(Color.black);
        coin.SetBorderWidth(3f);
        coin.SetBorderRadius(10f);

        coin.style.position                 = Position.Absolute;
        coin.transform.position             = origin;

        coinDisplayContainer.Add(coin);

        float animationTime                 = Random.Range(.75f, 1f);
        float delay                         = Random.Range(0f, animationTime * .9f);

        Sequence seq                        = DOTween.Sequence();

        Tween goToCorner                    = DOTween.To(() => coin.transform.position,
                                                x => coin.transform.position = x,
                                                new Vector3(coinFlyDestination.x, coinFlyDestination.y, coin.transform.position.z),
                                                animationTime - delay)
                                                .SetDelay(delay)
                                                .SetEase(Ease.InBack);

        Tween scaleDown                     = DOTween.To(() => coin.transform.scale,
                                                x => coin.transform.scale = x,
                                                new Vector3(0f, 0f, coin.transform.scale.z),
                                                animationTime - delay)
                                                .SetDelay(delay)
                                                .SetEase(Ease.InBack)
                                                .OnKill(() => coin.RemoveFromHierarchy());

        seq.Append(goToCorner);
        seq.Join(scaleDown);
        seq.Play();
    }

    public void SpawnCoin(int colorIndex, Vector3 origin, VisualElement parent, Vector2 destination)
    {
        coinFlyDestination = destination; //TODO: Calculate this at app start/screensize change or link to a UI element

        VisualElement coin = new VisualElement();
        coin.style.SetWidth(25f);
        coin.style.SetHeight(25f);

        float animationTime = Random.Range(.75f, 1f);
        float delay = Random.Range(0f, animationTime * .9f);

        coin.SetColor(UIManager.instance.GetColor(colorIndex));
        coin.SetBorderColor(Color.black);
        coin.SetBorderWidth(3f);
        coin.SetBorderRadius(10f);
        coin.style.position = Position.Absolute;

        parent.Add(coin);

        coin.transform.position = origin;

        Tween goToCorner = DOTween.To(() => coin.transform.position,
                            x => coin.transform.position = x,
                            new Vector3(coinFlyDestination.x, coinFlyDestination.y, coin.transform.position.z),
                            animationTime - delay)
                            .SetDelay(delay)
                            .SetEase(Ease.InBack)
                            .Play();

        Tween scaleDwn = DOTween.To(() => coin.transform.scale,
                            x => coin.transform.scale = x,
                            new Vector3(0f, 0f, coin.transform.scale.z),
                            animationTime - delay)
                            .SetDelay(delay)
                            .SetEase(Ease.InBack)
                            .OnKill(() => coin.RemoveFromHierarchy())
                            .Play();
    }

    public int GetPowerupsOwned(PowerupType type)
    {
        if (ownedPowerups.ContainsKey(type))
            return ownedPowerups[type];

        return 0;
    }

    public void AddCurrency(PowerupType type, int amount)
    {
        if (!ownedPowerups.ContainsKey(type))
            ownedPowerups.Add(type, 0);

        ownedPowerups[type] += amount;
    }

    public void SpendCurrency(PowerupType type, int amount)
    {
        if (!ownedPowerups.ContainsKey(type))
        {
            Debug.Log("Trying to spend a powerup that you do not have");
        }

        if (amount > ownedPowerups[type])
        {
            Debug.Log(string.Format("Trying to use {0} of powerup {1}. Only have {2} though"
                , amount.ToString(), type.ToString(), ownedPowerups[type].ToString()));

            return;
        }

        ownedPowerups[type] -= amount;

        this.PostNotification(Notifications.POWERUP_USED, type);
    }

    #endregion

    #region Private Functions

    #endregion

#if UNITY_EDITOR
    #region Dev Help Functions

    [MenuItem("Dev Commands/Give 1000 of Each Color")]
    public static void Give1000Coins()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Editor is not in Playmode. This function cannot be used");
            return;
        }

        for (int i = 0; i < UIManager.instance.ColorCount; i++)
            CurrencyManager.instance.AddCurrency(i, 1000);
    }

    [MenuItem("Dev Commands/Give 20 of Each Powerup")]
    public static void Give20Powerups()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Editor is not in Playmode. This function cannot be used");
            return;
        }

        CurrencyManager.instance.AddCurrency(PowerupType.HINT, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.REMOVE_SPECIAL_TILE, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.FILL_EMPTY, 20);
    }


    #endregion
#endif
}
