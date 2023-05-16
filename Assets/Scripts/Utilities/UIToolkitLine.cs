using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitLine : VisualElement
{
    private List<Vector2> points;
    private float thickness;
    private Color color;
    private LineCap cap;

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
}