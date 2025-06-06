using System.Collections;
using System.Linq;
using UnityEngine;
using Oculus.Avatar2;
using Meta.XR.MultiplayerBlocks.Shared;

public class AutoFaceSync : MonoBehaviour
{
    public float retryInterval = 0.5f; // 检查间隔时间
    private bool _isAttached = false;

    private void Start()
    {
        StartCoroutine(AttachFacePoseWhenReady());
    }

    private IEnumerator AttachFacePoseWhenReady()
    {
        while (!_isAttached)
        {
            // 查找本地 AvatarEntity
            var localAvatar = FindObjectsOfType<AvatarEntity>()
                .FirstOrDefault(a => a.IsLocal);

            if (localAvatar != null && localAvatar.IsCreated && !localAvatar.IsPendingAvatar)
            {
                if (localAvatar.GetComponent<SampleFacePoseBehavior>() == null)
                {
                    localAvatar.gameObject.AddComponent<SampleFacePoseBehavior>();
                    Debug.Log("[AutoAttachFacePose] SampleFacePoseBehavior 已成功挂载到本地 AvatarEntity 上！");
                }
                else
                {
                    Debug.Log("[AutoAttachFacePose] SampleFacePoseBehavior 已存在，不重复挂载。");
                }

                _isAttached = true;
                yield break;
            }

            yield return new WaitForSeconds(retryInterval);
        }
    }
}
