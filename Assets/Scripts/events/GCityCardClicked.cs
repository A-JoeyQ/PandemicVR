public class GCityCardClicked : GuiEvent
{
    private CityCard _cardClicked;

    public GCityCardClicked(CityCard cityCard)
    {
        _cardClicked = cityCard;
    }
    
    public override void Do(Timeline timeline)
    {
        
    }
    
    public override string GetLogInfo()
    {
        return $@"""cityID"" : {_cardClicked.cityID}
                ";
    }
}