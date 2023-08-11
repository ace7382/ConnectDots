using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfilePage : Page
{
    #region Inherited Functions

    public override void ShowPage(object[] args)
    {
        
    }

    public override IEnumerator AnimateIn()
    {
        yield return null;

        //DrawEXPGraph();
        DrawEXPBars();

        yield return null;
    }

    public override IEnumerator AnimateOut()
    {
        throw new System.NotImplementedException();
    }

    public override void HidePage()
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Private Functions

    private void DrawEXPBars()
    {
        //TODO - Use colors from UIManager for dots/progress lines
        
        VisualElement bwEXPBar          = uiDoc.rootVisualElement.Q<VisualElement>("BlackAndWhiteEXP").Q<VisualElement>("BG");
        Label bwCurrentLabel            = bwEXPBar.Q<Label>("CurrentLevel");
        Label bwNextLabel               = bwEXPBar.Q<Label>("NextLevel");
        Label bwProgressLabel           = bwEXPBar.Q<Label>("CurrentProgress");

        Vector2 bwLeftOrigin            = new Vector2(bwEXPBar.WorldToLocal(bwCurrentLabel.worldBound.center).x, -10f);
        Vector2 bwRightOrigin           = new Vector2(bwEXPBar.WorldToLocal(bwNextLabel.worldBound.center).x, -10f);

        bwCurrentLabel.text             = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE).ToString();
        bwNextLabel.text                = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE) + 1).ToString();
        bwProgressLabel.text            = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.BLACK_AND_WHITE).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE)).ToString();

        UIToolkitCircle bwLeftDot       = new UIToolkitCircle(bwLeftOrigin, 35f, Color.black);
        UIToolkitCircle bwRightDot      = new UIToolkitCircle(bwRightOrigin, 35f, Color.black);

        float bwDotDistance             = bwRightOrigin.x - bwLeftOrigin.x;
        Vector2 bwProgressStop          = new Vector2(
                                            bwLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.BLACK_AND_WHITE) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE))
                                                * bwDotDistance)
                                            , bwLeftOrigin.y);

        List<Vector2> bwProgPoints      = new List<Vector2>()
                                            { bwLeftOrigin, bwProgressStop };

        UIToolkitLine bwProgLine        = new UIToolkitLine(bwProgPoints, 30f, Color.black, LineCap.Round);

        bwEXPBar.Add(bwLeftDot);
        bwEXPBar.Add(bwRightDot);
        bwEXPBar.Add(bwProgLine);

        VisualElement redEXPBar         = uiDoc.rootVisualElement.Q<VisualElement>("RedEXP").Q<VisualElement>("BG");
        Label redCurrentLabel           = redEXPBar.Q<Label>("CurrentLevel");
        Label redNextLabel              = redEXPBar.Q<Label>("NextLevel");
        Label redProgressLabel          = redEXPBar.Q<Label>("CurrentProgress");

        Vector2 redLeftOrigin           = new Vector2(redEXPBar.WorldToLocal(redCurrentLabel.worldBound.center).x, -10f);
        Vector2 redRightOrigin          = new Vector2(redEXPBar.WorldToLocal(redNextLabel.worldBound.center).x, -10f);

        redCurrentLabel.text            = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.RED).ToString();
        redNextLabel.text               = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.RED) + 1).ToString();
        redProgressLabel.text           = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.RED).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.RED)).ToString();

        UIToolkitCircle redLeftDot      = new UIToolkitCircle(redLeftOrigin, 35f, Color.red);
        UIToolkitCircle redRightDot     = new UIToolkitCircle(redRightOrigin, 35f, Color.red);

        float redDotDistance            = redRightOrigin.x - redLeftOrigin.x;
        Vector2 redProgressStop         = new Vector2(
                                            redLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.RED) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.RED))
                                                * redDotDistance)
                                            , redLeftOrigin.y);

        List<Vector2> redProgPoints     = new List<Vector2>()
                                            { redLeftOrigin, redProgressStop };

        UIToolkitLine redProgLine       = new UIToolkitLine(redProgPoints, 30f, Color.red, LineCap.Round);

        redEXPBar.Add(redLeftDot);
        redEXPBar.Add(redRightDot);
        redEXPBar.Add(redProgLine);

        VisualElement purpleEXPBar      = uiDoc.rootVisualElement.Q<VisualElement>("PurpleEXP").Q<VisualElement>("BG");
        Label purpleCurrentLabel        = purpleEXPBar.Q<Label>("CurrentLevel");
        Label purpleNextLabel           = purpleEXPBar.Q<Label>("NextLevel");
        Label purpleProgressLabel       = purpleEXPBar.Q<Label>("CurrentProgress");

        Vector2 purpleLeftOrigin        = new Vector2(purpleEXPBar.WorldToLocal(purpleCurrentLabel.worldBound.center).x, -10f);
        Vector2 purpleRightOrigin       = new Vector2(purpleEXPBar.WorldToLocal(purpleNextLabel.worldBound.center).x, -10f);

        purpleCurrentLabel.text         = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.PURPLE).ToString();
        purpleNextLabel.text            = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.PURPLE) + 1).ToString();
        purpleProgressLabel.text        = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.PURPLE).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.PURPLE)).ToString();

        UIToolkitCircle purpleLeftDot   = new UIToolkitCircle(purpleLeftOrigin, 35f, Color.cyan);
        UIToolkitCircle purpleRightDot  = new UIToolkitCircle(purpleRightOrigin, 35f, Color.cyan);

        float purpleDotDistance         = purpleRightOrigin.x - purpleLeftOrigin.x;
        Vector2 purpleProgressStop      = new Vector2(
                                            purpleLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.PURPLE) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.PURPLE))
                                                * purpleDotDistance)
                                            , purpleLeftOrigin.y);

        List<Vector2> purpleProgPoints  = new List<Vector2>()
                                            { purpleLeftOrigin, purpleProgressStop };

        UIToolkitLine purpleProgLine    = new UIToolkitLine(purpleProgPoints, 30f, Color.cyan, LineCap.Round);

        purpleEXPBar.Add(purpleLeftDot);
        purpleEXPBar.Add(purpleRightDot);
        purpleEXPBar.Add(purpleProgLine);

        VisualElement blueEXPBar        = uiDoc.rootVisualElement.Q<VisualElement>("BlueEXP").Q<VisualElement>("BG");
        Label blueCurrentLabel          = blueEXPBar.Q<Label>("CurrentLevel");
        Label blueNextLabel             = blueEXPBar.Q<Label>("NextLevel");
        Label blueProgressLabel         = blueEXPBar.Q<Label>("CurrentProgress");

        Vector2 blueLeftOrigin          = new Vector2(blueEXPBar.WorldToLocal(blueCurrentLabel.worldBound.center).x, -10f);
        Vector2 blueRightOrigin         = new Vector2(blueEXPBar.WorldToLocal(blueNextLabel.worldBound.center).x, -10f);

        blueCurrentLabel.text           = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLUE).ToString();
        blueNextLabel.text              = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLUE) + 1).ToString();
        blueProgressLabel.text          = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.BLUE).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLUE)).ToString();

        UIToolkitCircle blueLeftDot     = new UIToolkitCircle(blueLeftOrigin, 35f, Color.blue);
        UIToolkitCircle blueRightDot    = new UIToolkitCircle(blueRightOrigin, 35f, Color.blue);

        float blueDotDistance           = blueRightOrigin.x - blueLeftOrigin.x;
        Vector2 blueProgressStop        = new Vector2(
                                            blueLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.BLUE) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLUE))
                                                * blueDotDistance)
                                            , blueLeftOrigin.y);

        List<Vector2> blueProgPoints    = new List<Vector2>()
                                            { blueLeftOrigin, blueProgressStop };

        UIToolkitLine blueProgLine      = new UIToolkitLine(blueProgPoints, 30f, Color.blue, LineCap.Round);

        blueEXPBar.Add(blueLeftDot);
        blueEXPBar.Add(blueRightDot);
        blueEXPBar.Add(blueProgLine);

        VisualElement greenEXPBar       = uiDoc.rootVisualElement.Q<VisualElement>("GreenEXP").Q<VisualElement>("BG");
        Label greenCurrentLabel         = greenEXPBar.Q<Label>("CurrentLevel");
        Label greenNextLabel            = greenEXPBar.Q<Label>("NextLevel");
        Label greenProgressLabel        = greenEXPBar.Q<Label>("CurrentProgress");

        Vector2 greenLeftOrigin         = new Vector2(greenEXPBar.WorldToLocal(greenCurrentLabel.worldBound.center).x, -10f);
        Vector2 greenRightOrigin        = new Vector2(greenEXPBar.WorldToLocal(greenNextLabel.worldBound.center).x, -10f);

        greenCurrentLabel.text          = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.GREEN).ToString();
        greenNextLabel.text             = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.GREEN) + 1).ToString();
        greenProgressLabel.text         = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.GREEN).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.GREEN)).ToString();

        UIToolkitCircle greenLeftDot    = new UIToolkitCircle(greenLeftOrigin, 35f, Color.green);
        UIToolkitCircle greenRightDot   = new UIToolkitCircle(greenRightOrigin, 35f, Color.green);

        float greenDotDistance          = greenRightOrigin.x - greenLeftOrigin.x;
        Vector2 greenProgressStop       = new Vector2(
                                            greenLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.GREEN) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.GREEN))
                                                * greenDotDistance)
                                            , greenLeftOrigin.y);

        List<Vector2> greenProgPoints   = new List<Vector2>()
                                            { greenLeftOrigin, greenProgressStop };

        UIToolkitLine greenProgLine     = new UIToolkitLine(greenProgPoints, 30f, Color.green, LineCap.Round);

        greenEXPBar.Add(greenLeftDot);
        greenEXPBar.Add(greenRightDot);
        greenEXPBar.Add(greenProgLine);

        VisualElement yellowEXPBar      = uiDoc.rootVisualElement.Q<VisualElement>("YellowEXP").Q<VisualElement>("BG");
        Label yellowCurrentLabel        = yellowEXPBar.Q<Label>("CurrentLevel");
        Label yellowNextLabel           = yellowEXPBar.Q<Label>("NextLevel");
        Label yellowProgressLabel       = yellowEXPBar.Q<Label>("CurrentProgress");

        Vector2 yellowLeftOrigin        = new Vector2(yellowEXPBar.WorldToLocal(yellowCurrentLabel.worldBound.center).x, -10f);
        Vector2 yellowRightOrigin       = new Vector2(yellowEXPBar.WorldToLocal(yellowNextLabel.worldBound.center).x, -10f);

        yellowCurrentLabel.text         = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.YELLOW).ToString();
        yellowNextLabel.text            = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.YELLOW) + 1).ToString();
        yellowProgressLabel.text        = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.YELLOW).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.YELLOW)).ToString();

        UIToolkitCircle yellowLeftDot   = new UIToolkitCircle(yellowLeftOrigin, 35f, Color.yellow);
        UIToolkitCircle yellowRightDot  = new UIToolkitCircle(yellowRightOrigin, 35f, Color.yellow);

        float yellowDotDistance         = yellowRightOrigin.x - yellowLeftOrigin.x;
        Vector2 yellowProgressStop      = new Vector2(
                                            yellowLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.YELLOW) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.YELLOW))
                                                * yellowDotDistance)
                                            , yellowLeftOrigin.y);

        List<Vector2> yellowProgPoints  = new List<Vector2>()
                                            { yellowLeftOrigin, yellowProgressStop };

        UIToolkitLine yellowProgLine    = new UIToolkitLine(yellowProgPoints, 30f, Color.yellow, LineCap.Round);

        yellowEXPBar.Add(yellowLeftDot);
        yellowEXPBar.Add(yellowRightDot);
        yellowEXPBar.Add(yellowProgLine);

        VisualElement orangeEXPBar      = uiDoc.rootVisualElement.Q<VisualElement>("OrangeEXP").Q<VisualElement>("BG");
        Label orangeCurrentLabel        = orangeEXPBar.Q<Label>("CurrentLevel");
        Label orangeNextLabel           = orangeEXPBar.Q<Label>("NextLevel");
        Label orangeProgressLabel       = orangeEXPBar.Q<Label>("CurrentProgress");

        Vector2 orangeLeftOrigin        = new Vector2(orangeEXPBar.WorldToLocal(orangeCurrentLabel.worldBound.center).x, -10f);
        Vector2 orangeRightOrigin       = new Vector2(orangeEXPBar.WorldToLocal(orangeNextLabel.worldBound.center).x, -10f);

        orangeCurrentLabel.text         = ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.ORANGE).ToString();
        orangeNextLabel.text            = (ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.ORANGE) + 1).ToString();
        orangeProgressLabel.text        = ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.ORANGE).ToString() + " / "
                                        + ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.ORANGE)).ToString();

        UIToolkitCircle orangeLeftDot   = new UIToolkitCircle(orangeLeftOrigin, 35f, Color.magenta);
        UIToolkitCircle orangeRightDot  = new UIToolkitCircle(orangeRightOrigin, 35f, Color.magenta);

        float orangeDotDistance         = orangeRightOrigin.x - orangeLeftOrigin.x;
        Vector2 orangeProgressStop      = new Vector2(
                                            orangeLeftOrigin.x + (
                                                (float)ProfileManager.instance.GetCurrentEXP(ProfileManager.EXPColor.ORANGE) /
                                                (float)ProfileManager.instance.GetNeededEXP(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.ORANGE))
                                                * orangeDotDistance)
                                            , orangeLeftOrigin.y);

        List<Vector2> orangeProgPoints  = new List<Vector2>()
                                            { orangeLeftOrigin, orangeProgressStop };

        UIToolkitLine orangeProgLine    = new UIToolkitLine(orangeProgPoints, 30f, Color.magenta, LineCap.Round);

        orangeEXPBar.Add(orangeLeftDot);
        orangeEXPBar.Add(orangeRightDot);
        orangeEXPBar.Add(orangeProgLine);
    }

    private void DrawEXPGraph()
    {
        VisualElement lineChart     = uiDoc.rootVisualElement.Q<VisualElement>("LineGraphContainer").Q<VisualElement>("BG");
        float lineChartHeight       = lineChart.resolvedStyle.height;

        float minYValue             = lineChartHeight / 2f - 100f;
        float maxYValue             = lineChartHeight / 2f - 700f; //These values are from the chart's lines' position.bottom values
        float chartYRange           = minYValue - maxYValue;

        Debug.Log(string.Format("Min Y: {0} | Max Y: {1} | Diff: {2}", minYValue, maxYValue, chartYRange));

        int maxLineValue            = Mathf.Max(ProfileManager.instance.HighestEXPColorLevel, 10);
        int increment               = maxLineValue / 5;

        lineChart.Q<VisualElement>("Row1").Q<Label>().text = increment.ToString();
        lineChart.Q<VisualElement>("Row2").Q<Label>().text = (increment * 2).ToString();
        lineChart.Q<VisualElement>("Row3").Q<Label>().text = (increment * 3).ToString();
        lineChart.Q<VisualElement>("Row4").Q<Label>().text = (increment * 4).ToString();
        lineChart.Q<VisualElement>("Row5").Q<Label>().text = (increment * 5).ToString();

        VisualElement bwCol         = lineChart.Q<VisualElement>("MarkingLine1");
        Vector2 bwOrigin            = new Vector2(lineChart.WorldToLocal(bwCol.worldBound.center).x
                                                 , lineChartHeight / 2f - 100f - (chartYRange * ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE) / maxLineValue));
        UIToolkitCircle bwCircle    = new UIToolkitCircle(bwOrigin, 30f, Color.black); //TODO: Use UIManager colors
        Label bwLabel = new Label(ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.BLACK_AND_WHITE).ToString());
        bwLabel.AddToClassList("HeaderLabel");
        bwLabel.style.position = Position.Absolute;
        bwLabel.style.fontSize = 20f;
        bwLabel.style.color = Color.white;
        bwLabel.style.alignSelf = Align.Center;
        bwCircle.Add(bwLabel);

        VisualElement redCol        = lineChart.Q<VisualElement>("MarkingLine2");
        Vector2 redOrigin           = new Vector2(lineChart.WorldToLocal(redCol.worldBound.center).x
                                                 , lineChartHeight / 2f - 100f - (chartYRange * ProfileManager.instance.GetEXPLevel(ProfileManager.EXPColor.RED) / maxLineValue));
        UIToolkitCircle redCircle   = new UIToolkitCircle(redOrigin, 30f, Color.red);

        lineChart.Add(bwCircle);
        lineChart.Add(redCircle);
    }

    #endregion
}
