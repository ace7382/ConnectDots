using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    public UIDocument uiDoc;
    public Vector2Int drawBetween;
    public bool go;

    List<VisualElement> tiles;
    UIToolkitLine line;

    public List<Vector2> points;

    public VisualElement lineContainer;

    public void Start()
    {
        //tiles = uiDoc.rootVisualElement.Query<VisualElement>("Tile").ToList();
        lineContainer = uiDoc.rootVisualElement.Q<VisualElement>("LineContainer");

        //for (int i = 0; i < tiles.Count; i++)
        //{
        //    tiles[i].name = "Tile " + i.ToString();
        //    tiles[i].RegisterCallback<PointerUpEvent>(DrawHere);
        //}

        //line = new UITookitLine(points, 30f);

        UIToolkitCircle a = new UIToolkitCircle(new Vector2(400f, 400f), 80f, Color.red);

        lineContainer.Add(a);

        List<Vector2> points = new List<Vector2>()
        {
            new Vector2(100, 100),
            new Vector2(200, 100),
            new Vector2(200, 200),
            new Vector2(300, 200)
        };

        //UITookitLine k = new UITookitLine(points, 30f);

        //lineContainer.Add(k);
    }

    public void Update()
    {
        //line.SetPoints(points);

        //if (go)
        //{
        //    //VisualElement t1 = tiles[0];
        //    //VisualElement t2 = tiles[1];

        //    //Vector2 pt1 = t1.worldBound.center;
        //    //Vector2 pt2 = t2.worldBound.center;

        //    //LineDrawer l = new LineDrawer(pt1, pt2, 20);
        //    if (line != null)
        //        lineContainer.Remove(line);

        //    line = new UITookitLine(poits, 15f);

        //    lineContainer.Add(line);

        //    go = false;
        //}
    }

    public void DrawHere(PointerUpEvent evt)
    {
        VisualElement tar = evt.target as VisualElement;

        points.Add(tar.worldBound.center);
    }
}