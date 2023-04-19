using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    #region Singleton

    public static UIManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private VisualTreeAsset levelSelectButton;
    [SerializeField] private BackgroundScroll scrollingBG;

    #endregion

    #region Public Properties

    public VisualTreeAsset LevelSelectButton { get { return levelSelectButton; } }

    #endregion

    #region Unity Functions

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    #endregion

    #region Public Functions

    public void SetBackground(Texture2D texture, Color color)
    {
        scrollingBG.Set(texture, color);
    }

    #endregion
}
