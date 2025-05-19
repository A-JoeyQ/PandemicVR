using System;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using Object = UnityEngine.Object;

public class PShareKnowledge : PlayerEvent, IInitializableEvent
{
    PlayerGUI playerFrom, playerTo;
    float ANIMATIONDURATION = 1f/ GameGUI.gui.AnimationTimingMultiplier;
    private int cityID;
    Vector3 initialPosition;
    Quaternion initialRotation;
    CityCard cardData;

    public PShareKnowledge(Player player) : base(player) {}
    
    public PShareKnowledge(PlayerGUI playerFrom, PlayerGUI playerTo) : base(Game.theGame.CurrentPlayer)
    {
        this.playerFrom = playerFrom;
        this.playerTo = playerTo;
        cityID = playerFrom.PlayerModel.GetCurrentCity();
        
    }

    public override void Do(Timeline timeline)
    {
        CityCardDisplay cityCard = playerFrom.GetCardInHand(cityID).GetComponent<CityCardDisplay>();
        initialPosition = cityCard.transform.position;
        initialRotation = cityCard.transform.rotation;
        cardData = cityCard.CityCardData;
        
        playerFrom.PlayerModel.RemoveCardInHand(cityID);
        playerTo.PlayerModel.AddCardToHand(cityID);
        playerTo.UpdateCardsState(CardGUIStates.None);
        playerFrom.UpdateCardsState(CardGUIStates.None);
        _player.DecreaseActionsRemaining(1);
    }

    public override float Act(bool qUndo = false)
    {
        playerFrom.Draw();
        GameObject cityCardCopy = Object.Instantiate(gameGUI.CityCardPrefab, initialPosition, initialRotation, gameGUI.AnimationCanvas.transform);
        CityCardDisplay cityCardCopyDisplay = cityCardCopy.GetComponent<CityCardDisplay>();
        Sequence sequence = DOTween.Sequence();
        cityCardCopyDisplay.CityCardData = cardData;
        GameObject toMoveTo = playerTo.getFirstCardInHand();
        if (toMoveTo != null)
            toMoveTo = playerTo.PlayerCards;
        sequence.Append(cityCardCopy.transform.DOMove(toMoveTo.transform.position, ANIMATIONDURATION));
        sequence.Join(cityCardCopy.transform.DORotate(toMoveTo.transform.rotation.eulerAngles, ANIMATIONDURATION));
        sequence.AppendCallback(() => {
            Object.Destroy(cityCardCopy);
            playerTo.Draw();
        });
        
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        return $@" ""cityId"" : {cityID},
                    ""playerFrom"" : ""{playerFrom.PlayerModel.Role}"",
                    ""playerTo"" : ""{playerTo.PlayerModel.Role}""
                ";
    }
    
    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["cityId"] is JValue jsonCityId)
        {
            cityID = jsonCityId.Value<int>();
        }
        
        if (jsonData["playerFrom"] is JValue jsonPlayerFrom)
        {
            string playerFromRoleString = jsonPlayerFrom.Value<string>();
            if (!string.IsNullOrEmpty(playerFromRoleString))
            {
                Player.Roles playerFromRole = Enum.Parse<Player.Roles>(playerFromRoleString);
                playerFrom = PlayerList.GetPlayerByRole(playerFromRole).playerGui;
            }
            else
            {
                playerFrom = null;
            }
        }

        if (jsonData["playerTo"] is JValue jsonPlayerTo)
        {
            string playerToRoleString = jsonPlayerTo.Value<string>();
            if (!string.IsNullOrEmpty(playerToRoleString))
            {
                Player.Roles playerToRole = Enum.Parse<Player.Roles>(playerToRoleString);
                playerTo = PlayerList.GetPlayerByRole(playerToRole).playerGui;
            }
            else
            {
                playerTo = null;
            }
        }
    }
}