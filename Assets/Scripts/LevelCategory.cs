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
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private Color                  color;
    [SerializeField] private Texture2D              levelSelectImage;
    [SerializeField] private Texture2D              backgroundImage;
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

    public Color Color { get { return color; } }
    public Texture2D LevelSelectImage { get { return levelSelectImage; } }
    public Texture2D BackgroundImage { get { return backgroundImage; } }
    public string FilePath { get { return filePath; } }
    public bool Unlocked { get { return unlocked; } }
    public int REQS_NumberOfObjectives { get { return lock_NumberOfObjectives; } }
    public List<PurchaseLock> REQS_Purchase { get { return lock_Purcahse; } }
    public List<Objective> REQS_Objective { get { return lock_CompletedObjectives; } }
    public List<LevelCategory> REQS_Category { get { return lock_CompletedLevelCategories; } }
    public List<TimeAttackStats> TimeAttacks { get { return timeAttackStats; } }
    public bool IsComplete { get { return isComplete; } set { isComplete = value; } }

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
}
