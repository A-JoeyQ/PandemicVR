using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEngine;

internal class PDiscardCard : PlayerEvent, IInitializableEvent
{
    private float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;
    private int cardToDiscard;
    private PlayerGUI playerGui;
    private Vector3 objectToDiscardPosition;
    private Quaternion objectToDiscardRotation;

    public PDiscardCard(Player player) : base(player)
    {
        playerGui = player.playerGui;
    }
    
    public PDiscardCard(int cardID, PlayerGUI playerGui)
    {
        this.cardToDiscard = cardID;
        this.playerGui = playerGui;
    }

    public override void Do(Timeline timeline)
    {
        UnityEngine.Debug.Log("Discarding card " + cardToDiscard + " player: " + playerGui.PlayerModel.Name);
        GameObject objectToDiscard = playerGui.GetCardInHand(cardToDiscard);
        objectToDiscardPosition = objectToDiscard.transform.position;
        objectToDiscardRotation = objectToDiscard.transform.rotation;
        playerGui.PlayerModel.RemoveCardInHand(cardToDiscard, true);
        game.actionCompleted = true; //This causes some problems perhaps
    }

    public override float Act(bool qUndo = false)
    {
        playerGui.Draw();
        Sequence sequence = DOTween.Sequence();
        GameObject cardToDiscardObject;
        if(cardToDiscard <24)
        {
            cardToDiscardObject= Object.Instantiate(gameGUI.CityCardPrefab, objectToDiscardPosition, objectToDiscardRotation, gameGUI.AnimationCanvas.transform);
            cardToDiscardObject.GetComponent<CityCardDisplay>().CityCardData = gameGUI.Cities[cardToDiscard].GetComponent<City>().city;
        }
        else
        {
            cardToDiscardObject = Object.Instantiate(gameGUI.EventCardPrefab, objectToDiscardPosition, objectToDiscardRotation, gameGUI.AnimationCanvas.transform);
            cardToDiscardObject.GetComponent<EventCardDisplay>().EventCardData = gameGUI.Events[cardToDiscard - 24];
        }
        sequence.Append(cardToDiscardObject.transform.DOMove(gameGUI.PlayerDeckDiscard.transform.position, ANIMATIONDURATION));
        sequence.Join(cardToDiscardObject.transform.DORotate(gameGUI.PlayerDeckDiscard.transform.eulerAngles, ANIMATIONDURATION));
        sequence.AppendCallback(() =>
        {
            cardToDiscardObject.transform.SetParent(gameGUI.PlayerDeckDiscard.transform); //Visual fix
            gameGUI.DrawBoard();
        });
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        return $@" ""cardToDiscard"" : {cardToDiscard},
                   ""eventInitiator"" : ""{_player.Role}""
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["cardToDiscard"] is JValue jsonCardToDiscard)
        {
            cardToDiscard = jsonCardToDiscard.Value<int>();
        }
    }
}