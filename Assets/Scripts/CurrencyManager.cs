using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    #region Singleton

    public static CurrencyManager instance;

    #endregion

    #region Private Variables

    private Dictionary<Color, int> ownedColors;

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
        ownedColors = new Dictionary<Color, int>();
    }

    #endregion

    #region Public Functions

    public void AddCurrency(Color color, int amount)
    {
        if (!ownedColors.ContainsKey(color))
            ownedColors.Add(color, 0);

        ownedColors[color] += amount;

        Debug.Log(this);
    }

    public override string ToString()
    {
        string ret = "***Current Currency***";

        foreach (KeyValuePair<Color, int> color in ownedColors)
            ret += "\n" + color.Key.ToString() + ": " + color.Value.ToString();

        return ret;
    }

    #endregion

    #region Private Functions

    #endregion
}
