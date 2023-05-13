using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class PowerupController
{
    #region Private Variables

    private VisualElement root;

    private VisualElement hintButton;
    private VisualElement removeSpecialTilesButton;
    private VisualElement fillEmptyButton;

    private Label instructions;

    private Board currentBoard;

    private Dictionary<Level.LineDefinitions, bool> linesAvaiableForHint;

    #endregion

    #region Public Properties

    #endregion

    #region Constructor

    public PowerupController(VisualElement root, bool timedMode, Board initialBoard)
    {
        this.root = root;

        hintButton = this.root.Q<VisualElement>("HintButton");
        removeSpecialTilesButton = this.root.Q<VisualElement>("RemoveSpecialTileButton");
        fillEmptyButton = this.root.Q<VisualElement>("FillEmptyButton");

        PowerupButtonController hintControl = new PowerupButtonController(PowerupType.HINT, hintButton);
        PowerupButtonController removeSpecialControl = new PowerupButtonController(PowerupType.REMOVE_SPECIAL_TILE, removeSpecialTilesButton);
        PowerupButtonController fillControl = new PowerupButtonController(PowerupType.FILL_EMPTY, fillEmptyButton);

        hintButton.userData = hintControl;
        removeSpecialTilesButton.userData = removeSpecialControl;
        fillEmptyButton.userData = fillControl;

        instructions = root.Q<Label>("Instructions");
        instructions.Hide();

        hintButton.RegisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.HINT));
        removeSpecialTilesButton.RegisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.REMOVE_SPECIAL_TILE));
        fillEmptyButton.RegisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.FILL_EMPTY));

        this.AddObserver(PowerupUsed, Notifications.POWERUP_USED);
        this.AddObserver(LineCompletionChanged, Notifications.LINE_COMPLETED);

        SetBoard(initialBoard);
    }

    #endregion

    #region Public Functions

    public void SetBoard(Board board)
    {
        currentBoard = board;
        currentBoard.UsingPowerup = PowerupType.none;
        instructions.Hide();

        linesAvaiableForHint = new Dictionary<Level.LineDefinitions, bool>();

        for (int i = 0; i < currentBoard.LevelsLineDefs.Count; i++)
            linesAvaiableForHint.Add(currentBoard.LevelsLineDefs[i], true);
    }

    public void Unregister()
    {
        this.RemoveObserver(PowerupUsed, Notifications.POWERUP_USED);
        this.RemoveObserver(LineCompletionChanged, Notifications.LINE_COMPLETED);

        hintButton.UnregisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.HINT));
        removeSpecialTilesButton.UnregisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.REMOVE_SPECIAL_TILE));
        fillEmptyButton.UnregisterCallback<PointerUpEvent>((evt) => PowerupButtonClicked(evt, PowerupType.FILL_EMPTY));

        (hintButton.userData as PowerupButtonController).UnregisterListeners();
        (removeSpecialTilesButton.userData as PowerupButtonController).UnregisterListeners();
        (fillEmptyButton.userData as PowerupButtonController).UnregisterListeners();
    }

    #endregion

    #region Private Functions

    private void PowerupUsed(object sender, object info)
    {
        //info  -   PowerupType -   The type of powerup used

        instructions.Hide();
        currentBoard.UsingPowerup = PowerupType.none;
    }

    private void PowerupButtonClicked(PointerUpEvent evt, PowerupType type)
    {
        //If the player doesnt own any
        //Show store popup
        //else
        //No powerup is being used
        //A powerup is being used, and it's the same one (cancel)
        //A power is being used, and a diff one is clicked (change)

        if (currentBoard.UsingPowerup == PowerupType.none || currentBoard.UsingPowerup != type)
        {
            if (type.Instructions() != string.Empty)
            {
                instructions.Show();
                instructions.text = type.Instructions();
                currentBoard.UsingPowerup = type;
            }
            else
            {
                if (type == PowerupType.HINT)
                    UseHint();
            }
        }
        else if (currentBoard.UsingPowerup == type) //Cancel
        {
            instructions.Hide();
            currentBoard.UsingPowerup = PowerupType.none;
        }
    }

    private void UseHint()
    {
        //Each line will need a solution path
        //board needs a way to draw the line
        //      if another line is on the solution path, remove the full other line
        //      if it's not possible to draw the line, needs a "hint failed"/powerup needed response
        //          (for instance, if a "fill gap" powerup is needed to allow the line to be drawn)
        //Powerup controller will need to track which lines have been solutioned
        //Will need to check end of level after use

        //CheckCompletedLine ->
        //      this "hears" LINE_COMPLETE and removes the line from the linesAvailableForHint
        //      this "hears" LINE_INCOMPLETE and adds it back to the linesAvailbleForHints

        List<Level.LineDefinitions> possible = linesAvaiableForHint
                                                .Where(x => x.Value)
                                                .Select(x => x.Key)
                                                .ToList();

        bool completedHint = false;

        if (possible.Count == 0)
        {
            goto cantDo;
        }

        int startingIndex = Random.Range(0, possible.Count);
        int index = startingIndex;

        Debug.Log(index);
        Debug.Log(possible.Count);

        do
        {
            completedHint = currentBoard.DrawHintLine(possible[index]);

            index++;

            if (index >= possible.Count)
                index = 0;


            Debug.Log(string.Format("CompletedHint: {0} // index: {1} // starting index: {2} \n" +
                                    "!completedHint: {3} // index != startingIndex: {4}\n" +
                                    "(!comletedHint && index != startingIndex) {5}",
                                    completedHint, index, startingIndex, !completedHint, index != startingIndex,
                                    !completedHint && index != startingIndex));
        }
        while (!completedHint && index != startingIndex);

        cantDo:

        if (!completedHint)
        {
            instructions.text = "No lines can currently be completed. Try using powerups!";
            instructions.Show();
        }
    }

    private void LineCompletionChanged(object sender, object info)
    {
        //sender    -   Line    -   the Line which was checked for completion
        //info      -   bool    -   whether the line was completed or not

        Line line       = (Line)sender;
        bool completed  = (bool)info;

        Level.LineDefinitions def = currentBoard.LevelsLineDefs.Find(x => x.colorIndex == line.colorIndex);
        
        linesAvaiableForHint[def] = !completed;
    }

    #endregion
}
