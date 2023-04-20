using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //TODO: remove serialization. It's jsut for debugging
public class Line
{
    [SerializeField] public List<Tile>   lineTiles;
    [SerializeField] public Color        color;
    [SerializeField] public bool         isCompleted;

    public List<Tile> Tiles { get { return lineTiles; } }
    public Tile LineHead { get { return GetLeadTile(); } }
    public Tile PreLineHead { get { return GetPreLeadTile(); } }
    public Tile FirstTile { get { return GetFirstTile(); } }

    public Line(Color color)
    {
        this.color = color;
        lineTiles = new List<Tile>();
        isCompleted = false;
    }

    public void SetStartEndTiles(Tile start, Tile end, EndTileRotation startRot, EndTileRotation endRot)
    {
        start.SetAsStartEnd(this, startRot);
        end.SetAsStartEnd(this, endRot);
    }

    public void CheckCompletedLine()
    {
        isCompleted = ContainsTwoEndTiles();

        if (isCompleted)
            Debug.Log("Are all lines complete? " + BoardCreator.instance.CheckLevelDone());
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

        if (lineTiles.Count == 2)
        {
            tileToAdd.SetState(this, lineTiles[previous], null);
        }
        else if (lineTiles.Count > 2)
        {
            tileToAdd.SetState(this, lineTiles[previous], null);

            lineTiles[previous].SetState(this, lineTiles[back2], tileToAdd);
        }
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
    }

    public void RemoveHeadTile()
    {
        LineHead.ClearLine();
        lineTiles.Remove(LineHead);
    }
}
