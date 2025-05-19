using DG.Tweening;
using UnityEngine;
using static Game;
using static GameGUI;

public class EEpidemicInitiate : EngineEvent, IIndirectEvent
{
    private float AnimationDuration = 1f / gui.AnimationTimingMultiplier;
    private const float scaleToCenterScale = 3f;

    public EEpidemicInitiate()
    {

    }

    public override void Do(Timeline timeline)
    {
        theGame.SetCurrentGameState(GameState.EPIDEMIC);
    }

    public override float Act(bool qUndo = false)
    {
        GameObject epidemicCard = Object.Instantiate(gui.EpidemicCardPrefab, gui.PlayerDeck.transform.position, gui.PlayerDeck.transform.rotation, gui.AnimationCanvas.transform);
        
        gui.DrawBoard();

        //Added to show the card towards the local player.
        Transform target = gui.CenterEyeAnchor;
        Vector3 moveTo = new Vector3(0, target.position.y / 2, 0);
        Vector3 lookAt = AnimationTemplates.CalculateLookTarget(target.position, moveTo);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(epidemicCard.transform.DOShakeRotation(AnimationDuration / 2, new Vector3(0f, 0f, scaleToCenterScale), 10, 90, false));
        sequence.Append(epidemicCard.transform.DOScale(new Vector3(scaleToCenterScale, scaleToCenterScale, 1f), AnimationDuration)).
            Join(epidemicCard.transform.DOMove(moveTo, AnimationDuration)).
            Join(epidemicCard.transform.DODynamicLookAt(lookAt,AnimationDuration)); //CHANGE HERE TO 0,0.05,0 FROM 0,0,0
        sequence.AppendInterval(AnimationDuration);
        sequence.Append(epidemicCard.transform.DOScale(new Vector3(1f, 1f, 1f), AnimationDuration)).
            Join(epidemicCard.transform.DORotate(gui.EpidemicCardBoard.transform.rotation.eulerAngles, AnimationDuration)).
            Join(epidemicCard.transform.DOMove(gui.EpidemicCardBoard.transform.position, AnimationDuration)).
            OnComplete(() => {
                Object.Destroy(epidemicCard);
                gui.EpidemicCardBoard.enabled = true;
            });
        sequence.Play();
        
        return sequence.Duration();
    }
}