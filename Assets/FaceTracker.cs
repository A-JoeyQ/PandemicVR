using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Oculus.Avatar2;

public class FaceTrackingLogger : MonoBehaviour
{
    [Serializable]
    public class ExpressionWeight
    {
        public string name;
        public float value;
    }

    [Serializable]
    public class FaceDataPoint
    {
        public float timeStamp;
        public List<ExpressionWeight> expressionWeights = new List<ExpressionWeight>();
    }

    private List<FaceDataPoint> dataPoints = new List<FaceDataPoint>();
    private OVRFaceExpressions faceExpressions;
    private string filePath;
    private float interval = 0.1f;
    private float timer = 0f;

    private bool warnedNoFaceExpressions = false;
    private bool warnedTrackingDisabled = false;
    private bool warnedInvalidExpressions = false;
    void Start()
    {
        faceExpressions = GetComponent<OVRFaceExpressions>();

        string folderPath = Path.Combine(Application.dataPath, "FaceTracker");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filename = "FaceTracking_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        filePath = Path.Combine(folderPath, filename);
        Debug.Log($"[FaceTrackingLogger] Saving to: {filePath}");
    }

    void Update()
    {
        if (faceExpressions == null)
        {
            if (!warnedNoFaceExpressions)
            {
                Debug.LogWarning("FaceExpressions is null");
                warnedNoFaceExpressions = true;
            }
            return;
        }

        if (!faceExpressions.FaceTrackingEnabled)
        {
            if (!warnedTrackingDisabled)
            {
                Debug.LogWarning("Face Tracking not enabled");
                warnedTrackingDisabled = true;
            }
            return;
        }

        if (!faceExpressions.ValidExpressions)
        {
            if (!warnedInvalidExpressions)
            {
                Debug.LogWarning("Face Expressions invalid");
                warnedInvalidExpressions = true;
            }
            return;
        }

        // 重置标志位（因为当前一切正常）
        warnedNoFaceExpressions = false;
        warnedTrackingDisabled = false;
        warnedInvalidExpressions = false;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            RecordDataPoint();
        }
    }
    /*  void Update()
      {

          if (faceExpressions == null)
              {
                  Debug.LogWarning("FaceExpressions is null");
                  return;
              }


          if (!faceExpressions.FaceTrackingEnabled)
              {

                  Debug.LogWarning("Face Tracking not enabled");
                  return;

              }

          if (!faceExpressions.ValidExpressions)
          {
              Debug.LogWarning("Face Expressions invalid");
              return;
          }

          timer += Time.deltaTime;
          if (timer >= interval)
          {
              timer = 0f;
              RecordDataPoint();
          }
          // Tracking is valid here
      }*/

    void RecordDataPoint()
    {
        var point = new FaceDataPoint
        {
            timeStamp = Time.time
        };

        foreach (OVRFaceExpressions.FaceExpression expr in Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression)))
        {
            if (expr == OVRFaceExpressions.FaceExpression.Invalid || expr == OVRFaceExpressions.FaceExpression.Max)
                continue;

            float value = faceExpressions[expr];
            point.expressionWeights.Add(new ExpressionWeight
            {
                name = expr.ToString(),
                value = value
            });
        }

        dataPoints.Add(point);
    }

    void OnApplicationQuit()
    {
        SaveToFile();
        //SaveToCsv();
    }

    void SaveToFile()
    {
        string json = JsonHelper.ToJson(dataPoints.ToArray(), true);
        File.WriteAllText(filePath, json);
        Debug.Log($"[FaceTrackingLogger] Data saved: {filePath}");
    }

    /* void SaveToCsv()
     {
         string csvPath = filePath.Replace(".json", ".csv");

         if (dataPoints.Count == 0 || dataPoints[0].expressionWeights.Count == 0)
         {
             Debug.LogWarning("[FaceTrackingLogger] No data to save to CSV.");
             return;
         }

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

         Debug.Log($"[FaceTrackingLogger] CSV saved in matrix format: {csvPath}");
     }*/


    /* void SaveToCsv()
     {
         string csvPath = filePath.Replace(".json", ".csv");

         using (StreamWriter writer = new StreamWriter(csvPath))
         {

             writer.Write("timeStamp,expressionName,value\n");

             foreach (var point in dataPoints)
             {
                 foreach (var expr in point.expressionWeights)
                 {
                     writer.WriteLine($"{point.timeStamp},{expr.name},{expr.value}");
                 }
             }
         }

         Debug.Log($"[FaceTrackingLogger] CSV saved: {csvPath}");
     }*/
}
