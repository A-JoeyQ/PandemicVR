using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System;

public class Timeline : MonoBehaviour
{

    public static Timeline theTimeline;
    bool QReprocessingEvents = false;

    protected List<TimelineEvent> myPendingEvents = new List<TimelineEvent>();
    protected List<TimelineEvent> myEvents = new List<TimelineEvent>();
    public bool HasPendingEvents() { return myPendingEvents.Count > 0; }

    public List<TimelineEvent> ProcessedEvents { get { return myEvents; } }

    private bool myQProcessingEvent = false;
    private bool myQReady = false;
    public bool QReady { get { return myQReady; } }

    public bool IsReplayMode { get; set; } = false;

    void Awake()
    {
        theTimeline = this;
        StartCoroutine(ProcessEvents());
    }

    void OnDestroy()
    {
        theTimeline = null;
    }

    public void ResetTimeline()
    {
        myPendingEvents.Clear();
        myEvents.Clear();
    }

    IEnumerator ProcessEvents()
    {
        while (true)
        {
            if (_executeDelay > 0)
            {
                yield return new WaitForSeconds(_executeDelay);
                _executeDelay = 0;
            }
            if (myPendingEvents.Count > 0)
            {
                myQReady = false;
                TimelineEvent e = myPendingEvents.Pop(0);
                myEvents.Add(e);

                myQProcessingEvent = true;
                try
                {
                    e.Do(this);
                    e.Notify();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
                myQProcessingEvent = false;
                float delay = e.Act();
                if (delay > 0)
                    yield return new WaitForSeconds(delay);
            }
            else
            {
                myQReady = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void AddEvent(TimelineEvent e)
    {
        if (QReprocessingEvents) return;

        Debug.Log("addEvent " + e.ToString());

        myPendingEvents.Add(e);

        if (myPendingEvents.Count > 100)
            Debug.Log("Control::addEvent - Warning: There are " + myPendingEvents.Count + " pending events.");
    }

    public void ClearPendingEvents()
    {
        myPendingEvents.Clear();
    }

    float _executeDelay = 0;
    public void ExecuteEvent(TimelineEvent e)
    {
        if (myQProcessingEvent)
        {
            Debug.LogError("Never execute an event from inside a do()");
            return;
        }

        myEvents.Add(e);
        myQProcessingEvent = true;
        e.Do(this);
        myQProcessingEvent = false;
        _executeDelay = e.Act();
    }
    public int QueueSize() { return myPendingEvents.Count; }

    public void ReprocessEvents(List<TimelineEvent> events)
    {
        theTimeline.StopAllCoroutines();
        myEvents.Clear();
        myPendingEvents.Clear();

        QReprocessingEvents = true;
        while (events.Count > 0)
        {
            TimelineEvent e = events.Pop(0);
            myEvents.Add(e);
            e.Do(this);
            //if ( PlayerList.playerAtPosition(0) != null )
            //Debug.Log("e: "+e.Id + "," + e.PlayerPosition+" stack=" + string.Join(",", PlayerList.playerAtPosition(0).StateStack.Select(s => s.ToString()).ToArray()));
        }
        QReprocessingEvents = false;
        StartCoroutine(ProcessEvents());

        this.ExecuteLater(0.1f, OnEventsReprocessed);
    }
    virtual public void OnEventsReprocessed()
    {
        // Draw GUI
        GameGUI.gui.Draw();
        //this.ExecuteLater(0.2f, () => GameGUI.theGameGUI.LoadOverlay.gameObject.SetActive(false));
    }

    public void Undo()
    {
        if (myEvents.Count == 0)
            return;

        // Find the first event
        TimelineEvent eventToUndo = myEvents[myEvents.Count - 1];

        if (!eventToUndo.QUndoable)
        {
            //WindowsVoice.speak("Can't undo your last event.");
            Debug.Log("Control::undo - Attempting to undo an event that can't be undone.");
            return;
        }

        myPendingEvents.Clear();

        if (eventToUndo.QContinueUndo)
        {
            for (int i = myEvents.IndexOf(eventToUndo);
                  eventToUndo.QContinueUndo && i >= 0; --i)
                eventToUndo = myEvents[i];
        }
        LazyUndo(eventToUndo);
        return;
    }
    public void LazyUndo(TimelineEvent eventToUndo)
    {
        int index = myEvents.IndexOf(eventToUndo);
        List<TimelineEvent> toProcess = new List<TimelineEvent>();
        toProcess.AddRange(myEvents.GetRange(0, index));
        myEvents.Clear();

        ReprocessEvents(toProcess);
    }
    public static string DefaultSaveName()
    {
        return System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
    }
    public string Save(string saveName)
    {
        if (saveName == null)
            saveName = DefaultSaveName();

        string dirName = Application.persistentDataPath + "/savedGames/" + saveName;
        System.IO.Directory.CreateDirectory(dirName);

        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        using (StreamWriter fs = new StreamWriter(dirName + "/Events", false))
            fs.Write(JsonConvert.SerializeObject(myEvents, Formatting.Indented, settings));

        using (StreamWriter fs = new StreamWriter(dirName + "/Pending", false))
            fs.Write(JsonConvert.SerializeObject(myPendingEvents, Formatting.Indented, settings));

        return saveName;
    }

    public void SaveScreenshot(string name)
    {
        string dirName = Application.persistentDataPath + "/savedGames/" + name;
        ScreenCapture.CaptureScreenshot(dirName + "/Snapshot.png");
    }

    public static List<TimelineEvent> Load(string name)
    {
        List<TimelineEvent> rv = new List<TimelineEvent>();

        string dirName = Application.persistentDataPath + "/savedGames/" + name;

        using (StreamReader fs = new StreamReader(dirName + "/Events"))
        {
            string obj = fs.ReadToEnd();
            rv.AddRange(
              JsonConvert.DeserializeObject<List<TimelineEvent>>(obj, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
                        );
        }

        using (StreamReader fs = new StreamReader(dirName + "/Pending"))
        {
            string obj = fs.ReadToEnd();
            rv.AddRange(
              JsonConvert.DeserializeObject<List<TimelineEvent>>(obj, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto })
                        );
        }

        return rv;
    }
}

