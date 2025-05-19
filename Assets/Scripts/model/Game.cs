using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static ENUMS;
using static GameGUI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{

    public const int CALLTOMOBILIZEINDEX = 24;
    public const int FORECASTINDEX = 25;
    public const int MOBILEHOSPITALINDEX = 26;
    public const int RESOURCEPLANNINGINDEX = 27;

    public Player MobileHospitalPlayer = null;


    public enum PlayerPrefSettings
    {
        LAST_FILE_LOADED
    }


    public enum GameState
    {
        INVALID = -1,
        SETTINGBOARD,
        PLAYERACTIONS,
        DRAWPLAYERCARDS,
        EPIDEMIC,
        DRAWINFECTCARDS,
        OUTBREAK,
        GAME_OVER
    }

    public enum EventState
    {
        NOTINEVENT,
        CONFIRMINGFORECAST,
        CONFIRMINGRESOURCEPLANNING,
        CONFIRMINGCALLTOMOBILIZE,
        CONFIRMINGMOBILEHOSPITAL,
        FORECAST,
        RESOURCEPLANNING,
        CALLTOMOBILIZE,
        EXECUTINGMOBILEHOSPITAL //Only used for PlayerGUI. The game logic is controlled by the flag MobileHospitalInExecution.
    }

    public enum EpidemicGameState
    {

        EPIDEMICINCREASE,
        EPIDEMICINFECT,
        EPIDEMICINTENSIFY
    }

    public int PlayerCardsSeed = -1; // -1 randomizes the game
    public int InfectionCardsSeed = -1; // -1 randomizes the game
    
    public Random.State playerCardsRandomGeneratorState;
    public Random.State infectionCardsRandomGeneratorState;
    
    public int InfectionRate = 0;
    public int[] InfectionRateValues = new int[] { 2, 2, 3, 4 };
    public int NumberOfDrawnInfectCards = 0;

    public int OutbreakCounter = 0;
    public List<int> OutbreakTracker = new List<int>();

    public int InitialCityID = 13;

    public static Game theGame = null;

    public Player CurrentPlayer = null;

    public GameState CurrentGameState { get; private set; } = GameState.INVALID;
    public GameState PreviousGameState { get; private set; } = GameState.INVALID;
    public EpidemicGameState epidemicGameState = EpidemicGameState.EPIDEMICINCREASE;

    private bool actionsInitiated = false;
    public bool actionCompleted = false;

    public List<int> PlayerCards = null;
    public List<int> PlayerCardsDiscard = null;

    public bool UseEventCards = true;
    public List<int> InfectionCards = null;
    public List<int> InfectionCardsDiscard = null;
    public int PlayerCardsDrawn;
    public int RedCubesOnBoard = 16;
    public int YellowCubesOnBoard = 16;
    public int BlueCubesOnBoard = 16;

    public bool RedCure = false;
    public bool YellowCure = false;
    public bool BlueCure = false;
    private bool turnEnded = false;

    public EventState InEventCard = EventState.NOTINEVENT;

    public City[] Cities { get; internal set; }
    public bool MobileHospitalInExecution { get; set; }

    public void init()
    {
        PlayerCards = new List<int>();
        PlayerCardsDiscard = new List<int>();
        InfectionCards = new List<int>();
        InfectionCardsDiscard = new List<int>();

        PlayerCardsDrawn = 0;

        InfectionRate = 0;
        OutbreakCounter = 0;
        MobileHospitalInExecution = false;

    }

    public int GetCurrentInfectionRate()
    {
        return InfectionRateValues[InfectionRate];
    }

    public void test()
    {
        //if(CurrentPlayer.PlayerCardsInHand.Count < 7)
        //    Timeline.theTimeline.addEvent(new EDealCardToPlayer(CurrentPlayer));
        Timeline.theTimeline.AddEvent(new EOutbreak(Cities[InitialCityID]));
    }

    public int getNumberOfCubesOnBoard(VirusName virus)
    {
        switch (virus)
        {
            case VirusName.Red:
                return RedCubesOnBoard;
            case VirusName.Yellow:
                return YellowCubesOnBoard;
            case VirusName.Blue:
                return BlueCubesOnBoard;
        }
        return -1;
    }
    public void Update()
    {

        if (InEventCard == EventState.CALLTOMOBILIZE)
        {
            //for all players check if they are done with this event
            if (PlayerList.GetAllPlayers().All(player => player.playerGui.callToMobilizeExecuted == true))
            {
                InEventCard = EventState.NOTINEVENT;
                //Redundant with PMobilizeEvent Do
                /*foreach (Player player in PlayerList.getAllPlayers())
                {
                    player.playerGui.ChangeToInEvent(EventState.NOTINEVENT);
                }*/
            }
            else return; // This return prevents from ending the turn of the current player if not
                         // all other players are done with the callToMobilize event.
        }
        else if (InEventCard != EventState.NOTINEVENT) return;

        if (PlayerList.GetAllPlayers().Any(player => player.PlayerCardsInHand.Count > 6)) return;
        
        if (CurrentGameState == GameState.DRAWPLAYERCARDS)
        {
            //Debug.Log("No more cards in the deck !");
            //Debug.Log("Cards drawn = " + PlayerCardsDrawn);
            //Problem: When drawing playerCard = epidemic, we do that and then set the gamestate back to DrawPLAYERCARDS BUT: actionsInitiated is set to false meaning this first if-case will run and draw a card
            //This happens even if the second card we drew was the epidemic card meaning we shouldn't draw another card.
            if (!actionsInitiated)
            {
                actionsInitiated = true;
                if(PlayerCardsDrawn >= 2)
                {
                    actionCompleted = true;
                }
                else
                {
                    Debug.Log("Draw Player Card: " + CurrentGameState + " Cards available: " + PlayerCards.Count);
                    Timeline.theTimeline.AddEvent(new PDealCard(CurrentPlayer));
                }
                
            }
            if (actionCompleted)
            {
                actionCompleted = false;
                if (PlayerCardsDrawn < 2)
                {
                    actionsInitiated = false;
                }
                else if (CurrentPlayer.PlayerCardsInHand.Count < 7 && PlayerCardsDrawn == 2)
                {
                    if (CurrentGameState != GameState.EPIDEMIC)
                        SetCurrentGameState(GameState.DRAWINFECTCARDS);
                }

            }   
        }
        else if (CurrentGameState == GameState.EPIDEMIC)
        {
            if (epidemicGameState == EpidemicGameState.EPIDEMICINCREASE)
            {
                Timeline.theTimeline.AddEvent(new EIncreaseInfectionRate());
                epidemicGameState = EpidemicGameState.EPIDEMICINFECT;
                Timeline.theTimeline.AddEvent(new EDrawInfectionCard(3, true));
            }
            else if (epidemicGameState == EpidemicGameState.EPIDEMICINFECT)
            {
                if (actionCompleted)
                {
                    epidemicGameState = EpidemicGameState.EPIDEMICINTENSIFY;
                    Timeline.theTimeline.AddEvent(new EIntensify());
                    SetCurrentGameState(PreviousGameState);
                }
            }
        }
        else if (CurrentGameState == GameState.OUTBREAK)
        {
            if (actionCompleted)
            {
                OutbreakTracker.Clear();
                if (NumberOfDrawnInfectCards >= InfectionRateValues[InfectionRate] && !turnEnded) 
                    // In the case where the Outbreak is triggered on the last Draw of Infection Card, end the turn.
                {
                    turnEnded = true;
                    Timeline.theTimeline.AddEvent(new PEndTurn());
                }
                else SetCurrentGameState(GameState.DRAWINFECTCARDS);
            }
        }

        else if (CurrentGameState == GameState.DRAWINFECTCARDS)
        {
            if (NumberOfDrawnInfectCards < InfectionRateValues[InfectionRate] && !turnEnded)
            {
                if (!actionsInitiated)
                {
                    actionsInitiated = true;
                    Timeline.theTimeline.AddEvent(new EDrawInfectionCard(1, true));
                }
                if (actionCompleted)
                {
                    actionsInitiated = false;
                    actionCompleted = false;
                }
            }
            else
            {
                if (!turnEnded && theGame.actionCompleted)
                {
                    turnEnded = true;
                    Timeline.theTimeline.AddEvent(new PEndTurn());
                }
            }
        }
    }

    public void Awake()
    {
        theGame = this;
    }

    public void OnDestroy()
    {
        if (theGame == this) theGame = null;
    }

    internal void SetCurrentGameState(GameState state)
    {
        turnEnded = false;
        PreviousGameState = CurrentGameState;
        CurrentGameState = state;
        actionsInitiated = false;
        actionCompleted = false;
        if (state == GameState.PLAYERACTIONS)
        {
            PlayerCardsDrawn = 0;
            NumberOfDrawnInfectCards = 0;
        }

        if (state == GameState.EPIDEMIC)
            epidemicGameState = EpidemicGameState.EPIDEMICINCREASE;
    }

    public void IncrementNumberOfCubesOnBoard(VirusName virus, int increment)
    {
        switch (virus)
        {
            case VirusName.Red:
                RedCubesOnBoard += increment;
                break;
            case VirusName.Yellow:
                YellowCubesOnBoard += increment;
                break;
            case VirusName.Blue:
                BlueCubesOnBoard += increment;
                break;
        }
    }

    public int DistanceFromCity(int cityID1, int cityID2)
    {
        if(cityID1 == cityID2)
            return 0;

        int distance = 0;
        HashSet<int> citiesToVisit = new HashSet<int>();
        HashSet<int> citiesVisited = new HashSet<int>();
        bool foundConnection = false;

        HashSet<int> newCitiesToVisit = new HashSet<int>();

        newCitiesToVisit.UnionWith(Cities[cityID2].city.neighbors);

        for (int i = 0; i < Cities.Length; i++)
        {
            distance++;
            citiesToVisit = new HashSet<int>(newCitiesToVisit);
            newCitiesToVisit.RemoveWhere(citiesVisited.Contains);
            citiesVisited.UnionWith(citiesToVisit);

            if (citiesVisited.Contains(cityID1))
            {
                foundConnection = true;
                break;
            }

            foreach (int city in citiesToVisit)
            {
                foreach (int neighbor in Game.theGame.Cities[city].city.neighbors)
                {
                    newCitiesToVisit.Add(neighbor);
                }
            }
        }

        if (foundConnection)
        {
            return distance;
        }
        else return -1;
    }

    internal void ChangeToInEvent(EventState state, Player callToMobilizePendingPlayer = null)
    // callToMobilizePendingPlayer is an optional parameter only used when a mobile hospital event occurs during
    // an ongoing call to mobilize event. This caused the MobileHospitalPlayer to remove a cube thus interrupting
    // the call to mobilize event for that specific player.
    {
        InEventCard = state;
        Debug.Log("Change Game to EventState: " +  InEventCard);
        if(state == EventState.CALLTOMOBILIZE)
        {
            List<Vector3> MovingPawnTranslations = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(-0.8f, 0.8f, 0),
                new Vector3(0.8f, -0.8f, 0),
                new Vector3(0.8f, 0.8f, 0)
            };
            
            if (callToMobilizePendingPlayer != null)
            {
                callToMobilizePendingPlayer.playerGui.DestroyMovingPawn();
                //callToMobilizePendingPlayer.GetCurrentCityScript().addPawn(callToMobilizePendingPlayer); // The player is added back to the city after being removed by the first call to CreateMovingPawn
                callToMobilizePendingPlayer.playerGui.ChangeToInEvent(state);
                int pawnNumber = 0;
                foreach (var item in callToMobilizePendingPlayer.GetCurrentCityScript().PawnsInCity)
                {
                    if (item != null)
                    {
                        if (item.PlayerModel.Role == callToMobilizePendingPlayer.Role)
                            break;
                        pawnNumber++;
                    }
                }
                callToMobilizePendingPlayer.playerGui.CreateMovingPawn(MovingPawnTranslations[pawnNumber]);        
            }
            else
            {
                // Steps to display the moving pawns appropriately
                // 1- Get all the cities where the players are
                // 2- Iterate over those cities
                // 3- For each player in this city, translate by index
                HashSet<City> cities = new HashSet<City>();
                
                foreach (Player player in PlayerList.GetAllPlayers())
                {
                    player.playerGui.ChangeToInEvent(state);
                    cities.Add(player.GetCurrentCityScript());
                }

                foreach (City city in cities)
                {
                    int index = 0; // To bypass the null indexes caused by MissingReferenceException
                                   // (due to CreateMovingPawn removing the pawn from the city model)
                    for (int i = 0; i < city.PawnsInCity.Length; i++)
                    {
                        if (city.PawnsInCity[i])
                        {                           
                            city.PawnsInCity[i].PlayerModel.playerGui.CreateMovingPawn(MovingPawnTranslations[index]);
                            index++;
                        }
                    }

                }

                    /*List<Pawn> pawnsInCity = player.GetCurrentCityScript().PawnsInCity.ToList();
                    pawnsInCity.RemoveAll(p => p == null);
                    
                    int index = pawnsInCity.FindIndex(item => item.PlayerModel.Role == player.Role);

                    /*foreach (var item in player.GetCurrentCityScript().PawnsInCity)
                    {
                        if (item != null)
                        {
                            if (item.PlayerModel.Role == player.Role)
                                break;
                            pawnNumber++;
                        }
                    }*/
                    //player.playerGui.CreateMovingPawn(MovingPawnTranslations[index]);           
                //} 
            }
           
        }
    }

    internal void MakePlayersWait()
    {
        foreach (Player player in PlayerList.GetAllPlayers())
        {
            player.playerGui.Waiting = true;
        }
    }

    internal void RemovePlayersWait()
    {
        foreach (Player player in PlayerList.GetAllPlayers())
        {
            player.playerGui.Waiting = false;
            player.playerGui.Draw();
        }
        CurrentPlayer.playerGui.ClearSelectedAction();
    }

    public GameObject AddPlayerCardToTransform(int cardToAdd, Transform transform, bool withButtonComponent, PlayerGUI pGUI = null, Transform adjustTransform = null)
    {
        GameObject cardToAddObject;
        if (cardToAdd > 23)
        {
            cardToAddObject = Instantiate(gui.EventCardPrefab, transform);
            cardToAddObject.GetComponent<EventCardDisplay>().EventCardData = gui.Events[cardToAdd - 24];
            if (pGUI != null)
            {
                if (pGUI.selectedCards.Contains(cardToAdd))
                {
                    cardToAddObject.GetComponent<EventCardDisplay>().border.gameObject.SetActive(true);
                }
                else
                {
                    cardToAddObject.GetComponent<EventCardDisplay>().border.gameObject.SetActive(false);
                }
            }

        }
        else
        {
            cardToAddObject = Instantiate(gui.CityCardPrefab, transform);
            cardToAddObject.GetComponent<CityCardDisplay>().CityCardData = Cities[cardToAdd].city;

        }
        if (withButtonComponent)
        {
            var buttonComponent = cardToAddObject.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() => pGUI.CardInHandClicked(cardToAdd));
        }

        if (adjustTransform != null)
        {
            cardToAddObject.transform.rotation = adjustTransform.rotation;
            cardToAddObject.transform.position = adjustTransform.position;
        }

        return cardToAddObject;
    }

    internal void CubeClicked(City city, VirusName virusName)
    {
        if(MobileHospitalPlayer != null && MobileHospitalInExecution)
        {
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            Timeline.theTimeline.AddEvent(new PMobileHospitalEvent(MobileHospitalPlayer, city, virusName));
            if (CurrentPlayer.ActionsRemaining == 0)
            {
                theGame.SetCurrentGameState(GameState.DRAWPLAYERCARDS);
            }
        }
        else
        {
            if(CurrentPlayerPad() != null)
            {
                CurrentPlayerPad().CubeClicked(city, virusName);
            }
        } 
            
    }
    public City GetCityById(int cityId)
    {
        foreach (City city in Cities)
        {
            if (city.city != null && city.city.cityID == cityId)
            {
                return city;
            }
        }
        return null;
    }
}
