using System.Collections.Generic;
using System.Linq;

public static class DialogManager
{
    private static List<DialogEvent> _dialogEvents = new List<DialogEvent>();
    public static IReadOnlyList<DialogEvent> DialogEvents => _dialogEvents.AsReadOnly();

    public static void AddDialogEvent(DialogEvent dialogEvent)
    {
        _dialogEvents.Add(dialogEvent);
        _dialogEvents = _dialogEvents.OrderBy(e => e.Timestamp).ToList();
    }

    public static void Clear()
    {
        _dialogEvents.Clear();
    }

    public static IEnumerable<DialogLine> GetDialogLinesForSpeaker(string speakerName)
    {
        return _dialogEvents
            .SelectMany(e => e.Lines)
            .Where(l => l.Speaker == speakerName);
    }
}
public class DialogEvent
{
    public float Timestamp { get; }
    public IReadOnlyList<DialogLine> Lines { get; }
    
    public DialogEvent(float timestamp, IEnumerable<DialogLine> lines)
    {
        Timestamp = timestamp;
        Lines = lines.OrderBy(l => l.StartTime).ToList().AsReadOnly();
    }
    
}

public class DialogLine
{
    public string Speaker { get; }
    public string Speech { get; }
    public float StartTime { get; }
    public float Duration { get; }

    public DialogLine(string speaker, string speech, float startTime, float duration)
    {
        Speaker = speaker;
        Speech = speech;
        StartTime = startTime;
        Duration = duration;
    }
}
