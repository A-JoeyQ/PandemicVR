using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

// 只有在 Unity 編輯器中才引入 UnityEditor 命名空間
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FaceDataSnapshot
{
    public float timestamp;
    public float[] expressions;

    public FaceDataSnapshot(float time, float[] exprs)
    {
        timestamp = time;
        expressions = exprs;
    }
}

[RequireComponent(typeof(OVRFaceExpressions))]
public class FaceDataRecorder : MonoBehaviour
{
    private OVRFaceExpressions _faceExpressions;
    private List<FaceDataSnapshot> _dataBuffer = new List<FaceDataSnapshot>();
    private bool _isRecording = true;

    void Start()
    {
        _faceExpressions = GetComponent<OVRFaceExpressions>();
    }

    void Update()
    {
        if (_isRecording && _faceExpressions.ValidExpressions)
        {
            float[] currentExpressions = _faceExpressions.ToArray();
            FaceDataSnapshot snapshot = new FaceDataSnapshot(Time.time, currentExpressions);
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

    public void SaveDataToFile()
    {
        string baseDirectory;

#if UNITY_EDITOR
        // 在編輯器中，使用 Application.dataPath，它直接指向 Assets 資料夾
        baseDirectory = Application.dataPath;
        Debug.Log("運行環境：Unity 編輯器。使用 Assets 資料夾作為儲存基礎路徑。");
#else
        // 在打包後的版本中，使用 persistentDataPath
        baseDirectory = Application.persistentDataPath;
        Debug.Log("運行環境：打包應用程式。使用 persistentDataPath 作為儲存基礎路徑。");
#endif

        string directoryPath = Path.Combine(baseDirectory, "FaceData");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fileName = $"FaceData_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
        string filePath = Path.Combine(directoryPath, fileName);

        StringBuilder sb = new StringBuilder();

        sb.Append("Timestamp,");
        for (int i = 0; i < (int)OVRFaceExpressions.FaceExpression.Max; i++)
        {
            sb.Append(((OVRFaceExpressions.FaceExpression)i).ToString());
            if (i < (int)OVRFaceExpressions.FaceExpression.Max - 1)
            {
                sb.Append(",");
            }
        }
        sb.AppendLine();

        foreach (var snapshot in _dataBuffer)
        {
            sb.Append(snapshot.timestamp.ToString("F3") + ",");
            for (int i = 0; i < snapshot.expressions.Length; i++)
            {
                sb.Append(snapshot.expressions[i].ToString("F4"));
                if (i < snapshot.expressions.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
        }

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log($"臉部數據成功保存到: {filePath}");

        _dataBuffer.Clear();

#if UNITY_EDITOR
        // 通知 Unity 編輯器刷新資產，以便新檔案立即顯示在 Project 視窗中
        AssetDatabase.ImportAsset(filePath);
#endif
    }
}