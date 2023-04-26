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
    BOTTOM
}

[System.Serializable] //TODO: remove serialization. It's jsut for debugging
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
    private VisualElement                       image;
    private VisualElement                       leftBorderVE;
    private VisualElement                       rightBorderVE;
    private VisualElement                       topBorderVE;
    private VisualElement                       bottomBorderVE;
    private bool                                top, right, bottom, left;
    private Line                                line;
    [SerializeField] private Vector2Int         position;
    [SerializeField] private TileState          state;
    [SerializeField] private EndTileRotation    endPieceRotation;

    private int multiplier = 1;
    private bool lineCancel = false;
    private int[] restrictedColors;

    #region Public Properties

    public Vector2          Position            { get { return position; } }
    public int              X                   { get { return position.x; } }
    public int              Y                   { get { return position.y; } }
    public VisualElement    Container           { get { return container; } }
    public VisualElement    Image               { get { return image; } }
    public int              Multiplier          { get { return multiplier; } }
    public bool             LineCancelled       { get { return lineCancel; } }
    
    public Line             Line            
    { 
        get { return line; }
        set
        {
            if (value == line)
                return;

            line = value;

            SetColor(line == null ? UIManager.instance.GetColor(0) : line.Color); ;
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
            {
                StyleColor s                    = new StyleColor(Color.clear); 
                image.style.backgroundColor     = s;
                container.style.backgroundColor = s;
            }
            else
                image.style.backgroundImage = new StyleBackground(BoardCreator.instance.GetTileStateTexture(value));

            state = value;
        }
    }

    #endregion

    #region Constructor

    public Tile(Vector2Int pos, VisualElement visualElement, bool blank = false,
                bool top = false, bool right = false, bool bottom = false, bool left = false)
    {
        container                   = visualElement;

        image                       = visualElement.Q<VisualElement>("Image");
        leftBorderVE                = visualElement.Q<VisualElement>("Left");
        rightBorderVE               = visualElement.Q<VisualElement>("Right");
        topBorderVE                 = visualElement.Q<VisualElement>("Top");
        bottomBorderVE              = visualElement.Q<VisualElement>("Bottom");

        image.pickingMode           = PickingMode.Ignore;
        leftBorderVE.pickingMode    = PickingMode.Ignore;
        rightBorderVE.pickingMode   = PickingMode.Ignore;
        topBorderVE.pickingMode     = PickingMode.Ignore;
        bottomBorderVE.pickingMode  = PickingMode.Ignore;

        if(!blank)
        { 
            this.top                = top;
            this.right              = right;
            this.bottom             = bottom;
            this.left               = left;

            topBorderVE.style.backgroundColor = top ? BoardCreator.instance.HardBorderColor : BoardCreator.instance.SoftBorderColor;
            topBorderVE.style.SetHeight(new StyleLength(top ? BoardCreator.instance.HardBorderSize : BoardCreator.instance.SoftBorderSize));
            rightBorderVE.style.backgroundColor = right ? BoardCreator.instance.HardBorderColor : BoardCreator.instance.SoftBorderColor;
            rightBorderVE.style.SetWidth(new StyleLength(right ? BoardCreator.instance.HardBorderSize : BoardCreator.instance.SoftBorderSize));
            bottomBorderVE.style.backgroundColor = bottom ? BoardCreator.instance.HardBorderColor : BoardCreator.instance.SoftBorderColor;
            bottomBorderVE.style.SetHeight(new StyleLength(bottom ? BoardCreator.instance.HardBorderSize : BoardCreator.instance.SoftBorderSize));
            leftBorderVE.style.backgroundColor = left ? BoardCreator.instance.HardBorderColor : BoardCreator.instance.SoftBorderColor;
            leftBorderVE.style.SetWidth(new StyleLength(left ? BoardCreator.instance.HardBorderSize : BoardCreator.instance.SoftBorderSize));

            if (top)                container.Add(topBorderVE);
            if (right)              container.Add(rightBorderVE);
            if (bottom)             container.Add(bottomBorderVE);
            if (left)               container.Add(leftBorderVE);

            State                   = TileState.EMPTY;
        }
        else
        {
            State = TileState.BLANK;
        }

        position                    = pos; 
    }

    #endregion

    #region Public Functions

    public bool CanExitEndPiece(Tile tileToEnter)
    {
        switch (endPieceRotation)
        {
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
        //Debug.Log("Can " + this + " enter " + tileToEnter);

        //Debug.Log(string.Format("{0} >>> {1}",
        //    "[" + this.X + ", " + this.Y + "]",
        //    "[" + tileToEnter.X + ", " + tileToEnter.Y + "]"));

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
        multiplier = mult;

        Label lab = container.Q<Label>("Multiplier");
        lab.text = "x" + multiplier.ToString();
        lab.style.Show();
    }

    public void SetLineCancel()
    {
        lineCancel = true;
        container.Q<Label>("Multiplier").Show(false); //Hide the multiplier incase the tile is marked canceled at the end of the level

        for (int i = 1; i <= 4; i++)
        {
            Label lab = container.Q<Label>("LineCancel" + i.ToString());
            lab.Show();
        }
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

    public void SetAsStartEnd(Line l, EndTileRotation rotation)
    {
        Line                = l;
        State               = TileState.END;
        endPieceRotation    = rotation;

        if (rotation == EndTileRotation.LEFT)
        {
            Image.style.rotate = new StyleRotate(new Rotate(270f));
        }
        else if (rotation == EndTileRotation.RIGHT)
        {
            Image.style.rotate = new StyleRotate(new Rotate(90f));
        }
        else if (rotation == EndTileRotation.TOP)
        {
            Image.style.rotate = new StyleRotate(new Rotate(0f));
        }
        else
        {
            Image.style.rotate = new StyleRotate(new Rotate(180f));
        }
    }

    public void SetState(Line l, Tile previous, Tile next)
    {
        if (state == TileState.END) //Start and end pts should never change state
            return;

        Line = l;

        if (next == null)
        {
            State = TileState.HEAD;

            if (previous == null)
                return;
            else if (previous.X == X - 1) //Coming in from left
                Image.style.rotate = new StyleRotate(new Rotate(270f));
            else if (previous.X == X + 1) //Coming in from right
                Image.style.rotate = new StyleRotate(new Rotate(90f));
            else if (previous.Y == Y - 1) //Coming in from top
                Image.style.rotate = new StyleRotate(new Rotate(0f));
            else //Bottom
                Image.style.rotate = new StyleRotate(new Rotate(180f));
        }
        else if (previous.X == X && next.X == X) //horizontal line
        {
            State = TileState.LINE;

            Image.style.rotate = new StyleRotate(new Rotate(0f));
        }
        else if (previous.Y == Y && next.Y == Y) //vertical line
        {
            State = TileState.LINE;

            Image.style.rotate = new StyleRotate(new Rotate(90f));
        }
        //B L corner
        else if (
            (previous.X == X && previous.Y == Y + 1 && next.X == X - 1 && next.Y == Y) ||
            (previous.X == X - 1 && previous.Y == Y && next.X == X && next.Y == Y + 1))
        {
            State = TileState.CORNER;

            Image.style.rotate = new StyleRotate(new Rotate(90f));
        }
        //B R corner
        else if ((previous.X == X && previous.Y == Y + 1 && next.X == X + 1 && next.Y == Y) ||
                (previous.X == X + 1 && previous.Y == Y && next.X == X && next.Y == Y + 1))
        {
            State = TileState.CORNER;

            Image.style.rotate = new StyleRotate(new Rotate(0f));
        }
        //T L Corner
        else if ((previous.X == X && previous.Y == Y - 1 && next.X == X - 1 && next.Y == Y) ||
            (previous.X == X - 1 && previous.Y == Y && next.X == X && next.Y == Y - 1))
        {
            State = TileState.CORNER;

            Image.style.rotate = new StyleRotate(new Rotate(180f));
        }
        //T R Corner
        else if ((previous.X == X + 1 && previous.Y == Y && next.X == X && next.Y == Y - 1) ||
            (previous.X == X && previous.Y == Y - 1 && next.X == X + 1 && next.Y == Y))
        {
            State = TileState.CORNER;

            Image.style.rotate = new StyleRotate(new Rotate(270f));
        }
    }

    public void ClearLine()
    {
        if (Line == null || State == TileState.END || State == TileState.BLANK)
            return;

        Line = null;
        State = TileState.EMPTY;
    }

    public void SetColor(Color c)
    {
        Image.style.unityBackgroundImageTintColor = c;
    }

    public void PuzzleComplete(float duration)
    {
        if (line != null)
        {
            if (line.LineCancelled)
                SetLineCancel();
            else
            {
                Color bgColor = new Color(Line.Color.r, Line.Color.g, Line.Color.b, Line.Color.a / 2f);

                Container.style.backgroundColor = Color.white;
                image.style.backgroundColor     = bgColor;
            }
        }

        if (!top)       topBorderVE.style.Hide();
        if (!right)     rightBorderVE.style.Hide();
        if (!bottom)    bottomBorderVE.style.Hide();
        if (!left)      leftBorderVE.style.Hide();

        Tween shake = DOTween.To(
                        () => container.worldTransform.rotation.eulerAngles,
                        x => container.transform.rotation = Quaternion.Euler(x),
                        new Vector3(0f, 0f, 360f), duration).SetEase(Ease.InOutBounce).SetLoops(1);
        
        shake.Play();
    }

    #endregion

    public override string ToString()
    {
        return string.Format("\nTile: [{0},{1}] STATE: {2} LINE: {3}\n", X, Y, State, Line == null ? "null" : Line);
    }
}
