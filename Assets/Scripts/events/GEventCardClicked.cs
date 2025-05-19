public class GEventCardClicked : GuiEvent
{
    private EventCard _eventSelected;

    public GEventCardClicked(EventCard eventSelected)
    {
        _eventSelected = eventSelected;
    }

    public override void Do(Timeline timeline)
    {
        
    }

    public override string GetLogInfo()
    {
        return $@"""eventName"" : ""{_eventSelected.eventName}"",
                    ""eventID"" : ""{_eventSelected.eventID}""
                ";
    }
}
