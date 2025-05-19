

using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using Newtonsoft.Json.Linq;

public class PFlyToCity : PlayerEvent, IInitializableEvent
{
    int flyFrom;
    private Vector3 originalCardPosition;
    private Quaternion originalCardRotation;
    int flyTo;
    float ANIMATIONDURATION = 1f / GameGUI.gui.AnimationTimingMultiplier;

    public PFlyToCity(Player player) : base(player) {}
    public PFlyToCity(int flyTo) : base(Game.theGame.CurrentPlayer)
    {
        this.flyTo = flyTo;
        flyFrom = _player.GetCurrentCity();
        originalCardPosition = _playerGui.GetCardInHand(flyTo).transform.position;
        originalCardRotation = _playerGui.GetCardInHand(flyTo).transform.rotation;
    }

    public override void Do(Timeline timeline)
    {
        _player.RemoveCardInHand(flyTo, true);
        _player.UpdateCurrentCity(flyTo, true);
        _player.DecreaseActionsRemaining(1);
    }

    public override float Act(bool qUndo = false)
    {
        _playerGui.Draw();
        DG.Tweening.Sequence sequence = DOTween.Sequence();
        GameObject cardToAddObject = game.AddPlayerCardToTransform(flyTo, gameGUI.PlayerDeckDiscard.transform, false, _playerGui);
        cardToAddObject.transform.position = originalCardPosition;
        cardToAddObject.transform.rotation = originalCardRotation;
        sequence.Append(cardToAddObject.transform.DOMove(gameGUI.PlayerDeckDiscard.transform.position, ANIMATIONDURATION));
        sequence.Join(cardToAddObject.transform.DORotate(gameGUI.PlayerDeckDiscard.transform.eulerAngles, ANIMATIONDURATION));
        sequence.AppendCallback(() =>
        {
            GameObject.Destroy(cardToAddObject);
            gameGUI.DrawBoard();
        });
        
        //Sequence sequence = DOTween.Sequence();
        //sequence.Append(cardToAddObject.transform.DOShakeRotation(durationMove / 2, new Vector3(0f, 0f, scaleToCenterScale), 10, 90, false));
        //sequence.Append(cardToAddObject.transform.DOScale(new Vector3(scaleToCenterScale, scaleToCenterScale, 1f), durationMove)).
        //    Join(cardToAddObject.transform.DOMove(new Vector3(0, 0, 0), durationMove));
        //sequence.AppendInterval(durationMove);
        //sequence.Append(cardToAddObject.transform.DOScale(new Vector3(1f, 1f, 1f), durationMove)).
        //    Join(cardToAddObject.transform.DOMove(gui.InfectionDiscard.transform.position, durationMove));

        City currentCity = Game.theGame.Cities[flyFrom];
        City cityToMoveTo = Game.theGame.Cities[flyTo];
        currentCity.RemovePawn(_player);
        currentCity.Draw();
        GameObject movingPawn = Object.Instantiate(gameGUI.PawnPrefab, currentCity.transform.position, currentCity.transform.rotation, gameGUI.AnimationCanvas.transform);
        //movingPawn.GetComponent<Image>().color = _playerGui.roleCard.RoleCardData.roleColor;
        //movingPawn.GetComponent<Outline>().enabled = true;
        movingPawn.GetComponent<Pawn>().SetRoleAndPlayer(_player);
        sequence.Append(movingPawn.transform.DOMove(cityToMoveTo.transform.position, ANIMATIONDURATION).OnComplete(() =>
        {
            cityToMoveTo.Draw();
            Object.Destroy(movingPawn);
        }));
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        return $@" ""flyFrom"" : {flyFrom},
                    ""flyTo"" : {flyTo}
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["flyFrom"] is JValue jsonFlyFrom)
        {
            flyFrom = jsonFlyFrom.Value<int>();
        }
        
        if (jsonData["flyTo"] is JValue jsonFlyTo)
        {
            flyTo = jsonFlyTo.Value<int>();
        }
    }
}