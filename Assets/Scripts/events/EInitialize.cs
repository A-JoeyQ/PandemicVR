using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using static GameGUI;
using static Game;

[System.Serializable]
public class EInitialize : EngineEvent, IInitializableEvent
{
    
    private int PlayerCardsSeed;
    private int InfectionCardsSeed;

    
    public EInitialize()
    {
        QUndoable = false;
    }

    public override void Do(Timeline c)
    {
        initializeSeeds();
        initializeModel();
        initializeGUI();

        // Find all TextMeshProUGUI components in the scene
        TextMeshProUGUI[] allTMPTexts = Object.FindObjectsOfType<TextMeshProUGUI>();

        // Loop through each one and disable its raycastTarget property
        foreach (TextMeshProUGUI tmpText in allTMPTexts)
        {
            tmpText.raycastTarget = false;
        }

        c.AddEvent(new EStartRound());
    }

    private void initializeSeeds()
    {
        if (PlayerCardsSeed == 0 && InfectionCardsSeed == 0)
        {
            int randomSeed = Mathf.Abs(System.DateTime.UtcNow.Ticks.GetHashCode());
        
            PlayerCardsSeed = theGame.PlayerCardsSeed == -1 ? randomSeed : theGame.PlayerCardsSeed;
            InfectionCardsSeed = theGame.InfectionCardsSeed == -1 ? randomSeed : theGame.InfectionCardsSeed;
        }
        
        Debug.Log("PlayerCardsSeed = " + PlayerCardsSeed);
        Debug.Log("InfectionCardsSeed = " + InfectionCardsSeed);
        
        Random.InitState(PlayerCardsSeed);
        theGame.playerCardsRandomGeneratorState = Random.state;
        
        Random.InitState(InfectionCardsSeed);
        theGame.infectionCardsRandomGeneratorState = Random.state;
    }

    private void initializeModel()
    {
        theGame.SetCurrentGameState(Game.GameState.SETTINGBOARD);

        //0 to 23 are city cards and 24 to 27 are event cards
        int numCards = Game.theGame.UseEventCards ? 28 : 24;
        theGame.PlayerCards = Enumerable.Range(0, numCards).ToList();
        theGame.PlayerCards.Shuffle(theGame.playerCardsRandomGeneratorState);
        theGame.playerCardsRandomGeneratorState = Random.state;

        ////TODO: remove this
        //for (int i = 0; i < 4; ++i)
        //    theGame.PlayerCards.Add(24+i);

        theGame.InfectionCards = Enumerable.Range(0, 24).ToList();
        theGame.InfectionCards.Shuffle(theGame.infectionCardsRandomGeneratorState);
        theGame.infectionCardsRandomGeneratorState = Random.state;

        int numCardsToDeal = PlayerList.Players.Count == 2 ? 3 : 2;
        foreach (Player player in PlayerList.Players)
        {
            for (int i = 0; i < numCardsToDeal; ++i)
            {
                Timeline.theTimeline.AddEvent(new PDealCard(player));
            }
        }

        Timeline.theTimeline.AddEvent(new EAddEpidemicCards());
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(3, true));
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(3, true));
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(2, true));
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(2, true));
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(1, true));
        Timeline.theTimeline.AddEvent(new EDrawInfectionCard(1, true));
        Timeline.theTimeline.AddEvent(new EInitializeFirstPlayer());

        ////foreach city add a cube of each color
        //foreach (City city in theGame.Cities)
        //{
        //    city.incrementNumberOfCubes(VirusName.Yellow, 1);
        //    city.incrementNumberOfCubes(VirusName.Red, 1);
        //    city.incrementNumberOfCubes(VirusName.Blue, 1);
        //}
    }
    
    private void initializeGUI()
    {
        foreach (PlayerGUI playerGUI in gui.PlayerPads)
        {
            if (PlayerList.Players.Any(p => p.Position == playerGUI.Position))
            {
                playerGUI.Init();
            }
            else
                playerGUI.gameObject.SetActive(false);
        }
       gui.PlayerPads = gui.PlayerPads.Where(p => p.gameObject.activeSelf).ToList();
       gui.Draw();
    }

    public override float Act(bool qUndo)
    {
        AudioPlayer.PlayClip(AudioPlayer.AudioClipEnum.SHUFFLE);
        gui.Draw();
        return 0;
    }

    public override string GetLogInfo()
    {
        StringBuilder cityJson = new StringBuilder();
        cityJson.Append("[\n");
        for (int i = 0; i < theGame.Cities.Length; i++)
        {
            City city = theGame.Cities[i];
            cityJson.Append("\t\t\t\t\t\t\t\t{");
            cityJson.Append($"\"id\": {city.city.cityID}, \"name\": \"{city.city.cityName}\", \"virusColor\": \"{city.city.virusInfo.virusName}\", \"connectedCities\": [{string.Join(',', city.city.neighbors)}]");
            cityJson.Append("}");
            
            if (i < theGame.Cities.Length - 1)
                cityJson.Append(", \n");
        }
        cityJson.Append("\n\t\t\t\t\t]");
        
        return $@" ""seeds"" : {{
                        ""playerCards"" : ""{PlayerCardsSeed}"",
                        ""infectionCards"" : ""{InfectionCardsSeed}""
                    }},
                    ""playerCount"" : ""{PlayerList.Players.Count}"",
                    ""cities"": {cityJson}
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["seeds"] is JObject seeds)
        {
            PlayerCardsSeed = seeds["playerCards"]?.Value<int>() ?? 0;
            InfectionCardsSeed = seeds["infectionCards"]?.Value<int>() ?? 0;
        }
    }
}
