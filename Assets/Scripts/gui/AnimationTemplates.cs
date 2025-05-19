using DG.Tweening;
using UnityEngine;
using static GameGUI;
using static Game;
using static UnityEngine.GraphicsBuffer;

//Create a new static class that holds useful animation templates
public static class AnimationTemplates
{
    /// <summary>
    /// Generate the position which the DOTween animation should look towards
    /// </summary>
    /// <param name="eyePosition">
    ///     Position of the players eyes
    /// </param>
    /// <param name="pausePosition">
    ///     Position where the DOTween animation pause the card for the player to view
    /// </param>
    /// <returns></returns>
    public static Vector3 CalculateLookTarget(Vector3 eyePosition, Vector3 pausePosition)
    {
        Vector3 direction = pausePosition - eyePosition;
        return eyePosition + 2 * direction;
    }
    public static Sequence HighlightCardAndMove(GameObject objectToAnimate, Transform finalLocation, float scaleToCenterScale, float animationDuration)
    {
        Transform previousParent = objectToAnimate.transform.parent;
        objectToAnimate.transform.SetParent(gui.AnimationCanvas.transform); //Added to try and fix overlay issues
        //Make the object move to the center and face the local network player
        Transform target = gui.CenterEyeAnchor;
        Vector3 moveTo = new Vector3(0, target.position.y/2, 0);
        Vector3 lookAt = CalculateLookTarget(target.position, moveTo);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(objectToAnimate.transform.DOShakeRotation(animationDuration / 2, new Vector3(0f, 0f, scaleToCenterScale), 10, 90, false));
        sequence.Append(objectToAnimate.transform.DOScale(new Vector3(scaleToCenterScale, scaleToCenterScale, 1f), animationDuration)).
            Join(objectToAnimate.transform.DOMove(moveTo, animationDuration)).
            Join(objectToAnimate.transform.DODynamicLookAt(lookAt, animationDuration));
        sequence.AppendInterval(2*animationDuration);
        sequence.Append(objectToAnimate.transform.DOScale(new Vector3(1f, 1f, 1f), animationDuration)).
            Join(objectToAnimate.transform.DORotate(finalLocation.rotation.eulerAngles, animationDuration)).
            Join(objectToAnimate.transform.DOMove(finalLocation.position, animationDuration));
        sequence.AppendCallback(() =>
        {
            objectToAnimate.transform.SetParent(previousParent);
        });
        return sequence;
    }
}
