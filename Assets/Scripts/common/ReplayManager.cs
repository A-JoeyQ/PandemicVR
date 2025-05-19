using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ReplayManager
{
    private static List<TimelineEvent> replayEvents;
    private static List<DialogEvent> dialogEvents;
    private static bool replayWithTimestamps;
    private static int currentEventIndex;
    private static int currentDialogIndex;
    private static Coroutine replayCoroutine;
    private static Coroutine dialogCoroutine;

    private static Furhat furhat;

    private static string speakerName = "Alpha";
    
    public static void StartReplay(int eventIndex, bool withTimestamps)
    {
      Timeline.theTimeline.IsReplayMode = true; 
      replayEvents = LogFileReader.logs.GetRange(0, eventIndex - 1);
      replayWithTimestamps = withTimestamps;
      currentEventIndex = 0;
      currentDialogIndex = 0;

      if (replayCoroutine != null)
      {
          Timeline.theTimeline.StopCoroutine(replayCoroutine);
      }

      if (dialogCoroutine != null)
      {
          Timeline.theTimeline.StopCoroutine(dialogCoroutine);
      }

      furhat = new Furhat("localhost", speakerName);
      
      replayCoroutine = Timeline.theTimeline.StartCoroutine(ProcessReplayEvents());
      dialogCoroutine = Timeline.theTimeline.StartCoroutine(ProcessDialogEvents());
    }
    
    private static IEnumerator ProcessReplayEvents()
    {
        Debug.Log("Processing Replay Events, currentEventIndex is " + currentEventIndex);
        while (currentEventIndex < replayEvents.Count)
        {
            while (Timeline.theTimeline.HasPendingEvents())
            {
                yield return new WaitForEndOfFrame(); // Events in the Timeline have the priority over ReplayEvents
            }
            TimelineEvent e = replayEvents[currentEventIndex];

            if (e != null)
            {
                //TODO: add a boolean to make it possible to replay with/out the timestamps
                if (!(e is IIndirectEvent))
                {
                    if (e is PlayerEvent && replayWithTimestamps)
                    {
                        float eventTime = e.Timestamp;
                        float currentReplayTime = Time.time - MainMenu.startTimestamp;

                        if (currentReplayTime < eventTime)
                        {
                            yield return new WaitForSeconds(eventTime - currentReplayTime);
                        }
                    }

                    Debug.Log("Playing event " + e);
                    e.Do(Timeline.theTimeline);
                    e.Notify();
                    
                    float delay = e.Act();
                    if (delay > 0)
                        yield return new WaitForSeconds(delay);
                    
                }
                else
                {
                    Debug.Log($"Skipping indirect event {currentEventIndex}: {e.GetType().Name}");
                }
            }
            
            currentEventIndex++;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Replay completed");
        Timeline.theTimeline.IsReplayMode = false;
    }

    private static IEnumerator ProcessDialogEvents()
    {
        var allDialogLines = DialogManager.GetDialogLinesForSpeaker(speakerName)
            .OrderBy(l => l.StartTime).ToList();
        
        while (currentDialogIndex < allDialogLines.Count)
        {
            float currentReplayTime = Time.time - MainMenu.startTimestamp;
          
            var line = allDialogLines[currentDialogIndex];
            
            float waitTime = line.StartTime - currentReplayTime;
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            
            furhat.Say(line.Speech);
            
            currentDialogIndex++;
        }

        yield return null;
            
        // Optional: Wait for the duration of the speech before moving to the next line
        //yield return new WaitForSeconds(line.Duration);

        }
}
