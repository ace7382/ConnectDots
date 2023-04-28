using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        Debug.Log(this);
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

    #endregion

    #region Private Functions

    #endregion
}
