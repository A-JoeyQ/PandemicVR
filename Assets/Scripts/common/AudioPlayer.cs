using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
  public enum AudioClipEnum
  {
    INVALID = -1, INTRO, CLICK,
    CARD_FLIP, CHIP, SHUFFLE
  }
  static AudioPlayer thePlayer = null;

  public AudioClip[] EnumeratedClips;
  AudioSource _mySource = null;

  void Awake()
  {
    thePlayer = this;
    _mySource = GetComponent<AudioSource>();
  }

  void Destroy()
  {
    if (thePlayer == this)
      thePlayer = null;
  }
  public static void PlayClip(AudioClipEnum clipEnum, float delay = 0, float volume = 1)
  {
    int index = (int)clipEnum;
    if (index < 0 || index >= thePlayer.EnumeratedClips.Length)
    {
      Debug.LogError("Count out of range: " + clipEnum.ToString());
      return;
    }
    AudioClip clip = thePlayer.EnumeratedClips[index];
    if (clip == null)
    {
      Debug.LogError("Clip not found: " + clipEnum.ToString());
      return;
    }
    if (delay > 0)
    {
      thePlayer.ExecuteLater(delay, () =>
      {
        thePlayer._mySource.PlayOneShot(clip, volume);
      });
    }
    else
    {
      thePlayer._mySource.PlayOneShot(clip, volume);
    }
  }
  public static void RepeatClip(AudioClipEnum clipEnum, int times, float delay = 0)
  {
    int index = (int)clipEnum;
    if (index < 0 || index >= thePlayer.EnumeratedClips.Length)
    {
      Debug.LogError("Count out of range: " + clipEnum.ToString());
      return;
    }
    AudioClip clip = thePlayer.EnumeratedClips[index];
    if (clip == null)
    {
      Debug.LogError("Clip not found: " + clipEnum.ToString());
      return;
    }
    thePlayer._mySource.clip = clip;
    for (int i = 0; i < times; ++i)
    {
      thePlayer.ExecuteLater(delay + clip.length * i, () => thePlayer._mySource.Play());
    }
  }
  public static void Stop()
  {
    thePlayer._mySource.Stop();
  }
  public static float ClipLength(AudioClipEnum clipEnum)
  {
    AudioClip clip = thePlayer.EnumeratedClips[(int)clipEnum];
    if (clip == null)
    {
      Debug.LogError("Clip not found: " + clipEnum.ToString());
      return 0;
    }
    return clip.length;
  }
}
