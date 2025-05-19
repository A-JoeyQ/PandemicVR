using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

public abstract class AreaOfInterest : MonoBehaviour
{
    public abstract string GetAoILog();
    public abstract AoIType GetAoIType();

    public abstract string GetJsonLog();
}
