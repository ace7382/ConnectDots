using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorCategory
{ 
    [EnumName("System Colors")] SYSTEM,
    [EnumName("Reds")]          REDS,
    [EnumName("Greens")]        GREENS,
    [EnumName("Blues")]         BLUES,
}

[System.Serializable]
public class GameColor
{
    public Color color;
    public string name;
    public ColorCategory category;
}
