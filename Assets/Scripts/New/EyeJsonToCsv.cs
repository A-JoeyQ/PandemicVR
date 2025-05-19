using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

public class EyeJsonToCsv : EditorWindow
{
    [MenuItem("Tools/Convert EyeTracking JSON to CSV")]
    public static void ConvertJsonToCsv()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Select EyeTracking JSON", Application.dataPath, "json");
        if (string.IsNullOrEmpty(jsonPath))
            return;

        string json = File.ReadAllText(jsonPath);

        // 将 JSON 包装为数组（防止没有 []）
        if (!json.TrimStart().StartsWith("["))
        {
            json = "[" + json + "]";
        }

        JArray dataArray = JArray.Parse(json);

        if (dataArray.Count == 0)
        {
            Debug.LogWarning("JSON 内容为空。");
            return;
        }

        string csvPath = jsonPath.Replace(".json", "_converted.csv");
        using (StreamWriter writer = new StreamWriter(csvPath))
        {
            // 写 CSV 头
            var first = (JObject)dataArray[0];
            List<string> headers = new List<string> { "timeStamp" };

            foreach (var prop in first.Properties())
            {
                if (prop.Name == "timeStamp") continue;

                if (prop.Value.Type == JTokenType.Object)
                {
                    foreach (var subProp in ((JObject)prop.Value).Properties())
                    {
                        headers.Add($"{prop.Name}.{subProp.Name}");
                    }
                }
                else
                {
                    headers.Add(prop.Name);
                }
            }

            writer.WriteLine(string.Join(",", headers));

            // 写每一行数据
            foreach (JObject item in dataArray)
            {
                List<string> row = new List<string>();
                row.Add(item["timeStamp"]?.ToString() ?? "0");

                foreach (var header in headers.GetRange(1, headers.Count - 1)) // 跳过 timeStamp
                {
                    var parts = header.Split('.');
                    if (parts.Length == 2)
                    {
                        var obj = item[parts[0]] as JObject;
                        row.Add(obj?[parts[1]]?.ToString() ?? "0");
                    }
                    else
                    {
                        row.Add(item[header]?.ToString() ?? "0");
                    }
                }

                writer.WriteLine(string.Join(",", row));
            }
        }

        Debug.Log($"? EyeTracking CSV 转换完成，路径：{csvPath}");
    }
}
