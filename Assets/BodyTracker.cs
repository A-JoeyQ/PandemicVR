using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Oculus.Avatar2;

[RequireComponent(typeof(OVRBody))]
[RequireComponent(typeof(OVRSkeleton))]
public class BodyTrackingLogger : MonoBehaviour
{
    [Serializable]
    public class JointData
    {
        public string jointName;
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class BodyDataPoint
    {
        public float timeStamp;
        public List<JointData> joints = new List<JointData>();
    }

    private List<BodyDataPoint> dataPoints = new List<BodyDataPoint>();
    private OVRSkeleton skeleton;
    private string filePath;
    private float interval = 0.1f;
    private float timer = 0f;

    void Start()
    {
        skeleton = GetComponent<OVRSkeleton>();

        string folderPath = Path.Combine(Application.dataPath, "BodyTracker");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filename = "BodyTracking_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json";
        filePath = Path.Combine(folderPath, filename);
        Debug.Log($"[BodyTrackingLogger] Saving to: {filePath}");
    }

    void Update()
    {
        if (skeleton.Bones == null || !skeleton.IsDataValid || !skeleton.IsDataHighConfidence)
            return;

        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            RecordDataPoint();
        }
    }

    void RecordDataPoint()
    {
        var point = new BodyDataPoint
        {
            timeStamp = Time.time
        };

        foreach (var bone in skeleton.Bones)
        {
            JointData joint = new JointData
            {
                jointName = bone.Id.ToString(),
                position = bone.Transform.position,
                rotation = bone.Transform.rotation
            };
            point.joints.Add(joint);
        }

        dataPoints.Add(point);
    }

    void OnApplicationQuit()
    {
        SaveToFile();
        SaveToCsv();
    }

    void SaveToFile()
    {
        string json = JsonHelper.ToJson(dataPoints.ToArray(), true);
        File.WriteAllText(filePath, json);
        Debug.Log($"[BodyTrackingLogger] Data saved: {filePath}");
    }


    void SaveToCsv()
    {
        string csvPath = filePath.Replace(".json", ".csv");

        using (StreamWriter writer = new StreamWriter(csvPath))
        {
         
            writer.Write("timeStamp,jointName,position.x,position.y,position.z,rotation.x,rotation.y,rotation.z,rotation.w\n");

            foreach (var point in dataPoints)
            {
                foreach (var joint in point.joints)
                {
                    writer.WriteLine($"{point.timeStamp}," +
                                     $"{joint.jointName}," +
                                     $"{joint.position.x},{joint.position.y},{joint.position.z}," +
                                     $"{joint.rotation.x},{joint.rotation.y},{joint.rotation.z},{joint.rotation.w}");
                }
            }
        }

        Debug.Log($"[BodyTrackingLogger] CSV saved: {csvPath}");
    }

}
