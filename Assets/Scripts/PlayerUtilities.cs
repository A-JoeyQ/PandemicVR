using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUtilities : MonoBehaviour
{
    private static PlayerUtilities instance;
    public GameObject rightSyntheticHandVisual;
    public GameObject leftSyntheticHandVisual;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }
    public void MovePlayerTo(int position)
    {
        NetworkPlayer.LocalInstance.MoveToSeat(position);
    }

    public static void DisableSyntheticHands()
    {
        instance.rightSyntheticHandVisual.SetActive(false);
        instance.leftSyntheticHandVisual.SetActive(false);
    }
}
