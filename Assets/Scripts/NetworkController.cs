using Fusion;
using Oculus.Interaction.AvatarIntegration;
using Oculus.Interaction.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
    public Hand leftHand;
    public Hand rightHand;


    public TMPro.TMP_Text StatusText;
    public GameObject[] GameModeButtons;
    public static NetworkController instance;
    public Transform[] SpawnLocations;

    public HandTrackingInputManager handTrackingInputManager;

    public NetworkRunner runner;
    public void Awake()
    {
        
        if(instance == null)
            instance = this;
    }

    public void Update(){
        //If press S key, start as server
        if(Input.GetKeyDown(KeyCode.S)){
            StartAsServer();
        }
        if(Input.GetKeyDown(KeyCode.C)){
            StartAsClient();
        }
        if(Input.GetKeyDown(KeyCode.J)){
            StartAsShared();
        }
    }
    public void StartAsServer()
    {
        StatusText.text = "Starting as Server...";
        StartGame(GameMode.Server);
    }
    public void StartAsClient()
    {
        StatusText.text = "Joining as Client...";
        StartGame(GameMode.Client);
    }

    public void StartAsShared()
    {
        StatusText.text = "Joining in Shared mode...";
        StartGame(GameMode.Shared);
    }
    async void StartGame(GameMode mode)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
        foreach (var button in GameModeButtons)
        {
            button.GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        // Create the Fusion runner and let it know that we will be providing user input
        //_runner = gameObject.AddComponent<NetworkRunner>();
        //_runner.ProvideInput = true;

        // Create the NetworkSceneInfo from the current scene
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        // Start or join (depends on gamemode) a session with a specific name
        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoin(NetworkRunner runner, PlayerRef playerRef)
    {
        NetworkObject netObj = runner.GetPlayerObject(playerRef);
        

    }
}
