using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class InputController : MonoBehaviour
{
    public static InputController instance;

    private Line draggingLine;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        draggingLine = null;
    }

    public void RegisterBoardInteractionCallbacks(VisualElement board)
    {
        board.RegisterCallback<PointerUpEvent>(ClickReleasedHandler);
        board.RegisterCallback<PointerLeaveEvent>(ClickReleasedHandler);
    }

    public void RegisterTileInteractionCallbacks(VisualElement tile)
    {
        tile.RegisterCallback<PointerDownEvent>(TileClickedHandler);
        tile.RegisterCallback<PointerOverEvent>(TileDraggedOverHandler);
        tile.RegisterCallback<PointerOutEvent>(TileDraggedOutHandler);
    }

    public void UnregisterBoardInteractionCallbacks(VisualElement board)
    {
        board.UnregisterCallback<PointerUpEvent>(ClickReleasedHandler);
        board.UnregisterCallback<PointerLeaveEvent>(ClickReleasedHandler);
    }

    public void UnregisterTileInteractionCallbacks(VisualElement tile)
    {
        tile.UnregisterCallback<PointerDownEvent>(TileClickedHandler);
        tile.UnregisterCallback<PointerOverEvent>(TileDraggedOverHandler);
        tile.UnregisterCallback<PointerOutEvent>(TileDraggedOutHandler);
    }

    #region Board Handlers

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

    #endregion

    #region Tile Handlers

    private void TileClickedHandler(PointerDownEvent evt)
    {
        VisualElement ve = (evt.target as VisualElement);
        Tile tile = ve.userData as Tile;

        Debug.Log(ve.name + " clicked");

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
        if (draggingLine == null)
            return;

        VisualElement ve = evt.target as VisualElement;
        Tile tile = ve.userData as Tile;

        Debug.Log("Dragged into " + ve.name + " State: " + tile.State);

        ////if (draggingLine.IsAdjacentToHead(tile) && draggingLine.LineHead.CanLeaveEndTile(tile))
        //if (draggingLine.IsAdjacentToHead(tile) && draggingLine.LineHead.CanEnterTile(tile))
        //{
        //    if (tile.State == TileState.EMPTY)
        //    {
        //        draggingLine.AddTile(tile);
        //    }
        //    else if (tile.Line == draggingLine && (tile.State == TileState.LINE || tile.State == TileState.CORNER))
        //    {
        //        draggingLine.RemoveAfterTile(tile); //TODO: if the tile isn't next to the head it won't cut it short. May need to see how pathfinding affects this, or pull it out to a higher level if
        //    }
        //    else if (tile.State == TileState.END && draggingLine == tile.Line
        //            //&& !draggingLine.ContainsTile(tile)  //TODO: Can probably combine these checks with the CanEnterTile function
        //            //&& tile.CanEnterEndTile(draggingLine.LineHead))
        //            )
        //    {
        //        if (!draggingLine.ContainsTile(tile)) //If it is the non-starting end tile
        //            draggingLine.AddTile(tile);
        //        else
        //            draggingLine.ClearTilesAndAdd(tile); //Clear out the line if it's just the starting end tile
        //    }
        //}
        //else if (!draggingLine.IsAdjacentToHead(tile)) //TODO: Move the draggingline.linehead.canenter tile into a sub-if above, and it will replace the need for this second call
        //{
        //    if (tile.State == TileState.EMPTY)
        //    {
        //        List<Tile> path = BoardCreator.instance.FindPath(draggingLine.LineHead, tile);

        //        if (path != null)
        //        {
        //            for (int i = 0; i < path.Count; i++)
        //            {
        //                if (path[i] == draggingLine.LineHead)
        //                    continue;

        //                draggingLine.AddTile(path[i]);
        //            }
        //        }
        //    }
        //}

        ////THIS ALL WORKS vvv
        //if (tile.Line == draggingLine)
        //{
        //    if (tile.State == TileState.LINE || tile.State == TileState.CORNER)         //Drag over the same line's pieces
        //        draggingLine.RemoveAfterTile(tile);
        //    else if (tile.State == TileState.END && draggingLine.ContainsTile(tile))    //Drag over the start point
        //        draggingLine.ClearTilesAndAdd(tile);
        //    else                                                                        //Drag over the other end point\
        //    {
        //        List<Tile> path = BoardCreator.instance.FindPath(draggingLine.LineHead, tile);

        //        if (path != null)
        //        {

        //            for (int i = 0; i < path.Count; i++)
        //            {
        //                if (path[i] == draggingLine.LineHead)
        //                    continue;

        //                draggingLine.AddTile(path[i]);
        //            }
        //        }
        //    }
        //}
        //else if (tile.Line == null) //This will exclude other lines and their end points
        //{
        //    if (draggingLine.LineHead.State == TileState.END && draggingLine.ContainsTwoEndTiles()) //Check to see if you're dragging FROM a completed line
        //        draggingLine.RemoveAfterTile(draggingLine.PreLineHead);

        //    List<Tile> path = BoardCreator.instance.FindPath(draggingLine.LineHead, tile);

        //    if (path != null)
        //    {

        //        for (int i = 0; i < path.Count; i++)
        //        {
        //            if (path[i] == draggingLine.LineHead)
        //                continue;

        //            draggingLine.AddTile(path[i]);
        //        }
        //    }
        //}
        ////^^^

        //TODO: Dragging from a completed line end > off the board > onto the same completed line end will restart the
        //      line. Probably should just have it do nothing

        if (tile.Line == draggingLine)
        {
            if (tile.State == TileState.LINE || tile.State == TileState.CORNER)         //Drag over the same line's pieces
                draggingLine.RemoveAfterTile(tile);
            else if (tile.State == TileState.END && draggingLine.ContainsTile(tile))    //Drag over the start point
                draggingLine.ClearTilesAndAdd(tile);
            else                                                                        //Drag over the other end point\
            {
                List<Tile> tempList = new List<Tile>(draggingLine.Tiles);
                List<Tile> path = BoardCreator.instance.FindPath(draggingLine.FirstTile, tile);

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
            List<Tile> path = BoardCreator.instance.FindPath(draggingLine.FirstTile, tile);

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


        //if (tile.Line == draggingLine)
        //{
        //    if (tile.State == TileState.LINE || tile.State == TileState.CORNER)         //Drag over the same line's pieces
        //        draggingLine.RemoveAfterTile(tile);
        //    else if (tile.State == TileState.END && draggingLine.ContainsTile(tile))    //Drag over the start point
        //        draggingLine.ClearTilesAndAdd(tile);
        //    else                                                                        //Drag over the other end point\
        //    {
        //        List<Tile> tempTiles = new List<Tile>(draggingLine.Tiles);
        //        draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

        //        List<Tile> path = BoardCreator.instance.FindPath(draggingLine.FirstTile, tile);

        //        if (path != null)
        //        {
        //            //draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

        //            for (int i = 0; i < path.Count; i++)
        //            {
        //                if (path[i] == draggingLine.FirstTile)
        //                    continue;

        //                draggingLine.AddTile(path[i]);
        //            }
        //        }
        //        else
        //        {
        //            for (int i = 1; i < tempTiles.Count; i++)
        //                draggingLine.AddTile(tempTiles[i]);
        //        }
        //    }
        //}
        //else if (tile.Line == null) //This will exclude other lines and their end points
        //{
        //    if (draggingLine.LineHead.State == TileState.END && draggingLine.ContainsTwoEndTiles()) //Check to see if you're dragging FROM a completed line
        //        draggingLine.RemoveAfterTile(draggingLine.PreLineHead);

        //    List<Tile> tempTiles = new List<Tile>(draggingLine.Tiles);
        //    draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

        //    List<Tile> path = BoardCreator.instance.FindPath(draggingLine.FirstTile, tile);

        //    if (path != null)
        //    {
        //        //draggingLine.ClearTilesAndAdd(draggingLine.FirstTile);

        //        for (int i = 0; i < path.Count; i++)
        //        {
        //            if (path[i] == draggingLine.FirstTile)
        //                continue;

        //            draggingLine.AddTile(path[i]);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 1; i < tempTiles.Count; i++) //Starts at 1 so the first tile isn't added
        //        {
        //            draggingLine.AddTile(tempTiles[i]);
        //        }
        //    }
        //}
    }

    private void TileDraggedOutHandler(PointerOutEvent evt)
    {

    }

    #endregion
}
