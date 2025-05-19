using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;
public class AoIBoard : AreaOfInterest
{
    void Start()
    {
    }

    public override string GetAoILog(){
        return "[Board]";
    }
    public override AoIType GetAoIType(){
        return AoIType.Board;
    }

    public override string GetJsonLog(){
        return $@" 
                    ""type"" : ""Board""
                ";
    }
}