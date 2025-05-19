using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameGUI;
using static Game;

internal class EAddEpidemicCards : EngineEvent, IIndirectEvent
{
    private const float OffsetEpidemicCards = 2.5f;
    private float DurationEpidemicMove = 1f / gui.AnimationTimingMultiplier;

    public EAddEpidemicCards()
    {
        QUndoable = true;
    }

    public override void Do(Timeline timeline)
    {
        theGame.PlayerCards = addEpidemicCards(theGame.PlayerCards);
    }

    public override float Act(bool qUndo = false)
    {
        float offset = 0;
        for (int i = 0; i < 3; i++)
        {
            GameObject epidemicCard = Object.Instantiate(gui.EpidemicCardPrefab, gui.AnimationCanvas.transform);
            epidemicCard.transform.rotation = gui.PlayerDeck.transform.rotation;
            epidemicCard.transform.Translate(new Vector3(offset - OffsetEpidemicCards, 0, 0));
            offset += OffsetEpidemicCards;
            epidemicCard.transform.DOMove(gui.PlayerDeck.transform.position, DurationEpidemicMove).OnComplete(() =>
            {
                gui.DrawBoard();
                Object.Destroy(epidemicCard);
            });
        }

        return DurationEpidemicMove;
    }

    static List<int> addEpidemicCards(List<int> originalList)
    {
        int third = originalList.Count / 3;

        // Divide the list into three parts
        var part1 = originalList.Take(third).ToList();
        var part2 = originalList.Skip(third).Take(third).ToList();
        var part3 = originalList.Skip(third * 2).ToList();

        // Add the value to each part
        part1.Add(28);
        part2.Add(28);
        part3.Add(28);

        // Shuffle each part
        part1.Shuffle(theGame.playerCardsRandomGeneratorState);
        part2.Shuffle(theGame.playerCardsRandomGeneratorState);
        part3.Shuffle(theGame.playerCardsRandomGeneratorState);
        theGame.playerCardsRandomGeneratorState = Random.state;

        // Join them back together
        var finalList = new List<int>();
        finalList.AddRange(part1);
        finalList.AddRange(part2);
        finalList.AddRange(part3);

        return finalList;
    }

}


