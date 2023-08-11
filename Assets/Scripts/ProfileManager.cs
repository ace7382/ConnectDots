using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    #region Enum

    public enum EXPColor
    {
        BLACK_AND_WHITE     = 0,
        RED                 = 1,
        PURPLE              = 2,
        BLUE                = 3,
        GREEN               = 4,
        YELLOW              = 5,
        ORANGE              = 6
    }

    #endregion

    #region Singleton

    public static ProfileManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private int[] neededEXP;

    #endregion

    #region Public Properties

    public int HighestEXPColorLevel { get { return expLevelsPerColor.Max(); } }

    #endregion

    #region Private variables

    [SerializeField] private int[]   expLevelsPerColor; //TODO: Remove SerializeField and don't set from inspector, just for testing atm
    [SerializeField] private int[]   currentEXPPerColor;

    #endregion

    #region Unity Function

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    #endregion

    #region Public Functions

    public int GetNeededEXP(int currentLevel)
    {
        return neededEXP[currentLevel];
    }
    
    public int GetEXPLevel(EXPColor color)
    {
        return expLevelsPerColor[(int)color];
    }

    public int GetCurrentEXP(EXPColor color)
    {
        return currentEXPPerColor[(int)color];
    }

    #endregion
}