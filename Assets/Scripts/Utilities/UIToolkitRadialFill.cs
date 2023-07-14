using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitRadialFill : VisualElement
{
    #region Private Variables

    private Vector2     center;
    private float       radius;
    private Color       color;
    private float       fillPercent;

    #endregion

    #region Constructor

    public UIToolkitRadialFill(Vector2 origin, float radius, Color color, float fillPercent)
    {
        center                  = origin;
        this.radius             = radius;
        this.color              = color;
        this.fillPercent        = fillPercent;

        generateVisualContent   += OnGenerateVisualContent;
    }

    #endregion

    #region Public Functions

    public void SetFillPercent(float fillPercent)
    {
        this.fillPercent        = fillPercent;
    }

    #endregion

    #region Private Functions

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Painter2D painter       = mgc.painter2D;
        painter.fillColor       = color;

        painter.BeginPath();
        painter.Arc(center, radius, new Angle(0f, AngleUnit.Degree), new Angle(fillPercent * 360f, AngleUnit.Degree));
        painter.Fill();
    }

    #endregion
}
