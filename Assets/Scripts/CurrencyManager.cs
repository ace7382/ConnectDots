using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    #region Singleton

    public static CurrencyManager instance;

    #endregion

    #region Private Variables

    private Dictionary<int, int> ownedColors;

    #endregion

    #region Public Properties

    public int TotalTokens { get { return ownedColors.Sum(x => x.Value); } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        ownedColors = new Dictionary<int, int>();
    }

    #endregion

    #region Public Functions

    public void AddCurrency(int colorIndex, int amount)
    {
        if (!ownedColors.ContainsKey(colorIndex))
            ownedColors.Add(colorIndex, 0);

        ownedColors[colorIndex] += amount;
    }

    public void SpendCurrency(int colorIndex, int amount)
    {
        if (amount > ownedColors[colorIndex])
        {
            Debug.Log(string.Format("Trying to spend {0} of color {1}. Only have {2} though"
                , amount.ToString(), colorIndex.ToString(), ownedColors[colorIndex].ToString()));

            return;
        }

        ownedColors[colorIndex] -= amount;
    }

    public override string ToString()
    {
        string ret = "***Current Currency***";

        foreach (KeyValuePair<int, int> color in ownedColors)
            ret += "\n" + color.Key.ToString() + ": " + color.Value.ToString();

        return ret;
    }

    public int GetCoinsForColorIndex(int index)
    {
        if (!ownedColors.ContainsKey(index))
            return 0;

        return ownedColors[index];
    }

    public List<Vector2Int> AwardCoins(List<Tile> levelTiles)
    {
        //Return is List<ColorIndex, NumberAwarded>

        return null;
    }

    #endregion

    #region Private Functions

    #endregion

#if UNITY_EDITOR
    #region Dev Help Functions

    [MenuItem("Dev Commands/Give 1000 of Each Color")]
    public static void LinkObjectivesToObjectiveManager()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Editor is not in Playmode. This function cannot be used");
            return;
        }

        for (int i = 0; i < UIManager.instance.ColorCount; i++)
            CurrencyManager.instance.AddCurrency(i, 1000);
    }

    #endregion
#endif
}
