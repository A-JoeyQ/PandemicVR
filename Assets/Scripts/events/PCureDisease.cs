using System;
using DG.Tweening;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

internal class PCureDisease : PlayerEvent, IInitializableEvent
{
    private ENUMS.VirusName virusName;
    private List<int> selectedCards;
    private const float ANIMATIONDURATION = 1f;
    private Vector3[] originalCardPositions;
    private Quaternion[] originalCardRotations;

    public PCureDisease(Player player) : base(player) {}
    
    public PCureDisease(List<int> selectedCards): base(Game.theGame.CurrentPlayer)
    {
        this.selectedCards = new List<int>(selectedCards);
        //SetupCardsAndFindDominantVirus();
    }

    private void SetupCardsAndFindDominantVirus()
    {
        originalCardPositions = new Vector3[selectedCards.Count];
        originalCardRotations = new Quaternion[selectedCards.Count];

        int numRed = 0;
        int numYellow = 0;
        int numBlue = 0;

        for (int i = 0; i < selectedCards.Count; i++)
        {
            originalCardPositions[i] = _playerGui.GetCardInHand(selectedCards[i]).transform.position;
            originalCardRotations[i] = _playerGui.GetCardInHand(selectedCards[i]).transform.rotation;
            switch (game.Cities[selectedCards[i]].city.virusInfo.virusName)
            {
                case ENUMS.VirusName.Blue:
                    numBlue++;
                    if(numBlue > 2)
                        virusName = ENUMS.VirusName.Blue;
                    break;
                case ENUMS.VirusName.Red:
                    numRed++;
                    if (numRed > 2)
                        virusName = ENUMS.VirusName.Red;
                    break;
                case ENUMS.VirusName.Yellow:
                    numYellow++;
                    if (numYellow > 2)
                        virusName = ENUMS.VirusName.Yellow;
                    break;
            }
        }
    }

    public override void Do(Timeline timeline)
    {
        SetupCardsAndFindDominantVirus();
        for (int i = 0; i < selectedCards.Count; i++)
        {
            _player.RemoveCardInHand(selectedCards[i], true);
        }
        switch (virusName)
        {
            case ENUMS.VirusName.Blue:
                game.BlueCure = true;
                break;
            case ENUMS.VirusName.Red:
                game.RedCure = true;
                break;
            case ENUMS.VirusName.Yellow:
                game.YellowCure = true;
                break;
        }

        _player.DecreaseActionsRemaining(1);
        _playerGui.ActionSelected = ActionTypes.None;
        // Check if all diseases are cured and if so, end the game

        if (game.BlueCure && game.RedCure && game.YellowCure)
        {
            Timeline.theTimeline.ClearPendingEvents();
            timeline.AddEvent(new EGameOver(ENUMS.GameOverReasons.PlayersWon));
        }
    }

    public override float Act(bool qUndo = false)
    {
        Sequence sequence = DOTween.Sequence();
        _playerGui.Draw();
        for (int i = 0; i < selectedCards.Count; i++)
        {
            GameObject cardToAddObject = game.AddPlayerCardToTransform(selectedCards[i], gameGUI.PlayerDeckDiscard.transform, false, _playerGui);
            cardToAddObject.transform.position = originalCardPositions[i];
            cardToAddObject.transform.rotation = originalCardRotations[i];
            sequence.Join(cardToAddObject.transform.DOMove(gameGUI.PlayerDeckDiscard.transform.position, ANIMATIONDURATION));
            sequence.Join(cardToAddObject.transform.DORotate(gameGUI.PlayerDeckDiscard.transform.eulerAngles, ANIMATIONDURATION));
        }
        sequence.Append(gameGUI.VialTokens[(int)virusName].transform.DOMove(gameGUI.VialTokensTransforms[(int)virusName].transform.position, ANIMATIONDURATION).OnComplete(() =>
            {
                gameGUI.DrawBoard();
            }));
        return sequence.Duration();
    }

    public override string GetLogInfo()
    {
        string cardIds = string.Join(", ", selectedCards);
        return $@" ""curedDisease"" : ""{virusName}"",
                    ""selectedCards"" : [{cardIds}]
                ";
    }

    public void InitializeGameObjects(JObject jsonData)
    {
        if (jsonData["curedDisease"] is JValue jsonCuredDisease)
        {
            string curedDiseaseString = jsonCuredDisease.Value<string>();
            virusName = Enum.Parse<ENUMS.VirusName>(curedDiseaseString);
        }

        if (jsonData["selectedCards"] is JArray jsonSelectedCards)
        {
            selectedCards = new List<int>();
            foreach (var token in jsonSelectedCards)
            {
                if (int.TryParse(token.ToString(), out int cardId))
                {
                    selectedCards.Add(cardId);
                }
                else
                {
                    Debug.LogWarning($"Failed to parse card ID: {token}");
                }
            }

            Debug.Log("Curing disease !");
            //SetupCardsAndFindDominantVirus();
        } 
    }
}