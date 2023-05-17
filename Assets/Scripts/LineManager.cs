using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

public class LineManager : MonoBehaviour
{
    #region Singleton

    public static LineManager instance;

    #endregion

    #region Inspector Variables

    [SerializeField] private UIDocument uiDoc;
    [SerializeField] private float      circleRadius; //TODO: Move this to the board
    [SerializeField] private float      lineThickness;

    #endregion

    #region Private Variables

    private VisualElement           lineContainer;

    private List<UIToolkitCircle>   endPoints;

    private Dictionary<Line, UIToolkitLine> lines;

    #endregion

    #region Public Properties

    public VisualElement            LineContainer { get { return lineContainer; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        lines       = new Dictionary<Line, UIToolkitLine>();
        endPoints   = new List<UIToolkitCircle>();
    }

    private void Start()
    {
        lineContainer = uiDoc.rootVisualElement.Q<VisualElement>("LineContainer");

        lineThickness = circleRadius * .9f;
    }

    #endregion

    #region Public Functions

    public void DrawEndPoint(Tile t)
    {
        //TODO: Might be a better place to put this
        //      but this is called during gameplay start which
        //      will keep the lines on the same sorting order as the game board
        uiDoc.sortingOrder = PageManager.instance.HighestSortOrder + .5f;

        if (t.State != TileState.END)
            return;

        UIToolkitCircle endPoint = new UIToolkitCircle(t.Container.worldBound.center, circleRadius, t.Line.Color);

        lineContainer.Add(endPoint);
        endPoints.Add(endPoint);

        if (t.EndPieceRotation != EndTileRotation.OPEN)
        {
            List<Vector2> linePoints = new List<Vector2>();
            linePoints.Add(t.Container.worldBound.center);

            switch(t.EndPieceRotation)
            {
                case EndTileRotation.LEFT:
                    linePoints.Add(new Vector2(t.Container.worldBound.xMin, t.Container.worldBound.center.y));
                    break;
                case EndTileRotation.RIGHT:
                    linePoints.Add(new Vector2(t.Container.worldBound.xMax, t.Container.worldBound.center.y));
                    break;
                case EndTileRotation.TOP:
                    linePoints.Add(new Vector2(t.Container.worldBound.center.x, t.Container.worldBound.yMin));
                    break;
                case EndTileRotation.BOTTOM:
                    linePoints.Add(new Vector2(t.Container.worldBound.center.x, t.Container.worldBound.yMax));
                    break;
            }

            UIToolkitLine l = new UIToolkitLine(linePoints, lineThickness, t.Line.Color, LineCap.Butt);
            lineContainer.Add(l);
        }
    }

    public void UpdateLine(Line line, List<Tile> tiles)
    {
        UIToolkitLine uiLine = null;

        if (lines.ContainsKey(tiles[0].Line))
        {
            uiLine = lines[tiles[0].Line];
        }
        else
        {
            uiLine = new UIToolkitLine(new List<Vector2>(), lineThickness, line.Color, LineCap.Round);
            lines.Add(line, uiLine);

            lineContainer.Add(uiLine);
        }

        List<Vector2> tileCenters = new List<Vector2>();

        for (int i = 0; i < tiles.Count; i++)
            tileCenters.Add(tiles[i].Container.worldBound.center);

        uiLine.SetPoints(tileCenters);
    }

    public IEnumerator ShrinkAllLines(float duration)
    {
        foreach (KeyValuePair<Line, UIToolkitLine> line in lines)
        {
            StartCoroutine(line.Value.ShrinkLine(true, true, duration));
        }

        yield return new WaitForSeconds(duration);

        Clear();
    }

    public void Clear()
    {
        lineContainer.SetOpacity(1f);
        lineContainer.Clear();
        lines.Clear();
        uiDoc.sortingOrder = 999;
    }

    #endregion
}
