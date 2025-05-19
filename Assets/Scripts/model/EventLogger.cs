using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public abstract class EventLogger
{
    public string GetEventLog(TimelineEvent timelineEvent)
    {
        string currentPlayerLog = Game.theGame.CurrentPlayer != null
            ? $@"                   ""currentPlayer"" : {{
                            ""role"" : ""{Game.theGame.CurrentPlayer.Role}"",
                            ""name"" : ""{Game.theGame.CurrentPlayer.Name}"",
                            ""currentCityID"" : {Game.theGame.CurrentPlayer.GetCurrentCity()},
                            ""currentCityName"" : ""{Game.theGame.CurrentPlayer.GetCurrentCityScript().city.cityName}""
                        }}"
            : null;
        
        string commonLog = 
            $@"""timestamp"" : ""{Time.time - MainMenu.startTimestamp}"",
                    ""eventType"" : ""{timelineEvent.GetType()}""{(currentPlayerLog != null ? $", \n {currentPlayerLog}" : "")}";
        
        
        string eventLog = timelineEvent.GetLogInfo();
        
        return
            $@"{{
                    {commonLog}{(eventLog != null ? $",\n\t\t\t\t   {eventLog}" : "")}
            }}";
    }

    public abstract void BroadcastLogs(TimelineEvent timelineEvent);
    
}

public class FileLogger : EventLogger, IDisposable
{
    private static string fileName = $"log_{DateTime.Now.ToString("ddMMyyyy_HHmmss")}.json";
    private static string filePath = "";
    private static bool isFirstWrite = true;
    private static bool isDisposed = false;
    
    public FileLogger()
    {
        if (filePath == "")
        {
            string logsFolderPath;

            if (Application.isEditor) // Dev mode
                logsFolderPath = Path.Combine(Application.dataPath, "Logs");
            else // Prod (build) mode
                logsFolderPath = Path.Combine(Application.persistentDataPath, "Logs");
        
            if (!Directory.Exists(logsFolderPath))
            {
                Directory.CreateDirectory(logsFolderPath);
            }
            filePath = Path.Combine(logsFolderPath, fileName);
        }
        File.WriteAllText(filePath, "[\n]");
    }
    
    public override void BroadcastLogs(TimelineEvent timelineEvent)
    {
        if (isDisposed) return;

        string logs = GetEventLog(timelineEvent);
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Seek(-2, SeekOrigin.End); // Move to just before the closing bracket
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    if (!isFirstWrite)
                    {
                        writer.Write(",\n");
                    }
                    else
                    {
                        isFirstWrite = false;
                    }
                    writer.Write(logs);
                    writer.Write("\n]");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LOG: Error writing to log file: {ex.Message}");
        }
    }

    public void FormatJsonFile()
    {
        if (isDisposed) return;

        try
        {
            string jsonContent = File.ReadAllText(filePath);
            JArray jsonArray = JArray.Parse(jsonContent);
            string formattedJson = jsonArray.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, formattedJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"LOG: Error formatting JSON file: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            FormatJsonFile();
            isDisposed = true;
        }
    }
}

public class NetworkLogger : EventLogger
{
    public override void BroadcastLogs(TimelineEvent timelineEvent)
    {
        //TODO: to be implemented
    }
}

