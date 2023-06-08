using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitLine : VisualElement
{
    private List<Vector2>   points;
    private float           thickness;
    private Color           color;
    private LineCap         cap;

    public List<Vector2>    Points      { get { return points; } }
    public Vector2          LastPoint   { get { return Points[Points.Count - 1]; } }

    public UIToolkitLine(List<Vector2> points, float width, Color color, LineCap cap)
    {
        this.points     = points;
        thickness       = width;
        this.color      = color;
        this.cap        = cap;

        generateVisualContent += OnGenerateVisualContent;
    }

    public void SetPoints(List<Vector2> points)
    {
        this.points = points;
        this.MarkDirtyRepaint();
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        if (points.Count == 0)
            return;

        Painter2D painter       = mgc.painter2D;

        painter.strokeColor     = color;
        painter.lineCap         = cap;
        painter.lineWidth       = thickness;
        painter.lineJoin        = LineJoin.Round;

        painter.BeginPath();

        painter.MoveTo(points[0]);

        for (int i = 1; i < points.Count; i++)
            painter.LineTo(points[i]);

        painter.Stroke();
    }

    public void AddNewPoint(Vector2 newPoint)
    {
        points.Add(newPoint);
    }

    public IEnumerator DrawTowardNewPoint(Vector2 newPoint, float duration)
    {
        Points.Add(Points[Points.Count - 1]);

        Tween draw  =   DOTween.To(
                            () => Points[Points.Count - 1]
                            , x => Points[Points.Count - 1] = x
                            , newPoint
                            , duration
                        )
                        .OnUpdate(() => this.MarkDirtyRepaint())
                        .SetEase(Ease.Linear)
                        .Play();

        yield return draw.WaitForCompletion();
    }

    public Tween DrawTowardNewPoint_Tween(Vector2 newPoint, float duration)
    {
        Points.Add(Points[Points.Count - 1]);

        Tween draw = DOTween.To(
                            () => Points[Points.Count - 1]
                            , x => Points[Points.Count - 1] = x
                            , newPoint
                            , duration
                        )
                        .OnUpdate(() => this.MarkDirtyRepaint());

        return draw;
    }

    public IEnumerator ShrinkLine(bool bothDirections, bool forward, float duration)
    {
        if (points.Count < 2)
            yield return null;

        Tween movePt        = null;

        int start           = forward ? 0 : points.Count - 1;
        int destination     = forward ? 1 : points.Count - 2;

        duration            = duration / (points.Count - 1);

        while (points.Count > 1)
        {
            if (movePt == null)
            {
                if (bothDirections)
                {
                    movePt = DOTween.Sequence();
                    Tween front = null;
                    Tween back = null;

                    if (points.Count == 2)
                    {
                        Vector2 mid = Vector2.Lerp(points[0], points[1], .5f);

                        front = DOTween.To(() => points[0],
                            x => points[0] = x,
                            mid,
                            duration / 2f)
                            .SetEase(Ease.Linear);

                        back = DOTween.To(() => points[1],
                            x => points[1] = x,
                            mid,
                            duration / 2f)
                            .SetEase(Ease.Linear);
                    }
                    else
                    {
                        front = DOTween.To(() => points[0],
                            x => points[0] = x,
                            points[1],
                            duration)
                            .SetEase(Ease.Linear);

                        back = DOTween.To(() => points[points.Count - 1],
                            x => points[points.Count - 1] = x,
                            points[points.Count - 2],
                            duration)
                            .SetEase(Ease.Linear);
                    }

                    (movePt as Sequence).Append(front);
                    (movePt as Sequence).Join(back);
                    movePt
                        .OnUpdate(() => this.MarkDirtyRepaint())
                        .OnComplete(() =>
                            {
                                movePt = null;
                                points.RemoveAt(points.Count - 1);
                                points.RemoveAt(0);
                            })
                        .Play();
                }
                else
                {
                    movePt = DOTween.To(() => points[start],
                        x => points[start] = x,
                        points[destination],
                        duration)
                        .OnUpdate(() => this.MarkDirtyRepaint())
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            points.RemoveAt(start);
                            start = forward ? 0 : points.Count - 1;
                            destination = forward ? 1 : points.Count - 2;
                            movePt = null;
                        })
                        .Play();
                }
            }

            yield return null;
        }
    }

    private IEnumerator ShrinkTowardPoint(bool forward, float duration)
    {
        Vector2 start       = forward ? points[0] : points[points.Count - 1];
        Vector2 destination = forward ? points[1] : points[points.Count - 2];

        Tween movePt = DOTween.To(() => start,
            x => destination = x,
            points[1], duration)
            .OnUpdate(() => this.MarkDirtyRepaint())
            .OnComplete(() => points.RemoveAt(forward ? 0 : points.Count - 1));

        yield return movePt.WaitForCompletion();
    }

    public override string ToString()
    {
        string ret = string.Format("Line {0} | Color: {1} | Thickness: {2} | Cap Style: {3}\n"
                    , name, color, thickness, cap);

        for (int i = 0; i < points.Count; i++)
        {
            ret += string.Format("Point {0}: {1}\n", i.ToString(), points[i].ToString());
        }

        return ret;
    }
}