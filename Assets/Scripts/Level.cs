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
        public Color            color;
    }

    [System.Serializable]
    public class SpecialTileDefinitions
    {
        public Vector2Int       tilePosition;
        public bool             blank;
        public bool             topBorder;
        public bool             rightBorder;
        public bool             bottomBorder;
        public bool             leftBorder;
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private LevelCategory levelCategory;
    [SerializeField] private string levelNum;
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    [SerializeField] private List<LineDefinitions> lines;
    [SerializeField] private List<SpecialTileDefinitions> specialTiles;

    #endregion

    #region Public Properties

    public LevelCategory                LevelCategory       { get { return levelCategory; } }
    public string                       LevelNumber         { get { return levelNum; } }
    public int                          Rows                { get { return rows; } }
    public int                          Cols                { get { return cols; } }
    public List<LineDefinitions>        Lines               { get { return lines;} }
    public List<SpecialTileDefinitions> SpecialTiles        { get { return specialTiles; } }

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

    #endregion
}
