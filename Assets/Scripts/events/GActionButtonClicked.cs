using System;
using Newtonsoft.Json.Linq;

public class GActionButtonClicked : GuiEvent, IInitializableEvent
{
    private ActionTypes _actionSelected;

    public GActionButtonClicked(Player player) : base(player)
    {
        
    }
    
    public GActionButtonClicked(ActionTypes actionSelected, Player player) : base(player)
    {
        _actionSelected = actionSelected;
    }
    
    public override void Do(Timeline timeline)
    {
        if (_actionSelected == ActionTypes.EndTurn)
        {
            _player.DecreaseActionsRemaining(_player.ActionsRemaining);
        }
        
    }
    
    public override string GetLogInfo()
    {
        return $@"""actionSelected"" : ""{_actionSelected}""
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["actionSelected"] is JValue jsonActionSelected)
        {
            string actionSelectedString = jsonActionSelected.Value<string>();
            _actionSelected = Enum.Parse<ActionTypes>(actionSelectedString);
        }
    }
}
