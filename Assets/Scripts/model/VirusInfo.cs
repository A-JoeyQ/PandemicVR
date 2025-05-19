using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

[CreateAssetMenu(fileName = "New Virus Icon", menuName = "Icons/Virus Icon")]
public class VirusInfo : ScriptableObject
{
    public VirusName virusName;
    public Color virusColor;

    public Sprite artwork;
}
