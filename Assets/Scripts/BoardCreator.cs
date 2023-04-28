using DG.Tweening;
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

    [SerializeField] private float boardBorder = 10f;

    #endregion

    #region Private Variables

    private VisualElement screen;
    private VisualElement board;
    [SerializeField] private List<Line> lines; //TODO: remove serialization. It's jsut for debugging
    private List<List<Tile>> tiles;

    #endregion

    #region Public Properties

    public float HardBorderSize { get { return hardBorderSize; } }
    public float SoftBorderSize { get { return softBorderSize; } }
    public Color HardBorderColor { get { return hardBorderColor; } }
    public Color SoftBorderColor { get { return softBorderColor; } }
    public Level CurrentLevel { get { return level; } }

    #endregion

    #region Unity Functions

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DOTween.SetTweensCapacity(1000, 100);
    }

    #endregion

    #region Public Functions

    public void Setup(UIDocument uiDoc, Level level)
    {
        this.level = level;
        this.uiDoc = uiDoc;

        //CreateBoard();
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
        }

        StartCoroutine(LevelCompleteAnimation());

        return true;
    }

    #endregion

    #region Private Functions

    private void CreateBoard()
    {
        screen = uiDoc.rootVisualElement.Q<VisualElement>("Screen");
        board = screen.Q<VisualElement>("Board");

        float tileSize              = Mathf.Floor((screen.worldBound.width - (2f * boardBorder)) / (float)level.Cols);

        board.SetWidth(new StyleLength(tileSize * level.Cols));

        InputController.instance.RegisterBoardInteractionCallbacks(screen);

        tiles = new List<List<Tile>>();

        for (int i = 0; i < level.Rows; i++)
            tiles.Add(new List<Tile>());

        List<Vector2Int> startEndPoints = new List<Vector2Int>();

        StyleFloat opacity0 = new StyleFloat(0f);

        for (int row = 0; row < level.Rows; row++)
        {
            for (int col = 0; col < level.Cols; col++)
            {
                VisualElement pref  = BoardCreator.instance.tilePrefab.Instantiate();
                VisualElement tile  = pref.Q<VisualElement>("Tile");

                tile.RemoveFromClassList("Tile");
                tile.style.backgroundColor = new StyleColor(Color.white);
                tile.SetWidth(new StyleLength(tileSize));
                tile.SetHeight(tile.style.width);

                Level.SpecialTileDefinitions rules = level.GetSpecialTileDef(col, row);
                Tile t;

                if (rules != null)
                {
                    t               = new Tile(new Vector2Int(col, row), tile, rules.blank, rules.topBorder, rules.rightBorder, rules.bottomBorder, rules.leftBorder);

                    if (rules.multiplier > 1)
                        t.SetMultiplier(rules.multiplier);
                    else if (rules.lineCancel)
                        t.SetLineCancel();

                    if(rules.restrictedColor1 != 0)
                    {
                        t.SetRestrictedColors(rules.restrictedColor1, rules.restrictedColor2);
                    }
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

                tile.style.opacity = opacity0;
            }
        }

        //TODO: maybe reevaluate borders. Can't round the corners with current process
        //tiles[0][0].Container.style.SetBorderRadius(15f, true, false, false, false);
        //tiles[0][level.Rows - 1].Container.style.SetBorderRadius(15f, false, false, true, false);
        //tiles[level.Cols - 1][0].Container.style.SetBorderRadius(15f, false, true, false, false);
        //tiles[level.Cols - 1][level.Rows - 1].Container.style.SetBorderRadius(15f, false, false, false);

        lines = new List<Line>();

        for (int i = 0; i < level.Lines.Count; i++)
        {
            lines.Add(new Line(level.Lines[i].colorIndex));
            lines[i].SetStartEndTiles(
                tiles[level.Lines[i].end1position.y][level.Lines[i].end1position.x],
                tiles[level.Lines[i].end2position.y][level.Lines[i].end2position.x],
                level.Lines[i].end1rotation, level.Lines[i].end2rotation);
        }
    }

    #endregion

    public IEnumerator LevelCompleteAnimation()
    {
        UIManager.instance.TopBar.CanClick = false;

        int diags = level.Rows + level.Cols - 1;
        float animLength = 1.05f;
        float spinDur = animLength / (float)(diags / 2);
        WaitForSeconds waitDur = new WaitForSeconds((animLength / (float)diags));

        Tween scaleUp = DOTween.To(() => board.transform.scale,
            x => board.transform.scale = x, new Vector3(1.2f, 1.2f, 1f), animLength/2f)
            .SetEase(Ease.InOutQuart).SetAutoKill(false);

        //Algo from https://www.geeksforgeeks.org/zigzag-or-diagonal-traversal-of-matrix/#
        for (int i = 1; i <= diags; i++)
        {
            int startCol = Mathf.Max(0, i - level.Rows);
            int count = Mathf.Min(i, Mathf.Min(level.Cols - startCol), level.Rows);

            for (int j = 0; j < count; j++)
            {
                tiles[Mathf.Min(level.Rows, i) - j - 1][startCol + j].PuzzleComplete(spinDur);
            }

            if (i != diags) yield return waitDur;
        }

        yield return scaleUp.Play().WaitForCompletion();

        Tween s = DOTween.To(() => board.transform.scale,
                    x => board.transform.scale = x, new Vector3(1f, 1f, 1f), animLength/2f)
                    .SetEase(Ease.OutBounce);

        s.Play();

        yield return new WaitForSeconds(animLength / 2f * .55f);

        float widthbound = board.worldBound.width / 2f * .7f;
        float heightbound = board.worldBound.height / 2f * .7f;
        Vector2 destination = new Vector2(screen.worldBound.width / 2f, screen.worldBound.height / -2f);

        Dictionary<int, int> coinsWon = new Dictionary<int, int>();
        Dictionary<int, int> notifications = new Dictionary<int, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Tile t = tiles[i][j];

                if (t.State == TileState.BLANK || t.LineCancelled)
                    continue;

                if (t.Line != null)
                {
                    if (notifications.ContainsKey(t.Line.colorIndex))
                        notifications[t.Line.colorIndex]++;
                    else
                        notifications.Add(t.Line.colorIndex, 1);
                }
                else
                {
                    if (notifications.ContainsKey(0))
                        notifications[0]++;
                    else
                        notifications.Add(0, 1);
                }

                for (int mult = 1; mult <= t.Multiplier; mult++)
                {
                    VisualElement coin = new VisualElement();
                    coin.style.SetWidth(25f);
                    coin.style.SetHeight(25f);

                    float animationTime = Random.Range(.75f, 1f);

                    int colorIndex = 0;

                    if (t.Line != null)
                        colorIndex = t.Line.colorIndex;

                    if (coinsWon.ContainsKey(colorIndex))
                        coinsWon[colorIndex]++;
                    else
                        coinsWon.Add(colorIndex, 1);

                    coin.style.backgroundColor = UIManager.instance.GetColor(colorIndex);
                    coin.style.SetBorderColor(Color.black);
                    coin.style.SetBorderWidth(3f);
                    coin.style.position = Position.Absolute;

                    screen.Add(coin);

                    float delay = Random.Range(0f, animationTime * .9f);

                    coin.transform.position = new Vector3(Random.Range(-widthbound, widthbound),
                                                Random.Range(-heightbound, heightbound),
                                                coin.transform.position.z);

                    Tween goToCorner = DOTween.To(() => coin.transform.position,
                                        x => coin.transform.position = x,
                                        new Vector3(destination.x, destination.y, coin.transform.position.z),
                                        animationTime - delay)
                                        .SetDelay(delay)
                                        .SetEase(Ease.InBack)
                                        .Play();

                    Tween scaleDwn = DOTween.To(() => coin.transform.scale,
                                        x => coin.transform.scale = x,
                                        new Vector3(0f, 0f, coin.transform.scale.z),
                                        animationTime - delay)
                                        .SetDelay(delay)
                                        .SetEase(Ease.InBack)
                                        .Play();
                }
            }
        }

        foreach (KeyValuePair<int, int> not in notifications)
        {
            object[] notifData = new object[3];
            notifData[0] = BoardCreator.instance.CurrentLevel;
            notifData[1] = not.Key;
            notifData[2] = not.Value;

            this.PostNotification(Notifications.TILES_COLORED, notifData);
        }

        object[] data = new object[1];
        data[0] = coinsWon;

        CurrentLevel.LevelComplete();

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<EndOfLevelPopup>(data));
    }

    public IEnumerator AnimateBoardIn()
    {
        yield return null; //This is needed because the width of the screen/board are not calculated on the same frame in which they're initialized

        CreateBoard();

        List<Tween> intweens = new List<Tween>();

        WaitForSeconds w = new WaitForSeconds(.01f);

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Tile t = tiles[i][j];

                Tween fadein = DOTween.To(() => t.Container.style.opacity.value,
                    x => t.Container.style.opacity = new StyleFloat(x),
                    1f, .15f);

                intweens.Add(fadein);

                fadein.Play();

                fadein.onKill += () => intweens.Remove(fadein);

                yield return w;
            }
        }

        while (intweens.Count > 0)
            yield return null;

        Debug.Log("Board Animate In Complete");
    }

    public IEnumerator AnimateBoardOut()
    {
        List<Tween> outtweens = new List<Tween>();

        WaitForSeconds w = new WaitForSeconds(.01f);

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            for (int j = tiles[i].Count - 1; j >= 0; j--)
            {
                Tile t = tiles[i][j];
                t.Container.style.opacity = new StyleFloat(1f);

                Tween fadeout = DOTween.To(() => t.Container.style.opacity.value,
                    x => t.Container.style.opacity = new StyleFloat(x),
                    0f, .15f);

                outtweens.Add(fadeout);

                fadeout.Play();

                fadeout.onKill += () => outtweens.Remove(fadeout);

                yield return w;
            }
        }

        while (outtweens.Count > 0)
            yield return null;
    }


    #region Pathfinding

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

    private List<Tile> GetNeighborList(Tile currentTile, int lineColorIndex)
    {
        List<Tile> neighbors = new List<Tile>();

        if (currentTile.X - 1 >= 0)
            if (currentTile.CanEnterTile(tiles[currentTile.Y][currentTile.X - 1], lineColorIndex))
                neighbors.Add(tiles[currentTile.Y][currentTile.X - 1]);

        if (currentTile.X + 1 < level.Cols)
            if (currentTile.CanEnterTile(tiles[currentTile.Y][currentTile.X + 1], lineColorIndex))
                neighbors.Add(tiles[currentTile.Y][currentTile.X + 1]);

        if (currentTile.Y - 1 >= 0)
            if (currentTile.CanEnterTile(tiles[currentTile.Y - 1][currentTile.X], lineColorIndex))
                neighbors.Add(tiles[currentTile.Y - 1][currentTile.X]);

        if (currentTile.Y + 1 < level.Rows)
            if (currentTile.CanEnterTile(tiles[currentTile.Y + 1][currentTile.X], lineColorIndex))
                neighbors.Add(tiles[currentTile.Y + 1][currentTile.X]);

        return neighbors;
    }

    public List<Tile> FindPath(Tile startTile, Tile endTile)
    {
        openList = new List<Tile>() { startTile };
        closedList = new List<Tile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                if (tiles[i][j].Line == startTile.Line)
                    tiles[i][j].gCost = 0;
                else
                    tiles[i][j].gCost = 50000;//int.MaxValue;

                tiles[i][j].CalcFCost();
                tiles[i][j].cameFromTile = null;
            }
        }

        startTile.gCost = 0;
        startTile.hCost = CalculateDistance(startTile, endTile);
        startTile.CalcFCost();

        int lineColorIndex = startTile.Line.colorIndex;

        while (openList.Count > 0)
        {
            Tile currentTile = GetLowestFCostTile(openList);

            if (currentTile == endTile)
            {
                return CalculatePath(endTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            List<Tile> temp = GetNeighborList(currentTile, lineColorIndex);

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

    #endregion
}
