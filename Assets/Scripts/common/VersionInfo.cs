using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class VersionInfo : MonoBehaviour
{
  public string versionNumber;
  public string buildName;

  void Start()
  {
    gameObject.GetComponent<Text>().text = "version "+buildName+"\r\nUnity: "+Application.unityVersion;
  }
#if UNITY_EDITOR
  public void Awake()
  {
    if (Application.isEditor && !Application.isPlaying)
    {
      buildName = versionNumber + " (" + System.DateTime.Now.ToShortDateString() + ")";
    }
  }
#endif
}
