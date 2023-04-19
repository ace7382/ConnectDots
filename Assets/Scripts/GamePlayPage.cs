using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayPage : Page
{
    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        //args[0]   -   Level   -   The Level to load

        BoardCreator.instance.Setup(uiDoc, (Level)args[0]);
    }

    public override void HidePage()
    {
        
    }

    #endregion
}
