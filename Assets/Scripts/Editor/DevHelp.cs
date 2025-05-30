#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class DevHelp : MonoBehaviour
{
    [MenuItem("Dev Commands/Reset Objectives")]
    public static void ResetObjectives()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Editor is in Playmode. This function cannot be used");
            return;
        }

        ObjectiveManager.ResetObjectives();
    }

    [MenuItem("Dev Commands/Reset Levels")]
    public static void ResetLevels()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Editor is in Playmode. This function cannot be used");
            return;
        }

        List<Level> allLevels = Resources.LoadAll<Level>("Levels").ToList();

        for (int i = 0; i < allLevels.Count; i++)
        {
            allLevels[i].ResetLevel();
        }
    }
}

#endif