using DG.Tweening;
using static Game;
using static AnimationTemplates;

public class PMobileHospitalCardPlayed : PlayerEvent
{ 
    public PMobileHospitalCardPlayed(Player player) : base(player)
    {
    }

    public override void Do(Timeline timeline)
    {
        theGame.MobileHospitalPlayer = _player;
        /*theGame.ChangeToInEvent(EventState.NOTINEVENT);*/
        _player.playerGui.ChangeToInEvent(EventState.NOTINEVENT);
        _player.RemoveCardInHand(MOBILEHOSPITALINDEX, true);
    }

    public override float Act(bool qUndo = false)
    {
        _playerGui.EnableContextButtons(false, false, false, false, false, false);
        Sequence sequence = HighlightCardAndMove(_playerGui.GetCardInHand(MOBILEHOSPITALINDEX), gameGUI.PlayerDeckDiscard.transform, 3f, ANIMATIONDURATION);
        sequence.onComplete += () =>
        {
            gameGUI.Draw();
            
        };

        return base.Act(qUndo);
    }

    public override string GetLogInfo()
    {
        return $@" ""eventInitiator"" : ""{theGame.MobileHospitalPlayer.Role}"",
                    ""eventTarget"" : ""{theGame.CurrentPlayer.Role}""
                ";
    }
}