using System.Collections;
using UnityEngine;
using Oculus.Avatar2; // 确保你正确引入Avatar SDK命名空间

public class AttachFaceTracking : MonoBehaviour
{
    [Tooltip("AttachFaceTracking LocalAvatar对象的名称（可按需修改）")]
    public string avatarObjectName = "LocalAvatar";

    void Start()
    {
        StartCoroutine(AttachFaceTrackingWhenReady());
    }

    IEnumerator AttachFaceTrackingWhenReady()
    {
        // 等待 LocalAvatar 动态生成
        yield return new WaitUntil(() => GameObject.Find(avatarObjectName) != null);

        GameObject localAvatar = GameObject.Find(avatarObjectName);

        if (localAvatar == null)
        {
            Debug.LogError("AttachFaceTracking LocalAvatar still not found.");
            yield break;
        }

        // 获取 AvatarEntity 组件
        OvrAvatarEntity avatarEntity = localAvatar.GetComponent<OvrAvatarEntity>();
        if (avatarEntity == null)
        {
            Debug.LogError("AttachFaceTracking OvrAvatarEntity component not found on LocalAvatar.");
            yield break;
        }

        // 添加或获取面部追踪组件
        OvrAvatarFaceTrackingBehaviorOvrPlugin faceTracking = localAvatar.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        if (faceTracking == null)
        {
            faceTracking = localAvatar.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        }

        // 设置到 avatarEntity 中
        avatarEntity.SetFacePoseProvider(faceTracking);

        Debug.Log("AttachFaceTracking Face tracking behavior attached successfully to LocalAvatar.");
    }
}










/*using UnityEngine;
using Oculus.Avatar2; // 确保你引用了Avatar 2 SDK的命名空间

public class AttachFaceTracking : MonoBehaviour
{

    void Start()
    {

        // 尝试找到名为 LocalAvatar 的 GameObject（可适配根据你的命名规则）
        GameObject localAvatar = GameObject.Find("LocalAvatar");
        if (localAvatar == null)
        {
            Debug.LogError("LocalAvatar not found.");
            return;
        }

        // 尝试获取 AvatarEntity 组件
        OvrAvatarEntity avatarEntity = localAvatar.GetComponent<OvrAvatarEntity>();
        if (avatarEntity == null)
        {
            Debug.LogError("OvrAvatarEntity component not found on LocalAvatar.");
            return;
        }

        // 创建或获取你想要挂载的面部追踪组件
        OvrAvatarFaceTrackingBehaviorOvrPlugin faceTracking = localAvatar.GetComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        if (faceTracking == null)
        {
            faceTracking = localAvatar.AddComponent<OvrAvatarFaceTrackingBehaviorOvrPlugin>();
        }

        // 将其赋值给 AvatarEntity 的 facePoseBehavior 字段
        avatarEntity.SetFacePoseProvider(faceTracking);

        Debug.Log("Face tracking behavior attached to LocalAvatar.");
    }
}
*/