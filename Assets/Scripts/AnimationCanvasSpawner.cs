using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static Unity.Collections.Unicode;

public class AnimationCanvasSpawner : NetworkBehaviour
{

    public static AnimationCanvasSpawner Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public NetworkObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation,Transform parent,Player PlayerModel, NetworkRunner runner)
    {
        NetworkObject obj = runner.Spawn(
            prefab, position, rotation, NetworkPlayer.LocalPlayerNetObj.InputAuthority,
            (runner, obj) => // onBeforeSpawned
            {
                obj.transform.SetParent(parent, false);
                obj.transform.rotation = rotation;
                obj.transform.position = position;

                obj.GetComponent<PawnMoveable>().SetRoleAndPlayer(PlayerModel);
                obj.GetComponent<PawnMoveable>().SetMoveable(true);
            }

        );
        return obj;
    }
}
