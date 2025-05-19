using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ENUMS;

public class InfectionCardDisplay : MonoBehaviour
{
    
    private CityCard cityCardData;

    public CityCard CityCardData
    {
        get { return cityCardData; }
        set { cityCardData = value; UpdateData(); }
    }

    private void UpdateData()
    {
        cityName.text = cityCardData.cityName;
        artwork.sprite = cityCardData.mainArtwork;
        virus.sprite = cityCardData.virusInfo.artwork;
        background.color = cityCardData.virusInfo.virusColor;
    }

    public Image background;
    public Image virus;

    public Image artwork;

    public TextMeshProUGUI cityName;
    
    public GameObject border;

    // Start is called before the first frame update
    //void Start()
    //{
    //    cityName.text = cityCardData.cityName;
    //    artwork.sprite = cityCardData.mainArtwork;
    //    virus.sprite = cityCardData.virusInfo.artwork;
    //    background.color = cityCardData.virusInfo.virusColor;

    //}
}