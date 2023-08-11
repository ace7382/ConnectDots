using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonStateChanger
{
    private bool            pressed;
    private VisualElement   button;
    private Color           originalColor;
    private Color           pressedColor;

    public ButtonStateChanger(VisualElement button, Color originalColor, Color pressedColor)
    {
        this.button         = button;
        this.originalColor  = originalColor;
        this.pressedColor   = pressedColor;
        pressed             = false;
    }

    public void OnPointerDown(PointerDownEvent evt)
    {
        button.style.right  = -4f;
        button.style.bottom = -4f;

        button.SetColor(pressedColor);

        pressed             = true;
    }

    public void OnPointerUp(PointerUpEvent evt)
    {
        if (pressed == false)
            return;

        button.SetColor(originalColor);

        StyleLength s       = new StyleLength(StyleKeyword.Auto);
        button.style.right  = s;
        button.style.bottom = s;

        pressed             = false;
    }

    public void OnPointerOff(PointerLeaveEvent evt)
    {
        OnPointerUp(null);
    }
}