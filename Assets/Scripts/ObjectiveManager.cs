using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        this.AddObserver(OnTileColored, Notifications.TILES_COLORED);
    }

    private void OnDisable()
    {
        this.RemoveObserver(OnTileColored, Notifications.TILES_COLORED);
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

    #endregion

    #region Private Functions

    private void OnTileColored(object sender, object info)
    {
        //info      -   object[]    -   An object array with the following information
        //info[o]   -   Level       -   The level the tiles were colored on
        //info[1]   -   int         -   The ColorIndex of the tiles colored
        //info[2]   -   int         -   The number of tiles colored

        object[] data = (object[])info;
        Level level = (Level)data[0];
        int colorIndex = (int)data[1];
        int amount = (int)data[2];

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
}
