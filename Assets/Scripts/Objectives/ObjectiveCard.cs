using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectiveCard
{
    #region Private Variables

    private Objective       objective;
    private VisualElement   card;

    #endregion

    #region Public Properties

    public Objective        Objective { get { return objective; } }

    #endregion

    #region Constructor

    public ObjectiveCard(Objective objective, VisualElement root)
    {
        this.objective  = objective;
        card            = root;

        Setup();
    }

    #endregion

    #region Private Functions

    private void Setup()
    {
        Label description                       = card.Q<Label>("Description");
        VisualElement icon                      = card.Q<VisualElement>("Icon");
        VisualElement isCompleteIcon            = card.Q<VisualElement>("IsCompleteIcon");
        VisualElement progressBarWidth          = card.Q<VisualElement>("Mask");
        VisualElement progressBarFill           = progressBarWidth.Q<VisualElement>("Fill");
        Label progressBarLabel                  = card.Q<Label>("ProgressBarLabel");

        description.text                        = objective.Description;
        isCompleteIcon                          .Show(objective.IsComplete);
        icon.style.backgroundImage              = new StyleBackground(objective.Icon);
        icon.parent.SetColor(objective.ProgressBarColor);

        progressBarFill.SetColor(objective.ProgressBarColor);
        progressBarWidth.SetWidth(new StyleLength(new Length(objective.GetProgressAsPercentage(), LengthUnit.Percent)));
        progressBarLabel.text                   = string.Format(objective.GetProgressAsString());
    }

    #endregion
}
