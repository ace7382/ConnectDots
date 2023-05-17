using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Level", menuName = "New Level")]
public class Level : ScriptableObject
{
    #region Classes

    [System.Serializable]
    public class LineDefinitions
    {
        public Vector2Int       end1position;
        public EndTileRotation  end1rotation;
        public Vector2Int       end2position;
        public EndTileRotation  end2rotation;
        public int              colorIndex;

        public List<Vector2Int> solutionPath;
    }

    [System.Serializable]
    public class SpecialTileDefinitions
    {
        public Vector2Int       tilePosition;
        public bool             blank;
        public int              multiplier;
        public bool             lineCancel;
        public int              restrictedColor1 = 0;
        public int              restrictedColor2 = 0;
    }

    [System.Serializable]
    public class BorderDefinition
    {
        public Vector2Int       leftUpTile;
        public Vector2Int       rightDownTile;
    }
    #endregion

    #region Inspector Variables

    [SerializeField] private LevelCategory levelCategory;
    [SerializeField] private string levelNum;
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    [SerializeField] private List<LineDefinitions> lines;
    [SerializeField] private List<SpecialTileDefinitions> specialTiles;
    [SerializeField] private List<BorderDefinition> borders;

    [SerializeField] private bool isComplete;

    #endregion

    #region Public Properties

    public LevelCategory                LevelCategory       { get { return levelCategory; } }
    public string                       LevelNumber         { get { return levelNum; } }
    public int                          Rows                { get { return rows; } }
    public int                          Cols                { get { return cols; } }
    public List<LineDefinitions>        Lines               { get { return lines;} }
    public List<SpecialTileDefinitions> SpecialTiles        { get { return specialTiles; } }
    public bool                         IsComplete          { get { return isComplete; } private set { isComplete = value; } }
    public List<BorderDefinition>       Borders             { get { return borders; } }

    #endregion

    #region Public Functions

    public SpecialTileDefinitions GetSpecialTileDef(int x, int y)
    {
        if (SpecialTiles == null || SpecialTiles.Count == 0)
            return null;

        int index = SpecialTiles.FindIndex(tile => tile.tilePosition.x == x && tile.tilePosition.y == y);

        if (index == -1)
            return null;
        else
            return SpecialTiles[index];
    }

    public void LevelComplete()
    {
        IsComplete = true;

        this.PostNotification(Notifications.LEVEL_COMPLETE);
    }

    public void ResetLevel()
    {
        IsComplete = false;
    }

    #endregion
}
