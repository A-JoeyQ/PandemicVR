using UnityEngine;
using System.IO;
using System.Collections.Generic;
using common;
using Newtonsoft.Json;
    
public static class LogFileReader
{
    public static List<TimelineEvent> logs; // Combined list of all logs
    
    public static List<TimelineEvent> ReadLogs(string path)
    {
        try
        {
            Debug.Log("Loading logs from " + path);
            string json = File.ReadAllText(path);
            
            DialogManager.Clear();
            
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new TimelineEventConverter() }
            };
            
            // Deserialize all logs into a single list
            logs = JsonConvert.DeserializeObject<List<TimelineEvent>>(json, settings);

            return logs;
        }
        catch (JsonException e)
        {
            Debug.LogError("Error parsing JSON: " + e.Message);
            return new List<TimelineEvent>();
        }
    }
}
