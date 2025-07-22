using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using static ENUMS;
using static Game;
using Fusion;

public class PlayerGUI : MonoBehaviour
{

    #region Properties and Fields
    internal bool Waiting = false;
    GameGUI gameGui;
    Game game;

    bool _isAnimating = false;
    List<int> _drawnCards = new List<int>();
    internal GameObject movingPawn = null;
    internal NetworkObject movingPawnNetwork = null;
    public CardGUIStates cardsState { get; private set; }
    public List<int> selectedCards;
    private GameObject flyLine;
    private GameObject flyLine2;
    private static List<PlayerGUI> playersToShareGUI;
    private static bool shareCardFromOtherPlayerToCurrent = false;


    public int Position;

    public ActionTypes ActionSelected;

    Player _player = null;
    public TMPro.TextMeshProUGUI CurrentInstructionText;
    public TMPro.TextMeshProUGUI playerNameText;
    
    public RoleCardDisplay roleCard;
    public Image roleCardBackground { get; private set; }

    public GameObject PlayerCards;
    private List<GameObject> cardsInHand;

    public GameObject ActionsContainer;
    public GameObject MoveAction;
    private Image MoveActionBackground;
    public GameObject FlyAction;
    private Image FlyActionBackground;
    public GameObject CharterAction;
    private Image CharterActionBackground;
    public GameObject TreatAction;
    private Image TreatActionBackground;
    public GameObject ShareAction;
    private Image ShareActionBackground;
    public GameObject FindCureAction;
    private Image FindCureActionBackground;
    public GameObject EndTurnAction;
    private Image EndTurnActionBackground;

    public GameObject[] ContextButtons;
    private ContextButtonStates contextButtonState;

    public GameObject [] ForeCastEventCards;
    internal List<int> ForeCastEventCardsIDs = new List<int>();
    internal int ForeCastEventCardSelected = -1;

    public GameObject[] ResourcePlanningEventCardsCities;
    public GameObject[] ResourcePlanningEventCardsEvents;
    public GameObject[] ResourcePlanningEventCardsEpidemic;
    
    internal List<int> ResourcePlanningEventCardsIDs = new List<int>();
    internal int ResourcePlanningEventCardSelected = -1;

    public EventState pInEvent = EventState.NOTINEVENT;
    
    private const int MAX_CARDS = 5;
    private const int MAX_SAME_COLOR_CARDS = 4;

    public bool callToMobilizePending = false;
    internal bool callToMobilizeExecuted = false;

    public Player PlayerModel
    {
        get { if (_player == null) _player = PlayerList.playerAtPosition(Position); return _player; }
        set { _player = value; }
    }

    public GameObject GetCardInHand(int cardID)
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardID < 24)
            {
                CityCardDisplay cityCard = cardsInHand[i].GetComponent<CityCardDisplay>();
                if (cityCard != null)
                {
                    if (cityCard.CityCardData.cityID == cardID)
                    {
                        return cardsInHand[i];
                    }
                }
            }
            else
            {
                EventCardDisplay eventCard = cardsInHand[i].GetComponent<EventCardDisplay>();
                if (eventCard != null)
                {
                    if (eventCard.EventCardData.eventID == cardID)
                    {
                        return cardsInHand[i];
                    }
                }
            }
        }
        return null;
    }

    public GameObject getFirstCardInHand()
    {
        if (cardsInHand.Count > 0)
        {
            return cardsInHand[0];
        }
        return null;
    }

    private int pilotCitySelected = -1;

    public GameObject[] pilotPawnsTagAlong;
    private Pawn pawnPilotSelected = null;
    private HorizontalLayoutGroup layout;

    #endregion

    public void Init()
    {
        gameGui = GameGUI.gui;
        game = Game.theGame;
        
        ActionSelected = ActionTypes.None;
        MoveActionBackground = MoveAction.transform.Find("highlight").GetComponent<Image>();
        FlyActionBackground = FlyAction.transform.Find("highlight").GetComponent<Image>();
        CharterActionBackground = CharterAction.transform.Find("highlight").GetComponent<Image>();
        TreatActionBackground = TreatAction.transform.Find("highlight").GetComponent<Image>();
        ShareActionBackground = ShareAction.transform.Find("highlight").GetComponent<Image>();
        FindCureActionBackground = FindCureAction.transform.Find("highlight").GetComponent<Image>();
        EndTurnActionBackground = EndTurnAction.transform.Find("highlight").GetComponent<Image>();
        roleCardBackground = roleCard.transform.Find("background").GetComponent<Image>();

        _player = null;
        cardsInHand = new List<GameObject>();
        roleCard.RoleCardData = GameGUI.gui.roleCards[(int)PlayerModel.Role];
        roleCard.gameObject.SetActive(true);
        Color targeColor = roleCard.RoleCardData.roleColor;
        targeColor.a = GameGUI.gui.playerUIOpacity;
        GetComponent<Image>().color = targeColor;

        changeContextText();

        playersToShareGUI = new List<PlayerGUI>();

        playerNameText.text = PlayerModel.Name;
        selectedCards = new List<int>();

        layout = PlayerCards.GetComponent<HorizontalLayoutGroup>();
    }

    public void Draw()
    {
        if (_isAnimating || PlayerModel == null) return;
        
        createCardsInHand();
        if (_player.PlayerCardsInHand.Count > 6)
        {
            drawHandleDiscard();
        }
        else if (cardsState != CardGUIStates.None || PlayerModel != game.CurrentPlayer)
        {
            changeHorizontalLayout(option: 2);
        }
        else 
        {
            changeHorizontalLayout(option: 3);
        }
        
        // When CallToMobilize is done, disable all the context buttons
        // Event handling
        if (pInEvent != EventState.NOTINEVENT && _player.PlayerCardsInHand.Count <= 6)
        {
            drawEventHandling();
        }
        
        else if (PlayerModel == game.CurrentPlayer) {
            ownTurnActionHandling(); 
        }
        else {
            notMyTurnHandling();
        }

        changeContextButtons();
        changeContextText();
    }

    private void changeContextButtons()
    {
        if (cardsState == CardGUIStates.None && pInEvent == EventState.NOTINEVENT)
        {
            EnableContextButtons(false, false, false, false, false, false);
            return;
        }
            
        switch (cardsState)
        {
            case CardGUIStates.CardsExpandedShareAction: //Might be redundant with notMyTurnHandling
                if ((shareCardFromOtherPlayerToCurrent && _player == game.CurrentPlayer) || (_player != game.CurrentPlayer && !shareCardFromOtherPlayerToCurrent))
                    EnableContextButtons(false, true, false, false, false, false);
                else EnableContextButtons(false, false, false, false, false, false);
                break;
            
            case CardGUIStates.CardsExpandedCharterActionToSelect:
                EnableContextButtons(false, true, false, false, false, false);
                break;
            
            case CardGUIStates.CardsExpanded:
            case CardGUIStates.CardsExpandedFlyActionToSelect:
            case CardGUIStates.CardsExpandedCureActionToSelect:
            //case CardGUIStates.CardsExpandedVirologistAction:
                EnableContextButtons(true, false, false, false, false, false);
                break;
            
            case CardGUIStates.CardsDiscarding:
                if(selectedCards.Count == 0) 
                    EnableContextButtons(false, false, false, false, false, false);
                else
                    EnableContextButtons(false, selectedCards[0] > 23, true, false, false, false);
                break;
        }

        switch (pInEvent)
        {
            case EventState.CONFIRMINGCALLTOMOBILIZE:
            case EventState.CONFIRMINGRESOURCEPLANNING:
            case EventState.CONFIRMINGMOBILEHOSPITAL:
            case EventState.CONFIRMINGFORECAST:
                EnableContextButtons(true, true, false, false, false, false);
                break;

            case EventState.FORECAST:
            case EventState.RESOURCEPLANNING:
                EnableContextButtons(false, true, false, true, true, false);
                break;
        }


    }

    private void notMyTurnHandling()
    {
        enableOwnTurnActions(false);
        if (cardsState == CardGUIStates.CardsExpandedShareAction)
        {
            if (PlayerModel.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
            {
                GetCardInHand(PlayerModel.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                EnableContextButtons(false, true, false, false, false, false);
            } else if (_player.PlayerCardsInHand.Contains(PlayerModel.GetCurrentCity()))
            {
                GetCardInHand(_player.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                EnableContextButtons(false, true, false, false, false, false);
            }
        }
    }

    private void createCardsInHand()
    {
        cardsInHand.Clear();
        PlayerCards.DestroyChildrenImmediate();

        foreach (int cardToAdd in _player.PlayerCardsInHand)
        {
            cardsInHand.Add(game.AddPlayerCardToTransform(cardToAdd, PlayerCards.transform, true, this));
            
        }
    }

    private void drawHandleDiscard()
    {
        changeHorizontalLayout(option: 1);
        UpdateCardsState(CardGUIStates.CardsDiscarding, false); // Not redrawing : infinite loop
        
        if (selectedCards.Count > 0)
        {
            //ContextButtons[1].SetActive(false);
            if (selectedCards[0] < 24)
            {
                GetCardInHand(selectedCards[0]).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
            }
            else
            {
                GetCardInHand(selectedCards[0]).GetComponent<EventCardDisplay>().border.gameObject.SetActive(true);
                ContextButtons[1].SetActive(true);
            }
            ContextButtons[2].SetActive(true);
        }
    }

    private void drawEventHandling()
    {
        if(pInEvent == EventState.FORECAST)
        {
            PlayerCards.SetActive(false);
            roleCard.gameObject.SetActive(false);

            ForeCastEventCards[0].transform.parent.gameObject.SetActive(true);

            for (int i = 0; i < ForeCastEventCards.Length; i++)
            {
                if (i <= ForeCastEventCardsIDs.Count - 1)
                {
                    CityCard infoCard = theGame.Cities[ForeCastEventCardsIDs[i]].city;
                    InfectionCardDisplay cardDisplay = ForeCastEventCards[i].GetComponentInChildren<InfectionCardDisplay>();
                    cardDisplay.CityCardData = infoCard;
                    ForeCastEventCards[i].SetActive(true);

                    if (infoCard.cityID == ForeCastEventCardSelected)
                    {
                        cardDisplay.border.gameObject.SetActive(true);
                    }
                    else
                    {
                        cardDisplay.border.gameObject.SetActive(false);
                    }
                }
                else ForeCastEventCards[i].SetActive(false);
            }

        }
        else if (pInEvent == EventState.CALLTOMOBILIZE)
        {
            EnableContextButtons(false, !callToMobilizeExecuted, false, false, false, false);
            if (!callToMobilizeExecuted)
            {
                MoveAction.SetActive(false);
                FlyAction.SetActive(false);
                CharterAction.SetActive(false);
                TreatAction.SetActive(false);
                FindCureAction.SetActive(false);
                ShareAction.SetActive(false);
                EndTurnAction.SetActive(false);
            }
        }
        else if (pInEvent == EventState.EXECUTINGMOBILEHOSPITAL)
        {
            if(theGame.MobileHospitalInExecution && theGame.MobileHospitalPlayer == PlayerModel) enableOwnTurnActions(false);
            if(!theGame.MobileHospitalInExecution) ownTurnActionHandling(); 
            EnableContextButtons(false, false, false, false, false, false);
        }
        else if (pInEvent == EventState.RESOURCEPLANNING)
        {

            PlayerCards.SetActive(false);
            roleCard.gameObject.SetActive(false);

            ResourcePlanningEventCardsCities[0].transform.parent.parent.gameObject.SetActive(true);
            
            for (int i = 0; i < ResourcePlanningEventCardsCities.Length; i++)
            {
                if (i <= ResourcePlanningEventCardsIDs.Count - 1)
                {
                    if (ResourcePlanningEventCardsIDs[i] < 24)
                    {
                        ResourcePlanningEventCardsEpidemic[i].SetActive(false);
                        ResourcePlanningEventCardsEvents[i].SetActive(false);
                        
                        CityCard infoCard = theGame.Cities[ResourcePlanningEventCardsIDs[i]].city;
                        CityCardDisplay cardDisplay = ResourcePlanningEventCardsCities[i].GetComponentInChildren<CityCardDisplay>();
                        cardDisplay.CityCardData = infoCard;
                        
                        ResourcePlanningEventCardsCities[i].SetActive(true);

                        if (infoCard.cityID == ResourcePlanningEventCardSelected)
                        {
                            cardDisplay.border.gameObject.SetActive(true);
                        }
                        else
                        {
                            cardDisplay.border.gameObject.SetActive(false);
                        }
                    }
                    else if (ResourcePlanningEventCardsIDs[i] < 28)
                    {
                        ResourcePlanningEventCardsEpidemic[i].SetActive(false);
                        ResourcePlanningEventCardsCities[i].SetActive(false);
                        
                        EventCard infoCard = GameGUI.gui.Events[ResourcePlanningEventCardsIDs[i] - 24];
                        EventCardDisplay cardDisplay = ResourcePlanningEventCardsEvents[i].GetComponentInChildren<EventCardDisplay>();
                        cardDisplay.EventCardData = infoCard;
                        
                        ResourcePlanningEventCardsEvents[i].SetActive(true);
                        
                        if (infoCard.eventID == ResourcePlanningEventCardSelected)
                        {
                            cardDisplay.border.gameObject.SetActive(true);
                        }
                        else
                        {
                            cardDisplay.border.gameObject.SetActive(false);
                        }

                    }
                    else if (ResourcePlanningEventCardsIDs[i] == 28)
                    {
                        ResourcePlanningEventCardsCities[i].SetActive(false);
                        ResourcePlanningEventCardsEvents[i].SetActive(false);

                        EpidemicCardDisplay cardDisplay = ResourcePlanningEventCardsEpidemic[i]
                            .GetComponentInChildren<EpidemicCardDisplay>();
                        
                        ResourcePlanningEventCardsEpidemic[i].SetActive(true);
                        
                        if (ResourcePlanningEventCardSelected == 28)
                        {
                            cardDisplay.border.gameObject.SetActive(true);
                        }
                        else
                        {
                            cardDisplay.border.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    public void EnableContextButtons(bool reject, bool accept, bool discard, bool left, bool right, bool reject2)
    {
        ContextButtons[0].SetActive(reject);
        ContextButtons[1].SetActive(accept);
        ContextButtons[2].SetActive(discard);
        ContextButtons[3].SetActive(left);
        ContextButtons[4].SetActive(right);
        ContextButtons[5].SetActive(reject2);
    }

    public Dictionary<string, string> availableActions()
    {   
        if(PlayerModel == null) return new Dictionary<string, string>();

        bool moveAction = false;
        bool flyAction = false;
        bool charterAction = false;
        bool findCureAction = false;
        bool treatAction = false;
        bool shareAction = false;
        bool roleAction = false;
        bool endTurnAction = false;

        //changeContextText(true);
        if (PlayerModel.ActionsRemaining > 0)
        {
            if (cardsState == CardGUIStates.None)
            {
                endTurnAction = true;

                moveAction = true;
                List<int>[] cardsOfEachColor = new List<int>[3];
                foreach (int card in PlayerModel.CityCardsInHand)
                {
                    if (card != PlayerModel.GetCurrentCity())
                        flyAction = true;
                    else
                        charterAction = true;
                }

/*                if (PlayerModel.GetCurrentCityScript().CubesInCity() || (_player.Role == Player.Roles.Virologist
                                                                         && !_player.secondRoleActionUsed))
                {
                    treatAction = true;
                }*/

                if (ableToFindCure())
                    if (PlayerModel.GetCurrentCity() == game.InitialCityID)
                        findCureAction = true;

                /*
                if (playersToShareGUI.Count > 0)
                {
                    //foreach (PlayerGUI playerGUI in playersToShareGUI) playerGUI.UpdateCardsState(CardGUIStates.None, false);
                    //playersToShareGUI.Clear();
                }
                */


                int countOtherPlayerInCity = 0;
                foreach (Player player in PlayerModel.GetCurrentCityScript().PlayersInCity)
                {
                    if (player != PlayerModel)
                    {
                        countOtherPlayerInCity++;
                        bool localShareCardFromOtherPlayerToCurrent = player.PlayerCardsInHand.Contains(_player.GetCurrentCity());

                        if (localShareCardFromOtherPlayerToCurrent || _player.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
                        {
                            shareAction = true;
                            //playersToShareGUI.Add(GameGUI.playerPadForPosition(player.Position));

                            if (localShareCardFromOtherPlayerToCurrent) break;
                        }
                    }
                }
            }

/*            if (_player.Role == Player.Roles.Virologist || _player.Role == Player.Roles.Pilot)
            {


                if (_player.Role == Player.Roles.Virologist)
                {
                    if (PlayerModel.CityCardsInHand.Count == 0 || PlayerModel.roleActionUsed) roleAction = false;
                    else roleAction = true;
                }
                else
                {
                    roleAction = true;
                }
            }*/
        }
        Dictionary<string, string> result = new Dictionary<string, string>
        {
            { "moveAction", bool2string(moveAction) },
            { "flyAction", bool2string(flyAction) },
            { "charterAction", bool2string(charterAction) },
            { "findCureAction", bool2string(findCureAction) },
            { "treatAction", bool2string(treatAction) },
            { "shareAction", bool2string(shareAction) },
            { "roleAction", bool2string(roleAction) },
            { "endTurnAction", bool2string(endTurnAction) }
        };

        return result;
    }
    private string bool2string(bool value)
    {
        return value ? "true" : "false";
    }
    private void ownTurnActionHandling()
    {
        bool moveAction = false;
        bool flyAction = false;
        bool charterAction = false;
        bool findCureAction = false;
        bool treatAction = false;
        bool shareAction = false;
        bool endTurnAction = false;

        //changeContextText(true);
        if (PlayerModel.ActionsRemaining > 0 && !Waiting)
        {
            if (cardsState == CardGUIStates.None)
            {
                endTurnAction = true;

                moveAction = true;
                List<int>[] cardsOfEachColor = new List<int>[3];
                foreach (int card in PlayerModel.CityCardsInHand)
                {
                    if (card != PlayerModel.GetCurrentCity())
                        flyAction = true;
                    else
                        charterAction = true;
                }

/*                if (PlayerModel.GetCurrentCityScript().CubesInCity() || (_player.Role == Player.Roles.Virologist 
                                                                         && !_player.secondRoleActionUsed)) 
                {
                    treatAction = true;
                }*/

                if (ableToFindCure())
                    if (PlayerModel.GetCurrentCity() == game.InitialCityID)
                        findCureAction = true;

                if (playersToShareGUI.Count > 0)
                { 
                    foreach (PlayerGUI playerGUI in playersToShareGUI) playerGUI.UpdateCardsState(CardGUIStates.None, false);
                    playersToShareGUI.Clear();
                }
                

                int countOtherPlayerInCity = 0;
                foreach (Player player in PlayerModel.GetCurrentCityScript().PlayersInCity)
                {
                    if (player != PlayerModel)
                    {
                        countOtherPlayerInCity++;
                        shareCardFromOtherPlayerToCurrent = player.PlayerCardsInHand.Contains(_player.GetCurrentCity());
                        
                        if (shareCardFromOtherPlayerToCurrent || _player.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
                        {
                            shareAction = true;
                            playersToShareGUI.Add(GameGUI.PlayerPadForPosition(player.Position));
                            
                            if (shareCardFromOtherPlayerToCurrent) break;
                        }
                    }
                }
            }
            else
            {
                if (cardsState == CardGUIStates.CardsExpandedCharterActionToSelect)
                {
                    GetCardInHand(PlayerModel.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                }
                
                if (cardsState == CardGUIStates.CardsExpandedShareAction)
                {
                    // Case 1: this (PlayerGui) sends the card to someone else.
                    if (PlayerModel.CityCardsInHand.Contains(_player.GetCurrentCity())) 
                        GetCardInHand(PlayerModel.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                    
                    // Case 2: PlayerGUI != this sends the card to this
                    else playersToShareGUI[0].GetCardInHand(playersToShareGUI[0].PlayerModel.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                }
            }
        }

        MoveAction.SetActive(moveAction);
        FlyAction.SetActive(flyAction);
        TreatAction.SetActive(treatAction);
        CharterAction.SetActive(charterAction);
        FindCureAction.SetActive(findCureAction);
        ShareAction.SetActive(shareAction);
        EndTurnAction.SetActive(endTurnAction);
    }

    private void changeHorizontalLayout(int option)
    {
        if (option == 1)
        {
            layout.gameObject.transform.localScale = new Vector3(0.86f, 0.86f, 1f);
            layout.padding.left = -695;
            layout.spacing = 31f;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;
        }
        else if (option == 2)
        {
            layout.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            layout.spacing = 30;
            layout.padding.left = -585;
            layout.childForceExpandWidth = false;
            layout.childControlWidth = false;
        }
        else if (option == 3)
        {
            layout.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
            layout.spacing = 22.5f;
            layout.padding.left = -15;
            layout.childForceExpandWidth = true;
            layout.childControlWidth = true;
        }
    }

    private bool ableToFindCure()
    {
        return (!game.RedCure && PlayerModel.RedCardsInHand.Count > 3) ||
               (!game.YellowCure && PlayerModel.YellowCardsInHand.Count > 3) ||
               (!game.BlueCure && PlayerModel.BlueCardsInHand.Count > 3);// ||
              // isVirologistAbleToCure();

    }

/*    private bool isVirologistAbleToCure()
    {
        return _player.Role == Player.Roles.Virologist && PlayerModel.CityCardsInHand.Count >= 5 && (
            (!game.RedCure && PlayerModel.RedCardsInHand.Count == 3) ||
            (!game.YellowCure && PlayerModel.YellowCardsInHand.Count == 3) ||
            (!game.BlueCure && PlayerModel.BlueCardsInHand.Count == 3)
        );
    }*/

    #region Buttons

    public void ForecastInfectionCardClicked(int position)
    {
        ForeCastEventCardSelected = ForeCastEventCardsIDs[position];
        Draw();
    }

    public void ResourcePlanningInfectionCardClicked(int position)
    {
        ResourcePlanningEventCardSelected = ResourcePlanningEventCardsIDs[position];
        Draw();
    }

    public void HandleContextButtonClicked(int buttonType)
    {
        switch (buttonType)
        {
            case 0:
                Timeline.theTimeline.AddEvent(new GContextButtonClicked("CloseButton"));
                CloseButtonClicked();
                break;
            case 1:
                AcceptButtonClicked();
                break;
            case 2:
                DiscardButtonClicked();
                break;
            case 3:
                Timeline.theTimeline.AddEvent(new GContextButtonClicked("LeftArrowButton"));
                LeftArrowButtonClicked();
                return;
            case 4:
                Timeline.theTimeline.AddEvent(new GContextButtonClicked("RightArrowButton"));
                RightArrowButtonClicked();
                return;
        }

        if (this != GameGUI.CurrentPlayerPad() && pInEvent == EventState.NOTINEVENT)
        {
            GameGUI.CurrentPlayerPad().ClearSelectedAction();
            GameGUI.CurrentPlayerPad().Draw();
        }

        UpdateCardsState(CardGUIStates.None);
        ClearSelectedAction();
    }
    public void ContextButtonClicked(int buttonType)
    {
        NetworkPlayer.LocalInstance.RPC_ContextButtonClicked(buttonType, Position);
    }

    private void RightArrowButtonClicked()
    {
        ContextButtons[5].SetActive(false);
        if(pInEvent == EventState.FORECAST)
        {
            int index = ForeCastEventCardsIDs.IndexOf(ForeCastEventCardSelected);

            int temp = ForeCastEventCardsIDs[index];
            if (index == ForeCastEventCardsIDs.Count - 1)
            {
                ForeCastEventCardsIDs.RemoveAt(index);
                ForeCastEventCardsIDs.Insert(0, temp);
            }
            else
            {
                ForeCastEventCardsIDs[index] = ForeCastEventCardsIDs[index + 1];
                ForeCastEventCardsIDs[index + 1] = temp;
            }
        }
        else if (pInEvent == EventState.RESOURCEPLANNING)
        {
            int index = ResourcePlanningEventCardsIDs.IndexOf(ResourcePlanningEventCardSelected);

            int temp = ResourcePlanningEventCardsIDs[index];
            if (index == ResourcePlanningEventCardsIDs.Count - 1)
            {
                ResourcePlanningEventCardsIDs.RemoveAt(index);
                ResourcePlanningEventCardsIDs.Insert(0, temp);
            }
            else
            {
                ResourcePlanningEventCardsIDs[index] = ResourcePlanningEventCardsIDs[index + 1];
                ResourcePlanningEventCardsIDs[index + 1] = temp;
            }
        }
        Draw();
    }

    private void LeftArrowButtonClicked()
    {
        ContextButtons[5].SetActive(false);
        if (pInEvent == EventState.FORECAST)
        {
            int index = ForeCastEventCardsIDs.IndexOf(ForeCastEventCardSelected);

            int temp = ForeCastEventCardsIDs[index];
            if (index == 0)
            {
                ForeCastEventCardsIDs.RemoveAt(index);
                ForeCastEventCardsIDs.Add(temp);
            }
            else
            {
                ForeCastEventCardsIDs[index] = ForeCastEventCardsIDs[index - 1];
                ForeCastEventCardsIDs[index - 1] = temp;
            }
        }
        else if (pInEvent == EventState.RESOURCEPLANNING)
        {
            int index = ResourcePlanningEventCardsIDs.IndexOf(ResourcePlanningEventCardSelected);

            int temp = ResourcePlanningEventCardsIDs[index];
            if (index == 0)
            {
                ResourcePlanningEventCardsIDs.RemoveAt(index);
                ResourcePlanningEventCardsIDs.Add(temp);
            }
            else
            {
                ResourcePlanningEventCardsIDs[index] = ResourcePlanningEventCardsIDs[index - 1];
                ResourcePlanningEventCardsIDs[index - 1] = temp;
            }
        }
        Draw();
    }

    private void CloseButtonClicked()
    {
        if (ActionSelected == ActionTypes.Share)
        {
            foreach (PlayerGUI playerGUI in playersToShareGUI)
            {
                playerGUI.ActionSelected = ActionTypes.None;
                playerGUI.ContextButtonClicked(0);

            }
        }
        pInEvent = EventState.NOTINEVENT;
    }

    private void AcceptButtonClicked()
    {
        if (pInEvent == EventState.CONFIRMINGCALLTOMOBILIZE) Timeline.theTimeline.AddEvent(new PCallToMobilizeCardPlayed(PlayerModel));
        else if (pInEvent == EventState.CONFIRMINGRESOURCEPLANNING) Timeline.theTimeline.AddEvent(new PResourcePlanningCardPlayed(PlayerModel));
        else if (pInEvent == EventState.CONFIRMINGMOBILEHOSPITAL) Timeline.theTimeline.AddEvent(new PMobileHospitalCardPlayed(PlayerModel));
        else if (pInEvent == EventState.CONFIRMINGFORECAST) Timeline.theTimeline.AddEvent(new PForecastCardPlayed(PlayerModel));
        
        else if (pInEvent == EventState.NOTINEVENT && cardsState == CardGUIStates.CardsDiscarding)
        {
            if (selectedCards[0] == 24) Timeline.theTimeline.AddEvent(new PCallToMobilizeCardPlayed(PlayerModel));
            if (selectedCards[0] == 25) Timeline.theTimeline.AddEvent(new PForecastCardPlayed(PlayerModel));
            if (selectedCards[0] == 26) Timeline.theTimeline.AddEvent(new PMobileHospitalCardPlayed(PlayerModel));
            if (selectedCards[0] == 27) Timeline.theTimeline.AddEvent(new PResourcePlanningCardPlayed(PlayerModel));
        }
        
        else if (pInEvent == EventState.CALLTOMOBILIZE)
        {
            callToMobilizeExecuted = true;
            DestroyMovingPawn();
            Timeline.theTimeline.AddEvent(new PMobilizeEvent(PlayerModel, PlayerModel.GetCurrentCity()));
            Draw();
        }
        else if (pInEvent == EventState.FORECAST)
        {
            Timeline.theTimeline.AddEvent(new PForecast(PlayerModel));
            return;
        }
        else if (pInEvent == EventState.RESOURCEPLANNING)
        {
            Timeline.theTimeline.AddEvent(new PResourcePlanning(PlayerModel));
            return;
        }

        if (cardsState == CardGUIStates.CardsExpandedFlyActionSelected)
        {
            Timeline.theTimeline.AddEvent(new PFlyToCity(selectedCards[0]));
        }
        else if (cardsState == CardGUIStates.CardsExpandedCureActionSelected)
        {
            Timeline.theTimeline.AddEvent(new PCureDisease(selectedCards));
        }
        else if (cardsState == CardGUIStates.CardsExpandedShareAction)
        {
            if (shareCardFromOtherPlayerToCurrent)
            {
                // From other player to current player
                shareCardFromOtherPlayerToCurrent = false;
                Timeline.theTimeline.AddEvent(new PShareKnowledge(playersToShareGUI[0], GameGUI.CurrentPlayerPad()));
            }
            else
            {
                // From current player to other player
                Timeline.theTimeline.AddEvent(new PShareKnowledge(GameGUI.CurrentPlayerPad(), this));
            }
            foreach (PlayerGUI playerGUI in playersToShareGUI) playerGUI.UpdateCardsState(CardGUIStates.None);
            Draw();

            /*playersToShareGUI.Clear();*/
        }
/*        else if (ActionSelected == ActionTypes.CharacterAction && PlayerModel.Role == Player.Roles.Pilot)
        {
            if (pawnPilotSelected != null)
            {
                Timeline.theTimeline.AddEvent(new PPilotFlyToCity(pilotCitySelected, pawnPilotSelected.PlayerModel));
            }
            else
            {
                Timeline.theTimeline.AddEvent(new PPilotFlyToCity(pilotCitySelected, null));
            }
        }*/
    }

    private void DiscardButtonClicked()
    {
        Timeline.theTimeline.AddEvent(new PDiscardCard(selectedCards[0], this));
        UpdateCardsState(CardGUIStates.None);
    }

    public void HandleActionButtonClicked(int action)
    {
        if (PlayerModel == null) return;
        if (PlayerModel != game.CurrentPlayer) return;
        if (PlayerModel.ActionsRemaining <= 0) return;

        ClearSelectedAction(!(action == 6));

        switch (action)
        {
            case 0: //move
                ActionSelected = ActionTypes.Move;
                MoveActionBackground.color = new Color(1f, 1f, 1f, .25f);
                CreateMovingPawn();
                Draw();
                break;
            case 1: //fly
                ActionSelected = ActionTypes.Fly;
                FlyActionBackground.color = new Color(1f, 1f, 1f, .25f);
                UpdateCardsState(CardGUIStates.CardsExpandedFlyActionToSelect); // calls draw
                break;
            case 2: //charter
                ActionSelected = ActionTypes.Charter;
                CharterActionBackground.color = new Color(1f, 1f, 1f, .25f);
                UpdateCardsState(CardGUIStates.CardsExpandedCharterActionToSelect); // calls draw
                CreateMovingPawn();
                break;
            case 3: //treat
                ActionSelected = ActionTypes.Treat;
                TreatActionBackground.color = new Color(1f, 1f, 1f, .25f);
                Draw();
                break;
            case 4: //share
                ActionSelected = ActionTypes.Share;
                ShareActionBackground.color = new Color(1f, 1f, 1f, .25f);
                UpdateCardsState(CardGUIStates.CardsExpandedShareAction); //calls draw


                /*if (PlayerModel.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
                {
                    UpdateCardsState(CardGUIStates.CardsExpandedShareAction);
                    
                }*/ // Redundant with ownTurnActionHandling check for share

                foreach (PlayerGUI playerGUI in playersToShareGUI)
                {
                    playerGUI.UpdateCardsState(CardGUIStates.CardsExpandedShareAction); // calls draw
                    if (playerGUI.PlayerModel.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
                    {
                        playerGUI.GetCardInHand(PlayerModel.GetCurrentCity()).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
                    }
                }

                //if (!fromCurrentPlayerToOther) _player.playerGui.EnableContextButtons(false, false, false, false, false, false);
                break;
            case 5: //find cure
                ActionSelected = ActionTypes.FindCure;
                FindCureActionBackground.color = new Color(1f, 1f, 1f, .25f);
                UpdateCardsState(CardGUIStates.CardsExpandedCureActionToSelect); // calls draw
                break;
            case 6: //character action

/*                if (_player.Role == Player.Roles.Virologist || _player.Role == Player.Roles.Pilot)
                {
                    Debug.Log("Character action clicked: " + _player.Role);
                    bool enableAction = true;

                    if (_player.Role == Player.Roles.Virologist)
                    {
                        if (PlayerModel.CityCardsInHand.Count == 0 || PlayerModel.roleActionUsed) enableAction = false;
                        else
                        {
                            UpdateCardsState(CardGUIStates.CardsExpandedVirologistAction); // calls draw
                        }
                    }

                    if (enableAction)
                    {
                        ActionSelected = ActionTypes.CharacterAction;
                        UpdateCardsState(CardGUIStates.None);
                        roleCardBackground.GetComponent<Outline>().enabled = true;
                    }
                }*/
                break;

            case 7: // End turn
                ActionSelected = ActionTypes.EndTurn;
                EndTurnActionBackground.color = new Color(1f, 1f, 1f, .25f);
                // Set the remaining actions to 0
                //PlayerModel.DecreaseActionsRemaining(PlayerModel.ActionsRemaining);
                break;
        }
        Timeline.theTimeline.AddEvent(new GActionButtonClicked(ActionSelected, theGame.CurrentPlayer));
    }
    public void ActionButtonClicked(int action)
    {
        NetworkPlayer.LocalInstance.RPC_ActionButtonClicked(action, Position);
    }

    public void HandleCardInHandClicked(int cardClicked)
    {
        //Something about game.GameState == PlayerActions, i.e we should only allow input and consequences of card clicked when we can act, not during dealing cards etc. (this is a issue if we need to discard during GameState.DealingCards, need another way)
        if (Waiting) return;

        CityCardDisplay cardClickedScript = null;
        EventCardDisplay eventCardDisplay = null;

        if (cardClicked < 24)
        {
            cardClickedScript = GetCardInHand(cardClicked).GetComponent<CityCardDisplay>();
            if (pInEvent == EventState.NOTINEVENT && cardsState == CardGUIStates.CardsExpanded)
                // not logged if an event card is selected or if the player cards are not expanded
                Timeline.theTimeline.AddEvent(new GCityCardClicked(cardClickedScript.CityCardData));
        }
        else
        {
            eventCardDisplay = GetCardInHand(cardClicked).GetComponent<EventCardDisplay>();
        }
        
        if (cardsState == CardGUIStates.CardsDiscarding)
        {
            selectedCards.Clear();
            selectedCards.Add(cardClicked);
            Draw();
        }
        if (_player == game.CurrentPlayer)
        {
            if (cardsState == CardGUIStates.None)
            {
                ClearSelectedAction();
                UpdateCardsState(CardGUIStates.CardsExpanded);
                //EnableContextButtons(true, false, false, false, false, false);
                //ContextButtons[0].SetActive(true);
                return;
            }
        }
        if (cardClicked < 24)
        {
            if (cardsState == CardGUIStates.CardsExpandedCharterActionToSelect)
            {
                if (cardClicked != _player.GetCurrentCity())
                {
                    return;
                }
                else
                {
                    selectedCards.Add(cardClicked);
                    if (flyLine != null)
                    {
                        Destroy(flyLine);
                    }
                    cardClickedScript.border.gameObject.SetActive(true);
                }
            }

            if (cardsState == CardGUIStates.CardsExpandedFlyActionToSelect || cardsState == CardGUIStates.CardsExpandedFlyActionSelected)
            {
                UpdateCardsState(CardGUIStates.CardsExpandedFlyActionSelected, false);
                selectedCards.Clear();
                selectedCards.Add(cardClicked);

                if (cardClickedScript != null)
                {
                    if (flyLine != null)
                    {
                        Destroy(flyLine);
                    }

                    removeBorderFromCardsInHand();
                    cardClickedScript.border.gameObject.SetActive(true);
                    City cityToMoveTo = game.Cities[cardClicked];
                    City cityToMoveFrom = game.Cities[_player.GetCurrentCity()];

                    Debug.Log(cityToMoveFrom);
                    //cityToMoveFrom.PawnsInCity[_player.Position].gameObject.GetComponent<Outline>().enabled = true;
                    CreateLineBetweenCities(cityToMoveTo, cityToMoveFrom);
                    ContextButtons[1].SetActive(true);
                }
            }

            if (cardsState == CardGUIStates.CardsExpandedCureActionToSelect || cardsState == CardGUIStates.CardsExpandedCureActionSelected)
            {
                if (selectedCards.Contains(cardClicked))
                {
                    selectedCards.Remove(cardClicked);
                    GetCardInHand(cardClicked).GetComponent<CityCardDisplay>().border.gameObject.SetActive(false);
                    ContextButtons[1].SetActive(false);
                }
                else
                {
                    if (AddCardAndTestForCure(cardClickedScript))
                    {
                        UpdateCardsState(CardGUIStates.CardsExpandedCureActionSelected, false);
                        ContextButtons[1].SetActive(true);
                    }

                }
            }
        }
        else
        {
            if (pInEvent == EventState.NOTINEVENT && cardsState != CardGUIStates.CardsDiscarding)
            {
                Timeline.theTimeline.AddEvent(new GEventCardClicked(eventCardDisplay.EventCardData));

                if (cardClicked == 24) pInEvent = EventState.CONFIRMINGCALLTOMOBILIZE;
                else if (cardClicked == 25) pInEvent = EventState.CONFIRMINGFORECAST;
                else if (cardClicked == 26) pInEvent = EventState.CONFIRMINGMOBILEHOSPITAL;
                else if (cardClicked == 27) pInEvent = EventState.CONFIRMINGRESOURCEPLANNING;
                if (pInEvent != EventState.NOTINEVENT)
                {
                    selectedCards.Add(cardClicked);
                    Draw();
                }
            }
        }
    }
    public void CardInHandClicked(int cardClicked)
    {

        NetworkPlayer.LocalInstance.RPC_CardInHandClicked(cardClicked, Position);

    }

    public void CityClicked(City city)
    {
        if (ActionSelected == ActionTypes.Treat && 
            _player.GetCurrentCity() == city.city.cityID &&
            _player.ActionsRemaining > 0 && city.CubesInCity())
        {

            Timeline.theTimeline.AddEvent(new PTreatDisease(city));

        }/* else if (ActionSelected == ActionTypes.Treat && _player.Role == Player.Roles.Virologist 
                   && _player.ActionsRemaining > 0 && city.CubesInCity() && !_player.secondRoleActionUsed) {

            Debug.Log("Treating City :", city);

            VirusName? virusFound = city.FirstVirusFoundInCity();
            if (virusFound.HasValue)
            {
                VirusName virusColor = virusFound.Value;
                Debug.Log("Virus color found in the city: " + virusColor);

                if ((virusColor == VirusName.Red && _player.RedCardsInHand.Any()) ||
                    (virusColor == VirusName.Yellow && _player.YellowCardsInHand.Any()) ||
                    (virusColor == VirusName.Blue && _player.BlueCardsInHand.Any()))
                {
                    _player.secondRoleActionUsed = true;
                    Timeline.theTimeline.AddEvent(new PTreatDisease(city));
                }
            }

            *//*Debug.Log("Red Cards in hand ? " + _player.RedCardsInHand.Any());
            Debug.Log("Yellow Cards in hand ? " + _player.YellowCardsInHand.Any());
            Debug.Log("Blue Cards in hand ? " + _player.BlueCardsInHand.Any());*//*
        }*/

        /*else if (ActionSelected == ActionTypes.CharacterAction && PlayerModel.Role == Player.Roles.Pilot)
        {
            City cityToMoveTo = game.Cities[city.city.cityID];
            City cityToMoveFrom = game.Cities[_player.GetCurrentCity()];

            int distance = game.DistanceFromCity(cityToMoveFrom.city.cityID, cityToMoveTo.city.cityID);

            if (distance > 0 && distance < 3)
            {
                pilotCitySelected = city.city.cityID;
                
                Timeline.theTimeline.AddEvent(new GCityClicked(city)); // Only logging possible options
                
                if (flyLine != null) Destroy(flyLine);
                if (flyLine2 != null) Destroy(flyLine2);

                //cityToMoveFrom.PawnsInCity[_player.Position].gameObject.GetComponent<Outline>().enabled = true; //This line is not working for the new 3D pawns.
                CreateLineBetweenCities(cityToMoveTo, cityToMoveFrom);

                int counterPlayers = 0;
                foreach (Player player in cityToMoveFrom.PlayersInCity)
                {
                    if (player != PlayerModel)
                    {
                        GameObject pawn = pilotPawnsTagAlong[counterPlayers];
                        pawn.SetActive(true);
                        Pawn pawnScript = pawn.GetComponent<Pawn>();
                        pawnScript.SetRoleAndPlayer(player);
                        pawnScript.IsInterfaceElement = true;
                        counterPlayers++;
                    }
                }

                if (pawnPilotSelected != null)
                    CreateLineBetweenGameObjects(cityToMoveTo.gameObject, getPawnInCurrentCity(pawnPilotSelected).gameObject, gameGui.roleCards[(int)pawnPilotSelected.PawnRole]);

                EnableContextButtons(true, true, false, false, false, false);
                changeContextText();
            }

        }*/
    }

    internal void CubeClicked(City city, VirusName virusName)
    {
        if (ActionSelected == ActionTypes.Treat && _player.GetCurrentCity() == city.city.cityID 
            && _player.ActionsRemaining > 0 && city.CubesInCity())
        {
            AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
            Timeline.theTimeline.AddEvent(new PTreatDisease(city, virusName));
        }
/*        else if( ActionSelected == ActionTypes.Treat && _player.Role == Player.Roles.Virologist 
                                                     && _player.ActionsRemaining > 0 
                                                     && city.CubesInCity() 
                                                     && (!PlayerModel.secondRoleActionUsed))
        {
            foreach (int card in PlayerModel.CityCardsInHand)
            {
                if (game.Cities[card].city.virusInfo.virusName == virusName)
                {
                    AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.CLICK);
                    Timeline.theTimeline.AddEvent(new PTreatDisease(city, virusName));
                    PlayerModel.secondRoleActionUsed = true;
                    break;
                }
            }
        }*/
    }

    internal void PawnClicked(Pawn pawn)
    {
        foreach (GameObject pawnObject in pilotPawnsTagAlong)
        {
            pawnObject.GetComponent<Outline>().enabled = false;
        }

        if (flyLine2 != null)
        {
            Destroy(flyLine2);
        }

        if (pawn != pawnPilotSelected)
        {
            pawn.GetComponent<Outline>().enabled = true;
            pawnPilotSelected = pawn;
            Timeline.theTimeline.AddEvent(new GPawnClicked(pawnPilotSelected));
            CreateLineBetweenGameObjects(game.Cities[pilotCitySelected].gameObject, getPawnInCurrentCity(pawn).gameObject, gameGui.roleCards[(int)pawn.PawnRole]);
        }
        else
        {
            pawnPilotSelected = null;

        }
        changeContextText();
    }

    private Pawn getPawnInCurrentCity(Pawn pawn)
    {
        foreach (Pawn pawnInCity in PlayerModel.GetCurrentCityScript().PawnsInCity)
        {
            if(pawnInCity != null && pawnInCity.PawnRole == pawn.PawnRole)
                return pawnInCity;
        }
        return null;
    }

    #endregion

    private void CountSelectedCardColors(out int numRedCards, out int numYellowCards, out int numBlueCards)
    {
        numRedCards = 0;
        numBlueCards = 0;
        numYellowCards = 0;
        
        foreach (int card in selectedCards)
        {
            switch (game.Cities[card].city.virusInfo.virusName)
            {
                case VirusName.Red:
                    numRedCards++;
                    break;
                case VirusName.Yellow:
                    numYellowCards++;
                    break;
                case VirusName.Blue:
                    numBlueCards++;
                    break;
            }
        }
    }
    /*
     * Check whether the disease can be cured after selecting a card
     * Returns true when curing the disease is possible, false otherwise
     */
    private bool AddCardAndTestForCure(CityCardDisplay cardClickedScript)
    {
        //bool isVirologist = PlayerModel.Role == Player.Roles.Virologist;
        bool isVirologist = false;
        VirusName cardClickedVirusName = cardClickedScript.CityCardData.virusInfo.virusName;

        int cardID = cardClickedScript.CityCardData.cityID;
        

        CountSelectedCardColors(out int numRedCards, out int numYellowCards, out int numBlueCards);
        
        if (CanAddCard(cardClickedVirusName, numRedCards, numBlueCards, numYellowCards, isVirologist))
        {
            selectedCards.Add(cardID);
            switch (cardClickedVirusName)
            {
                case VirusName.Red:
                    numRedCards++;
                    break;
                case VirusName.Blue:
                    numBlueCards++;
                    break;
                case VirusName.Yellow:
                    numYellowCards++;
                    break;
            }
            GetCardInHand(cardID).GetComponent<CityCardDisplay>().border.gameObject.SetActive(true);
        }
        
        // Test if curing the disease if possible
        return TestForCure(numRedCards, numBlueCards, numYellowCards, isVirologist);
    }

    private bool CanAddCard(VirusName cardClickedVirusName, int numRedCards, int numBlueCards,
        int numYellowCards, bool isVirologist)
    {
        
        int numCardsOfClickedColor =
            GetNumCardsOfClickedColor(cardClickedVirusName, numRedCards, numBlueCards, numYellowCards);
        int numCardsOfOtherColor =
            GetNumCardsOfOtherColors(cardClickedVirusName, numRedCards, numBlueCards, numYellowCards);

        int numOtherColors = 0;
        switch (cardClickedVirusName)
        {
            case VirusName.Blue:
                if (numYellowCards > 0) numOtherColors++;
                if (numRedCards > 0) numOtherColors++;
                break;
            case VirusName.Red:
                if (numBlueCards > 0) numOtherColors++;
                if (numYellowCards > 0) numOtherColors++;
                break;
            case VirusName.Yellow:
                if (numRedCards > 0) numOtherColors++;
                if (numBlueCards > 0) numOtherColors++;
                break;
        }

        if (isVirologist)
        {
            if (numCardsOfClickedColor == 2 && IsCureAlreadyFound(cardClickedVirusName))
            {
                Debug.Log($"The {cardClickedVirusName} disease has already been cured." +
                          $" As a virologist, you can't pick more than 2 of its cards");
                return false;
            }
        }
        else
        {
            if (IsCureAlreadyFound(cardClickedVirusName))
            {
                Debug.Log($"Cure has already been found for {cardClickedVirusName}");
                return false;
            }
        }

        bool canAdd;
        if (numCardsOfClickedColor == MAX_SAME_COLOR_CARDS && (numCardsOfOtherColor == 0)) canAdd = false;
        else
        {
            if (isVirologist)
            {
               /* canAdd = ((numCardsOfClickedColor + numCardsOfOtherColor <= MAX_CARDS) 
                          && (numCardsOfClickedColor != MAX_SAME_COLOR_CARDS - 1 || numCardsOfOtherColor != 2)) 
                         || ((numCardsOfClickedColor < MAX_SAME_COLOR_CARDS) && (numCardsOfOtherColor == 0));*/
               canAdd = ((numCardsOfClickedColor + numCardsOfOtherColor < MAX_CARDS) 
                         && !((numCardsOfClickedColor == 3 && numCardsOfOtherColor == 2)
                              || (numCardsOfClickedColor == 0 && numCardsOfOtherColor == 4 && numOtherColors == 1) 
                              //numCardsOfClickedColor is updated when the card is added to selectedCards
                              //numOtherColors is the number of colors of numCardsOfOtherColor
                              || (numCardsOfClickedColor == 2 && numCardsOfOtherColor == 3)
                              || (numCardsOfClickedColor == 4 && numCardsOfOtherColor == 0)))
                         || ((numCardsOfClickedColor < MAX_SAME_COLOR_CARDS) && (numCardsOfOtherColor == 0));
                Debug.Log($"Virologist {cardClickedVirusName}, card addition branch, canAdd: {canAdd}, " +
                          $"numColor : {numOtherColors}");
            }
            else
            {
                canAdd = (numCardsOfClickedColor < MAX_SAME_COLOR_CARDS) && (numCardsOfOtherColor == 0);
            }
        }
        
        return canAdd;
    }

    private bool TestForCure(int numRedCards, int numBlueCards, int numYellowCards, bool isVirologist)
    {
        bool isCurePossible = false;
        if (!isVirologist)
        {
            if (selectedCards.Count == MAX_SAME_COLOR_CARDS) isCurePossible = true;
        }
        else
        {
            if ((selectedCards.Count == MAX_CARDS && (numRedCards == 3 && numBlueCards + numYellowCards == 2)
                    || (numBlueCards == 3 && numRedCards + numYellowCards == 2)
                    || (numYellowCards == 3 && numRedCards + numBlueCards == 2)) 
                || (selectedCards.Count == MAX_SAME_COLOR_CARDS &&
                    (numRedCards == MAX_SAME_COLOR_CARDS 
                     || numYellowCards == MAX_SAME_COLOR_CARDS 
                     || numBlueCards == MAX_SAME_COLOR_CARDS)))
            {
                isCurePossible = true; 
            }
        }
        return isCurePossible;
    }
    
    private bool IsCureAlreadyFound(VirusName virusName)
    {
        bool isCureFound = false;
        switch (virusName)
        {
            case VirusName.Red:
                isCureFound = game.RedCure;
                break;
            case VirusName.Blue:
                isCureFound = game.BlueCure;
                break;
            case VirusName.Yellow:
                isCureFound = game.YellowCure;
                break;
        }

        return isCureFound;
    }

    private int GetNumCardsOfClickedColor(VirusName virusName, int numRedCards, int numBlueCards, int numYellowCards)
    {
        int numCardsOfClickedColor = 0;
        switch (virusName)
        {
            case VirusName.Red:
                numCardsOfClickedColor = numRedCards;
                break;
            case VirusName.Blue:
                numCardsOfClickedColor = numBlueCards;
                break;
            case VirusName.Yellow:
                numCardsOfClickedColor = numYellowCards;
                break;
        }

        return numCardsOfClickedColor;
    }

    private int GetNumCardsOfOtherColors(VirusName virusName, int numRedCards, int numBlueCards, int numYellowCards)
    {
        int numCardsOfOtherColors = 0;
        switch (virusName)
        {
            case VirusName.Red:
                numCardsOfOtherColors = numBlueCards + numYellowCards;
                break;
            case VirusName.Blue:
                numCardsOfOtherColors = numRedCards + numYellowCards;
                break;
            case VirusName.Yellow:
                numCardsOfOtherColors = numBlueCards + numRedCards;
                break;
        }

        return numCardsOfOtherColors;
    }
    
    private void enableOwnTurnActions(bool enabled)
    {
        MoveAction.SetActive(enabled);
        FlyAction.SetActive(enabled);
        CharterAction.SetActive(enabled);
        TreatAction.SetActive(enabled);
        ShareAction.SetActive(enabled);
        FindCureAction.SetActive(enabled);
        EndTurnAction.SetActive(enabled); //TODO : check interference with events
    }

    public void CreateMovingPawn(Vector3? translation = null)
    {
        City currentCity = PlayerModel.GetCurrentCityScript();
        currentCity.RemovePawn(PlayerModel);
        currentCity.Draw();

        if (NetworkPlayer.LocalInstance.IsSharedAuthority())
        {
            Vector3 position = translation == null ? currentCity.transform.position : currentCity.transform.position + (Vector3)translation;
            //movingPawn = Instantiate(gameGui.PawnPrefab, position, currentCity.transform.rotation, gameGui.AnimationCanvas.transform);

            //movingPawn.GetComponent<Pawn>().SetRoleAndPlayer(PlayerModel);
            //movingPawn.GetComponent<Pawn>().SetMoveable(true);

            movingPawnNetwork =  AnimationCanvasSpawner.Instance.SpawnNetworkObject(gameGui.MoveablePawnPrefab, position, currentCity.transform.rotation, gameGui.AnimationCanvas.transform,PlayerModel, NetworkPlayer.LocalInstance.Runner);//NetworkPlayer.LocalInstance.SpawnMoveablePawn(gameGui.MoveablePawnPrefab, position, currentCity.transform.rotation, gameGui.AnimationCanvas.transform);
            Debug.Log("Input auth for moving pawn: " + movingPawnNetwork.InputAuthority);
        }
        
        /*
        movingPawn.GetComponent<Pawn>().CanMove = true;
        movingPawn.GetComponent<Pawn>().SetRoleAndPlayer(PlayerModel);
        movingPawn.GetComponent<Outline>().enabled = true;
        */
    }

    public void ClearSelectedAction(bool clear = true)
    {
        foreach(int card in selectedCards)
        {
            if (card < 24){
                GameObject cardObject = GetCardInHand(card);
                if (cardObject != null)
                {
                    cardObject.GetComponent<CityCardDisplay>().border.gameObject.SetActive(false);
                }
            }
            else{
                GameObject cardObject = GetCardInHand(card);
                if (cardObject != null)
                {
                    cardObject.GetComponent<EventCardDisplay>().border.gameObject.SetActive(false);
                }
            }   
        }
        
        selectedCards.Clear();

        if (flyLine != null)
        {
            Destroy(flyLine);
        }

        DestroyMovingPawn();
        MoveActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        FlyActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        CharterActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        TreatActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        ShareActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        FindCureActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        EndTurnActionBackground.color = new Color(.2f, .2f, .2f, .2f);
        roleCardBackground.GetComponent<Outline>().enabled = false;

        ActionSelected = ActionTypes.None;
        UpdateCardsState(CardGUIStates.None, false);

        if (clear) EnableContextButtons(false, false, false, false, false, false);

        pilotCitySelected = -1;
        if (flyLine != null) Destroy(flyLine);
        flyLine = null;

        if (flyLine2 != null) Destroy(flyLine2);
        flyLine2 = null;

        pawnPilotSelected = null;
        foreach (GameObject pawn in pilotPawnsTagAlong)
        {
            pawn.GetComponent<Outline>().enabled = false;
            pawn.SetActive(false);
        }

        roleCard.gameObject.SetActive(true);
        ActionsContainer.SetActive(true);
        PlayerCards.SetActive(true);

        changeContextText();
    }

    public void DestroyMovingPawn()
    {

        if (movingPawnNetwork != null)
        {
            movingPawnNetwork.GetComponent<PawnMoveable>().Despawn();
            movingPawnNetwork = null;
        }
        City currentCity = game.Cities[PlayerModel.GetCurrentCity()];
        if (!currentCity.PlayersInCity.Contains(PlayerModel))
        {
            currentCity.AddPawn(PlayerModel);
            currentCity.Draw();
        }
    }

    private void CreateLineBetweenCities(City cityToMoveTo, City cityToMoveFrom)
    {
        flyLine = new GameObject("Line - FlyAction");
        flyLine.transform.SetParent(gameGui.AnimationCanvas.transform, false);
        flyLine.transform.position = cityToMoveFrom.PawnsInCity[_player.Position].transform.position;
        flyLine.AddComponent<LineRenderer>();
        LineRenderer lr = flyLine.GetComponent<LineRenderer>();
        lr.sortingLayerName = "Animation";
        lr.material = gameGui.lineMaterial;
        lr.startColor = roleCard.RoleCardData.roleColor;
        lr.endColor = roleCard.RoleCardData.roleColor;
        lr.startWidth = 0.003f; //Niklas: Changed from 0.1 due to rescaling
        lr.endWidth = 0.003f; //Niklas: Changed from 0.1 due to rescaling

        Vector3 middlePoint = (cityToMoveFrom.PawnsInCity[_player.Position].transform.position + cityToMoveTo.transform.position) / 2 + new Vector3(0, 0.05f, 0);
        //For loop to create a 3d curve
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 0.05f)
        {
            var tangent1 = Vector3.Lerp(cityToMoveFrom.PawnsInCity[_player.Position].transform.position, middlePoint, ratio);
            var tangent2 = Vector3.Lerp(middlePoint, cityToMoveTo.transform.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }
        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());

        //lr.SetPosition(0, cityToMoveFrom.PawnsInCity[_player.Position].transform.position);
        //lr.SetPosition(1, cityToMoveTo.transform.position);
    }

    private void CreateLineBetweenGameObjects(GameObject one, GameObject two, RoleCard roleData)
    {
        flyLine2 = new GameObject("Line - FlyAction");
        flyLine2.transform.SetParent(gameGui.AnimationCanvas.transform, false);
        flyLine2.transform.position = one.transform.position;
        flyLine2.AddComponent<LineRenderer>();
        LineRenderer lr = flyLine2.GetComponent<LineRenderer>();
        lr.sortingLayerName = "Animation";
        lr.material = gameGui.lineMaterial;
        lr.startColor = roleData.roleColor;
        lr.endColor = roleData.roleColor;
        lr.startWidth = 0.003f; //Niklas: Changed from 0.1 due to rescaling
        lr.endWidth = 0.003f; //Niklas: Changed from 0.1 due to rescaling

        Vector3 middlePoint = (one.transform.position + two.transform.position) / 2 + new Vector3(0, 0.05f, 0);
        //For loop to create a 3d curve
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 0.05f)
        {
            var tangent1 = Vector3.Lerp(one.transform.position, middlePoint, ratio);
            var tangent2 = Vector3.Lerp(middlePoint, two.transform.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }
        lr.positionCount = pointList.Count;
        lr.SetPositions(pointList.ToArray());

        //lr.SetPosition(0, one.transform.position);
        //lr.SetPosition(1, two.transform.position);
    }

    private void removeBorderFromCardsInHand()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CityCardDisplay cityCard = cardsInHand[i].GetComponent<CityCardDisplay>();
            if (cityCard != null)
            {
                cityCard.border.gameObject.SetActive(false);
            }
            else
            {

                EventCardDisplay eventCard = cardsInHand[i].GetComponent<EventCardDisplay>();
                if (eventCard != null)
                {
                    eventCard.border.gameObject.SetActive(false);
                }
            }
        }
    }

    private void changeContextText()
    {
        if (Waiting && pInEvent != EventState.EXECUTINGMOBILEHOSPITAL)
        {
            CurrentInstructionText.text = "Waiting...";
            return;
        }
        else if (pInEvent == EventState.CONFIRMINGCALLTOMOBILIZE || pInEvent == EventState.CONFIRMINGRESOURCEPLANNING ||
            pInEvent == EventState.CONFIRMINGMOBILEHOSPITAL || pInEvent == EventState.CONFIRMINGFORECAST)
        {
            CurrentInstructionText.text = "Playing Event Card\nDo you confirm?";
            return;
        }
        else if (pInEvent == EventState.FORECAST)
        {
            CurrentInstructionText.text = "Event - Forecasting \nSelect any card.\nUse arrows to move";
            return;
        }
        else if (pInEvent == EventState.RESOURCEPLANNING)
        {
            CurrentInstructionText.text = "Event - Resource Planning \nSelect any card.\nUse arrows to move";
            return;
        }
        else if (pInEvent == EventState.CALLTOMOBILIZE)
        {
            //if(movingPawn != null)
            if (!callToMobilizeExecuted)
                CurrentInstructionText.text = "Event - Call to Mobilize \nMove 1-2 or accept to stay";
            else
                CurrentInstructionText.text = "Event - Call to Mobilize \nWaiting";
            
            return;
        }
        if (theGame.MobileHospitalInExecution && theGame.MobileHospitalPlayer == _player)
        {
            CurrentInstructionText.text = "Event - Mobile Hospital \nRemove a cube.";
            return;
        }

        if (game.CurrentPlayer != _player)
        {

            if(cardsState == CardGUIStates.CardsDiscarding)
            {
                CurrentInstructionText.text = "Discard a card";
                return;
            }
            else if (cardsState == CardGUIStates.CardsExpandedShareAction)
            {
                if (_player.PlayerCardsInHand.Contains(game.CurrentPlayer.GetCurrentCity()))
                {
                    CurrentInstructionText.text = "Waiting for approval";
                } else if (game.CurrentPlayer.PlayerCardsInHand.Contains(_player.GetCurrentCity()))
                {
                    CurrentInstructionText.text = "Accept card?";
                }
                
                return;
            }
            else
            {
                CurrentInstructionText.text = "Not your turn.";
                return;
            }
        }
        string textToreturn = $"<b>{PlayerModel.ActionsRemaining}</b> actions left."; 

        string additionalMessage = "";

        if(cardsState == CardGUIStates.CardsDiscarding)
        {
            if(selectedCards.Count == 0)
            {
                additionalMessage += "Select to discard";
            }
            else
            {
                if (selectedCards[0] > 23)
                    additionalMessage += "Use/Discard event";
                else
                    additionalMessage += "Discard city";
            }   
        }
        else if(ActionSelected == ActionTypes.Move)
        {
            additionalMessage += "Move your pawn";
        }
        else if(ActionSelected == ActionTypes.Fly)
        {
            if(cardsState == CardGUIStates.CardsExpandedFlyActionToSelect)
            {
                additionalMessage += "Pick a card to fly to";
            }
            else if(cardsState == CardGUIStates.CardsExpandedFlyActionSelected)
            {
                additionalMessage += "Complete action?";
            }
        }
        else if (ActionSelected == ActionTypes.Charter)
        {
            if (cardsState == CardGUIStates.CardsExpandedCharterActionToSelect)
            {
                additionalMessage += "Move to any city";
            }
        }
        else if (ActionSelected == ActionTypes.Treat)
        {
            additionalMessage += "Pick a cube";
        }
        else if (ActionSelected == ActionTypes.Share)
        {
            if (cardsState == CardGUIStates.CardsExpandedShareAction)
            {
                if(shareCardFromOtherPlayerToCurrent)
                    additionalMessage += "Accept card ?";
                else
                    additionalMessage = "Waiting for approval";
            }
        }
        else if (ActionSelected == ActionTypes.FindCure)
        {
            additionalMessage += "Complete action?";
        }
        else if (ActionSelected == ActionTypes.CharacterAction)
        {
            /*if (PlayerModel.Role == Player.Roles.Virologist)
                additionalMessage += "Remove a cube";
            else if (PlayerModel.Role == Player.Roles.Pilot)
            {
                if (pilotCitySelected == -1)
                    additionalMessage += "Touch city within 2";
                else
                {
                    if (pawnPilotSelected != null)
                    {
                        string playerRole = pawnPilotSelected.PlayerModel.Role.GetDescription();
                        
                        additionalMessage += $"<size=11>Travel with the <size=12><b>{playerRole}</b>?</size></size>";
                    }
                    else
                    {
                        additionalMessage += "Travel <b>alone</b>?";
                    }
                }
            }*/
        }

        if (additionalMessage != "")
        {
            textToreturn += "\n" + additionalMessage;
        }

        CurrentInstructionText.text = textToreturn;
    }

    public void drawLater(float time)
    {
        _isAnimating = true;
        this.ExecuteLater(time, doneAnimating);
    }

    void doneAnimating()
    {
        _isAnimating = false;
        Draw();
    }


    internal void ChangeToInEvent(EventState state, bool shouldDraw = true)
    {
        pInEvent = state;
        //game.ChangeToInEvent(pInEvent); //Niklas: Maybe fix
        if (shouldDraw)
            Draw();
        //else
        //    ClearContextButtons();
    }

    public void UpdateCardsState(CardGUIStates newCardState, bool redraw = true)
    {
        cardsState = newCardState;
        if(redraw) 
            Draw();
    }

}

public enum ActionTypes
{
    Move,
    Fly,
    Charter,
    Treat,
    Share,
    FindCure,
    CharacterAction,
    EndTurn,
    None
}

public enum CardGUIStates
{
    None,
    CardsExpanded,
    CardsExpandedFlyActionToSelect,
    CardsExpandedFlyActionSelected,
    CardsExpandedShareAction,
    //CardsExpandedVirologistAction,
    CardsExpandedCharterActionToSelect,
    CardsExpandedCureActionToSelect,
    CardsExpandedCureActionSelected,
    CardsDiscarding
}

public enum ContextButtonStates
{
    Reject,
    Accept,
    None
}