using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ENUMS;

public class EventCardDisplay : MonoBehaviour
{

    private EventCard eventCardData;

    public EventCard EventCardData
    {
        get { return eventCardData; }
        set { eventCardData = value; UpdateData();}
    }

    public Image border;
    public Image artwork;
    public TextMeshProUGUI eventName;
    public TextMeshProUGUI eventText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateData();
    }

    private void UpdateData()
    {
        eventName.text = EventCardData.eventName;
        eventText.text = EventCardData.eventText;
        artwork.sprite = EventCardData.mainArtwork;
    }
}
