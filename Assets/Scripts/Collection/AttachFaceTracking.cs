using System.Collections;
using UnityEngine;
using Oculus.Avatar2; // ȷ������ȷ����Avatar SDK�����ռ�

public class AttachFaceTracking : MonoBehaviour
{
    [Tooltip("AttachFaceTracking LocalAvatar��������ƣ��ɰ����޸ģ�")]
    public string avatarObjectName = "LocalAvatar";

    void Start()
    {
        StartCoroutine(AttachFaceTrackingWhenReady());
    }

    IEnumerator AttachFaceTrackingWhenReady()
    {
        // �ȴ� LocalAvatar ��̬����
        yield return new WaitUntil(() => GameObject.Find(avatarObjectName) != null);

        GameObject localAvatar = GameObject.Find(avatarObjectName);

        if (localAvatar == null)
        {
            Debug.LogError("AttachFaceTracking LocalAvatar still not found.");
            yield break;
        }

        // ��ȡ AvatarEntity ���
        OvrAvatarEntity avatarEntity = localAvatar.GetComponent<OvrAvatarEntity>();
        if (avatarEntity == null)
        {
            Debug.LogError("AttachFaceTracking OvrAvatarEntity component not found on LocalAvatar.");
            yield break;
        }

        // ��ӻ��ȡ�沿׷�����
        OvrAvatarFaceTrackingBehaviorOvrPlugin faceTracking = localAvatar.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        if (faceTracking == null)
        {
            faceTracking = localAvatar.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        }

        // ���õ� avatarEntity ��
        avatarEntity.SetFacePoseProvider(faceTracking);

        Debug.Log("AttachFaceTracking Face tracking behavior attached successfully to LocalAvatar.");
    }
}










/*using UnityEngine;
using Oculus.Avatar2; // ȷ����������Avatar 2 SDK�������ռ�

public class AttachFaceTracking : MonoBehaviour
{

    void Start()
    {

        // �����ҵ���Ϊ LocalAvatar �� GameObject����������������������
        GameObject localAvatar = GameObject.Find("LocalAvatar");
        if (localAvatar == null)
        {
            Debug.LogError("LocalAvatar not found.");
            return;
        }

        // ���Ի�ȡ AvatarEntity ���
        OvrAvatarEntity avatarEntity = localAvatar.GetComponent<OvrAvatarEntity>();
        if (avatarEntity == null)
        {
            Debug.LogError("OvrAvatarEntity component not found on LocalAvatar.");
            return;
        }

        // �������ȡ����Ҫ���ص��沿׷�����
        OvrAvatarFaceTrackingBehaviorOvrPlugin faceTracking = localAvatar.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        if (faceTracking == null)
        {
            faceTracking = localAvatar.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        }

        // ���丳ֵ�� AvatarEntity �� facePoseBehavior �ֶ�
        avatarEntity.SetFacePoseProvider(faceTracking);

        Debug.Log("Face tracking behavior attached to LocalAvatar.");
    }
}
*/