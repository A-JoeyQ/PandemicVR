using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ENUMS;

public class CityCardDisplay : MonoBehaviour
{

    private CityCard cityCardData;

    public CityCard CityCardData
    {
        get { return cityCardData; }
        set { cityCardData = value; UpdateData(); }
    }

    public Image backgroundColor;
    public Image backgroundWhite;
    public Image virusTop;
    public Image virusBottom;

    public Image artwork;
    public Image flag;
    public Image border;

    public TextMeshProUGUI population;
    public TextMeshProUGUI cityName;
    public TextMeshProUGUI countryName;


    // Start is called before the first frame update
    void Start()
    {
        UpdateData();
    }

    private void UpdateData()
    {
        population.text = cityCardData.population;
        cityName.text = cityCardData.cityName;
        countryName.text = cityCardData.countryName;
        artwork.sprite = cityCardData.mainArtwork;
        flag.sprite = cityCardData.flagArtwork;
        virusTop.sprite = cityCardData.virusInfo.artwork;
        virusBottom.sprite = cityCardData.virusInfo.artwork;
        backgroundColor.color = cityCardData.virusInfo.virusColor;
    }
}
