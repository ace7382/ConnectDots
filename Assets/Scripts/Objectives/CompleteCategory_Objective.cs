using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Category Complete Objective", menuName = "New Objective - Category Complete")]
public class CompleteCategory_Objective : Objective
{
    #region Inherited Functions

    public override float GetProgressAsPercentage()
    {
        List<Level> levs = LevelCategory.GetLevels();
        int complete = levs.FindAll(x => x.IsComplete).Count;

        return (float)complete / (float)levs.Count * 100f;
    }

    public override string GetProgressAsString()
    {
        List<Level> levs = LevelCategory.GetLevels();
        int complete = levs.FindAll(x => x.IsComplete).Count;

        return string.Format("{0} / {1}", complete.ToString(), levs.Count.ToString());
    }

    public override void Reset()
    {
        List<Level> allLevels = LevelCategory.GetLevels();

        for (int i = 0; i < allLevels.Count; i++)
        {
            allLevels[i].ResetLevel();
        }
    }

    #endregion

    #region Public Functions

    public void CheckComplete()
    {
        IsComplete = LevelCategory.GetLevels().FindIndex(x => !x.IsComplete) == -1; //This will be -1 if all levels in the cat are complete
    }

    #endregion
}
