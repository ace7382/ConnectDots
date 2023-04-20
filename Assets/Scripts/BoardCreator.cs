using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardCreator : MonoBehaviour
{
    #region Singleton

    public static BoardCreator instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private VisualTreeAsset tilePrefab;
    [SerializeField] private Level level;

    [SerializeField] private Sprite dotSprite;
    [SerializeField] private Sprite lineSprite;
    [SerializeField] private Sprite cornerSprite;

    [SerializeField] private float hardBorderSize;
    [SerializeField] private Color hardBorderColor;

    [SerializeField] private float softBorderSize;
    [SerializeField] private Color softBorderColor;

    #endregion

    #region Private Variables

    private VisualElement screen;
    [SerializeField] private List<Line> lines; //TODO: remove serialization. It's jsut for debugging
    private List<List<Tile>> tiles;

    #endregion

    #region Public Properties

    public float HardBorderSize { get { return hardBorderSize; } }
    public float SoftBorderSize { get { return softBorderSize; } }
    public Color HardBorderColor { get { return hardBorderColor; } }
    public Color SoftBorderColor { get { return softBorderColor; } }

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

    public void Setup(UIDocument uiDoc, Level level)
    {
        this.level = level;
        this.uiDoc = uiDoc;

        CreateBoard();
    }

    public Sprite GetTileStateTexture(TileState state)
    {
        if (state == TileState.END)
        {
            return dotSprite;
        }
        else if (state == TileState.LINE)
        {
            return lineSprite;
        }
        else if (state == TileState.CORNER)
        {
            return cornerSprite;
        }
        else if (state == TileState.HEAD)
        {
            return dotSprite;
        }

        return null;
    }

    public bool CheckLevelDone()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].isCompleted)
                return false;
        }

        InputController.instance.UnregisterBoardInteractionCallbacks(screen);

        for (int i = 0; i < lines.Count; i++)
        {
            for (int t = 0; t < lines[i].lineTiles.Count; t++)
            {
                InputController.instance.UnregisterTileInteractionCallbacks(lines[i].lineTiles[t].Container);
            }

            StartCoroutine(HighlightLine(lines[i]));
        }

        return true;
    }

    #endregion

    #region Private Functions

    private void CreateBoard()
    {
        screen                      = uiDoc.rootVisualElement.Q<VisualElement>("Screen");
        VisualElement board         = screen.Q<VisualElement>("Board");

        board.style.width           = 100 * level.Cols;
        board.style.maxWidth        = board.style.width;
        board.style.minWidth        = board.style.width;

        InputController.instance.RegisterBoardInteractionCallbacks(screen);

        tiles = new List<List<Tile>>();

        for (int i = 0; i < level.Rows; i++)
            tiles.Add(new List<Tile>());

        List<Vector2Int> startEndPoints = new List<Vector2Int>();

        for (int row = 0; row < level.Rows; row++)
        {
            for (int col = 0; col < level.Cols; col++)
            {
                VisualElement pref  = BoardCreator.instance.tilePrefab.Instantiate();
                VisualElement tile  = pref.Q<VisualElement>("Tile");

                Level.SpecialTileDefinitions rules = level.GetSpecialTileDef(col, row);
                Tile t;

                if (rules != null)
                {
                    t               = new Tile(new Vector2Int(col, row), tile, rules.blank, rules.topBorder, rules.rightBorder, rules.bottomBorder, rules.leftBorder);
                }
                else
                {
                    t               = new Tile(new Vector2Int(col, row), tile, false, row == 0, col == level.Cols - 1
                                        , row == level.Rows - 1, col == 0);
                }

                tile.name           = "Tile (" + t.X.ToString() + ", " + t.Y.ToString() + ")";
                tile.AddToClassList("Tile");
                tile.userData       = t;

                board.Add(tile);

                InputController.instance.RegisterTileInteractionCallbacks(tile);

                tiles[row].Add(t);

                startEndPoints.Add(new Vector2Int(col, row));
            }
        }

        lines = new List<Line>();

        for (int i = 0; i < level.Lines.Count; i++)
        {
            lines.Add(new Line(level.Lines[i].color));
            lines[i].SetStartEndTiles(
                tiles[level.Lines[i].end1position.y][level.Lines[i].end1position.x],
                tiles[level.Lines[i].end2position.y][level.Lines[i].end2position.x],
                level.Lines[i].end1rotation, level.Lines[i].end2rotation);
        }
    }

    #endregion

    public IEnumerator HighlightLine(Line l)
    {
        Color bgColor = new Color(l.color.r, l.color.g, l.color.b, l.color.a / 2f);
        WaitForSeconds w = new WaitForSeconds(.07f);

        for (int i = 0; i < l.lineTiles.Count; i++)
        {
            l.lineTiles[i].PuzzleComplete();

            yield return w;
        }
    }

    private List<Tile> openList;
    private List<Tile> closedList;

    private int CalculateDistance(Tile a, Tile b)
    {
        int xDis = Mathf.Abs(a.X - b.X);
        int yDis = Mathf.Abs(a.Y - b.Y);

        return xDis + yDis;
    }

    private Tile GetLowestFCostTile(List<Tile> tileList)
    {
        Tile lowestFCostTile = tileList[0];

        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i].fCost < lowestFCostTile.fCost)
                lowestFCostTile = tileList[i];
        }

        return lowestFCostTile;
    }

    private List<Tile> CalculatePath(Tile endTile)
    {
        List<Tile> path = new List<Tile>();

        path.Add(endTile);

        Tile currentTile = endTile;

        while(currentTile.cameFromTile != null)
        {
            path.Add(currentTile.cameFromTile);
            currentTile = currentTile.cameFromTile;
        }

        path.Reverse();

        return path;
    }

    private List<Tile> GetNeighborList(Tile currentTile)
    {
        List<Tile> neighbors = new List<Tile>();

        if (currentTile.X - 1 >= 0)
            if (currentTile.CanEnterTile(tiles[currentTile.Y][currentTile.X - 1]))
                neighbors.Add(tiles[currentTile.Y][currentTile.X - 1]);

        if (currentTile.X + 1 < level.Cols)
            if (currentTile.CanEnterTile(tiles[currentTile.Y][currentTile.X + 1]))
                neighbors.Add(tiles[currentTile.Y][currentTile.X + 1]);

        if (currentTile.Y - 1 >= 0)
            if (currentTile.CanEnterTile(tiles[currentTile.Y - 1][currentTile.X]))
                neighbors.Add(tiles[currentTile.Y - 1][currentTile.X]);

        if (currentTile.Y + 1 < level.Rows)
            if (currentTile.CanEnterTile(tiles[currentTile.Y + 1][currentTile.X]))
                neighbors.Add(tiles[currentTile.Y + 1][currentTile.X]);

        string deb = "Tile " + currentTile.Position + "'s neighbors: ";
        for (int i = 0; i < neighbors.Count; i++)
        {
            deb += "\n" + "Neighbor " + i.ToString() + ": " + neighbors[i].Position;
        }
        Debug.Log(deb);

        return neighbors;
    }

    public List<Tile> FindPath(Tile startTile, Tile endTile)
    {
        Debug.Log("Finding Path From: " + startTile.Position + " to: " + endTile.Position);

        openList = new List<Tile>() { startTile };
        closedList = new List<Tile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                if (tiles[i][j].Line == startTile.Line)
                    tiles[i][j].gCost = 0;
                else
                    tiles[i][j].gCost = 5000;//int.MaxValue;

                tiles[i][j].CalcFCost();
                tiles[i][j].cameFromTile = null;
            }
        }

        startTile.gCost = 0;
        startTile.hCost = CalculateDistance(startTile, endTile);
        startTile.CalcFCost();

        while (openList.Count > 0)
        {
            Tile currentTile = GetLowestFCostTile(openList);

            if (currentTile == endTile)
            {
                return CalculatePath(endTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            List<Tile> temp = GetNeighborList(currentTile);

            for (int i = 0; i < temp.Count; i++)
            {
                if (closedList.Contains(temp[i]))
                    continue;

                //int tempGCost = currentTile.gCost + CalculateDistance(currentTile, temp[i]);

                //if (tempGCost < temp[i].gCost)
                //{
                    temp[i].cameFromTile = currentTile;
                    //temp[i].gCost = tempGCost;
                    temp[i].hCost = CalculateDistance(temp[i], endTile);
                    temp[i].CalcFCost();

                    if (!openList.Contains(temp[i]))
                        openList.Add(temp[i]);
                //}
            }
        }

        return null;
    }
}
