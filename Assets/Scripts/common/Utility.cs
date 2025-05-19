using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public static class Utility
{
    public static T GetComponentOnChild<T>(this Transform tx, string pathToChild)
    {
        return tx.Find(pathToChild).GetComponent<T>();
    }

    public static T GetComponentOnChild<T>(this GameObject obj, string pathToChild)
    {
        return obj.transform.Find(pathToChild).GetComponent<T>();
    }
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        if (source == null)
            return true;
        return !source.Any();
    }
    public static void setAlpha(this Image img, float alpha)
    {
        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
    }
    public static string FullPath(this Transform tx)
    {
        string msg = tx.name;
        while (tx.parent != null)
        {
            msg = tx.parent.name + "/" + msg;
            tx = tx.parent;
        }
        return msg;
    }

    public static void ExecuteLater(this MonoBehaviour behaviour, float delay, System.Action fn)
    {
        behaviour.StartCoroutine(_realExecute(delay, fn));
    }
    static IEnumerator _realExecute(float delay, System.Action fn)
    {
        yield return new WaitForSeconds(delay);
        fn();
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    public static T PopRandom<T>(this IList<T> list)
    {
        return list.Pop(UnityEngine.Random.Range(0, list.Count));
    }
    public static T Pop<T>(this IList<T> list, int index)
    {
        T item = list[index];
        list.RemoveAt(index);
        return item;
    }

    public static T Pop<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            throw new InvalidOperationException("Cannot pop from empty list.");

        var lastIndex = list.Count - 1;
        var item = list[lastIndex];
        list.RemoveAt(lastIndex);
        return item;
    }

    public static void DestroyChildrenImmediate(this GameObject go)
    {
        GameObject[] toDestroy = new GameObject[go.transform.childCount];
        for (int i = 0; i < go.transform.childCount; ++i) toDestroy[i] = go.transform.GetChild(i).gameObject;
        for (int i = 0; i < toDestroy.Length; ++i)
            GameObject.DestroyImmediate(toDestroy[i]);
    }
    public static void Shuffle<T>(this IList<T> list, Random.State randomState)
    {
        // I copied this one from: http://stackoverflow.com/questions/273313/randomize-a-listt
        Random.state = randomState;
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }


    /*
  public static GameObject moveCopyTo(GameObject source, Vector3 destination,
    Vector3 targetRotation, float time, float delay = 0f,
    string textToDisplay = "")
  {
    return moveCopyTo(source, destination, targetRotation, Vector3.one, time, delay, textToDisplay);
  }
  public static GameObject moveCopyTo(GameObject source, Vector3 destination,
    Vector3 targetRotation, Vector3 targetScale, float time, float delay = 0f,
    string textToDisplay = "")
  {
    GameObject movingResource = cloneOnCanvas(source);
    if (textToDisplay.Length > 0)
      movingResource.GetComponentOnChild<Text>("Text").text = textToDisplay;

    movingResource.gameObject.SetActive(false);
    movingResource.transform.DOMove(destination, time)
      .SetEase(Ease.InOutQuad).SetDelay(delay).OnStart(() => movingResource.gameObject.SetActive(true));
    movingResource.transform.DORotate(targetRotation, time)
      .SetEase(Ease.InOutQuad).SetDelay(delay);
    movingResource.transform.DOScale(targetScale, time)
      .SetEase(Ease.InOutQuad).SetDelay(delay);
    Destroy(movingResource, time+delay);
    return movingResource;
  }
  public static GameObject cloneOnCanvas(GameObject source)
  {
    GameObject movingResource = Instantiate(source);
    movingResource.SetActive(true);
    movingResource.transform.SetParent(GameGUI.theGameGUI.AnimationCanvas.transform, false);
    movingResource.transform.rotation = source.transform.rotation;
    movingResource.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
    movingResource.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
    movingResource.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
    movingResource.transform.position = source.transform.position;
    movingResource.GetComponent<RectTransform>().sizeDelta = source.GetComponent<RectTransform>().rect.size;
    movingResource.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
    return movingResource;
  }
  public static GameObject moveCopyTo(GameObject source, Transform destination,
    float time, float delay = 0, string textToDisplay = "")
  {
    RectTransform destRect = destination.GetComponent<RectTransform>();
    float amount = Mathf.Min(
        (destRect.rect.size.x * destRect.transform.localScale.x) / (source.GetComponent<RectTransform>().rect.size.x * source.transform.localScale.x),
        (destRect.rect.size.y * destRect.transform.localScale.y) / (source.GetComponent<RectTransform>().rect.size.y * source.transform.localScale.y));
    Vector2 scale = new Vector2(amount, amount);
    //Vector3 newDest = destRect.TransformPoint(destRect.rect.center);
    return moveCopyTo(source, destination.position, 
      destination.rotation.eulerAngles, scale, time, delay, textToDisplay);
  }
  public static void animateObjectList(List<GameObject> movingSprites,
    Vector3 source, Transform destination, float duration, 
    AudioPlayer.AudioClipEnum clip = AudioPlayer.AudioClipEnum.INVALID)
  {
    for (int i = 0; i < movingSprites.Count; ++i)
    {
      GameObject localObj = movingSprites[i];
      localObj.transform.SetParent(theGameGUI.GameCanvas.transform, false);
      localObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
      localObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
      localObj.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
      localObj.GetComponent<RectTransform>().sizeDelta = new Vector2(32, 32);
      localObj.transform.position = source;
      //localObj.transform.position = Money.transform.position;
      localObj.SetActive(false);
      float delay = (duration / 2) / movingSprites.Count * i;
      localObj.transform.DOMove(
        destination.position, duration - delay)
        .SetDelay(delay)
        .OnStart(() =>
        {
          localObj.SetActive(true);
          if (clip != AudioPlayer.AudioClipEnum.INVALID)
            AudioPlayer.PlayClip(clip);
        })
        .OnComplete(() => Destroy(localObj));
      localObj.transform.DORotate(destination.rotation.eulerAngles, duration);
    }
  }
  */

}
