using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfileCardController
{
    #region Private Variables

    private VisualElement   card;
    private VisualElement   profilePic;
    private Label           profileName;
    private Label           profileTitle;
    private Label           profileLevel;

    #endregion

    #region Constructor

    public ProfileCardController(VisualElement card)
    {
        this.card           = card;
        profilePic          = card.Q<VisualElement>("ProfileIcon");
        profileName         = card.Q<Label>("ProfileName");
        profileTitle        = card.Q<Label>("ProfileTitle");
        profileLevel        = card.Q<Label>("ProfileLevel");

        profileLevel.text   = string.Format("LVL - {0}", ProfileManager.instance.TotalLevel.ToString());

        //TODO: Register listener for levelup if there will be any places that the player
        //      can level up with the profile card visible
    }

    #endregion
}
