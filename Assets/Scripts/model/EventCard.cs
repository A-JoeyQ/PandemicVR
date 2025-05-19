using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ENUMS;

[CreateAssetMenu(fileName = "New Event Card", menuName = "Cards/Event Card")]
public class EventCard : ScriptableObject
{
    public int eventID;
    public string eventName;
    public string eventText;

    public Sprite mainArtwork;
}
