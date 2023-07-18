using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    #region Singleton

    public static ObjectiveManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private List<Objective> objectives;

    #endregion

    #region Private Variables

    private List<Objective> completeObjectives = new List<Objective>();

    #endregion

    #region Public Properties

    public int CompletedObjectivesCount { get { return completeObjectives.Count; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        for (int i = 0; i < objectives.Count; i++)
            if (objectives[i].IsComplete)
                MarkAsComplete(objectives[i]);
    }

    private void OnEnable()
    {
        this.AddObserver(OnTileColored, Notifications.TILES_COLORED);
        this.AddObserver(OnLevelCompleted, Notifications.LEVEL_COMPLETE);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnTileColored, Notifications.TILES_COLORED);
        this.RemoveObserver(OnLevelCompleted, Notifications.LEVEL_COMPLETE);
    }

    #endregion

    #region Public Functions

    public static void ResetObjectives()
    {
        List<Objective> allobjectives = Resources.LoadAll<Objective>("Objectives").ToList();

        for (int i = 0; i < allobjectives.Count; i++)
        {
            allobjectives[i].Reset();
        }
    }

    public void MarkAsComplete(Objective o)
    {
        objectives.Remove(o);
        completeObjectives.Add(o);

        this.PostNotification(Notifications.OBJECTIVE_COMPLETE);
    }

    public List<Objective> GetObjectivesForCategory(LevelCategory cat)
    {
        List<Objective> retList = new List<Objective>();

        for (int i = 0; i < objectives.Count; i++)
            if (objectives[i].LevelCategory == cat)
                retList.Add(objectives[i]);

        for (int i = 0; i < completeObjectives.Count; i++)
            if (completeObjectives[i].LevelCategory == cat)
                retList.Add(completeObjectives[i]);

        //TODO: Sort so that they're in the same order always

        return retList;
    }

    public List<Objective> GetCompletedObjectivesForCategory(LevelCategory cat)
    {
        List<Objective> retList = new List<Objective>();

        for (int i = 0; i < completeObjectives.Count; i++)
            if (completeObjectives[i].LevelCategory == cat)
                retList.Add(completeObjectives[i]);

        //TODO: Sort?

        return retList;
    }

    public List<Objective> GetAllObjectives()
    {
        List<Objective> ret = new List<Objective>();

        ret.AddRange(objectives.FindAll(x => !x.IsAchievement));
        ret.AddRange(completeObjectives.FindAll(x => !x.IsAchievement));

        //TODO: Sort

        return ret;
    }

    public List<Objective> GetAllAchievements()
    {
        List<Objective> ret = new List<Objective>();

        ret.AddRange(objectives.FindAll(x => x.IsAchievement));
        ret.AddRange(completeObjectives.FindAll(x => x.IsAchievement));

        //TODO: Sort

        return ret;
    }

    public int GetNumberOfUnclaimedAndCompleteObjectives()
    {
        int ret = completeObjectives.FindAll(x => x.IsComplete && !x.RewardClaimed).Count;

        return ret;
    }

    public int GetNumberOfUnclaimedAndCompleteObjectives(LevelCategory cat)
    {
        return completeObjectives.FindAll(x => x.LevelCategory == cat && x.IsComplete && !x.RewardClaimed).Count;
    }

    #endregion

    #region Private Functions

    private void OnLevelCompleted(object sender, object info)
    {
        //Sender    -   Level       -   The level that was completed
        //info      -   N/A

        Level completedLevel = (Level)sender;

        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i] is CompleteCategory_Objective)
            {
                CompleteCategory_Objective objective = (CompleteCategory_Objective)objectives[i];

                if (objective.LevelCategory == completedLevel.LevelCategory)
                    objective.CheckComplete();
            }
        }
    }

    private void OnTileColored(object sender, object info)
    {
        //Sender    -   N/A
        //info      -   object[]    -   An object array with the following information
        //info[0]   -   Level       -   The level the tiles were colored on
        //info[1]   -   int         -   The ColorIndex of the tiles colored
        //info[2]   -   int         -   The number of tiles colored

        object[] data       = (object[])info;
        Level level         = (Level)data[0];
        int colorIndex      = (int)data[1];
        int amount          = (int)data[2];

        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i] is TileColorObjective)
            {
                TileColorObjective objective = (TileColorObjective)objectives[i];

                //If the level category is blank or the same
                //if the level is blank or the same
                //if the color index is the same
                //Add

                if (objective.LevelCategory == null || objective.LevelCategory == level.LevelCategory)
                {
                    if (objective.Level == null || objective.Level == level)
                    {
                        if (objective.ColorIndex == colorIndex)
                        {
                            objective.AddProgress(amount);
                        }
                    }
                }
            }
        }
    }

    #endregion


#if UNITY_EDITOR
    #region Dev Help Functions

    [MenuItem("Dev Commands/Link Objectives")]
    public static void LinkObjectivesToObjectiveManager()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Editor is in Playmode. This function cannot be used");
            return;
        }

        GameObject.FindObjectOfType<ObjectiveManager>().objectives = Resources.LoadAll<Objective>("Objectives").ToList();
    }

    #endregion
#endif

}
