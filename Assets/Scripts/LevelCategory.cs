using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Category", menuName = "New Category")]
public class LevelCategory : ScriptableObject
{
    #region Classes

    [System.Serializable]
    public class PurchaseLock
    {
        public int colorIndex;
        public int amount;
    }

    [System.Serializable]
    public class TimeAttackStats
    {
        public string   difficulty;
        public float    totalTimeInSeconds;
        public float    timeAddedOnCompletePuzzle;
        public int      numberOfPuzzles;

        public double   bestTimeInSeconds;
        public double   bronzeTimeInSeconds;
        public double   silverTimeInSeconds;
        public double   goldTimeInSeconds;
        public double   starTimeInSeconds;
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private List<Color>            colors;
    [SerializeField] private Texture2D              levelSelectImage;
    [SerializeField] private string                 filePath;

    [SerializeField] private bool                   unlocked;
    [SerializeField] private int                    lock_NumberOfObjectives;
    [SerializeField] private List<Objective>        lock_CompletedObjectives;
    [SerializeField] private List<LevelCategory>    lock_CompletedLevelCategories;
    [SerializeField] private List<PurchaseLock>     lock_Purcahse;

    [SerializeField] private List<TimeAttackStats>  timeAttackStats;

    [SerializeField] private bool                   isComplete;

    #endregion

    #region Public Properties

    public List<Color>                              Colors                  { get { return colors; } }
    public Texture2D                                LevelSelectImage        { get { return levelSelectImage; } }
    public string                                   FilePath                { get { return filePath; } }
    public bool                                     Unlocked                { get { return unlocked; } }
    public int                                      REQS_NumberOfObjectives { get { return lock_NumberOfObjectives; } }
    public List<PurchaseLock>                       REQS_Purchase           { get { return lock_Purcahse; } }
    public List<Objective>                          REQS_Objective          { get { return lock_CompletedObjectives; } }
    public List<LevelCategory>                      REQS_Category           { get { return lock_CompletedLevelCategories; } }
    public List<TimeAttackStats>                    TimeAttacks             { get { return timeAttackStats; } }
    public bool                                     IsComplete              { get { return isComplete; } set { isComplete = value; } }
    public int                                      LevelsComplete          { get { return GetCompletedLevels().Count; } }
    public int                                      BronzeTimeMedals        { get { return GetTimeMedals(0); } }
    public int                                      SilverTimeMedals        { get { return GetTimeMedals(1); } }
    public int                                      GoldTimeMedals          { get { return GetTimeMedals(2); } }
    public int                                      StarTimeMedals          { get { return GetTimeMedals(3); } }

    #endregion

    #region Public Functions

    public List<Level> GetLevels()
    {
        return Resources.LoadAll<Level>("Levels/" + FilePath).ToList();
    }

    public void UnlockCategory()
    {
        for (int i = 0; i < REQS_Purchase.Count; i++)
        {
            CurrencyManager.instance.SpendCurrency(REQS_Purchase[i].colorIndex, REQS_Purchase[i].amount);
        }

        unlocked = true;

        this.PostNotification(Notifications.CATEGORY_UNLOCKED);
    }

    #endregion

    #region Private Functions

    private List<Level> GetCompletedLevels()
    {
        return GetLevels().FindAll(x => x.IsComplete);
    }

    private int GetTimeMedals(int medalColor)
    {
        if (medalColor == 0)
            return TimeAttacks.FindAll(x => x.bestTimeInSeconds >= x.bronzeTimeInSeconds).Count;
        if (medalColor == 1)
            return TimeAttacks.FindAll(x => x.bestTimeInSeconds >= x.silverTimeInSeconds).Count;
        if (medalColor == 2)
            return TimeAttacks.FindAll(x => x.bestTimeInSeconds >= x.goldTimeInSeconds).Count;
        if (medalColor == 3)
            return TimeAttacks.FindAll(x => x.bestTimeInSeconds >= x.starTimeInSeconds).Count;

        return -1;
    }

    #endregion
}
