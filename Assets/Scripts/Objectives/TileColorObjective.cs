using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileColorObjective", menuName = "New Objective - Tile Color")]
public class TileColorObjective : Objective
{
    #region Inspector Variables

    [Header("Tile Color Objective Fields")]
    [SerializeField] private int colorIndex;
    [SerializeField] private int goal;
    [SerializeField] private int progress;

    #endregion

    #region Public Properties

    public int ColorIndex { get { return colorIndex; } }
    public int Goal { get { return goal; } }
    public int Progress { 
        get { return progress; } 
        private set
        {
            if (value >= goal)
            { 
                progress    = goal;
                IsComplete  = true;
            }
            else
                progress = value;
        }
    }

    #endregion

    #region Inherited Functions

    public override string GetProgressAsString()
    {
        return string.Format("{0} / {1}", Progress.ToString(), Goal.ToString());
    }

    public override float GetProgressAsPercentage()
    {
        return IsComplete ? 100f : (float)progress / (float)goal * 100f;
    }

    public override void Reset()
    {
        base.Reset();

        Progress    = 0;
        IsComplete  = false;
    }

    #endregion

    #region Public Function

    public void AddProgress(int amountToAdd)
    {
        if (amountToAdd < 0 || IsComplete)
            return;

        Progress = Progress + amountToAdd;
    }

    #endregion
}