using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
public class AoICity : AreaOfInterest
{

    private CityCard cityCard;
    private City city;

    public override string GetAoILog(){
        city = GetComponent<City>();
        if(city == null){
            return "[City]City not found";
        }
        cityCard = city.city;
        return "[City]"+cityCard.cityName + "," + cityCard.virusInfo.virusName;
    }
    public override AoIType GetAoIType(){
        return AoIType.City;
    }

    public override string GetJsonLog(){
        city = GetComponent<City>();
        if(city == null){
            return $@" 
                    ""type"" : ""City"",
                    ""city"" : ""City not found""
                ";
        }
        cityCard = city.city;
        return $@" 
                    ""type"" : ""City"",
                    ""city"" : ""{cityCard.cityName}"",
                    ""color"" : ""{cityCard.virusInfo.virusName}"",
                    ""cubesRed"" : {city.getNumberOfCubes(VirusName.Red)},
                    ""cubesYellow"" : {city.getNumberOfCubes(VirusName.Yellow)},
                    ""cubesBlue"" : {city.getNumberOfCubes(VirusName.Blue)}
                ";
    }
}
