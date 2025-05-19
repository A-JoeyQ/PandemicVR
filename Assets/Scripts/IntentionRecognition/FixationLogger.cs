using System;
using System.IO;
using UnityEngine;
public class FixationLogger
{
    private static string fileName = $"fixations_{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.json";
    private static string filePath = "";

    public FixationLogger()
    {
        if (filePath == "")
        {
            string fixationFolderPath;

            if (Application.isEditor) // Dev mode
                fixationFolderPath = Path.Combine(Application.dataPath, "Fixations");
            else // Prod (build) mode
                fixationFolderPath = Path.Combine(Application.persistentDataPath, "Fixations");
        
            Debug.Log(fixationFolderPath);
            if (!Directory.Exists(fixationFolderPath))
            {
                Directory.CreateDirectory(fixationFolderPath);
            }
            filePath = Path.Combine(fixationFolderPath, fileName);
        }
    }

    public void LogFixation(Fixation fixation)
    {
        string logs = fixation.GetFixationLog();
        try
        {
            
            using (StreamWriter writer = File.AppendText(filePath))
            {
                writer.WriteLine(logs);
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"LOG: Error writing to log file: {ex.Message}");
        }
    }
}

