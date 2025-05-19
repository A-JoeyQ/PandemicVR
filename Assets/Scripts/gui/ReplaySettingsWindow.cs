using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class ReplaySettingsWindow: MonoBehaviour
{
    public TMP_Dropdown fileDropdown;
    public TMP_InputField eventInputField;
    public Toggle replayWithTimestampsToggle;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI errorMessageText;
    
    private UnityAction<int, bool> onConfirmAction;
    private int minValue;
    private int maxValue;

    private List<string> jsonFiles = new List<string>();

    private string logsFolderPath = Application.isEditor
        ? Path.Combine(Application.dataPath, "Logs")
        : Path.Combine(Application.persistentDataPath, "Logs");
    
    private static ReplaySettingsWindow instance; //Singleton pattern

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        gameObject.SetActive(false);
        PopulateDropdown();
        
        fileDropdown.onValueChanged.AddListener(OnFileSelected);
    }

    public static void Show(UnityAction<int, bool> confirmAction)
    {
        if (instance == null)
        {
            Debug.LogError("ReplaySettingsDialog instance is not found in the scene.");
            return;
        }
        
        instance.onConfirmAction = confirmAction;
        instance.gameObject.SetActive(true);
        instance.ClearInputs();
        //instance.eventInputField.text = minValue.ToString();
       
        
        if (instance.errorMessageText != null)
        {
            instance.errorMessageText.text = "";
            instance.errorMessageText.gameObject.SetActive(false);
        }
        
        instance.replayWithTimestampsToggle.isOn = true;
        
        instance.confirmButton.onClick.AddListener(instance.OnConfirm);
        instance.cancelButton.onClick.AddListener(instance.OnCancel);
        
    }

    private void PopulateDropdown()
    {
        fileDropdown.ClearOptions();

        jsonFiles = Directory.GetFiles(logsFolderPath, "*.json").Select(System.IO.Path.GetFileName).ToList();
        
        jsonFiles.Insert(0, "Select a file");
        
        fileDropdown.AddOptions(jsonFiles);
    }

    private void OnFileSelected(int index)
    {
        if (index == 0)
        {
            ClearInputs();
            return;
        }

        string selectedFile = jsonFiles[index];
        string filePath = System.IO.Path.Combine(logsFolderPath, selectedFile);

        List<TimelineEvent> logs = LogFileReader.ReadLogs(filePath);

        if (logs.Count > 0)
        {
            minValue = 1;
            maxValue = logs.Count;

            if (instance.eventInputField.placeholder is TextMeshProUGUI placeholderText)
            {
                placeholderText.text = $"Range: {minValue} - {maxValue}";
            }
        }
    }

    private void OnConfirm()
    {
        if (int.TryParse(eventInputField.text, out int eventIndex))
        {
            if (eventIndex >= minValue && eventIndex <= maxValue)
            {
                eventIndex = Mathf.Clamp(eventIndex, minValue, maxValue);
                onConfirmAction?.Invoke(eventIndex, replayWithTimestampsToggle.isOn);
                Close();
            }
            else
            {
                DisplayErrorMessage($"Please enter a value between {minValue} and {maxValue}.");
            }
        }
        else 
        {
            DisplayErrorMessage("Please enter a valid integer.");
        }
        
    }

    private void OnCancel()
    {
        Close();
    }

    private void Close()
    {
        confirmButton.onClick.RemoveListener(OnConfirm);
        cancelButton.onClick.RemoveListener(OnCancel);
        gameObject.SetActive(false);
    }
    
    private void DisplayErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Error message text component is not assigned.");
        }
    }

    private void ClearInputs()
    {
        eventInputField.text = string.Empty;
        
        if (eventInputField.placeholder is TextMeshProUGUI placeholderText)
        {
            placeholderText.text = "Range: N/A";
        }
    }
    /*void OnGUI()
    {
        GUILayout.Label("Replay Settings", EditorStyles.boldLabel);

        eventIndex = EditorGUILayout.IntField($"Replay until event: ({minValue} - {maxValue})", eventIndex);
        
        replayWithAnimations = EditorGUILayout.Toggle("Replay with animations", replayWithAnimations);

        GUILayout.Space(20);

        if (GUILayout.Button("Confirm"))
        {
            onConfirm?.Invoke(eventIndex, replayWithAnimations);
            Close();
        }

        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
    }*/
}
