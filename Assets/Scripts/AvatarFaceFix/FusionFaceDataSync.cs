using Fusion;
using Oculus.Avatar2;
using UnityEngine;
using System; // For Buffer.BlockCopy

/// <summary>
/// This NetworkBehaviour synchronizes ONLY the facial expression data,
/// following official Fusion and Meta Avatar best practices.
/// </summary>
// [修正] 移除了 [RequireComponent(typeof(OvrAvatarEntity))]，以支援動態載入流程
public class FusionFaceDataSync : NetworkBehaviour
{
    // A Networked property to store the actual length of the data.
    [Networked]
    private int FaceDataLength { get; set; }

    // A Networked property to store the facial data, using the Fusion-native NetworkArray.
    [Networked, Capacity(300)] // 72 floats * 4 bytes/float = 288 bytes. 300 is a safe capacity.
    private NetworkArray<byte> FaceData { get; }

    // --- Local Player Components ---
    private OvrAvatarEntity _avatarEntity;
    private OVRFaceExpressions _localFaceExpressions; // Used only on the local client to GET data.

    // --- Remote Player Components ---
    private NetworkedFacePoseBehavior _networkedPoseBehavior; // Used only on remote clients to APPLY data.

    // --- Data Buffers ---
    private readonly float[] _localFaceWeights = new float[(int)CAPI.ovrAvatar2FaceExpression.Count];
    private byte[] _remoteFaceBytes;
    private float[] _remoteFaceWeights;

    public override void Spawned()
    {
        _avatarEntity = GetComponent<OvrAvatarEntity>();

        if (Object.HasInputAuthority)
        {
            // --- This is the LOCAL player ---
            Debug.Log("Local avatar spawned. Setting up for face data RECORDING.");
            // We need OVRFaceExpressions to read the local hardware data.
            _localFaceExpressions = gameObject.AddComponent<OVRFaceExpressions>();
        }
        else
        {
            // --- This is a REMOTE player ---
            Debug.Log("Remote avatar spawned. Setting up for face data PLAYBACK.");
            // Add the behavior component that will apply the received data.
            _networkedPoseBehavior = gameObject.AddComponent<NetworkedFacePoseBehavior>();
            // Tell the avatar entity to use our custom behavior as its data source.
            _avatarEntity.SetFacePoseProvider(_networkedPoseBehavior);

            // Initialize buffers for receiving data.
            _remoteFaceWeights = new float[(int)CAPI.ovrAvatar2FaceExpression.Count];
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the local player (with input authority) sends data.
        if (Object.HasInputAuthority)
        {
            if (_localFaceExpressions != null && _localFaceExpressions.ValidExpressions)
            {
                // 1. Get the latest expression data.
                _localFaceExpressions.CopyTo(_localFaceWeights, 0);

                // 2. Serialize the float[] into a byte[].
                var bytes = MarshalFloatArray(_localFaceWeights);
                if (bytes != null)
                {
                    // 3. Write the data into the [Networked] properties. Fusion will handle the rest.
                    FaceDataLength = bytes.Length;
                    FaceData.CopyFrom(bytes, 0, bytes.Length);
                }
            }
        }
    }

    public override void Render()
    {
        // Only remote players (without input authority) receive and apply data.
        if (!Object.HasInputAuthority)
        {
            if (FaceDataLength > 0 && _networkedPoseBehavior != null)
            {
                // 1. Ensure our byte buffer is the correct size.
                if (_remoteFaceBytes == null || _remoteFaceBytes.Length != FaceDataLength)
                {
                    _remoteFaceBytes = new byte[FaceDataLength];
                }

                // 2. Read the data from the NetworkArray.
                for (int i = 0; i < FaceDataLength; ++i)
                {
                    _remoteFaceBytes[i] = FaceData[i];
                }

                // 3. Deserialize the byte[] back into a float[].
                UnmarshalFloatArray(_remoteFaceBytes, ref _remoteFaceWeights);

                // 4. Feed the float[] into our custom provider on the remote avatar.
                var provider = _networkedPoseBehavior.FacePoseProvider as NetworkedFacePoseProvider;
                provider?.ReceiveFaceData(_remoteFaceWeights);
            }
        }
    }

    // --- Serialization Helpers ---
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
