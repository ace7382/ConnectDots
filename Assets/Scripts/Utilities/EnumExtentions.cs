using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Instructions : Attribute
{
    #region Properties

    public string EnumInstructions { get; protected set; }

    #endregion

    #region Constructor

    public Instructions(string value)
    {
        this.EnumInstructions = value;
    }

    #endregion
}

public class EnumName : Attribute
{
    #region Properties

    public string Name { get; protected set; }

    #endregion

    #region Constructor

    public EnumName(string value)
    {
        this.Name = value;
    }

    #endregion
}

public static class EnumExtentions
{
    public static string Instructions(this Enum value)
    {
        Type type               = value.GetType();
        FieldInfo fieldInfo     = type.GetField(value.ToString());
        Instructions[] attribs  = fieldInfo.GetCustomAttributes(typeof(Instructions), false) as Instructions[];

        return attribs.Length > 0 ? attribs[0].EnumInstructions : null;
    }

    public static string Name(this Enum value)
    {
        Type type               = value.GetType();
        FieldInfo fieldInfo     = type.GetField(value.ToString());
        EnumName[] attribs      = fieldInfo.GetCustomAttributes(typeof(EnumName), false) as EnumName[];

        return attribs.Length > 0 ? attribs[0].Name : null;
    }
}