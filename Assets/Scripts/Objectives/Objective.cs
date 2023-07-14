using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Objective : ScriptableObject
{
    #region Inspector Variables

    [SerializeField] protected string           id;
    [SerializeField] protected string           description;
    [SerializeField] protected LevelCategory    levelCategory;
    [SerializeField] protected Level            level;
    [SerializeField] protected int              progressBarColorIndex;
    [SerializeField] protected Texture2D        icon;
    [SerializeField] protected bool             isAchievement;

    #endregion

    #region Private Variables

    [SerializeField] private bool isComplete = false; //TODO: Remove serialization; just for debug

    #endregion

    #region Public Properties

    public bool IsComplete { 
        get { return isComplete; } 
        protected set
        {
            if (value)
                OnComplete();

            isComplete = value;
        }
    }

    public string           ID                  { get { return id; } }
    public Level            Level               { get { return level; } }
    public LevelCategory    LevelCategory       { get { return levelCategory; } }
    public string           Description         { get { return description; } }
    public Texture2D        Icon                { get { return icon; } }
    public Color            ProgressBarColor    { get { return UIManager.instance.GetColor(progressBarColorIndex); } }
    public bool             IsAchievement       { get { return isAchievement; } }

    #endregion

    #region Abstract Functions

    public abstract string GetProgressAsString();
    public abstract float GetProgressAsPercentage();
    public abstract void Reset();

    #endregion

    #region Public Functions

    public void OnComplete()
    {
        ObjectiveManager.instance.MarkAsComplete(this);

        Debug.Log("Object Complete " + description);
    }

    #endregion
}
