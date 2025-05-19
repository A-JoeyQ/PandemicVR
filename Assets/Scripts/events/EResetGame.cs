using UnityEngine;
using System.Collections.Generic;
using static GameGUI;
using static Game;

[System.Serializable]
public class EResetGame : EngineEvent
{
    public EResetGame()
    {
        QUndoable = false;
    }

    public override void Do(Timeline c)
    {
        theGame.init();
        theGame.CurrentPlayer = null;
        if (!Timeline.theTimeline.IsReplayMode)
            PlayerList.Players.Clear();
        foreach (PlayerGUI pad in gui.PlayerPads)
            pad.PlayerModel = null;
    }
    
}
