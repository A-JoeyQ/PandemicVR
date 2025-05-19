// File: Assets/Scripts/New/JsonToCsvConverter.cs

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class JsonToCsvConverter : EditorWindow
{
    [MenuItem("Tools/Convert Face JSON to CSV")]
    public static void ConvertJsonToCsv()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select FaceTracking JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(jsonPath))
            return;

        string json = File.ReadAllText(jsonPath);
        FaceTrackingLogger.FaceDataPoint[] dataPoints = JsonHelper.FromJson<FaceTrackingLogger.FaceDataPoint>(json);

        if (dataPoints.Length == 0 || dataPoints[0].expressionWeights.Count == 0)
        {
            Debug.LogWarning("No data in JSON.");
            return;
        }

        string csvPath = jsonPath.Replace(".json", "_converted.csv");

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            writer.Write("timeStamp");
            foreach (var expr in dataPoints[0].expressionWeights)
            {
                writer.Write($",{expr.name}");
            }
            writer.WriteLine();

            foreach (var point in dataPoints)
            {
                writer.Write($"{point.timeStamp}");
                foreach (var expr in point.expressionWeights)
                {
                    writer.Write($",{expr.value}");
                }
                writer.WriteLine();
            }
        }

        Debug.Log($"CSV saved to: {csvPath}");
    }
}
