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

    public static CurrencyManager           instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private UIDocument     uiDoc;

    #endregion

    #region Private Variables

    private VisualElement                   coinDisplayContainer;
    private Dictionary<PowerupType, int>    ownedPowerups;
    private int[]                           ownedSegments;
    private List<RewardChest>               ownedRewardChests;

    #endregion

    #region Public Properties

    public int                              SegmentColorCount   { get { return System.Enum.GetNames(typeof(ColorCategory)).Length; } }
    public int                              TotalSegments       { get { return ownedSegments.Sum(); } }
    public int                              TotalRewardChests   { get { return ownedRewardChests.Count; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        ownedPowerups       = new Dictionary<PowerupType, int>();
        ownedSegments       = new int[SegmentColorCount];
        ownedRewardChests   = new List<RewardChest>();

        //TODO: Remove this///
        CurrencyManager.instance.AddCurrency(PowerupType.HINT, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.REMOVE_SPECIAL_TILE, 20);
        CurrencyManager.instance.AddCurrency(PowerupType.FILL_EMPTY, 20);

        for (int i = 0; i < SegmentColorCount; i++)
        {
            CurrencyManager.instance.AddCurrency((ColorCategory)i, Random.Range(50, 12000));
        }

        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.LEVELUP));
        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.LEVELUP));
        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.LEVELUP));
        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.POWERUP));
        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.POWERUP));
        CurrencyManager.instance.AddRewardChest(RewardChest.GetChest(RewardChestType.POWERUP));
        //////////////////////
    }

    private void Start()
    {
        coinDisplayContainer = uiDoc.rootVisualElement.Q<VisualElement>("Container");
    }

    #endregion

    #region Public Functions

    public void AddCurrency(ColorCategory colorCategory, int amount)
    {
        ownedSegments[(int)colorCategory] += amount;
    }

    public void SpendCurrency(ColorCategory colorCategory, int amount)
    {
        if (amount > ownedSegments[(int)colorCategory])
        {
            return;
        }

        ownedSegments[(int)colorCategory] -= amount;
    }

    public int GetCoinsForColorIndex(ColorCategory colorCategory)
    {
        return ownedSegments[(int)colorCategory];
    }

    public int AwardCoins(Tile tile)
    {
        if (tile.State == TileState.BLANK || tile.LineCancelled)
            return 0;

        int numToAdd = tile.Multiplier;

        AddCurrency(tile.Line == null ? 0 : UIManager.instance.GetGameColor(tile.Line.ColorIndex).category, numToAdd);

        return numToAdd;
    }

    public void SpawnCoin(ColorCategory colorCategory, Vector3 origin, Vector2 destination, float animationTime = 0f)
    {
        VisualElement coin                  = new VisualElement();
        coin.SetWidth(25f);
        coin.SetHeight(coin.style.width);
        coin.SetColor(UIManager.instance.GetColor(colorCategory));
        coin.SetBorderColor(Color.black);
        coin.SetBorderWidth(3f);
        coin.SetBorderRadius(10f);

        coin.style.position                 = Position.Absolute;
        coin.transform.position             = origin;

        coinDisplayContainer.Add(coin);

        animationTime                       = animationTime == 0f ? Random.Range(.75f, 1f) : animationTime;
        float delay                         = Random.Range(0f, animationTime * .9f);

        Sequence seq                        = DOTween.Sequence();

        Tween goToCorner                    = DOTween.To(() => coin.transform.position,
                                                x => coin.transform.position = x,
                                                new Vector3(destination.x, destination.y, coin.transform.position.z),
                                                animationTime - delay)
                                                .SetDelay(delay)
                                                .SetEase(Ease.InBack)
                                                .OnKill(() => coin.RemoveFromHierarchy());

        //TODO: Scaling and fading seem to happen basically instantly. Might need to use
        //      different properties to fade them out, or might want to just have a destination object
        //      that throbs/flashes and have the coins just remove themselves

        //Tween scaleDown                     = DOTween.To(() => coin.transform.scale,
        //                                        x => coin.transform.scale = x,
        //                                        Vector3.zero,
        //                                        animationTime - delay)
        //                                        .SetDelay(delay)
        //                                        .SetEase(Ease.InBack)
        //                                        .OnKill(() => coin.RemoveFromHierarchy());

        //Tween fade                          = DOTween.To(
        //                                        () => coin.style.opacity.value
        //                                        , x => coin.SetOpacity(x)
        //                                        , 0f
        //                                        , animationTime - delay
        //                                    )
        //                                    .SetDelay(delay)
        //                                    .SetEase(Ease.Linear)
        //                                    .OnKill(() => coin.RemoveFromHierarchy());

        seq.Append(goToCorner);
        //seq.Join(fade);
        //seq.Join(scaleDown);
        seq.Play();
    }

    public void SpawnPowerups(PowerupType powerupType, Vector3 origin, Vector2 destination, float animationTime = 0f)
    {
        VisualElement coin                  = UIManager.instance.PowerupButton.Instantiate();
        VisualElement container             = coin.Q<VisualElement>("Container");
        VisualElement bg                    = container.Q<VisualElement>("BG");
        VisualElement icon                  = bg.Q<VisualElement>("Icon");

        coin.SetWidth(100f);
        coin.SetHeight(coin.style.width);
        container.SetWidth(coin.style.width);
        container.SetHeight(coin.style.width);
        bg.SetWidth(80f);
        bg.SetHeight(bg.style.width);
        icon.SetMargins(10f);

        coin.Q<VisualElement>("Counter").RemoveFromHierarchy();
        icon.SetImage(UIManager.instance.GetPowerupIcon(powerupType));

        coin.style.position                 = Position.Absolute;
        coin.transform.position             = origin;

        coinDisplayContainer.Add(coin);

        animationTime                       = animationTime == 0f ? Random.Range(.75f, 1f) : animationTime;
        float delay                         = Random.Range(0f, animationTime * .9f);

        Tween goToDestination               = DOTween.To(() => coin.transform.position,
                                                x => coin.transform.position = x,
                                                new Vector3(destination.x, destination.y, coin.transform.position.z),
                                                animationTime - delay)
                                                .SetDelay(delay)
                                                .SetEase(Ease.InBack)
                                                .OnKill(() => coin.RemoveFromHierarchy());

        goToDestination.Play();
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

    public bool CanAfford(ShopItem item)
    {
        //Find an index where the curr man has fewer coins than the cost amount
        //if index == 0+ -> cannot afford (return false)
        //if index == -1 (doesn't exist) -> can afford (return true)

        return item.Costs.FindIndex(x => GetCoinsForColorIndex(x.colorCategory) < x.amount) == -1;
    }

    public RewardChest GetRewardChest(int index)
    {
        return ownedRewardChests[index];
    }

    public void AddRewardChest(RewardChest chestToAdd)
    {
        ownedRewardChests.Add(chestToAdd);
    }
    
    public void OpenRewardChest(RewardChest chest)
    {



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

        for (int i = 0; i < System.Enum.GetNames(typeof(ColorCategory)).Length; i++)
            CurrencyManager.instance.AddCurrency((ColorCategory)i, 1000);
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
