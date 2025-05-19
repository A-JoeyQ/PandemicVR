using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

public class AoIInfectionCard : AreaOfInterest
{

    private InfectionCardDisplay infectionCard;

    public override string GetAoILog()
    {
        infectionCard = GetComponent<InfectionCardDisplay>();
        return "[InfectionCard]" + infectionCard.cityName.text;
    }
    public override AoIType GetAoIType()
    {
        return AoIType.InfectionCard;
    }

    public override string GetJsonLog()
    {
        infectionCard = GetComponent<InfectionCardDisplay>();
        return $@" 
                    ""type"" : ""InfectionCard"",
                    ""city"" : ""{infectionCard.cityName.text}""
                ";

    }
}
