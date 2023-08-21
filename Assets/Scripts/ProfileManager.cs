using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    #region Singleton

    public static ProfileManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private int[] neededEXP;

    #endregion

    #region Public Properties

    public int HighestEXPColorLevel { get { return expLevelsPerColor.Max(); } }
    public int TotalLevel           { get { return expLevelsPerColor.Sum(); } }

    #endregion

    #region Private variables

    private int[]   expLevelsPerColor; //TODO: Remove SerializeField and don't set from inspector, just for testing atm
    private int[]   currentEXPPerColor;

    #endregion

    #region Unity Function

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        expLevelsPerColor   = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
        currentEXPPerColor  = new int[7] { 0, 0, 0, 0, 0, 0, 0 };
    }

    #endregion

    #region Public Functions

    public int GetNeededEXP(int currentLevel)
    {
        return neededEXP[currentLevel];
    }
    
    public int GetEXPLevel(ColorCategory color)
    {
        return expLevelsPerColor[(int)color];
    }

    public int GetNextEXPLevel(ColorCategory color)
    {
        //TODO: Set a max level probably

        return GetEXPLevel(color) + 1;
    }

    public int GetCurrentEXP(ColorCategory color)
    {
        return currentEXPPerColor[(int)color];
    }

    public bool AddEXP(ColorCategory color, int amount)
    {
        int index                       = (int)color;
        int nextLevel                   = GetNeededEXP(GetEXPLevel(color));
        bool leveledUp                  = false;

        currentEXPPerColor[index]       += amount;
        
        while (currentEXPPerColor[index] >= nextLevel)
        {
            leveledUp                   = true;
            currentEXPPerColor[index]   -= nextLevel;
            expLevelsPerColor[index]++;
            nextLevel                   = GetNeededEXP(GetEXPLevel(color));
        }

        return leveledUp;
    }

    #endregion

    #region Private Functions

    private void LevelUp(ColorCategory color)
    {
        //TODO: Handle max level
    }

    #endregion
}