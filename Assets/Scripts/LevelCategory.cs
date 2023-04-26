using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Category", menuName = "New Category")]
public class LevelCategory : ScriptableObject
{
    #region Inspector Variables

    public Color        Color;
    public Texture2D    LevelSelectImage;
    public Texture2D    BackgroundImage;
    public string       FilePath;

    #endregion

    #region Public Functions

    public List<Level> GetLevels()
    {
        return Resources.LoadAll<Level>("Levels/" + FilePath).ToList();
    }

    #endregion
}
