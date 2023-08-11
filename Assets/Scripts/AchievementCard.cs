using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AchievementCard
{
    #region Private Variables

    private Objective       achievement;
    private VisualElement   card;

    #endregion

    #region Constructor

    public AchievementCard(Objective achievement, VisualElement cardRoot)
    {
        this.achievement    = achievement;
        card                = cardRoot;

        Setup();
    }

    #endregion

    #region Private Functions

    private void Setup()
    {
        Label title                 = card.Q<Label>("Title");
        Label description           = card.Q<Label>("Description");
        Label progLabel             = card.Q<Label>("ProgressPercent");
        VisualElement icon          = card.Q<VisualElement>("ImageCircle");

        title.text                  = achievement.name;
        description.text            = achievement.Description;
        progLabel.text              = "80%";

        icon.SetImage(achievement.Icon);

        Vector2 origin              = card.WorldToLocal(icon.worldBound.center);
        UIToolkitCircle radial      = new UIToolkitCircle(origin, 100f, Color.green);

        card.Add(radial);
        radial.BringToFront();
    }

    #endregion
}
