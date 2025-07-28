using Oculus.Avatar2;
using UnityEngine;

/// <summary>
/// This is a MonoBehaviour that acts as a bridge. It provides our custom
/// NetworkedFacePoseProvider to the OvrAvatarEntity.
/// This component is added to REMOTE avatars.
/// </summary>
public class NetworkedFacePoseBehavior : OvrAvatarFacePoseBehavior
{
    // Creates and holds an instance of our custom provider.
    private readonly NetworkedFacePoseProvider _facePoseProvider = new NetworkedFacePoseProvider();

    // The Avatar SDK accesses this property to get the face data source.
    public override OvrAvatarFacePoseProviderBase FacePoseProvider => _facePoseProvider;
}
