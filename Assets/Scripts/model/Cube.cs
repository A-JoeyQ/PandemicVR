using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cube : MonoBehaviour
{
    public VirusInfo virusInfo;
    public CityCard cityCard = null;

    // Start is called before the first frame update
    void Start()
    {
        Image image = this.GetComponent<Image>();
        if(image == null)
        {
            MeshRenderer meshRenderer = this.GetComponentInChildren<MeshRenderer>();
            meshRenderer.material.color = virusInfo.virusColor;
        }else{
            image.color = virusInfo.virusColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
