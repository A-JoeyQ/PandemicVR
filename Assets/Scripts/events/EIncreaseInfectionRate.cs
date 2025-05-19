using DG.Tweening;
using UnityEngine;
using static GameGUI;
using static Game;

public class EIncreaseInfectionRate : EngineEvent, IIndirectEvent
{
    private float ANIMATIONDURATION = 1f / gui.AnimationTimingMultiplier;

    public override void Do(Timeline timeline)
    {
        theGame.InfectionRate++;
    }

    public override float Act(bool qUndo = false)
    {
        GameObject moveFrom = gui.GetInfectionRateMarker(theGame.InfectionRate - 1);
        GameObject moveTo = gui.GetInfectionRateMarker(theGame.InfectionRate);
        moveFrom.DestroyChildrenImmediate();
        GameObject marker = Object.Instantiate(gui.InfectionRateMarkerPrefab, moveFrom.transform.position, moveFrom.transform.rotation, gui.AnimationCanvas.transform);
        marker.transform.DOMove(moveTo.transform.position, ANIMATIONDURATION).OnComplete(() =>
        {
            Object.Destroy(marker);
            gui.DrawBoard();
        });
        return ANIMATIONDURATION;
    }
}
