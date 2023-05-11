using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TimedModeCard
{
    #region Private Variables

    private LevelCategory.TimeAttackStats settings;
    private VisualElement card;

    #endregion

    #region Constructor

    public TimedModeCard(LevelCategory.TimeAttackStats settings, VisualElement root)
    {
        this.settings   = settings;
        card            = root;

        Setup();
    }

    #endregion

    #region Private Functions

    private void Setup()
    {
        Label modeName      = card.Q<Label>("DifficultyLabel");
        Label puzzleCount   = card.Q<Label>("PuzzleCount");
        Label startingTime  = card.Q<Label>("StartingTime");
        Label bonus         = card.Q<Label>("CompletionBonus");
        Label bronzeTime    = card.Q<VisualElement>("BronzeAwardContainer").Q<Label>();
        Label silverTime    = card.Q<VisualElement>("SilverAwardContainer").Q<Label>();
        Label goldTime      = card.Q<VisualElement>("GoldAwardContainer").Q<Label>();
        Label starTime      = card.Q<VisualElement>("StarAwardContainer").Q<Label>();
        Label bestTime      = card.Q<Label>("BestTime");
        VisualElement best  = card.Q<VisualElement>("LeftPanel").Q<VisualElement>("Icon");

        modeName.text       = settings.difficulty;
        puzzleCount.text    = string.Format(" - {0} Puzzles", settings.numberOfPuzzles.ToString());

        startingTime.text   = TimeSpan.FromSeconds(settings.totalTimeInSeconds).ToString("mm\\:ss");
        bonus.text          = string.Format("{0}s / Puzzle", settings.timeAddedOnCompletePuzzle.ToString("+0;-#"));
        
        bronzeTime.text     = TimeSpan.FromSeconds(settings.bronzeTimeInSeconds).ToString("mm\\:ss");
        silverTime.text     = TimeSpan.FromSeconds(settings.silverTimeInSeconds).ToString("mm\\:ss");
        goldTime.text       = TimeSpan.FromSeconds(settings.goldTimeInSeconds).ToString("mm\\:ss");
        starTime.text       = TimeSpan.FromSeconds(settings.starTimeInSeconds).ToString("mm\\:ss");

        bestTime.text       = TimeSpan.FromSeconds(settings.bestTimeInSeconds).ToString("mm\\:ss\\.fff");
        best.style
            .backgroundImage= settings.GetHighestMedal();
    }

    #endregion
}
