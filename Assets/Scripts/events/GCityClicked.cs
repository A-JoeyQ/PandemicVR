public class GCityClicked : GuiEvent
{
    private City _selectedCity;

    public GCityClicked(City city)
    {
        _selectedCity = city;
    }

    public override void Do(Timeline timeline)
    {
        
    }

    public override string GetLogInfo()
    {
        return $@"""cityID"" : {_selectedCity.city.cityID}
                ";
    }
}
