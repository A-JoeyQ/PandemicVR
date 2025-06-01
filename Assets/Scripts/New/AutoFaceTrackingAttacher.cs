using UnityEngine;

using Oculus.Avatar2;


public class AutoFaceTrackingAttacher : MonoBehaviour
{
    private bool _isAttached = false;

    void Update()
    {
        if (_isAttached) return;

        var localEntity = Object.FindFirstObjectByType<OvrAvatarEntity>();
        if (localEntity == null || !localEntity.IsLocal) return;

        var faceTracking = localEntity.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        var eyeTracking = localEntity.GetComponent<OvrAvatarEyeTrackingBehaviorOvrPlugin>();

        if (eyeTracking == null)
        {
            eyeTracking = localEntity.gameObject.AddComponent<OvrAvatarEyeTrackingBehaviorOvrPlugin>();
            Debug.Log("已挂载 EyeTracking 组件");
        }

        localEntity.SetEyePoseProvider(eyeTracking);
        Debug.Log("已设置 EyePoseProvider");


        if (faceTracking == null)
        {
            faceTracking = localEntity.gameObject.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
            Debug.Log("已挂载 FaceTracking 组件");
        }

        localEntity.SetFacePoseProvider(faceTracking);
        Debug.Log("已设置 FacePoseProvider");

        _isAttached = true;
    }
}

/*好像能用，待测试
public class AutoFaceTrackingAttacher : MonoBehaviour
{
    private bool _isAttached = false;

    void Update()
    {
        if (_isAttached) return;

        var localEntity = FindObjectOfType<OvrAvatarEntity>();
        if (localEntity == null || !localEntity.IsLocal) return;

        var faceTracking = localEntity.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        if (faceTracking == null)
        {
            faceTracking = localEntity.gameObject.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
            Debug.Log("已挂载 FaceTracking 组件");
        }

        localEntity.SetFacePoseProvider(faceTracking);
        Debug.Log("已设置 FacePoseProvider");

        _isAttached = true;
    }
}*/

