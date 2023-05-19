using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileBorder
{
    #region Private Variables

    private VisualElement   ve;
    private bool            active;
    private bool            tilesToLeftAndRight;
    private Tile            leftUpTile;
    private Tile            rightDownTile;

    #endregion

    #region Public Properties

    public bool             TilesToLeftAndRight { get { return tilesToLeftAndRight; } }
    public bool             Active              { get { return active; } }
    public Tile             LeftUpTile          { get { return leftUpTile; } }
    public Tile             RightDownTile       { get { return rightDownTile; } }
    public VisualElement    VisualElement       { get { return ve; } }

    #endregion

    #region Constructor

    public TileBorder(VisualElement ve, bool leftToRight, Tile leftUp)
    {
        this.ve             = ve;
        tilesToLeftAndRight = leftToRight;
        leftUpTile          = leftUp;
    }

    #endregion

    #region Public Functions

    public void SetRightDownTile(Tile rightDownTile)
    {
        this.rightDownTile  = rightDownTile;
    }

    public void SetActive(bool showing)
    {
        //We don't want to use show/hide bc that will affect the board's layout
        //ve.SetOpacity(showing ? 100f : 0f); 
        active = showing;

        if (tilesToLeftAndRight)
        {
            leftUpTile.SetBorders_Right(showing);
            rightDownTile.SetBorders_Left(showing);
        }
        else
        {
            leftUpTile.SetBorders_Bottom(showing);
            rightDownTile.SetBorders_Top(showing);
        }
    }

    #endregion

    #region Unity Functions

    public override string ToString()
    {
        return "Tiles To Left and Right? " + tilesToLeftAndRight + "\nLeft up Tile: " + leftUpTile + "\nRight Down Tile: " + rightDownTile;
    }

    #endregion
}
