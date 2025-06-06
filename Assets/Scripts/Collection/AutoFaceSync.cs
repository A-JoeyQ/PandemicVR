using System.Collections;
using System.Linq;
using UnityEngine;
using Oculus.Avatar2;
using Meta.XR.MultiplayerBlocks.Shared;

public class AutoFaceSync : MonoBehaviour
{
    public float retryInterval = 0.5f; // �����ʱ��
    private bool _isAttached = false;

    private void Start()
    {
        StartCoroutine(AttachFacePoseWhenReady());
    }

    private IEnumerator AttachFacePoseWhenReady()
    {
        while (!_isAttached)
        {
            // ���ұ��� AvatarEntity
            var localAvatar = FindObjectsOfType<AvatarEntity>()
                .FirstOrDefault(a => a.IsLocal);

            if (localAvatar != null && localAvatar.IsCreated && !localAvatar.IsPendingAvatar)
            {
                if (localAvatar.GetComponent<SampleFacePoseBehavior>() == null)
                {
                    localAvatar.gameObject.AddComponent<SampleFacePoseBehavior>();
                    Debug.Log("[AutoAttachFacePose] SampleFacePoseBehavior �ѳɹ����ص����� AvatarEntity �ϣ�");
                }
                else
                {
                    Debug.Log("[AutoAttachFacePose] SampleFacePoseBehavior �Ѵ��ڣ����ظ����ء�");
                }

                _isAttached = true;
                yield break;
            }

            yield return new WaitForSeconds(retryInterval);
        }
    }
}
