using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static ENUMS;
using static GameGUI;
using static Game;

public class EIncreaseOutbreak : EngineEvent, IIndirectEvent
{
    private float ANIMATIONDURATION = 1f / gui.AnimationTimingMultiplier;


    public override void Do(Timeline timeline)
    {
        theGame.OutbreakCounter++;
        if(theGame.OutbreakCounter >= 4)
        {
            Timeline.theTimeline.ClearPendingEvents();
            timeline.AddEvent(new EGameOver(GameOverReasons.TooManyOutbreaks));
        }
    }

    public override float Act(bool qUndo = false)
    {

        Transform moveFrom = gui.OutbreakMarkerTransforms[theGame.OutbreakCounter -1];
        Transform moveTo = gui.OutbreakMarkerTransforms[theGame.OutbreakCounter];
        moveFrom.gameObject.DestroyChildrenImmediate();
        GameObject marker = Object.Instantiate(gui.OutbreakMarkerPrefab, moveFrom.position, moveFrom.rotation, gui.AnimationCanvas.transform);
        marker.transform.DOMove(moveTo.transform.position, ANIMATIONDURATION).OnComplete(() =>
        {
            Object.Destroy(marker);
            gui.DrawBoard();
        });

        return ANIMATIONDURATION;
    }


}
