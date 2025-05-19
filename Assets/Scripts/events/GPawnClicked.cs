public class GPawnClicked : GuiEvent
{
    private Pawn _playerSelected;

    public GPawnClicked(Pawn pawn)
    {
        _playerSelected = pawn;
    }

    public override void Do(Timeline timeline)
    {
        
    }

    public override string GetLogInfo()
    {
        return $@"""playerSelected"" : ""{_playerSelected.PlayerModel.Role}""
                ";
    }
}
