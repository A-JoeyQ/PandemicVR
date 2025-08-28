using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks; // 引入 Task 命名空间以使用异步操作

public class FaceDataSnapshot
{
    public float timestamp;
    public float[] expressions;

    public FaceDataSnapshot(float time, float[] exprs)
    {
        timestamp = time;
        expressions = exprs;
    }

    public static string GetCsvHeader()
    {
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
        return sb.ToString();
    }

    public string ToCsvRow()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(timestamp.ToString("F3") + ",");
        for (int i = 0; i < expressions.Length; i++)
        {
            sb.Append(expressions[i].ToString("F4"));
            if (i < expressions.Length - 1)
            {
                sb.Append(",");
            }
        }
        return sb.ToString();
    }
}


[RequireComponent(typeof(OVRFaceExpressions))]
public class FaceDataRecorder : MonoBehaviour
{
    [Header("同步控制器")]
    [Tooltip("將 EyeTracker 中的 boardCanvas 拖拽到這裡，以同步開始/停止記錄")]
    public Canvas boardCanvas; // 用于同步的“开关”

    private OVRFaceExpressions _faceExpressions;
    private StreamWriter writer;
    private Queue<FaceDataSnapshot> dataQueue = new Queue<FaceDataSnapshot>();
    private bool isLogging = false;

    void Start()
    {
        _faceExpressions = GetComponent<OVRFaceExpressions>();
        InitializeWriter();
    }

    private void InitializeWriter()
    {
        string rootPath;
#if UNITY_EDITOR
        rootPath = Application.dataPath;
#else
        rootPath = Application.persistentDataPath;
#endif

        string finalFolderPath = Path.Combine(rootPath, "FaceData");

        try
        {
            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }

            string fileName = $"FaceData_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(finalFolderPath, fileName);

            writer = new StreamWriter(filePath, true);
            writer.WriteLine(FaceDataSnapshot.GetCsvHeader());
            isLogging = true;
            Debug.Log($"[FaceDataRecorder] 已準備記錄，檔案位於: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FaceDataRecorder] 初始化寫入器時發生錯誤: {e.Message}");
            isLogging = false;
        }
    }

    void Update()
    {
        // 步骤 1: 仅在 boardCanvas 激活时采集数据到队列
        if (isLogging && boardCanvas != null && boardCanvas.gameObject.activeSelf && _faceExpressions.ValidExpressions)
        {
            float[] currentExpressions = _faceExpressions.ToArray();
            // 确保时间戳与 EyeTracker 对齐 (假设 MainMenu.startTimestamp 可访问)
            float timestamp = Time.time - MainMenu.startTimestamp;
            FaceDataSnapshot snapshot = new FaceDataSnapshot(timestamp, currentExpressions);
            dataQueue.Enqueue(snapshot);
        }

        // 步骤 2: 模仿 EyeTrackingLogger，使用 Task.Run 异步地将队列数据写入文件
        while (dataQueue.Count > 0)
        {
            FaceDataSnapshot dataPoint = dataQueue.Dequeue();
            string rowData = dataPoint.ToCsvRow();
            // 将文件写入操作放到后台线程，避免阻塞主线程
            Task.Run(() => writer.WriteLine(rowData));
        }
    }

    void OnApplicationQuit()
    {
        // 停止采集
        isLogging = false;

        // 关键：在应用退出前，同步地将队列中所有剩余数据写入文件，确保不丢失
        Debug.Log($"[FaceDataRecorder] 應用程式正在退出，正在寫入剩餘的 {dataQueue.Count} 條數據...");
        while (dataQueue.Count > 0)
        {
            FaceDataSnapshot dataPoint = dataQueue.Dequeue();
            writer.WriteLine(dataPoint.ToCsvRow()); // 在这里同步写入
        }

        if (writer != null)
        {
            writer.Flush(); // 确保缓冲区内容完全写入磁盘
            writer.Close();
            writer = null;
            Debug.Log("[FaceDataRecorder] 記錄已停止，檔案已成功關閉。");
        }
    }
}


/*using UnityEngine;
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

    // 輔助方法：獲取 CSV 標頭
    public static string GetCsvHeader()
    {
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
        return sb.ToString();
    }

    // 輔助方法：將數據轉換為 CSV 的一行
    public string ToCsvRow()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(timestamp.ToString("F3") + ",");
        for (int i = 0; i < expressions.Length; i++)
        {
            sb.Append(expressions[i].ToString("F4"));
            if (i < expressions.Length - 1)
            {
                sb.Append(",");
            }
        }
        return sb.ToString();
    }
}

[RequireComponent(typeof(OVRFaceExpressions))]
public class FaceDataRecorder : MonoBehaviour
{
    private OVRFaceExpressions _faceExpressions;

    // --- [修改] 仿照 EyeTrackingLogger 的寫入機制 ---
    private StreamWriter writer;
    private Queue<FaceDataSnapshot> dataQueue = new Queue<FaceDataSnapshot>();
    private bool isLogging = false;
    // -----------------------------------------

    void Start()
    {
        _faceExpressions = GetComponent<OVRFaceExpressions>();
        InitializeWriter();
    }

    private void InitializeWriter()
    {
        string rootPath;

#if UNITY_EDITOR
        rootPath = Application.dataPath;
#else
        rootPath = Application.persistentDataPath;
#endif

        string finalFolderPath = Path.Combine(rootPath, "FaceData");

        try
        {
            if (!Directory.Exists(finalFolderPath))
            {
                Directory.CreateDirectory(finalFolderPath);
            }

            string fileName = $"FaceData_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filePath = Path.Combine(finalFolderPath, fileName);

            // --- [修改] 打開一個檔案流並寫入標頭 ---
            writer = new StreamWriter(filePath, true); // true for append
            writer.WriteLine(FaceDataSnapshot.GetCsvHeader());
            isLogging = true;
            Debug.Log($"[FaceDataRecorder] 已開始記錄，檔案位於: {filePath}");
            // -----------------------------------------
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FaceDataRecorder] 初始化寫入器時發生錯誤: {e.Message}");
            isLogging = false;
        }
    }

    void Update()
    {
        // 步驟 1: 收集數據到佇列中
        if (isLogging && _faceExpressions.ValidExpressions)
        {
            float[] currentExpressions = _faceExpressions.ToArray();
            FaceDataSnapshot snapshot = new FaceDataSnapshot(Time.time, currentExpressions);
            dataQueue.Enqueue(snapshot);
        }

        // 步驟 2: 將佇列中的數據寫入檔案流
        // 為避免效能問題，我們可以每幀只寫入一定數量的數據，或者一次性寫完
        while (dataQueue.Count > 0)
        {
            FaceDataSnapshot dataPoint = dataQueue.Dequeue();
            writer.WriteLine(dataPoint.ToCsvRow());
        }
    }

    void OnApplicationQuit()
    {
        // 確保所有剩餘的數據都被寫入
        while (dataQueue.Count > 0)
        {
            FaceDataSnapshot dataPoint = dataQueue.Dequeue();
            writer.WriteLine(dataPoint.ToCsvRow());
        }

        // --- [修改] 安全地關閉檔案流 ---
        if (writer != null)
        {
            writer.Flush(); // 確保緩存中的所有內容都寫入檔案
            writer.Close();
            writer = null;
        }
        isLogging = false;
        Debug.Log("[FaceDataRecorder] 記錄已停止，檔案已關閉。");
        // ----------------------------------
    }
}
*/