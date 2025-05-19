using static Game;

public class PEndTurn : PlayerEvent, IIndirectEvent
{
    float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;

    public PEndTurn() : base(Game.theGame.CurrentPlayer)
    {
    }

    public override void Do(Timeline timeline)
    {
        theGame.CurrentPlayer.playerGui.ChangeToInEvent(EventState.NOTINEVENT);
        theGame.CurrentPlayer = PlayerList.nextPlayer(_player);
        theGame.CurrentPlayer.ResetTurn();
        theGame.SetCurrentGameState(GameState.PLAYERACTIONS);
    }

    public override float Act(bool qUndo = false)
    {
        PlayerGUI nextPlayerGUI = GameGUI.CurrentPlayerPad();
        nextPlayerGUI.Draw();
        _playerGui.Draw();
        gameGUI.Draw();
        return 0;
    }

    
}