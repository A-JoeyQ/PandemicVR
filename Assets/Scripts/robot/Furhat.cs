using System.Collections.Generic;
using UnityEngine;
using TCPFurhatComm;

public class Furhat
{
    private FurhatInterface furhatInterface ;
    private string furhatIP;
    private string speakerName;

    private Dictionary<string, Vector3> playerLocations = new Dictionary<string, Vector3>();

    public Furhat(string furhatIP, string speakerName)
    {
        this.furhatIP = furhatIP;
        this.speakerName = speakerName;
        furhatInterface = new FurhatInterface(furhatIP);
        //furhatInterface.ChangeVoice(VOICES.EN_US_AMAZON_MATTHEW);
        //Say(Dialog.DialogEvents[0].Lines[0].Speech);
    }

    public void Say(string speech)
    {
        furhatInterface.Say(speech, abort:true);
    }

    public void Gesture(string gesture)
    {
        furhatInterface.Gesture(gesture);
    }

    public void Gaze(double locx, double locy, double locz)
    {
        furhatInterface.Gaze(locx, locy, locz);
    }
    
}

