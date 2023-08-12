using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Board
{
    #region Private Variables

    private List<List<Tile>>    tiles;
    private List<TileBorder>    tileBorders;
    private List<Line>          lines;
    private Level               level;
    private VisualElement       boardVE;
    private VisualElement       screen;

    private Line                draggingLine;
    private bool                canClick;

    private PowerupType         usingPowerup;

    private long                timeStart, timeEnd;

    #endregion

    #region Public Properties

    public List<List<Tile>>             Tiles           { get { return tiles; } }
    public List<Level.LineDefinitions>  LevelsLineDefs  { get { return level.Lines; }}
    public bool                         CanClick        { get { return canClick; } }
    public PowerupType                  UsingPowerup    { get { return usingPowerup; } set { usingPowerup = value; } }

    #endregion

    #region Constructor

    public Board(Level level, VisualElement root)
    {
        this.level      = level;
        draggingLine    = null;

        screen          = root.Q<VisualElement>("Screen");
        boardVE         = root.Q<VisualElement>("NewBoard");
        boardVE.Show();
    }

    #endregion

    #region Public Functions

    public void CreateBoard()
    {
        RegisterScreenLevelInteraction();

        boardVE.Clear();
        boardVE.transform.scale     = Vector3.one;

        canClick                    = false;

        float borderThickness       = 8f;
        float borderSizePercent     = .75f;

        float boardPadding          = 15f;

        float tileSize              = Mathf.Floor(
                                        (screen.worldBound.width
                                            - (2f * UIManager.instance.Board_SpaceOnEdge)
                                            - ((level.Cols - 1) * borderThickness)) 
                                        / (float)level.Cols);

        boardVE.SetWidth(new StyleLength(
                            (tileSize * level.Cols) 
                            + (borderThickness * (level.Cols - 1))
                            + (2f * boardPadding)
                        ));

        boardVE.SetPadding(boardPadding);
        boardVE.SetBorderRadius(25f);
        boardVE.SetColor(new Color(0f, 0f, 0f, .5f));

        LineManager.instance.SetLineSizes(tileSize / 4f, (tileSize / 3f) * .75f);
        float tileBorderRadius = tileSize / 5f;

        tiles                       = new List<List<Tile>>();
        tileBorders                 = new List<TileBorder>();

        for (int i = 0; i < level.Rows; i++)
            tiles.Add(new List<Tile>());

        for (int row = 0; row < level.Rows; row++)
        {
            VisualElement rowParent = UIManager.instance.RowPrefab_New.Instantiate();
            VisualElement rowVE     = rowParent.Q<VisualElement>("Row");

            boardVE.Add(rowVE);
            rowParent.RemoveFromHierarchy();

            VisualElement horizontalBorders = null;

            if (row != level.Rows - 1)
            {
                VisualElement hBoarderParent        = UIManager.instance.RowPrefab_New.Instantiate();
                horizontalBorders                   = hBoarderParent.Q<VisualElement>("Row");
                
                boardVE.Add(horizontalBorders);

                hBoarderParent.RemoveFromHierarchy();
            }

            for (int col = 0; col < level.Cols; col++)
            {
                VisualElement tileParent = UIManager.instance.TilePrefab_New.Instantiate();
                VisualElement tile = tileParent.Q<VisualElement>("Tile_New");

                tile.SetWidth(tileSize * .9f);
                tile.SetHeight(tile.style.width);
                tile.SetMargins(tileSize * .05f);
                tile.SetBorderRadius(tileBorderRadius);
                tile.SetOpacity(0f);

                rowVE.Add(tile);
                tileParent.RemoveFromHierarchy();

                Level.SpecialTileDefinitions rules = level.GetSpecialTileDef(col, row);
                Tile t = new Tile(new Vector2Int(col, row), tile, rules == null ? false : rules.blank);

                if (rules != null)
                {
                    if (rules.multiplier > 1)   t.SetMultiplier(rules.multiplier);
                    if (rules.lineCancel)       t.SetLineCancel();

                    if (rules.restrictedColor1 != 0)
                    {
                        t.SetRestrictedColors(rules.restrictedColor1, rules.restrictedColor2);
                    }
                }

                if (horizontalBorders != null)
                {
                    VisualElement hBorder           = new VisualElement();

                    float wid                       = borderSizePercent * tile.style.width.value.value;

                    hBorder.SetWidth(new StyleLength(wid));
                    hBorder.SetMargins((tileSize - wid) / 2f, false, true, false, true);
                    hBorder.SetHeight(borderThickness);
                    hBorder.SetBorderRadius(borderThickness / 2f);
                    hBorder.SetColor(Color.white);
                    hBorder.pickingMode             = PickingMode.Ignore;

                    horizontalBorders.Add(hBorder);

                    TileBorder b                    = new TileBorder(hBorder, false, t);
                    hBorder.userData                = b;
                    tileBorders.Add(b);
                }
                
                if (col != level.Cols - 1)
                {
                    VisualElement vBorder           = new VisualElement();

                    vBorder.SetWidth(borderThickness);
                    vBorder.SetHeight(new StyleLength(borderSizePercent * tile.style.width.value.value));
                    vBorder.SetBorderRadius(borderThickness / 2f);
                    vBorder.SetColor(Color.white);
                    vBorder.pickingMode             = PickingMode.Ignore;

                    rowVE.Add(vBorder);

                    TileBorder b                    = new TileBorder(vBorder, true, t);
                    vBorder.userData                = b;
                    tileBorders.Add(b);

                    if (horizontalBorders != null)
                    {
                        VisualElement spacer        = new VisualElement();

                        spacer.SetWidth(borderThickness);
                        spacer.SetHeight(spacer.style.width);
                        spacer.pickingMode          = PickingMode.Ignore;

                        horizontalBorders.Add(spacer);
                    }
                }

                tile.name       = "Tile (" + t.X.ToString() + ", " + t.Y.ToString() + ")";
                tile.userData   = t;

                RegisterTileInteraction(tile);

                tiles[row].Add(t);
            }
        }

        for (int i = 0; i < tileBorders.Count; i++)
        {
            TileBorder b = tileBorders[i];

            b.SetRightDownTile(b.TilesToLeftAndRight ?
                tiles[b.LeftUpTile.Y][b.LeftUpTile.X + 1] :
                tiles[b.LeftUpTile.Y + 1][b.LeftUpTile.X]
                );

            b.SetActive(level.Borders.FindIndex(x => x.leftUpTile == b.LeftUpTile.Position && x.rightDownTile == b.RightDownTile.Position) > -1);
            b.VisualElement.SetOpacity(0f);
        }
    }

    public void SetupLines()
    {
        lines = new List<Line>();

        for (int i = 0; i < level.Lines.Count; i++)
        {
            lines.Add(new Line(level.Lines[i].colorIndex));
            lines[i].SetStartEndTiles(
                tiles[level.Lines[i].end1position.y][level.Lines[i].end1position.x],
                tiles[level.Lines[i].end2position.y][level.Lines[i].end2position.x],
                level.Lines[i].end1rotation, level.Lines[i].end2rotation);
        }

        this.AddObserver(CheckLevelDone, Notifications.LINE_COMPLETED);
    }

    public void UnregisterListeners()
    {
        LineManager.instance.Clear();

        this.RemoveObserver(CheckLevelDone, Notifications.LINE_COMPLETED);

        UnregisterScreenLevelInteraction();
        UnregisterTileInteraction();
    }

    public void CheckLevelDone(object sender, object info)
    {
        //sender    -   Line    -   the Line which was checked for completion
        //info      -   bool    -   whether the line was completed or not

        if (!(bool)info)
            return;

        for (int i = 0; i < lines.Count; i++)
            if (!lines[i].IsCompleted)
                return;

        timeEnd = DateTime.Now.Ticks;

        this.PostNotification(Notifications.BOARD_COMPLETE);
    }

    #region Animations

    public IEnumerator AnimateBoardIn()
    {
        canClick = false;

        yield return null; //This is needed because the width of the screen/board are not calculated on the same frame in which they're initialized

        CreateBoard();

        yield return null; //This is needed for the line manager to get tiles' positions

        SetupLines();

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

        for (int i = 0; i < tileBorders.Count; i++)
        {
            if (!tileBorders[i].Active)
                continue;

            VisualElement ve = tileBorders[i].VisualElement;

            Tween fadein = DOTween.To(
                            () => ve.style.opacity.value
                            , x => ve.style.opacity = new StyleFloat(x)
                            , 1f
                            , .1f);

            intweens.Add(fadein);
            fadein.Play();
            fadein.onKill += () => intweens.Remove(fadein);

            yield return w;
        }

        while (intweens.Count > 0)
            yield return null;

        timeStart = DateTime.Now.Ticks;

        canClick = true;
    }

    public IEnumerator BoardInWithoutAnimation()
    {
        CreateBoard();

        yield return null;

        SetupLines();

        for (int i = 0; i < tileBorders.Count; i++)
        {
            if (tileBorders[i].Active)
                tileBorders[i].VisualElement.SetOpacity(100f);
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                Tile t = tiles[i][j];
                t.Container.style.opacity = 1f;
            }
        }

        canClick = true;
    }

    public IEnumerator AnimateBoardOut()
    {
        canClick                = false;

        float tileDuration      = .15f;
        float waitDuration      = .01f;
        float lineDuration      = (tiles.Sum(x => x.Count) * waitDuration) + (tileDuration - waitDuration);
        List<Tween> outtweens   = new List<Tween>();
        WaitForSeconds w        = new WaitForSeconds(waitDuration);


        Tween fadeLines = DOTween.To(() => LineManager.instance.LineContainer.style.opacity.value
                                    , x => LineManager.instance.LineContainer.SetOpacity(x)
                                    , 0f
                                    , lineDuration)
                                    .SetEase(Ease.Linear)
                                    .OnComplete(() => LineManager.instance.Clear());

        fadeLines.OnKill(() => outtweens.Remove(fadeLines));
        outtweens.Add(fadeLines);
        fadeLines.Play();

        for (int i = 0; i < tileBorders.Count; i++)
        {
            VisualElement ve = tileBorders[i].VisualElement;

            Tween fadein = DOTween.To(
                            () => ve.style.opacity.value
                            , x => ve.style.opacity = new StyleFloat(x)
                            , 0f
                            , .10f)
                            .Play();
        }

        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            for (int j = tiles[i].Count - 1; j >= 0; j--)
            {
                Tile t          = tiles[i][j];
                t.Container.SetOpacity(1f);

                Tween fadeout   = DOTween.To(() => t.Container.style.opacity.value
                                    , x => t.Container.SetOpacity(x)
                                    , 0f
                                    , tileDuration);

                outtweens.Add(fadeout);

                fadeout.Play();

                fadeout.onKill  += () => outtweens.Remove(fadeout);

                yield return w;
            }
        }

        while (outtweens.Count > 0)
            yield return null;
    }

    public IEnumerator TimeBoardComplete()
    {
        LineManager.instance.Clear();

        for (int i = 0; i < tiles.Count; i++)
            for (int j = 0; j < tiles[i].Count; j++)
                tiles[i][j].SetTileColorOnPuzzleComplete(level);

        Tween shrinkBoard = DOTween.To(() => boardVE.transform.scale,
            x => boardVE.transform.scale = x,
            Vector3.zero, .3f).SetEase(Ease.InQuad);

        yield return shrinkBoard.WaitForCompletion();
    }

    public IEnumerator LevelCompleteAnimation()
    {
        canClick                = false;
        UIManager.instance
            .TopBar.CanClick    = false;

        int diags               = level.Rows + level.Cols - 1;
        float animLength        = 1.05f;
        float spinDur           = animLength / (float)(diags / 2);
        WaitForSeconds waitDur  = new WaitForSeconds((animLength / (float)diags));

        Tween boardReturn = DOTween.To(() => boardVE.transform.scale,
                                    x => boardVE.transform.scale = x
                                    , Vector3.one
                                    , animLength / 2f)
                                    .SetEase(Ease.OutBounce)
                                    .Pause();

        Tween lineReturn = DOTween.To(() => LineManager.instance.LineContainer.transform.scale,
                                    x => LineManager.instance.LineContainer.transform.scale = x
                                    , Vector3.one
                                    , animLength / 2f)
                                    .SetEase(Ease.OutBounce)
                                    .Pause();

        Tween scaleUp = DOTween.To(() => boardVE.transform.scale,
                                    x => boardVE.transform.scale = x
                                    , new Vector3(1.2f, 1.2f, 1f)
                                    , animLength / 2f)
                                    .SetEase(Ease.InOutQuart)
                                    .Pause();

        Tween lineScaleup = DOTween.To(() => LineManager.instance.LineContainer.transform.scale,
                                    x => LineManager.instance.LineContainer.transform.scale = x
                                    , new Vector3(1.2f, 1.2f, 1f)
                                    , animLength / 2f)
                                    .SetEase(Ease.InOutQuart)
                                    .Pause();

        Sequence seq            = DOTween.Sequence();
        seq.SetAutoKill(false); //without this a warning is thrown by Dotween in the yield .WaitForCompleteion *shrug*

        seq.Append(scaleUp);
        seq.Join(lineScaleup);
        seq.Append(boardReturn);
        seq.Join(lineReturn);

        //Algo from https://www.geeksforgeeks.org/zigzag-or-diagonal-traversal-of-matrix/#
        for (int i = 1; i <= diags; i++)
        {
            int startCol        = Mathf.Max(0, i - level.Rows);
            int count           = Mathf.Min(i, Mathf.Min(level.Cols - startCol), level.Rows);

            for (int j = 0; j < count; j++)
            {
                int r = Mathf.Min(level.Rows, i) - j - 1;
                tiles[r][startCol + j].SetTileColorOnPuzzleComplete(level);
                tiles[r][startCol + j].SpinTile(spinDur);
            }

            if (i != diags) yield return waitDur;
        }

        yield return seq.WaitForCompletion();

        Dictionary<ColorCategory, int> coinsAwarded = SpawnCoinsOnBoardComplete();

        yield return new WaitForSeconds(.75f);

        object[] data               = new object[3];
        data[0]                     = coinsAwarded;
        data[1]                     = level;
        data[2]                     = TimeSpan.FromTicks(timeEnd - timeStart);

        //TODO - Mark the currently played level as complete. Probably want to do that outside of the board though
        //CurrentLevel.LevelComplete();

        PageManager.instance.StartCoroutine(PageManager.instance.AddPageToStack<EndOfLevelPopup>(data));
    }

    public Dictionary<ColorCategory, int> SpawnCoinsOnBoardComplete()
    {
        Dictionary<ColorCategory, int> ret = new Dictionary<ColorCategory, int>();

        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles[i].Count; j++)
            {
                int awardedCoins = CurrencyManager.instance.AwardCoins(tiles[i][j]);

                if (awardedCoins != 0)
                {
                    ColorCategory cat = UIManager.instance.GetGameColor(
                                            tiles[i][j].Line == null ? 0
                                            : tiles[i][j].Line.ColorIndex)
                                        .category;

                    for (int am = 0; am < awardedCoins; am++)
                    {
                        CurrencyManager.instance.SpawnCoin(
                            cat
                            , tiles[i][j].Container.worldBound.center
                            , UIManager.instance.TopBar.CoinsButton.worldBound.center); //TODO: This might need to be updated on screen change?
                    }

                    if (ret.ContainsKey(cat))
                        ret[cat] += awardedCoins;
                    else
                        ret.Add(cat, awardedCoins);
                }
            }
        }

        return ret;
    }

    #endregion

    #endregion

    #region Private Functions

    #region Powerups

    private void RemoveSpecialTiles(Tile t)
    {
        //Multiplier
        //Line cancel
        //Restricted colors

        if (t.Multiplier == 1 && !t.LineCancelled && !t.HasColorRestriction)
        {
            Debug.Log("Not a special Tile");
            return;
        }

        t.RemoveMultiplier();
        t.RemoveLineCancel();
        t.RemoveRestrictedColors();

        CurrencyManager.instance.SpendCurrency(PowerupType.REMOVE_SPECIAL_TILE, 1);
    }

    private void FillEmptyTile(Tile t)
    {
        if (t.State != TileState.BLANK)
            return;

        Tile above = null;
        Tile toRight = null;
        Tile below = null;
        Tile toLeft = null;

        if (t.Y != 0)
        {
            List<Tile> row = Tiles.Find(r => r[0].Y == t.Y - 1);

            above = row.Find(abv => abv.X == t.X);
        }

        if (t.X != level.Cols - 1)
        {
            //x + 1 && y = y
            List<Tile> row = Tiles.Find(r => r[0].Y == t.Y);

            toRight = row.Find(rig => rig.X == t.X + 1);
        }

        if (t.Y != level.Rows - 1)
        {
            List<Tile> row = Tiles.Find(r => r[0].Y == t.Y + 1);

            below = row.Find(abv => abv.X == t.X);
        }

        if (t.X != 0)
        {
            List<Tile> row = Tiles.Find(r => r[0].Y == t.Y);

            toLeft = row.Find(rig => rig.X == t.X - 1);
        }

        if (above != null && above.State != TileState.BLANK)        above.RemoveBorders(false, false, true, false);
        if (toRight != null && toRight.State != TileState.BLANK)    toRight.RemoveBorders(false, false, false, true);
        if (below != null && below.State != TileState.BLANK)        below.RemoveBorders(true, false, false, false);
        if (toLeft != null && toLeft.State != TileState.BLANK)      toLeft.RemoveBorders(false, true, false, false);

        t.ConvertFromBlankToEmpty(
            !(above != null && above.State != TileState.BLANK)         //Top
            , !(toRight != null && toRight.State != TileState.BLANK)   //Right
            , !(below != null && below.State != TileState.BLANK)      //Bottom
            , !(toLeft != null && toLeft.State != TileState.BLANK)     //Left
        );

        CurrencyManager.instance.SpendCurrency(PowerupType.FILL_EMPTY, 1);
    }

    public bool DrawHintLine(Level.LineDefinitions lineToDraw)
    {
        List<Tile> tilesToDraw = new List<Tile>();

        for(int i = 0; i < lineToDraw.solutionPath.Count; i++)
        {
            Tile tileToCheck = tiles[lineToDraw.solutionPath[i].y][lineToDraw.solutionPath[i].x];

            if (tileToCheck.State == TileState.BLANK || 
                (tileToCheck.HasColorRestriction && !tileToCheck.RestrictedColors.Contains(lineToDraw.colorIndex)))
            {
                break;
            }

            tilesToDraw.Add(tileToCheck);
        }

        if (tilesToDraw.Count == lineToDraw.solutionPath.Count)
        {
            //Draw line

            Line l = tilesToDraw[0].Line;
            l.ClearTilesAndAdd(tilesToDraw[0]);

            for (int i = 1; i < tilesToDraw.Count; i++)
            {
                Tile currentTile = tilesToDraw[i];

                if (currentTile.Line != null && currentTile.Line.ColorIndex != lineToDraw.colorIndex)
                {
                    Tile first = currentTile.Line.FirstTile;
                    currentTile.Line.ClearTilesAndAdd(first);
                    first.Line.CheckCompletedLine();
                }

                l.AddTile(currentTile);
            }

            CurrencyManager.instance.SpendCurrency(PowerupType.HINT, 1);

            l.CheckCompletedLine();

            return true;
        }
        else
        {
            //Tell user to use powerups to make an available path
            return false;
        }
    }

    #endregion

    #region Interactions

    private void RegisterScreenLevelInteraction()
    {
        screen.RegisterCallback<PointerUpEvent>(ClickReleasedHandler);
        screen.RegisterCallback<PointerLeaveEvent>(ClickReleasedHandler);
    }

    private void RegisterTileInteraction(VisualElement tile)
    {
        tile.RegisterCallback<PointerDownEvent>(TileClickedHandler);
        tile.RegisterCallback<PointerOverEvent>(TileDraggedOverHandler);
    }

    private void UnregisterScreenLevelInteraction()
    {
        screen.UnregisterCallback<PointerUpEvent>(ClickReleasedHandler);
        screen.UnregisterCallback<PointerLeaveEvent>(ClickReleasedHandler);
    }

    private void UnregisterTileInteraction()
    {
        for (int i = 0; i < tiles.Count; i++)
            for (int j = 0; j < tiles[i].Count; j++)
            {
                tiles[i][j].Container.UnregisterCallback<PointerDownEvent>(TileClickedHandler);
                tiles[i][j].Container.UnregisterCallback<PointerOverEvent>(TileDraggedOverHandler);
            }
    }

    private void ClickReleasedHandler(PointerUpEvent evt)
    {
        if (draggingLine == null)
            return;

        draggingLine.CheckCompletedLine();

        draggingLine = null;
    }

    private void ClickReleasedHandler(PointerLeaveEvent evt)
    {
        ClickReleasedHandler(new PointerUpEvent());
    }

    private void TileClickedHandler(PointerDownEvent evt)
    {
        if (!canClick)
            return;

        VisualElement ve = (evt.target as VisualElement);
        Tile tile = ve.userData as Tile;

        Debug.Log(ve.name + " clicked");

        if (usingPowerup == PowerupType.REMOVE_SPECIAL_TILE)
        {
            RemoveSpecialTiles(tile);
            return;
        }
        else if (usingPowerup == PowerupType.FILL_EMPTY)
        {
            FillEmptyTile(tile);
            return;
        }

        if (draggingLine == null)
        {
            //Start line
            if (tile.State == TileState.END)
            {
                draggingLine = tile.Line;
                draggingLine.ClearTilesAndAdd(tile);
            }
            else if (tile.State == TileState.LINE || tile.State == TileState.CORNER)
            {
                draggingLine = tile.Line;
                draggingLine.RemoveAfterTile(tile);
            }
            else if (tile.State == TileState.HEAD)
            {
                draggingLine = tile.Line;
            }
        }
    }

    private void TileDraggedOverHandler(PointerOverEvent evt)
    {
        if (draggingLine == null || !canClick)
            return;

        VisualElement ve = evt.target as VisualElement;
        Tile tile = ve.userData as Tile;

        if (tile.Line == draggingLine)
        {
            if (tile.State == TileState.LINE || tile.State == TileState.CORNER)         //Drag over the same line's pieces
                draggingLine.RemoveAfterTile(tile);
            else if (tile.State == TileState.END && draggingLine.ContainsTile(tile))    //Drag over the start point
                draggingLine.ClearTilesAndAdd(tile);
            else                                                                        //Drag over the other end point\
            {
                List<Tile> tempList = new List<Tile>(draggingLine.Tiles);
                List<Tile> path = FindPath(draggingLine.FirstTile, tile);

                if (path != null)
                {
                    draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

                    for (int i = 1; i < path.Count; i++) //First tile will always be the same?
                    {
                        draggingLine.AddTile(path[i]);
                    }
                }
                else
                {
                    draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

                    for (int i = 1; i < tempList.Count; i++)
                    {
                        draggingLine.AddTile(tempList[i]);
                    }
                }
            }
        }
        else if (tile.Line == null) //This will exclude other lines and their end points
        {
            if (draggingLine.LineHead.State == TileState.END && draggingLine.ContainsTwoEndTiles()) //Check to see if you're dragging FROM a completed line
                draggingLine.RemoveAfterTile(draggingLine.PreLineHead);

            List<Tile> tempList = new List<Tile>(draggingLine.Tiles);
            List<Tile> path = FindPath(draggingLine.FirstTile, tile);

            if (path != null)
            {
                draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

                for (int i = 1; i < path.Count; i++) //First tile will always be the same?
                {
                    draggingLine.AddTile(path[i]);
                }
            }
            else
            {
                draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

                for (int i = 1; i < tempList.Count; i++)
                {
                    draggingLine.AddTile(tempList[i]);
                }
            }
        }
    }

    #endregion

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

        while (currentTile.cameFromTile != null)
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
                    tiles[i][j].gCost = 0 + tiles[i][j].State == TileState.END ? 10 : 0;
                else
                    tiles[i][j].gCost = 50000;//int.MaxValue;

                tiles[i][j].CalcFCost();
                tiles[i][j].cameFromTile = null;
            }
        }

        startTile.gCost = 0;
        startTile.hCost = CalculateDistance(startTile, endTile);
        startTile.CalcFCost();

        int lineColorIndex = startTile.Line.ColorIndex;

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

                temp[i].cameFromTile = currentTile;
                temp[i].hCost = CalculateDistance(temp[i], endTile);
                temp[i].CalcFCost();

                if (!openList.Contains(temp[i]))
                    openList.Add(temp[i]);
            }
        }

        return null;
    }

    #endregion

    #endregion
}
