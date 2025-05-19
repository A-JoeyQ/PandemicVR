public class GContextButtonClicked : GuiEvent
{
    private string _contextButtonSelected;

    public GContextButtonClicked(string contextButtonSelected)
    {
        _contextButtonSelected = contextButtonSelected;
    }

    public override void Do(Timeline timeline)
    {
        
    }

    public override string GetLogInfo()
    {
        return $@"""contextButtonSelected"" : ""{_contextButtonSelected}""
                ";
    }
}
