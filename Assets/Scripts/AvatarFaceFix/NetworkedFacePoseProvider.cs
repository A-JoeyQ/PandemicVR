using Oculus.Avatar2;

/// <summary>
/// A plain C# class that acts as a custom face data source for remote avatars.
/// It receives expression weights from the network.
/// </summary>
public class NetworkedFacePoseProvider : OvrAvatarFacePoseProviderBase
{
    // Buffer to hold the latest expression weights received from the network.
    private readonly float[] _faceExpressionWeights = new float[(int)CAPI.ovrAvatar2FaceExpression.Count];
    private bool _hasReceivedData = false;

    /// <summary>
    /// Called by the networking script to feed new data into this provider.
    /// </summary>
    public void ReceiveFaceData(float[] newWeights)
    {
        if (newWeights != null && newWeights.Length == _faceExpressionWeights.Length)
        {
            System.Array.Copy(newWeights, _faceExpressionWeights, _faceExpressionWeights.Length);
            _hasReceivedData = true;
        }
    }

    /// <summary>
    /// The Avatar SDK calls this method to get the face pose. We provide our buffered data.
    /// </summary>
    protected override bool GetFacePose(OvrAvatarFacePose facePose)
    {
        if (_hasReceivedData)
        {
            // This is the correct way to apply data, based on the source code you provided:
            // Copy our data into the public array of the facePose object.
            if (facePose.expressionWeights != null)
            {
                _faceExpressionWeights.CopyTo(facePose.expressionWeights, 0);
            }
            return true; // We successfully provided a pose.
        }
        return false; // We don't have data yet.
    }
}