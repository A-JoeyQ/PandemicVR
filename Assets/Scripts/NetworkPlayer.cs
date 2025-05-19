using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using static ENUMS;
using Meta.XR.MultiplayerBlocks.Shared;
using Oculus.Interaction.AvatarIntegration;
using Oculus.Avatar2;
using Photon.Voice.Unity.UtilityScripts;
using Oculus.Interaction.Input;

public class NetworkPlayer : NetworkBehaviour

{

    public static NetworkObject LocalPlayerNetObj { get; private set; }
    public static NetworkPlayer LocalInstance { get; private set; }

    [Networked]
    public int seatIndex { get; private set; } = -1;

    [Networked]
    public int startingPosition { get; private set; } = -1;

    public bool blockTreating = false;
    private bool isRightPinching = false;
    private bool isLeftPinching = false;

    public float rpcCooldownTime = 0.15f;
    private float lastRPCCall = -Mathf.Infinity;
    public void Start()
    {
        
        AvatarEntity avatarEntity = GetComponent<AvatarEntity>();
        if (avatarEntity != null)
        {
            Debug.Log("Register Event for Override");
            avatarEntity.OnSkeletonLoadedEvent.AddListener(OnOverrideTracking);
            avatarEntity.OnSkeletonLoadedEvent.AddListener(AddColliderToAvatar);
            //avatarEntity.OnSkeletonLoadedEvent.AddListener(AddSaveRecording);
        }
        else
        {
            Debug.Log("AvatarEntity Null");
        }

        MainMenu.instance.OnGameStart.AddListener(OnGameStart);
    }
    public void Update()
    {
        if(Runner.LocalPlayer == Object.InputAuthority)
        {
            bool rightPinch = NetworkController.instance.rightHand.GetFingerIsPinching(HandFinger.Index);
            bool leftPinch = NetworkController.instance.leftHand.GetFingerIsPinching(HandFinger.Index);
            if(!rightPinch && isRightPinching)
            {
                //Stopped pinching
                blockTreating = false;
            }
            if(!leftPinch && isLeftPinching)
            {
                blockTreating = false;
            }
            isRightPinching = rightPinch;
            isLeftPinching = leftPinch;
        }
    }
    public override void Spawned()
    {
        base.Spawned();
        PlayerRef playerRef = Object.InputAuthority;
        Debug.Log("Player Spawned with Ref: " + playerRef + " and AsIndex: " + playerRef.AsIndex);
        int seat = playerRef.AsIndex % NetworkController.instance.SpawnLocations.Length;
        seatIndex = seat;
        if (playerRef == Runner.LocalPlayer)
        {
            
            LocalPlayerNetObj = this.GetComponent<NetworkObject>();
            LocalInstance = this;
            MainMenu.instance.localPlayerJoined();

            if (OVRManager.instance)
            {
                Transform spawnTransform = NetworkController.instance.SpawnLocations[seat];
                Transform _cameraRig = OVRManager.instance.GetComponentInChildren<OVRCameraRig>().transform;
                _cameraRig.position = spawnTransform.position;
                _cameraRig.rotation = spawnTransform.rotation;
            }
        }

        MainMenu.instance.playerJoined();
    }

    private void OnGameStart()
    {
        startingPosition = seatIndex;
    }
    private void AddSaveRecording(OvrAvatarEntity ent)
    {
        GameObject VoiceObject = GameGUI.gui.CenterEyeAnchor.GetChild(0).gameObject;
        if(VoiceObject != null)
        {
            Debug.Log("Adding Save Audio Script!");
            VoiceObject.AddComponent<SaveOutgoingSpeech>();
        }
    }
    private void AddColliderToAvatar(OvrAvatarEntity ent)
    {
        //Don't need and want to add the eye tracking components to the local avatar, no need to know that we look at ourselves, would cause interference. 
        if (ent != null && Runner.LocalPlayer != Object.InputAuthority)
        {
            GameObject avatarObject = ent.gameObject;
            if (avatarObject != null)
            {
                Transform chestTransform = avatarObject.transform.GetChild(0);
                if (chestTransform != null)
                {
                    GameObject chest = chestTransform.gameObject;
                    chest.AddComponent<CapsuleCollider>();
                    chest.GetComponent<CapsuleCollider>().radius = 0.15f;
                    chest.GetComponent<CapsuleCollider>().height = 0.6f;
                    chest.GetComponent<CapsuleCollider>().center = new Vector3(-0.15f, 0, 0);
                    //X direction
                    chest.GetComponent<CapsuleCollider>().direction = 0;
                    chest.layer = 9; //Eyetracking layer
                }
                Transform headTransform = avatarObject.transform.GetChild(1);
                if (headTransform != null)
                {
                    GameObject head = headTransform.gameObject;
                    head.AddComponent<CapsuleCollider>();
                    head.GetComponent<CapsuleCollider>().radius = 0.15f;
                    head.GetComponent<CapsuleCollider>().height = 0.4f;
                    head.GetComponent<CapsuleCollider>().center = new Vector3(0.1f, 0, 0);
                    //X direction
                    head.GetComponent<CapsuleCollider>().direction = 0;
                    head.layer = 9; //Eyetracking layer
                }

                //Add AoIAvatar script
                avatarObject.AddComponent<AoIAvatar>();
            }
        }
    }
    public void MoveToSeat(int position)
    {
        
        if (OVRManager.instance)
        {
            seatIndex = position;
            Transform spawnTransform = NetworkController.instance.SpawnLocations[position];
            Transform _cameraRig = OVRManager.instance.GetComponentInChildren<OVRCameraRig>().transform;
            _cameraRig.position = spawnTransform.position;
            _cameraRig.rotation = spawnTransform.rotation;
        }
    }

    void OnOverrideTracking(OvrAvatarEntity ent)
    {
        if (Object.InputAuthority == Runner.LocalPlayer)
        {
            Debug.Log("Overriding BodyTracking");
            ent.SetBodyTracking(NetworkController.instance.handTrackingInputManager);
            PlayerUtilities.DisableSyntheticHands();
        }
    }

    public bool IsSharedAuthority()
    {
        return Runner.IsSharedModeMasterClient;
    }

    public NetworkObject SpawnMoveablePawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        NetworkObject obj = Runner.Spawn(
            prefab, position, rotation, null,
            (runner, obj) => // onBeforeSpawned
            {
                obj.transform.SetParent(parent, false);
                obj.transform.rotation = rotation;
                obj.transform.position = position;
            }

        );
        return obj;
        
    }
    
    public void DespawnNetworkObject(NetworkObject obj)
    {
        Runner.Despawn(obj);
    }
    #region Start Session
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnStartSession(RpcInfo info = default)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
        RPC_OnStartSessionRelay(info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnStartSessionRelay(RpcInfo info = default)
    {
        MainMenu.instance.init();
    }
    #endregion
    #region Role Selection
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnRoleClicked(Player.Roles roleToChangeTo, int playerArea, RpcInfo info = default)
    {
        //Prevent spamming the button which could cause sync issues
        if(Time.time >= lastRPCCall + rpcCooldownTime) {
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            RPC_OnRoleClickedRelay(roleToChangeTo, playerArea, info);
            Debug.Log("Role " + roleToChangeTo + " pressed for area " + playerArea + " by " + info.Source);
            //MainMenu.instance.PlayerLoginAreas[playerArea].HandleOnRoleClicked(roleToChangeTo);
            lastRPCCall = Time.time;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnRoleClickedRelay(Player.Roles roleToChangeTo, int playerArea, RpcInfo info = default)
    {
        Debug.Log("Role " + roleToChangeTo + " pressed for area " + playerArea + " by " + info.Source);
        MainMenu.instance.PlayerLoginAreas[playerArea].HandleOnRoleClicked(roleToChangeTo);
    }
    #endregion
    #region Role Selection
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnResetMenu(RpcInfo info = default)
    {
        RPC_OnResetMenuRelay(info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnResetMenuRelay(RpcInfo info = default)
    {
        MainMenu.instance.init();
    }
    #endregion
    #region Start Game
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnPlayButton(RpcInfo info = default)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
        RPC_OnPlayButtonRelay(info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnPlayButtonRelay(RpcInfo info = default)
    {
        MainMenu.instance.OnStartGame();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnStartGameWithSeed(int playerCardSeed, int infectionCardSeed, RpcInfo info = default)
    {
        RPC_OnStartGameWithSeedRelay(playerCardSeed, infectionCardSeed, info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_OnStartGameWithSeedRelay(int playerCardSeed, int infectionCardSeed, RpcInfo info = default)
    {
        MainMenu.instance.HandleOnStartGame(playerCardSeed, infectionCardSeed);
    }
    #endregion

    #region Pawn Move
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnPawnSnap(int eventType, int playerArea, int cityID, int distance = 0, RpcInfo info = default)
    {
        RPC_OnPawnSnapRelay(eventType,playerArea, cityID, distance);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OnPawnSnapRelay(int eventType, int playerArea, int cityID, int distance = 0, RpcInfo info = default)
    {
        Player playerModel = GameGUI.PlayerPadForPosition(playerArea).PlayerModel;
        Debug.Log("Pawn snap event type " + eventType + " for " + playerModel.Role + " by " + info.Source);
        switch (eventType)
        {
            case 0:
                Timeline.theTimeline.AddEvent(new PCharterEvent(Game.theGame.Cities[cityID]));
                break;
            case 1:
                Timeline.theTimeline.AddEvent(new PMobilizeEvent(playerModel, cityID)); //1
                break;

            case 2:
                Timeline.theTimeline.AddEvent(new PMoveEvent(cityID, distance));
                break;
        }
    }
    #endregion

    #region Pawn Clicked
    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PawnClicked(Player.Roles pawnRole,int playerArea, RpcInfo info = default)
    {
        Debug.Log("Pawn " + pawnRole + " clicked for area " + playerArea + " by " + info.Source);
        Debug.Log("Finding pawn to initiate action");
        foreach(GameObject pawnObject in Game.theGame.CurrentPlayer.playerGui.pilotPawnsTagAlong)
        {
            
            Pawn pawn = pawnObject.GetComponent<Pawn>();
            Debug.Log("Checking pawn " + pawn.PawnRole);
            if (pawn.PawnRole == pawnRole)
            {
                Debug.Log("Found pawn to initiate action");
                Game.theGame.CurrentPlayer.playerGui.PawnClicked(pawn);
                break;
            }
        }
    }
    #endregion
    #region Action Button Clicked
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ActionButtonClicked(int action, int playerArea, RpcInfo info = default)
    {
        if (Time.time >= lastRPCCall + rpcCooldownTime)
        {
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            RPC_ActionButtonClickedRelay(action, playerArea, info);
            lastRPCCall = Time.time;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ActionButtonClickedRelay(int action, int playerArea, RpcInfo info = default)
    {
        
        GameGUI.PlayerPadForPosition(playerArea).HandleActionButtonClicked(action);
    }
    #endregion

    #region Context Button Clicked
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ContextButtonClicked(int context, int playerArea, RpcInfo info = default)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
        RPC_ContextButtonClickedRelay(context, playerArea, info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ContextButtonClickedRelay(int context, int playerArea, RpcInfo info = default)
    {
        
        GameGUI.PlayerPadForPosition(playerArea).HandleContextButtonClicked(context);
    }
    #endregion


    #region Cube Clicked
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_CubeClicked(VirusName virusName, int cityID, RpcInfo info = default)
    {
        RPC_CubeClickedRelay(virusName, cityID, info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CubeClickedRelay(VirusName virusName, int cityID, RpcInfo info = default)
    {
        
        Game.theGame.Cities[cityID].handleCubeClicked(virusName);
    }
    #endregion

    #region City Clicked
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_CityClicked(int cityID, RpcInfo info = default)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
        RPC_CityClickedRelay(cityID, info);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CityClickedRelay(int cityID, RpcInfo info = default)
    {
        Game.theGame.Cities[cityID].HandleClicked();
    }
    #endregion

    #region Card Clicked
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_CardInHandClicked(int cardClicked, int playerArea, RpcInfo info = default)
    {
        if(Time.time >= lastRPCCall + rpcCooldownTime)
        {
            lastRPCCall = Time.time;
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            RPC_CardInHandClickedRelay(cardClicked, playerArea, info);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CardInHandClickedRelay(int cardClicked, int playerArea, RpcInfo info = default)
    {
        
        GameGUI.PlayerPadForPosition(playerArea).HandleCardInHandClicked(cardClicked);
    }

    #endregion
}
