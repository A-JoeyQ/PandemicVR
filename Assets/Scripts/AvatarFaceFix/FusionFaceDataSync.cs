using Fusion;
using Oculus.Avatar2;
using UnityEngine;
using System; // 用于 Buffer.BlockCopy

/// <summary>
/// 【最终版 - 详尽注释和日志】
/// 此脚本负责同步面部表情数据，并实现了“网络回环”功能。
/// “网络回环”：本地玩家的虚拟形象也通过网络数据通道驱动，确保您看到的自己和别人看到的您完全一致。
/// 它使用 OnSkeletonLoadedEvent 来确保初始化时机正确。
/// 它与官方的 AvatarBehaviourFusionCustom 等脚本协同工作，专门处理面部数据。
/// </summary>
public class FusionFaceDataSync : NetworkBehaviour
{
    // --- 网络属性 ---
    // Fusion会自动同步这些带有 [Networked] 标记的属性。

    /// <summary>
    /// [Networked] 属性，用于存储面部表情数据字节数组的实际长度。
    /// </summary>
    [Networked]
    private int FaceDataLength { get; set; }

    /// <summary>
    /// [Networked] 属性，用于存储实际的面部表情数据。
    /// Capacity是预分配的容量，72个浮点数 * 4字节/浮点数 = 288字节，300是一个安全值。
    /// </summary>
    [Networked, Capacity(300)]
    private NetworkArray<byte> FaceData { get; }

    // --- C# 成员变量 ---

    // 核心的虚拟形象组件的引用
    private OvrAvatarEntity _avatarEntity;
    // 【本地玩家专用】用于从硬件读取面部数据的组件
    private OVRFaceExpressions _localFaceExpressions;
    // 【所有玩家共用】用于接收网络数据并应用到虚拟形象上的“驱动器”组件
    private NetworkedFacePoseBehavior _networkedPoseBehavior;

    // --- 数据缓冲区 ---

    // 【本地玩家专用】用于临时存放从硬件读取的浮点数表情数据
    private readonly float[] _localFaceWeights = new float[(int)CAPI.ovrAvatar2FaceExpression.Count];
    // 用于临时存放从网络接收的字节表情数据
    private byte[] _remoteFaceBytes;
    // 用于临时存放从字节转换回的浮点数表情数据
    private float[] _remoteFaceWeights;

    /// <summary>
    /// 当网络对象在场景中生成时，由Fusion调用。这是脚本的入口点。
    /// 它的唯一工作是安全地订阅OnSkeletonLoadedEvent事件，以确保后续的初始化在正确的时间点执行。
    /// </summary>
    public override void Spawned()
    {
        _avatarEntity = GetComponent<OvrAvatarEntity>();
        if (_avatarEntity != null)
        {
            // 订阅“当骨骼加载完毕后”这个事件。我们的初始化逻辑会放到 OnAvatarSkeletonLoaded 方法中。
            _avatarEntity.OnSkeletonLoadedEvent.AddListener(OnAvatarSkeletonLoaded);
            Debug.Log($"[{gameObject.name}] Spawned成功。正在等待骨骼加载...");
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] 错误：找不到OvrAvatarEntity组件！", this);
        }
    }

    /// <summary>
    /// 这个方法在虚拟形象的骨骼和基础模型加载完毕后才会被调用，是进行设置最安全的时间点。
    /// </summary>
    private void OnAvatarSkeletonLoaded(OvrAvatarEntity entity)
    {
        // 这是一个好习惯：一旦事件被触发，就立刻取消订阅，防止意外的重复调用。
        entity.OnSkeletonLoadedEvent.RemoveListener(OnAvatarSkeletonLoaded);
        Debug.Log($"[{gameObject.name}] 骨骼加载完毕，开始进行初始化设置。");

        // --- 为所有玩家进行统一的“播放器”设置 ---
        // 因为本地玩家也需要通过网络回环来播放表情，所以所有玩家都需要这个“驱动器”组件。
        _networkedPoseBehavior = gameObject.AddComponent<NetworkedFacePoseBehavior>();
        _remoteFaceWeights = new float[(int)CAPI.ovrAvatar2FaceExpression.Count];

        // 将虚拟形象的表情数据源设置为我们自己的驱动器。
        _avatarEntity.SetFacePoseProvider(_networkedPoseBehavior);
        Debug.Log($"[{gameObject.name}] 已将FacePoseProvider设置为自定义的NetworkedFacePoseBehavior。");

        // --- 区分本地和远程玩家，进行差异化设置 ---
        if (Object.HasInputAuthority)
        {
            // 如果是本地玩家，我们额外需要一个“录制器”来采集数据。
            Debug.Log($"[{gameObject.name}] 判断为【本地玩家】，添加OVRFaceExpressions用于数据采集。模式：【录制并回环播放】");
            _localFaceExpressions = gameObject.AddComponent<OVRFaceExpressions>();
        }
        else
        {
            // 远程玩家只需要播放，无需额外操作。
            Debug.Log($"[{gameObject.name}] 判断为【远程玩家】。模式：【播放】");
        }
    }

    /// <summary>
    /// 由Fusion在每个网络“滴答”(tick)时调用。非常适合发送数据。
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        // 只有拥有输入权限的本地玩家才需要发送数据。
        if (Object.HasInputAuthority)
        {
            // 确保数据采集器有效
            if (_localFaceExpressions != null && _localFaceExpressions.ValidExpressions)
            {
                // 1. 从硬件读取数据到浮点数缓冲区
                _localFaceExpressions.CopyTo(_localFaceWeights, 0);

                // 2. 将浮点数数组序列化（转换）为字节数组
                var bytes = MarshalFloatArray(_localFaceWeights);
                if (bytes != null)
                {
                    // 3. 将字节数组和其长度写入到[Networked]属性中，Fusion会自动将这些变化同步给其他客户端。
                    FaceDataLength = bytes.Length;
                    FaceData.CopyFrom(bytes, 0, bytes.Length);
                    // 取消下面这行注释可以进行高频日志轰炸，用于精细调试
                    // Debug.Log($"[{gameObject.name}] SENDING > 发送面部数据，长度: {bytes.Length}");
                }
            }
        }
    }

    /// <summary>
    /// 由Fusion在每一帧渲染前调用。非常适合应用视觉上的更新。
    /// </summary>
    public override void Render()
    {
        // 这段逻辑现在对【所有】玩家（本地和远程）都执行，以实现网络回环。
        if (FaceDataLength > 0 && _networkedPoseBehavior != null && _avatarEntity != null)
        {
            // 强制设置Provider，以对抗其他脚本的覆盖，确保我们的数据通道始终畅通。
            _avatarEntity.SetFacePoseProvider(_networkedPoseBehavior);

            // 确保我们的字节缓冲区大小是正确的
            if (_remoteFaceBytes == null || _remoteFaceBytes.Length != FaceDataLength)
            {
                _remoteFaceBytes = new byte[FaceDataLength];
            }

            // 从[Networked]属性中读取数据到我们的字节缓冲区
            for (int i = 0; i < FaceDataLength; ++i)
            {
                _remoteFaceBytes[i] = FaceData[i];
            }

            // 将字节数组反序列化（转换）回浮点数数组
            UnmarshalFloatArray(_remoteFaceBytes, ref _remoteFaceWeights);

            // 从驱动器中获取我们自定义的“数据仓库”(Provider)
            var provider = _networkedPoseBehavior.FacePoseProvider as NetworkedFacePoseProvider;
            if (provider != null)
            {
                // 将最新的浮点数数据喂给“数据仓库”，等待SDK来取用。
                provider.ReceiveFaceData(_remoteFaceWeights);
                Debug.Log($"[{gameObject.name}] APPLYING < 正在应用面部数据。长度: {FaceDataLength}, CheekRaiser值: {_remoteFaceWeights[(int)CAPI.ovrAvatar2FaceExpression.CheekRaiserL]}");
            }
        }
    }

    // --- 序列化帮助方法 (无需改动) ---
    private static byte[] MarshalFloatArray(float[] floats)
    {
        if (floats == null || floats.Length == 0) return null;
        byte[] bytes = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static void UnmarshalFloatArray(byte[] bytes, ref float[] floats)
    {
        if (bytes == null || bytes.Length == 0) return;
        if (floats == null || floats.Length != bytes.Length / sizeof(float))
        {
            floats = new float[bytes.Length / sizeof(float)];
        }
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
    }
}