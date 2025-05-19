using static ENUMS;
using static GameGUI;
using static Game;

public class EGameOver : EngineEvent, IIndirectEvent
{
    public GameOverReasons Reason;

    public EGameOver(GameOverReasons reason)
    {
        this.Reason = reason;
        theGame.SetCurrentGameState(GameState.GAME_OVER);
    }

    public override void Do(Timeline timeline)
    {


    }
    public override float Act(bool qUndo = false)
    {
        if (Reason == GameOverReasons.PlayersWon)
        {
            gui.GameEndWin.SetActive(true);
        }
        else
        {
            gui.GameEndLose.SetActive(true);
        }
        return 0;
    }

    public override string GetLogInfo()
    {
        return $@" ""reason"" : ""{Reason}""
                ";
    }
}


