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
    
    public int GetEXPLevel(ColorCategory color)
    {
        return expLevelsPerColor[(int)color];
    }

    public int GetCurrentEXP(ColorCategory color)
    {
        return currentEXPPerColor[(int)color];
    }

    #endregion
}