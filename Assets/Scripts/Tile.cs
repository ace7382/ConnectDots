using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum TileState
{
    END,
    EMPTY,
    LINE,
    CORNER,
    HEAD,
    BLANK
}

public enum EndTileRotation
{ 
    LEFT,
    RIGHT,
    TOP,
    BOTTOM,
    OPEN
}

[System.Serializable] //TODO: remove serialization. It's just for debugging
public class Tile
{
    #region Pathfinding

    public int gCost;
    public int fCost;
    public int hCost;
    public Tile cameFromTile;

    public void CalcFCost()
    {
        fCost = hCost + gCost;
    }

    #endregion

    private VisualElement                       container;

    private bool                                top, right, bottom, left;
    private Line                                line;
    private Vector2Int                          position;
    private TileState                           state;
    private EndTileRotation                     endPieceRotation;

    private int                                 multiplier                  = 1;
    private bool                                lineCancel                  = false;
    private int[]                               restrictedColors;

    #region Public Properties

    public Vector2          Position            { get { return position; } }
    public int              X                   { get { return position.x; } }
    public int              Y                   { get { return position.y; } }
    public VisualElement    Container           { get { return container; } }
    public int              Multiplier          { get { return multiplier; } }
    public bool             LineCancelled       { get { return lineCancel; } }
    public int[]            RestrictedColors    { get { return restrictedColors; } }
    public bool             HasColorRestriction { get { return !(restrictedColors == null); } }
    public EndTileRotation  EndPieceRotation    { get { return endPieceRotation; } }
    
    public Line             Line            
    { 
        get { return line; }
        set
        {
            if (value == line)
                return;

            line = value;
        }
    }

    public TileState State 
    {
        get { return state; } 

        private set
        {
            if (state == value)
                return;

            if (value == TileState.BLANK)
                container.SetColor(Color.clear);

            state = value;
        }
    }

    #endregion

    #region Constructor

    public Tile(Vector2Int pos, VisualElement visualElement, bool blank = false)
    {
        container                   = visualElement;

        State                       = blank ? TileState.BLANK : TileState.EMPTY;

        position                    = pos; 
    }

    #endregion

    #region Public Functions

    public void SetBorders_Top(bool top)        { this.top    = top; }
    public void SetBorders_Right(bool right)    { this.right  = right; }
    public void SetBorders_Bottom(bool bottom)  { this.bottom = bottom; }
    public void SetBorders_Left(bool left)      { this.left   = left; }

    public bool CanExitEndPiece(Tile tileToEnter)
    {
        switch (endPieceRotation)
        {
            case EndTileRotation.OPEN:
                {
                    if (Line.Tiles.Count == 1)
                        return true;
                    else
                        return tileToEnter.Line == Line;
                }
            case EndTileRotation.LEFT:
                return tileToEnter.X == X - 1 && tileToEnter.Y == Y;
            case EndTileRotation.RIGHT:
                return tileToEnter.X == X + 1 && tileToEnter.Y == Y;
            case EndTileRotation.TOP:
                return tileToEnter.X == X && tileToEnter.Y == Y - 1;
            case EndTileRotation.BOTTOM:
                return tileToEnter.X == X && tileToEnter.Y == Y + 1;
        }

        return false;
    }

    public bool CanEnterTile(Tile tileToEnter, int potentialColor)
    {
        if (tileToEnter.State == TileState.BLANK)
            return false;

        if (tileToEnter.restrictedColors != null)
            if (tileToEnter.restrictedColors[0] != potentialColor && tileToEnter.restrictedColors[1] != potentialColor)
                    return false;

        if (State == TileState.END) //If this tile is an end piece, check that it can be exited
        {
            if (!CanExitEndPiece(tileToEnter))
                return false;
        }

        if (tileToEnter.State == TileState.END) //If entering an end piece, check that the entrance in the correct direction
        {
            switch (tileToEnter.endPieceRotation)
            {
                case EndTileRotation.OPEN:
                    return tileToEnter.Line == Line;
                case EndTileRotation.LEFT:
                    return tileToEnter.X == X + 1 && tileToEnter.Y == Y;
                case EndTileRotation.RIGHT:
                    return tileToEnter.X == X - 1 && tileToEnter.Y == Y;
                case EndTileRotation.TOP:
                    return tileToEnter.X == X && tileToEnter.Y == Y + 1;
                case EndTileRotation.BOTTOM:
                    return tileToEnter.X == X && tileToEnter.Y == Y - 1;
            }
        }
        else if (tileToEnter.State == TileState.EMPTY)
        {
            if (tileToEnter.X == X + 1 && tileToEnter.Y == Y) //Moving into tile to the right
                return !(right || tileToEnter.left);
            else if (tileToEnter.X == X - 1 && tileToEnter.Y == Y) //left
                return !(left || tileToEnter.right);
            else if (tileToEnter.X == X && tileToEnter.Y - 1 == Y) //bottom
                return !(bottom || tileToEnter.top);
            else if (tileToEnter.X == X && tileToEnter.Y + 1 == Y)
                return !(top || tileToEnter.bottom);
        }
        else if ((tileToEnter.State == TileState.HEAD || tileToEnter.State == TileState.LINE || tileToEnter.State == TileState.CORNER) 
                && Line != null && Line.ContainsTile(tileToEnter))
        {
            int indexOfTileToEnter = Line.Tiles.FindIndex(x => x == tileToEnter);

            if (indexOfTileToEnter < 1)
                return false;

            if (Line.Tiles[indexOfTileToEnter - 1] == this)
                return true;
        }

        return false;
    }

    public void SetMultiplier(int mult)
    {
        multiplier  = mult;
        Label lab   = container.Q<Label>("Multiplier");
        lab.text    = "x" + multiplier.ToString();
        lab.Show();
    }

    public void RemoveMultiplier()
    {
        multiplier  = 1;
        container.Q<Label>("Multiplier").Hide();
    }

    public void SetLineCancel()
    {
        lineCancel = true;
        container.Q<Label>("Multiplier").Show(false); //Hide the multiplier incase the tile is marked canceled at the end of the level
        container.Q<VisualElement>("LineCancelIcon").Show();
    }

    public void RemoveLineCancel()
    {
        lineCancel = false;
        container.Q<VisualElement>("LineCancelIcon").Hide();
    }

    public void SetRestrictedColors(int colorIndex0, int colorIndex1)
    {
        restrictedColors = new int[2];

        restrictedColors[0] = colorIndex0;
        restrictedColors[1] = colorIndex1;

        Color[] pixels = UIManager.instance.RestrictedTile.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
            if (pixels[i].a == 1f)
                pixels[i] = UIManager.instance.GetColor(restrictedColors[0]);
            else
                pixels[i] = UIManager.instance.GetColor(restrictedColors[1]);

        Texture2D final = new Texture2D(UIManager.instance.RestrictedTile.width, UIManager.instance.RestrictedTile.height);
        final.SetPixels(pixels);
        final.Apply();

        Container.style.backgroundImage = final;
        Container.style.unityBackgroundImageTintColor = new Color(1f, 1f, 1f, .5f);
    }

    public void RemoveRestrictedColors()
    {
        restrictedColors = null;

        Container.style.backgroundImage = null;
    }

    public void SetAsStartEnd(Line l, EndTileRotation rotation)
    {
        Line                = l;
        endPieceRotation    = rotation;
        State               = TileState.END;

        LineManager.instance.DrawEndPoint(this);
    }

    public void SetState(Line l, Tile previous, Tile next)
    {
        //TODO: Probably don't need most of this. It was largely here
        //      to choose the correct image and rotation. Cant fully
        //      remove rn though becase a lot of the line drawing logic
        //      relies on the states

        if (state == TileState.END) //End state doesn't change
            return;
            
        Line = l;

        if (next == null)
        {
            State = TileState.HEAD;
        }
        else if (previous.X == X && next.X == X) //horizontal line
        {
            State = TileState.LINE;
        }
        else if (previous.Y == Y && next.Y == Y) //vertical line
        {
            State = TileState.LINE;
        }
        //B L corner
        else if (
            (previous.X == X && previous.Y == Y + 1 && next.X == X - 1 && next.Y == Y) ||
            (previous.X == X - 1 && previous.Y == Y && next.X == X && next.Y == Y + 1))
        {
            State = TileState.CORNER;
        }
        //B R corner
        else if ((previous.X == X && previous.Y == Y + 1 && next.X == X + 1 && next.Y == Y) ||
                (previous.X == X + 1 && previous.Y == Y && next.X == X && next.Y == Y + 1))
        {
            State = TileState.CORNER;
        }
        //T L Corner
        else if ((previous.X == X && previous.Y == Y - 1 && next.X == X - 1 && next.Y == Y) ||
            (previous.X == X - 1 && previous.Y == Y && next.X == X && next.Y == Y - 1))
        {
            State = TileState.CORNER;
        }
        //T R Corner
        else if ((previous.X == X + 1 && previous.Y == Y && next.X == X && next.Y == Y - 1) ||
            (previous.X == X && previous.Y == Y - 1 && next.X == X + 1 && next.Y == Y))
        {
            State = TileState.CORNER;
        }
    }

    public void ClearLine()
    {
        if (Line == null || State == TileState.END || State == TileState.BLANK)
            return;

        Line = null;
        State = TileState.EMPTY;
    }

    public void SetTileColorOnPuzzleComplete(Level levelCompleted)
    {
        if (line != null)
        {
            if (line.LineCancelled)
                SetLineCancel();
            else
            {
                Color bgColor = new Color(Line.Color.r, Line.Color.g, Line.Color.b, 1f);
                Container.SetColor(bgColor);
            }
        }

        if (!lineCancel)
        {
            object[] data   = new object[3];
            data[0]         = levelCompleted;
            data[1]         = line == null ? 0 : line.colorIndex;
            data[2]         = 1;

            this.PostNotification(Notifications.TILES_COLORED, data);
        }

        Container.style.backgroundImage = null; //Remove color restriction images so it's clear that it's a "white" tile
    }

    public void SpinTile(float duration)
    {
        Tween spin = DOTween.To(
                () => container.worldTransform.rotation.eulerAngles,
                x => container.transform.rotation = Quaternion.Euler(x),
                new Vector3(0f, 0f, 360f), duration)
                .SetEase(Ease.InOutBounce)
                .SetLoops(1)
                .Play();
    }

    public void ConvertFromBlankToEmpty(bool top, bool right, bool bottom, bool left)
    {
        this.State  = TileState.EMPTY;

        container.SetColor(Color.white);
        
        this.top    = top;
        this.right  = right;
        this.bottom = bottom;
        this.left   = left;

        //TODO: Rework changing blank to emptys
    }

    public void RemoveBorders(bool removeTop, bool removeRight, bool removeBottom, bool removeLeft)
    {
        if (removeTop)      top     = false;
        if (removeRight)    right   = false;
        if (removeBottom)   bottom  = false;
        if (removeLeft)     left    = false;

        //TODO: Rework changing blank to emptys
    }

    #endregion

    public override string ToString()
    {
        return string.Format("\nTile: [{0},{1}] STATE: {2} LINE: {3}\n", X, Y, State, Line == null ? "null" : Line);
    }
}
