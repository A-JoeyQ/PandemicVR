using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

// 只有在 Unity 編輯器中才引入 UnityEditor 命名空間
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 儲存單一時間點的身體骨骼數據快照。
/// </summary>
public class BodyDataSnapshot
{
    public float timestamp;
    public bool isDataHighConfidence;
    // **[修正]** 儲存原始的 BodyJointLocation 陣列，它包含了姿態和有效性標記
    public OVRPlugin.BodyJointLocation[] jointLocations;

    public BodyDataSnapshot(float time, bool isConfident, OVRPlugin.BodyJointLocation[] locations)
    {
        timestamp = time;
        isDataHighConfidence = isConfident;

        // 創建一個副本以避免引用問題
        jointLocations = new OVRPlugin.BodyJointLocation[locations.Length];
        locations.CopyTo(jointLocations, 0);
    }
}

/// <summary>
/// 負責記錄 OVRBody 提供的身體追蹤數據。
/// 它會將數據緩存在記憶體中，並在應用程式退出時保存到 CSV 檔案。
/// </summary>
[RequireComponent(typeof(OVRBody))]
public class BodyDataRecorder : MonoBehaviour
{
    private OVRBody _ovrBody;
    private OVRSkeleton.IOVRSkeletonDataProvider _skeletonProvider;

    private List<BodyDataSnapshot> _dataBuffer = new List<BodyDataSnapshot>();
    private bool _isRecording = true;

    // 用於標頭和骨骼枚舉的輔助列表
    private List<OVRPlugin.BoneId> _boneIds;

    void Start()
    {
        _ovrBody = GetComponent<OVRBody>();
        _skeletonProvider = _ovrBody;

        InitializeBoneIds();
    }

    void Update()
    {
        if (!_isRecording) return;

        var skeletonData = _skeletonProvider.GetSkeletonPoseData();

        if (skeletonData.IsDataValid && _ovrBody.BodyState.HasValue)
        {
            var bodyState = _ovrBody.BodyState.Value;

            // **[修正]** 直接傳遞 bodyState.JointLocations，類型現在匹配
            var snapshot = new BodyDataSnapshot(
                Time.time,
                skeletonData.IsDataHighConfidence,
                bodyState.JointLocations
            );
            _dataBuffer.Add(snapshot);
        }
    }

    void OnApplicationQuit()
    {
        if (_dataBuffer.Count > 0)
        {
            SaveDataToFile();
        }
    }

    private void InitializeBoneIds()
    {
        _boneIds = new List<OVRPlugin.BoneId>();
        var skeletonType = _skeletonProvider.GetSkeletonType();

        // 僅處理上半身或全身骨架
        if (skeletonType == OVRSkeleton.SkeletonType.Body || skeletonType == OVRSkeleton.SkeletonType.FullBody)
        {
            for (int i = (int)OVRPlugin.BoneId.Body_Start + 1; i < (int)OVRPlugin.BoneId.Body_End; i++)
            {
                _boneIds.Add((OVRPlugin.BoneId)i);
            }
        }
    }

    public void SaveDataToFile()
    {
        string baseDirectory;

#if UNITY_EDITOR
        baseDirectory = Application.dataPath;
#else
        baseDirectory = Application.persistentDataPath;
#endif

        string directoryPath = Path.Combine(baseDirectory, "BodyData");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileName = $"BodyData_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(directoryPath, fileName);

        StringBuilder sb = new StringBuilder();

        sb.Append("Timestamp,IsHighConfidence,");
        foreach (var boneId in _boneIds)
        {
            string boneName = boneId.ToString().Replace("Body_", "");
            sb.Append($"{boneName}_Trans_X,{boneName}_Trans_Y,{boneName}_Trans_Z,");
            sb.Append($"{boneName}_Rot_X,{boneName}_Rot_Y,{boneName}_Rot_Z,{boneName}_Rot_W,");
        }
        sb.AppendLine();

        foreach (var snapshot in _dataBuffer)
        {
            sb.Append($"{snapshot.timestamp.ToString("F3")},{snapshot.isDataHighConfidence},");
            foreach (var boneId in _boneIds)
            {
                // **[修正]** 先獲取 BodyJointLocation，再從中獲取 Pose
                var jointLocation = snapshot.jointLocations[(int)boneId];
                var pose = jointLocation.Pose;

                // 只有在數據有效時才寫入，否則寫入0
                if (jointLocation.PositionValid)
                {
                    sb.Append($"{pose.Position.x.ToString("F4")},{pose.Position.y.ToString("F4")},{pose.Position.z.ToString("F4")},");
                }
                else
                {
                    sb.Append("0,0,0,");
                }

                if (jointLocation.OrientationValid)
                {
                    sb.Append($"{pose.Orientation.x.ToString("F4")},{pose.Orientation.y.ToString("F4")},{pose.Orientation.z.ToString("F4")},{pose.Orientation.w.ToString("F4")},");
                }
                else
                {
                    sb.Append("0,0,0,0,");
                }
            }
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"身體數據成功保存到: {filePath}");

        _dataBuffer.Clear();

#if UNITY_EDITOR
        AssetDatabase.ImportAsset(filePath);
#endif
    }
}