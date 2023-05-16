using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //TODO: remove serialization. It's jsut for debugging
public class Line
{
    [SerializeField] public List<Tile>      lineTiles;
    [SerializeField] public int             colorIndex;
    [SerializeField] public bool            isCompleted;

    public List<Tile>       Tiles           { get { return lineTiles; } }
    public Tile             LineHead        { get { return GetLeadTile(); } }
    public Tile             PreLineHead     { get { return GetPreLeadTile(); } }
    public Tile             FirstTile       { get { return GetFirstTile(); } }
    public bool             LineCancelled   { get { return lineTiles.FindIndex(x => x.LineCancelled) >= 0; } }
    public Color            Color           { get { return UIManager.instance.GetColor(colorIndex); } }

    public Line(int colorIndex)
    {
        this.colorIndex     = colorIndex;
        lineTiles           = new List<Tile>();
        isCompleted         = false;
    }

    public void SetStartEndTiles(Tile start, Tile end, EndTileRotation startRot, EndTileRotation endRot)
    {
        start.SetAsStartEnd(this, startRot);
        end.SetAsStartEnd(this, endRot);
    }

    public void CheckCompletedLine()
    {
        isCompleted = ContainsTwoEndTiles();

        //TODO: Since moving to drawing lines vs images,
        //      there's been a bug where a level doesn't complete
        //      when all lines are connected. If you re-trigger the line
        //      completion, it will eventually register it as complete (usually right on the next check)
        //      Cant get it to reliably repro though.

        Debug.Log(isCompleted); 
        
        this.PostNotification(Notifications.LINE_COMPLETED, isCompleted);
    }

    public bool ContainsTwoEndTiles()
    {
        return lineTiles.FindAll(x => x.State == TileState.END).Count == 2;
    }

    public void ClearTilesAndAdd(Tile tileToAdd)
    {
        for (int i = lineTiles.Count - 1; i >= 0; i--)
        {
            lineTiles[i].ClearLine();
        }

        lineTiles.Clear();

        AddTile(tileToAdd);
    }

    public bool IsAdjacentToHead(Tile checkTile)
    {
        Tile head = LineHead;

        if (head.X == checkTile.X && (head.Y - 1 == checkTile.Y || head.Y + 1 == checkTile.Y))
            return true;

        if (head.Y == checkTile.Y && (head.X - 1 == checkTile.X || head.X + 1 == checkTile.X))
            return true;

        return false;
    }

    public void AddTile(Tile tileToAdd)
    {
        lineTiles.Add(tileToAdd);

        int back2       = lineTiles.Count - 3;
        int previous    = lineTiles.Count - 2;

        if (lineTiles.Count < 2)
            return;

        tileToAdd.SetState(this, lineTiles[previous], null);
        lineTiles[previous].SetState(this, lineTiles.Count > 2 ? lineTiles[back2] : null, tileToAdd);

        LineManager.instance.UpdateLine(this, lineTiles);
    }

    public bool ContainsTile(Tile t)
    {
        return lineTiles.Contains(t);
    }
    
    private Tile GetLeadTile()
    {
        return lineTiles[lineTiles.Count - 1];
    }

    private Tile GetPreLeadTile()
    {
        if (lineTiles.Count < 2)
        {
            Debug.LogWarning("Trying to get the PreLead tile on a line with only 1 tile");
            return GetLeadTile();
        }

        return lineTiles[lineTiles.Count - 2];
    }

    private Tile GetFirstTile()
    {
        return lineTiles[0];
    }

    public void RemoveAfterTile(Tile removeAfterThisTile)
    {
        int indexOfTile = lineTiles.IndexOf(removeAfterThisTile);
        
        for (int i = lineTiles.Count - 1; i > indexOfTile; i--)
        {
            lineTiles[i].ClearLine();
            lineTiles.RemoveAt(i);
        }

        //There will always be 2 tiles if this function is called
        lineTiles[indexOfTile].SetState(this, lineTiles[indexOfTile - 1], null);

        LineManager.instance.UpdateLine(this, lineTiles);
    }

    public void RemoveHeadTile()
    {
        LineHead.ClearLine();
        lineTiles.Remove(LineHead);
    }
}
