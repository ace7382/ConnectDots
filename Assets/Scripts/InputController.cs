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
    private bool canClick;

    public bool CanAcceptClick { get { return canClick; } }

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

    public void CanClick(bool canClick = true)
    {
        this.canClick = canClick;
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
        if (!canClick)
            return;

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
        if (draggingLine == null || !canClick)
            return;

        VisualElement ve = evt.target as VisualElement;
        Tile tile = ve.userData as Tile;

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
    }

    private void TileDraggedOutHandler(PointerOutEvent evt)
    {

    }

    #endregion
}
