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
            Debug.Log("�ѹ��� EyeTracking ���");
        }

        localEntity.SetEyePoseProvider(eyeTracking);
        Debug.Log("������ EyePoseProvider");


        if (faceTracking == null)
        {
            faceTracking = localEntity.gameObject.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
            Debug.Log("�ѹ��� FaceTracking ���");
        }

        localEntity.SetFacePoseProvider(faceTracking);
        Debug.Log("������ FacePoseProvider");

        _isAttached = true;
    }
}

/*�������ã�������
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
            Debug.Log("�ѹ��� FaceTracking ���");
        }

        localEntity.SetFacePoseProvider(faceTracking);
        Debug.Log("������ FacePoseProvider");

        _isAttached = true;
    }
}*/

