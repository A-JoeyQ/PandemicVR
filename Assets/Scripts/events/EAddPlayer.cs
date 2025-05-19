using Newtonsoft.Json.Linq;
using static Game;

[System.Serializable]
public class EAddPlayer : EngineEvent, IInitializableEvent
{
    public int TablePositionId;
    public Player.Roles PlayerRole;
    public string PlayerName;


    EAddPlayer()
    {
        QUndoable = false;
    }
    public EAddPlayer(int tableId, Player.Roles role, string name)
    {
        TablePositionId = tableId;
        PlayerRole = role;
        PlayerName = name;
    }

    public override void Do(Timeline c)
    {
        if (!Timeline.theTimeline.IsReplayMode)
        {
            Player p = new Player(TablePositionId, PlayerRole, PlayerName);
            PlayerList.Players.Add(p);
            p.UpdateCurrentCity(theGame.InitialCityID, false);
        }
    }

    public override float Act(bool qUndo = false)
    {

        //Sequence sequence = DOTween.Sequence();
        //sequence.Append(currentPawnImage.DOFade(0f, 0f));
        //sequence.Append(currentPawnImage.DOFade(1f, 1f));
        //sequence.Append(currentPawn.transform.DOMove(pawnPosition,1f));
        //sequence.Play();
        theGame.Cities[theGame.InitialCityID].Draw();
        return 0f;
    }

    public override string GetLogInfo()
    {
        return $@" ""tablePositionId"" : {TablePositionId},
                    ""playerRole"" : ""{PlayerRole}"",
                    ""playerName"" : ""{PlayerName}""
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        // Used to populate the PlayerList to construct the PlayerEvents through the TimelineEventConverter
        // The PlayerList is *NOT* flushed by the Do of EResetGame, and NOT refilled through the Do of this (EAddPlayer)
        Player p = new Player(TablePositionId, PlayerRole, PlayerName);
        PlayerList.Players.Add(p);
        p.UpdateCurrentCity(theGame.InitialCityID, false);
    }
}