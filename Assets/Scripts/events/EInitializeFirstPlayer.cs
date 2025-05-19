using static GameGUI;
using static Game;

internal class EInitializeFirstPlayer : EngineEvent, IIndirectEvent
{

    private Player startPlayer;

    public EInitializeFirstPlayer()
    {
        QUndoable = true;

    }

    public override void Do(Timeline timeline)
    {

        startPlayer = PlayerList.Players.GetRandom();
        int highestPopulation = 0;
        foreach (Player player in PlayerList.Players)
        {
            foreach (int card in player.CityCardsInHand)
            {
                int population = int.Parse(theGame.Cities[card].city.population.Replace(".", ""));
                if (population > highestPopulation)
                {
                    highestPopulation = population;
                    startPlayer = player;
                }
            }
        }

        PlayerList.setOrderToClockwiseWithStartAt(startPlayer);
        theGame.CurrentPlayer = startPlayer;
        startPlayer.ResetTurn();
    }

    public override float Act(bool qUndo = false)
    {
        gui.DrawCurrentPlayerArea();
        return 0f;
    }

    public override string GetLogInfo()
    {
        return $@" ""startPlayer"" : ""{startPlayer.Role}""
                ";
    }
}