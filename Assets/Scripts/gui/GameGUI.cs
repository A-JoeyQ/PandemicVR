using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEditor;
using static ENUMS;
using static GameGUI;
using static Game;
using UnityEngine.EventSystems;
public class GameGUI : MonoBehaviour
{
    public static GameGUI gui = null;

    public int AnimationTimingMultiplier;

    public TextMeshProUGUI BigTextMessage;

    public Sprite[] ContextButtonTextures;

    public Texture PlayerCardBack;
    public GameObject CityCardPrefab;
    public GameObject[] Cities;
    public Material lineMaterial;
    public GameObject EventCardPrefab;
    public EventCard[] Events;

    public GameObject InfectionCardPrefab;
    public Texture InfectionCardBack;
    public GameObject cubePrefab;
    public GameObject cubeGrabBoxPrefab;

    public GameObject EpidemicCardPrefab;
    public Image EpidemicCardBoard;

    public GameObject CureVialPrefab;

    //public GameObject LoadOverlay;
    public GameObject BackgroundCanvas;
    public GameObject LinesCanvas;
    public GameObject CityCanvas;
    public GameObject TokenCanvas;
    public GameObject PlayerCanvas;
    public GameObject AnimationCanvas;
    public GameObject GameEndWin;
    public GameObject GameEndLose;

    public List<PlayerGUI> PlayerPads;
    public RoleCard[] roleCards;
    public float playerUIOpacity;

    public GameObject PlayerDeck;
    public GameObject PlayerDeckDiscard;
    public TextMeshProUGUI PlayerDeckCount;

    public GameObject InfectionDeck;
    public GameObject InfectionDiscard;
    public GameObject InfectionCardBackPrefab;
    public TextMeshProUGUI InfectionDeckCount;

    public GameObject OutbreakMarkerPrefab;
    public Transform[] OutbreakMarkerTransforms;

    public GameObject InfectionRateMarkerPrefab;
    public Transform[] InfectionRateMarkerTransforms;

    public GameObject[] VialTokens;
    public Transform[] VialTokensTransforms;

    public GameObject Pawns;
    public GameObject PawnPrefab;
    public GameObject MoveablePawnPrefab;

    public List<GameObject> RedCubes;
    public List<GameObject> YellowCubes;
    public List<GameObject> BlueCubes;
    public VirusInfo[] VirusInfos;

    public TextMeshProUGUI DebugText;

    public Transform CenterEyeAnchor;

    private Vector3[] redCubePositions;
    private Vector3[] yellowCubePositions;
    private Vector3[] blueCubePositions;

    public static GameObject CloneOnCanvas(GameObject source, GameObject targetCanvas)
    {
        GameObject movingResource = Instantiate(source);
        movingResource.SetActive(true);
        movingResource.transform.SetParent(targetCanvas.transform, false);
        movingResource.transform.rotation = source.transform.rotation;
        movingResource.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        movingResource.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        movingResource.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        movingResource.transform.position = source.transform.position;
        movingResource.GetComponent<RectTransform>().sizeDelta = source.GetComponent<RectTransform>().rect.size;
        movingResource.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        return movingResource;
    }

    void Awake()
    {
        gui = this;
    }

    void Start()
    {
        MoveCityInfoToGame();
        SaveCubesInitialPositions();
        CreateNeighborLines();
    }

    private void MoveCityInfoToGame()
    {
        theGame.Cities = new City[Cities.Length];
        for (int i = 0; i < Cities.Length; i++)
        {
            theGame.Cities[i] = Cities[i].GetComponent<City>();
        }
    }

    private void SaveCubesInitialPositions()
    {
        redCubePositions = new Vector3[RedCubes.Count];
        yellowCubePositions = new Vector3[YellowCubes.Count];
        blueCubePositions = new Vector3[BlueCubes.Count];
        for (int i = 0; i < RedCubes.Count; i++)
        {
            redCubePositions[i] = RedCubes[i].transform.position;
            yellowCubePositions[i] = YellowCubes[i].transform.position;
            blueCubePositions[i] = BlueCubes[i].transform.position;
        }
    }

    private void CreateNeighborLines()
    {
        //go through all Cities and connect them together graphically using a line based on their neighbors, don't repeat connections
        foreach (GameObject city in Cities)
        {
            City cityScript = city.GetComponent<City>();
            foreach (int neighbor in cityScript.city.neighbors)
            {
                if (neighbor > cityScript.city.cityID)
                {
                    GameObject line = new GameObject("Line - " + cityScript.transform.name + "_" + Cities[neighbor].transform.name);
                    line.transform.SetParent(LinesCanvas.transform, false);
                    line.transform.position = cityScript.transform.position;
                    line.AddComponent<LineRenderer>();
                    LineRenderer lr = line.GetComponent<LineRenderer>();
                    lr.sortingLayerName = "Lines";
                    lr.material = lineMaterial;
                    lr.startColor = Color.white;
                    lr.endColor = Color.white;
                    lr.startWidth = 0.002f; //Change for perspective view for clarity (from 0.025f)
                    lr.endWidth = 0.002f; //Change for perspective view for clarity (from 0.025f)
                    lr.SetPosition(0, cityScript.transform.position);
                    lr.SetPosition(1, Cities[neighbor].transform.position);
                }
            }
        }
    }

    void OnDestroy()
    {
        gui = null;
    }

    public static PlayerGUI CurrentPlayerPad()
    {
        if (Game.theGame.CurrentPlayer == null)
        {
            Debug.LogError("Requesting playerGUI for the current player which hasn't been set");
            return null;
        }
        return PlayerPadForPlayer(Game.theGame.CurrentPlayer);
    }

    public static PlayerGUI PlayerPadForPlayer(Player player)
    {
        return PlayerPadForPosition(player.Position);
    }

    public static PlayerGUI PlayerPadForPosition(int position)
    {
        PlayerGUI retVal = gui.PlayerPads.FirstOrDefault(p => p.Position == position);
        if (retVal == null)
            Debug.LogError("Requesting playerGUI for player at position " + position + " which doesn't exist.");
        return retVal;
    }

    public void Draw()
    {
        DrawBoard();
        DrawPlayerAreas();
    }

    public void DrawBoard()
    {
        PlayerDeckCount.text = theGame.PlayerCards.Count.ToString();
        InfectionDeckCount.text = theGame.InfectionCards.Count.ToString();

        foreach (Transform item in OutbreakMarkerTransforms)
        {
            item.gameObject.DestroyChildrenImmediate();
        }

        foreach (Transform item in InfectionRateMarkerTransforms)
        {
            item.gameObject.DestroyChildrenImmediate();
        }

        PlayerDeckDiscard.DestroyChildrenImmediate();
        if(theGame.PlayerCardsDiscard.Count > 0)
        {
            //add last card to discard pile
            theGame.AddPlayerCardToTransform(theGame.PlayerCardsDiscard.Last(), PlayerDeckDiscard.transform, false);
        }

                    
        Instantiate(InfectionRateMarkerPrefab, InfectionRateMarkerTransforms[theGame.InfectionRate].position, InfectionRateMarkerTransforms[theGame.InfectionRate].rotation, InfectionRateMarkerTransforms[theGame.InfectionRate]);
        Instantiate(OutbreakMarkerPrefab, OutbreakMarkerTransforms[theGame.OutbreakCounter].position, OutbreakMarkerTransforms[theGame.OutbreakCounter].rotation, OutbreakMarkerTransforms[theGame.OutbreakCounter]);

        DawCureVialsOnBoard();

        DrawBigContextText();

        DrawCubes();
    }

    private void DrawCubes()
    {
        for (int i = 0; i < RedCubes.Count; i++)
        {
            RedCubes[i].SetActive(i < theGame.RedCubesOnBoard);
            YellowCubes[i].SetActive(i < theGame.YellowCubesOnBoard);
            BlueCubes[i].SetActive(i < theGame.BlueCubesOnBoard);
        }
    }

    private void DawCureVialsOnBoard()
    {
        for (int i = 0; i < VialTokensTransforms.Length; i++)
        {
            VialTokensTransforms[i].gameObject.DestroyChildrenImmediate();
            if (i == (int)VirusName.Red && theGame.RedCure || i == (int)VirusName.Yellow && theGame.YellowCure || i == (int)VirusName.Blue && theGame.BlueCure)
            {
                GameObject vial = Instantiate(CureVialPrefab, VialTokensTransforms[i].position, VialTokensTransforms[i].rotation, VialTokensTransforms[i]);
                vial.GetComponent<Image>().color = gui.VirusInfos[i].virusColor;
            }
        }
    }

    public void DrawPlayerAreas()
    {
        foreach (PlayerGUI pad in gui.PlayerPads)
        {
            pad.Draw();
        }
    }

    public void DrawCurrentPlayerArea()
    {
        foreach (PlayerGUI pad in gui.PlayerPads)
        {
            if (theGame.CurrentPlayer == pad.PlayerModel)
                pad.Draw();
        }
    }

    public GameObject getCubePosition(VirusName virusName, int cubeNumber)
    {
        if(cubeNumber >= RedCubes.Count || cubeNumber < 0)
        {
            switch (virusName)
            {
                case VirusName.Red:
                    return RedCubes[0];
                case VirusName.Yellow:
                    return YellowCubes[0];
                case VirusName.Blue:
                    return BlueCubes[0];
                default:
                    Debug.LogError("Unknown virus name: " + virusName);
                    return YellowCubes[0]; //fallback
            }
        }
        switch (virusName)
        {
            case VirusName.Red:
                return RedCubes[cubeNumber];
            case VirusName.Yellow:
                return YellowCubes[cubeNumber];
            case VirusName.Blue:
                return BlueCubes[cubeNumber];
            default:
                Debug.LogError("Unknown virus name: " + virusName);
                return YellowCubes[0];
        }
    }
    public GameObject GetCubeToDuplicate(VirusInfo virusInfo, int increment)
    {
        switch (virusInfo.virusName)
        {
            case VirusName.Red:
                return RedCubes[theGame.RedCubesOnBoard + increment];
            case VirusName.Yellow:
                return YellowCubes[theGame.YellowCubesOnBoard + increment];
            case VirusName.Blue:
                return BlueCubes[theGame.BlueCubesOnBoard + increment];
            default:
                Debug.LogError("Unknown virus name: " + virusInfo.virusName);
                return null;
        }
    }

    public GameObject GetInfectionRateMarker(int targetInfectionRate)
    {
        return InfectionRateMarkerTransforms[targetInfectionRate].gameObject;
    }

    public GameObject GetOutbreakMarker(int targetOutbreak)
    {
        return OutbreakMarkerTransforms[targetOutbreak].gameObject;
    }

    public void DrawBigContextText()
    {
        switch (theGame.CurrentGameState)
        {
            case GameState.SETTINGBOARD:
                GameGUI.gui.BigTextMessage.text = "Set up phase";
                break;
            case GameState.PLAYERACTIONS:
                GameGUI.gui.BigTextMessage.text = theGame.CurrentPlayer.Name.ToString() + "'s turn";
                break;
            case GameState.DRAWPLAYERCARDS:
                GameGUI.gui.BigTextMessage.text = "Drawing Player Cards: " + Game.theGame.PlayerCardsDrawn;
                break;
            case GameState.EPIDEMIC:
                switch (theGame.epidemicGameState)
                {
                    case EpidemicGameState.EPIDEMICINCREASE:
                        GameGUI.gui.BigTextMessage.text = "Epidemic: Increase";
                        break;
                    case EpidemicGameState.EPIDEMICINFECT:
                        GameGUI.gui.BigTextMessage.text = "Epidemic: Infect";
                        break;
                    case EpidemicGameState.EPIDEMICINTENSIFY:
                        GameGUI.gui.BigTextMessage.text = "Epidemic: Intensify";
                        break;
                }
                break;
            case GameState.DRAWINFECTCARDS:
                GameGUI.gui.BigTextMessage.text = "Drawing Infection Cards";
                break;
            case GameState.OUTBREAK:
                GameGUI.gui.BigTextMessage.text = "Outbreak!";
                break;
            case GameState.GAME_OVER:
                GameGUI.gui.BigTextMessage.text = "Game Over!";
                break;
            default:
                break;
        }
    }

    private void Update()
    {
        DebugText.text = "State: " + theGame.CurrentGameState + "\n";
        DebugText.text += "Previous: " + theGame.PreviousGameState + "\n";
        if (theGame.CurrentGameState == GameState.EPIDEMIC)
            DebugText.text += "Epidemic State: " + theGame.epidemicGameState + "\n";
        if (theGame.CurrentPlayer != null)
        {
            DebugText.text += "Current Player: " + theGame.CurrentPlayer.Role + "\n";
            DebugText.text += "Action Selected: " + CurrentPlayerPad().ActionSelected + "\n";
            DebugText.text += "Actions remaining: " + CurrentPlayerPad().PlayerModel.ActionsRemaining + "\n";
            DebugText.text += "Cards State: " + CurrentPlayerPad().cardsState + "\n";
            DebugText.text += "Cards in Hand: " + string.Join(", ", CurrentPlayerPad().PlayerModel.PlayerCardsInHand) + "\n";
        }
        DebugText.text += "Infection Cards in Deck: " + string.Join(", ", theGame.InfectionCards.Skip(theGame.InfectionCards.Count-8)) + "\n";
        //add debug text to check if an animation is running
        //DebugText.text += "Animation running?: " + Timeline.theTimeline.isAnimationRunning() + "\n";
    }

}