using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class EyeTrackingLogger : MonoBehaviour
{
    private string filePath;
    private StreamWriter writer;
    private bool isLogging = false;
    private Queue<EyeTrackDataPoint> dataQueue = new Queue<EyeTrackDataPoint>();

    void Start()
    {
        string fileName = $"EyeTracking_{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.json";

        string logsFolderPath;

        if (Application.isEditor) // Dev mode
            logsFolderPath = Path.Combine(Application.dataPath, "EyeTrackData");
        else // Prod (build) mode
            logsFolderPath = Path.Combine(Application.persistentDataPath, "EyeTrackData");
    
        if (!Directory.Exists(logsFolderPath))
        {
            Directory.CreateDirectory(logsFolderPath);
        }
        Debug.Log("[EyeTrackingLogger] Folder Created: " + logsFolderPath);
        filePath = Path.Combine(logsFolderPath, fileName);
        Debug.Log("[EyeTrackingLogger] File Path: " + filePath);
        
        // Open the file for writing
        writer = new StreamWriter(filePath);
        isLogging = true;
    }

    void Update()
    {
        // Write data points from the queue to the file
        while (dataQueue.Count > 0)
        {
            var dataPoint = dataQueue.Dequeue();
            string logInfo = dataPoint.GetLogInfo();
            logInfo = logInfo + ",";
            Task.Run(() => writer.WriteLine(logInfo));
        }
    }

    public void Save(EyeTrackDataPoint dataPoint)
    {
        if (isLogging)
        {
            dataQueue.Enqueue(dataPoint);
        }
    }

    void OnApplicationQuit()
    {
        // Stop logging and close the file
        isLogging = false;
        if (writer != null)
        {
            writer.Close();
        }
    }
}

