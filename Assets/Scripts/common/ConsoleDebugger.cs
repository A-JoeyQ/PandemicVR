using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleDebugger : MonoBehaviour
{

    public static void Log(string message)
    {
        Debug.Log("[ConsoleDebugger] " + message);
    }
}
