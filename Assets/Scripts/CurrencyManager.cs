using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    #region Singleton

    public static CurrencyManager instance;

    #endregion

    #region Private Variables

    private Dictionary<int, int> ownedColors;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
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

    #endregion

    #region Private Functions

    #endregion
}
