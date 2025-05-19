using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube3D : MonoBehaviour
{
    public VirusInfo virusInfo;

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Renderer>().material.color = virusInfo.virusColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
