using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorCategory
{ 
    [EnumName("Blacks/Whites")] BLACK_AND_WHITE     = 0,
    [EnumName("Reds")]          RED                 = 1,
    [EnumName("Purples")]       PURPLE              = 2,
    [EnumName("Blues")]         BLUE                = 3,
    [EnumName("Greens")]        GREEN               = 4,
    [EnumName("Yellows")]       YELLOW              = 5,
    [EnumName("Oranges")]       ORANGE              = 6
}

[System.Serializable]
public class GameColor
{
    public Color color;
    public string name;
    public ColorCategory category;
}
