using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using static GameGUI;
using static Game;

// Game flow:
// EInitialize
// EStartRound
// EEndGame
public class EStartRound : EngineEvent, IIndirectEvent
{
    float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;

    public override void Do(Timeline timeline)
    {
        theGame.CurrentPlayer = PlayerList.Players[0];
        theGame.SetCurrentGameState(GameState.PLAYERACTIONS);
    }
    public override float Act(bool qUndo = false)
    {
        // clear the load overlay
        //GameGUI.theGameGUI.LoadOverlay.SetActive(false);
        //GameGUI.theGameGUI.GameCanvas.SetActive(true);



        return 0;
    }
}

public class EEndGame : EngineEvent
{
    public override void Do(Timeline timeline)
    {
        theGame.SetCurrentGameState(GameState.GAME_OVER);
    }
    public override float Act(bool qUndo = false)
    {
        //WindowsVoice.speak("Game Over.");
        gui.Draw();
        return 0;
    }
}