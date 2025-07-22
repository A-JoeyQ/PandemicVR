using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;
using Oculus.Interaction;
using UnityEngine.Events;
using Fusion;
public class MainMenu : MonoBehaviour
{

    //public GameObject LoadCanvas;
    public GameObject MainMenuCanvas;
    public GameObject[] GameCanvases;

    public Button PlayButton;
    public Button ResumeButton;
    public Button StartSessionButton;
    public TMPro.TMP_Text PlayerCountText;

    public GameObject MultiplayerMenu;

    public Button[] MenuButtons;

    public static MainMenu instance;

    //public GameObject SavedGameEntryPrefab;

    // Use this for initialization
    public string[] PlayerNames; 
    public PlayerLoginArea[] PlayerLoginAreas;
    public HashSet<Player.Roles> FreeRoles = new HashSet<Player.Roles>();

    //[Networked, Capacity(4), OnChangedRender(nameof(FreeRolesChanged))]
    //public NetworkLinkedList<Player.Roles> FreeRolesNet { get; }

    public static float startTimestamp;

    public UnityEvent OnGameStart;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        foreach (var canvas in GameCanvases) 
        {
            canvas.SetActive(false);
        }
        MainMenuCanvas.SetActive(false);
        //LoadCanvas.SetActive(false);
        foreach (var button in MenuButtons)
        {
            button.interactable = false;
        }

        foreach (PlayerLoginArea area in PlayerLoginAreas)
            area.enabled = false;
    }

    private void FreeRolesChanged()
    {
        Debug.Log("network update roles.");
        /*FreeRoles.Clear();
        foreach(int role in FreeRolesNet)
        {
            FreeRoles.Add((Player.Roles)role);
        }
        UpdateRoles();*/
    }

    void Start()
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.INTRO, 0, 0.2f);
    }

    public void localPlayerJoined()
    {
        MultiplayerMenu.SetActive(false);
        MainMenuCanvas.SetActive(true);
    }
    public void playerJoined(){
        PlayerCountText.text = "Players: " + NetworkPlayer.LocalPlayerNetObj.Runner.ActivePlayers.Count();
    }
    public void init()
    {
        FreeRoles.Clear();
        StartSessionButton.gameObject.SetActive(false);
        foreach (var button in MenuButtons)
        {
            button.interactable = true;
        }

        foreach (PlayerLoginArea area in PlayerLoginAreas)
            area.enabled = true;

        setPlayButtonState();

        if (PlayerPrefs.GetString(Game.PlayerPrefSettings.LAST_FILE_LOADED.ToString(), "NONE") == "NONE")
            ResumeButton.interactable = false;

        foreach (Player.Roles role in Enum.GetValues(typeof(Player.Roles)))
            AddRole(role);

        foreach (PlayerLoginArea area in PlayerLoginAreas)
            area.ResetPlayerLoginArea();
    }


    public void OnResetMenu()
    {
        NetworkPlayer.LocalInstance.RPC_OnResetMenu();
    }
    public void OnStartSession()
    {
        NetworkPlayer.LocalInstance.RPC_OnStartSession();
    }
    public void HandleOnStartGame(int playerCardSeed, int infectionCardSeed)
    {
        OnGameStart.Invoke();
        // Capture the start timestamp (used as an offset for the logs timestamps)
        startTimestamp = Time.time;

        // Stop playing the intro music
        AudioPlayer.Stop();
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);

        // Turn off the main menu and start the game
        MainMenuCanvas.SetActive(false);
        foreach (var canvas in GameCanvases)
        {
            canvas.SetActive(true);
        }

        Game.theGame.InfectionCardsSeed = infectionCardSeed;
        Game.theGame.PlayerCardsSeed = playerCardSeed;
        // Init the timeline with initial events
        Timeline.theTimeline.ResetTimeline();
        Timeline.theTimeline.AddEvent(new EResetGame());
        foreach (PlayerLoginArea area in PlayerLoginAreas)
        {
            if (area.IsPlaying())
                Timeline.theTimeline.AddEvent(new EAddPlayer(area.Position, area.Role.Value, area.PlayerName));
        }

        EInitialize initEvt = new EInitialize();

        Timeline.theTimeline.AddEvent(initEvt);

        string saveName = Timeline.theTimeline.Save(null);
        PlayerPrefs.SetString(Game.PlayerPrefSettings.LAST_FILE_LOADED.ToString(),
          saveName);
    }
    public void OnStartGame()
    {
        

        //Not used for now
        if (NetworkPlayer.LocalPlayerNetObj.HasInputAuthority)
        {
            //Niklas: Done to synchronize the seed across all
            
        }

    }
    public void OnPlayButton()
    {

        // The one that press the start button synchronize its random state. SHOULD BE MOVED TO ONPLAYBUTTON which is the one that registers the actual click.
        int randomSeed = Mathf.Abs(System.DateTime.UtcNow.Ticks.GetHashCode());
        int playerCardSeed = Game.theGame.PlayerCardsSeed == -1 ? randomSeed : Game.theGame.PlayerCardsSeed;
        int infectionCardSeed = Game.theGame.InfectionCardsSeed == -1 ? randomSeed : Game.theGame.InfectionCardsSeed;

        NetworkPlayer.LocalInstance.RPC_OnStartGameWithSeed(playerCardSeed, infectionCardSeed);
    }

    //public void OnLoadButton()
    //{
    //    AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
    //    // Bring up (or hide) the Load screen
    //    MainMenuCanvas.SetActive(!MainMenuCanvas.activeInHierarchy);
    //    LoadCanvas.SetActive(!LoadCanvas.activeInHierarchy);

    //    if (LoadCanvas.activeInHierarchy)
    //        StartCoroutine(populateSavedGameList());
    //    else
    //        setPlayButtonState();
    //}

    public void OnResumeButton()
    {
        loadGame(PlayerPrefs.GetString(Game.PlayerPrefSettings.LAST_FILE_LOADED.ToString()));
    }
    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    //IEnumerator populateSavedGameList()
    //{
    //    Transform savedGameList = LoadCanvas.transform.Find("Panel/Scroll View/Viewport/Content");
    //    savedGameList.gameObject.DestroyChildrenImmediate();

    //    Directory.CreateDirectory(Application.persistentDataPath + "/savedGames");
    //    foreach (string dirName in Directory.GetDirectories(Application.persistentDataPath + "/savedGames").Reverse())
    //    {
    //        DateTime gameStart = DateTime.MinValue;
    //        string timeString;
    //        string strippedDirName;
    //        try
    //        {
    //            strippedDirName = dirName.Split('\\').Last();
    //            string[] dateParts = strippedDirName.Split('_'); // yyyy_MM_dd_HH_mm_ss
    //            int year = int.Parse(dateParts[0]);
    //            int month = int.Parse(dateParts[1]);
    //            int day = int.Parse(dateParts[2]);
    //            int hour = int.Parse(dateParts[3]);
    //            int minute = int.Parse(dateParts[4]);
    //            int second = int.Parse(dateParts[5]);
    //            gameStart = new DateTime(year, month, day, hour, minute, second);
    //            timeString = " @ ";
    //            if (hour < 10) timeString += "0" + hour; else timeString += hour;
    //            timeString += ":";
    //            if (minute < 10) timeString += "0" + ((minute / 5) * 5); else timeString += ((minute / 5) * 5);
    //        }
    //        catch (Exception)
    //        {
    //            Debug.Log("Skipping directory: " + dirName);
    //            //Debug.LogException(e);
    //            continue;
    //        }
    //        DateTime midnightToday = DateTime.Today;
    //        GameObject saveEntry = Instantiate(SavedGameEntryPrefab);
    //        saveEntry.transform.SetParent(savedGameList);
    //        saveEntry.transform.localScale = Vector3.one;

    //        RawImage screenCapture = saveEntry.transform.Find("Screen Capture").GetComponent<RawImage>();
    //        Text dateText = saveEntry.transform.Find("Date Text").GetComponent<Text>();
    //        Button loadButton = saveEntry.GetComponent<Button>();

    //        if (gameStart >= midnightToday)
    //            dateText.text = "Today" + timeString;
    //        else if (gameStart >= midnightToday.AddDays(-1))
    //            dateText.text = "Yesterday" + timeString;
    //        else if (gameStart >= midnightToday.AddDays(-6))
    //            dateText.text = gameStart.DayOfWeek.ToString() + timeString;
    //        else
    //            dateText.text = gameStart.ToString("d MMM yy") + timeString;

    //        loadButton.onClick.AddListener(() => loadGame(strippedDirName));

    //        if (File.Exists(dirName + "/Snapshot.png"))
    //        {
    //            string url = "file:///" + dirName.Replace("\\", "/").Replace(" ", "%20") + "/Snapshot.png";
    //            //var www = new WWW(url);
    //            //yield return www;
    //            //Texture2D tex = new Texture2D(www.texture.width, www.texture.height);
    //            //www.LoadImageIntoTexture(tex);
    //            //screenCapture.texture = tex;

    //            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
    //            {
    //                yield return webRequest.SendWebRequest();

    //                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
    //                {
    //                    Debug.Log(webRequest.error);
    //                }
    //                else
    //                {
    //                    // Get downloaded texture once the request has completed
    //                    Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
    //                    screenCapture.texture = tex;
    //                }
    //            }
    //        }
    //    }
    //}

    public void loadGame(string name)
    {
        // Stop the intro music
        AudioPlayer.Stop();
        PlayerPrefs.SetString(Game.PlayerPrefSettings.LAST_FILE_LOADED.ToString(), name);
        // Startup the game
        MainMenuCanvas.SetActive(false);
        //LoadCanvas.SetActive(false);
        foreach (var canvas in GameCanvases)
        {
            canvas.SetActive(true);
        }
        // Init the timeline with the saved game
        Timeline.theTimeline.ResetTimeline();
        Timeline.theTimeline.ReprocessEvents(Timeline.Load(name));
    }


    public void setPlayButtonState()
    {
        int count = PlayerLoginAreas.Count(o => o.IsPlaying());
        PlayButton.interactable = count >= PlayerList.MIN_PLAYERS &&
          count <= PlayerList.MAX_PLAYERS;
    }
    public void RemoveRole(Player.Roles role)
    {
        FreeRoles.Remove(role);
        //FreeRolesNet.Remove(role);
    }
    public void AddRole(Player.Roles role)
    {
        FreeRoles.Add(role);
        /*if (!FreeRolesNet.Contains(role))
        {
            FreeRolesNet.Add(role);
        }*/
    }
    internal void UpdateRoles()
    {
        foreach (PlayerLoginArea area in PlayerLoginAreas)
        {
            //HashSet<Player.Roles> freeRoles = FreeRolesNet.ToHashSet();
            area.UpdateRole(FreeRoles);
        }
        setPlayButtonState();
    }
}
