using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitCircle : VisualElement
{
    private Vector2     center;
    private float       radius;
    private Color       color;

    public Vector2      Center      { get { return center; } }

    public UIToolkitCircle(Vector2 origin, float radius, Color color)
    {
        center      = origin;
        this.radius = radius;
        this.color  = color;

        generateVisualContent += OnGenerateVisualContent;
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Painter2D painter   = mgc.painter2D;
        painter.fillColor   = color;

        painter.BeginPath();
        painter.Arc(center, radius, new Angle(0f, AngleUnit.Degree), new Angle(360f, AngleUnit.Degree));

        painter.Fill();
    }

    public override string ToString()
    {
        return "Circle: " + center + " Color: " + color + " Radius:" + radius;
    }
}
