using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
public class AoICube : AreaOfInterest
{
    private Cube cube;
    public override string GetAoILog()
    {
        cube = GetComponent<Cube>();
        if (cube.cityCard == null)
        {
            return "[Cube]" + cube.virusInfo.virusName;
        }
        return "[Cube]" + cube.virusInfo.virusName + ",(" + cube.cityCard.cityName + ")";
    }
    public override AoIType GetAoIType()
    {
        return AoIType.Cube;
    }

    public override string GetJsonLog()
    {
        cube = GetComponent<Cube>();
        if (cube.cityCard == null)
        {
            return $@" 
                    ""type"" : ""Cube"",
                    ""virusName"" : ""Not in city"",
                    ""city"" : ""Not in city""
                ";
        }
        return $@" 
                    ""type"" : ""Cube"",
                    ""virusName"" : ""{cube.virusInfo.virusName}"",
                    ""city"" : ""{cube.cityCard.cityName}""
                ";
    }
}
