namespace EvolutionTwo.Presentation;

public readonly record struct BroadcastPortraitFrame(string Clip, int Frame, PortraitGaze Gaze);

/// <summary>Real-time performance: independent conversational looks and result-triggered gestures.</summary>
public sealed class BroadcastAnimationDirector
{
    private readonly IReadOnlyDictionary<string, double[]> _durations;
    private readonly bool[] _partner = new bool[2];
    private readonly double[] _lookChanged = { -10, -10 };
    private double _time, _eventStarted, _nextGestureAt;
    private int _speechSequence = -1, _turn = -1, _completed, _gestureSequence;
    private Random _random = new(773);
    private double _nextQuietGlance = 7, _quietGlanceEnd;
    private int _quietActor;
    private string? _event;
    private double _excitedStarted = -100;
    public bool IsExcited => _time - _excitedStarted < 8;
    public string? CurrentEvent => _event;
    public BroadcastPortraitFrame[] Frames { get; } = new BroadcastPortraitFrame[2];

    public BroadcastAnimationDirector(IReadOnlyDictionary<string, double[]> durations)
    {
        _durations = durations;
        Reset();
    }

    public void Reset()
    {
        _time = _eventStarted = _nextGestureAt = 0;
        _speechSequence = -1; _turn = -1; _completed = 0;
        _gestureSequence = 0;
        _random=new(773); _nextQuietGlance=7; _quietGlanceEnd=0;
        _event = null;
        _excitedStarted = -100;
        for (var actor = 0; actor < 2; actor++)
        {
            _partner[actor] = false; _lookChanged[actor] = -10;
            Frames[actor] = new("conversation", 0, PortraitGaze.Board);
        }
    }

    public void Update(double time, int speechSequence, int speaker, double speechAge, bool speaking,
        int completed, int gestureSequence = 0, string? gesture = null, ArenaAttention? attention = null)
    {
        if (time < _time || completed < _completed) Reset();
        _time = time;
        if (speechSequence != _speechSequence) { _speechSequence = speechSequence; _turn++; }
        _completed = completed;
        if (_event is not null && time - _eventStarted >= _durations[_event].Sum())
        {
            if (_event == "stand") _excitedStarted = _eventStarted + _durations[_event].Sum();
            _event = null;
            // Carry the celebration through sitting down; never insert a calm
            // idle frame between the standing reaction and its spoken aftermath.
            Array.Fill(_partner, false); Array.Fill(_lookChanged, time - 1);
        }
        if (gestureSequence != _gestureSequence)
        {
            _gestureSequence = gestureSequence;
            // React with this line or stay seated; never replay an old event later.
            if (gesture == "excited" && _event is null)
            {
                if (!IsExcited) _excitedStarted = time;
            }
            else if (gesture is not null && _event is null && time >= _nextGestureAt)
            {
                _excitedStarted = -100;
                _event = gesture; _eventStarted = time; _nextGestureAt = time + 25;
            }
        }
        if (_event is not null)
        {
            var frame = FrameAt(_durations[_event], time - _eventStarted);
            for (var actor = 0; actor < 2; actor++)
                Frames[actor] = new(_event, frame, actor == 1 && frame >= 4 ? PortraitGaze.Partner : PortraitGaze.Board);
            return;
        }

        if (IsExcited)
        {
            var age = time - _excitedStarted;
            // Hold the emotional beat, share it with Rene, then settle toward
            // the rink. Avoid an automatic left-right scan after every event.
            var pose = age < 2.6 ? 0 : age < 5.8 ? 6 : 0;
            var talking = speaking && speaker == 0 && MouthAt(speechAge) != 0;
            Frames[0] = new("excited", pose + (talking ? 1 : 0), pose == 6 ? PortraitGaze.Partner : PortraitGaze.Board);
            // René answers the invitation later and can keep speaking independently.
            var partner = age >= 3.25 && age < 6.4;
            var mouth = speaking && speaker == 1 ? MouthAt(speechAge) : 0;
            Frames[1] = new("conversation", partner ? (mouth == 0 ? 5 : mouth + 5) : mouth,
                partner ? PortraitGaze.Partner : PortraitGaze.Board);
            return;
        }

        var looks = ConversationLooks(_turn % 4, speaker, speechAge);
        // In a quiet stretch, an occasional solitary glance; the other keeps watching.
        if (!speaking && speechAge > 6)
        {
            if(time>=_nextQuietGlance)
            {
                _quietActor=_random.Next(2);_quietGlanceEnd=time+.6+_random.NextDouble();
                _nextQuietGlance=time+7+_random.NextDouble()*12;
            }
            looks = (time<_quietGlanceEnd && _quietActor==0,time<_quietGlanceEnd && _quietActor==1);
        }
        for (var actor = 0; actor < 2; actor++)
        {
            var looking = actor == 0 ? looks.Gerard : looks.Rene;
            if (looking != _partner[actor]) { _partner[actor] = looking; _lookChanged[actor] = time; }
            var transitioning = time - _lookChanged[actor] < .12;
            var mouth = speaking && speaker == actor ? MouthAt(speechAge) : 0;
            var blinkPeriod = actor == 0 ? 4.7 : 6.1;
            var blink = attention?.Blinking(actor, time) ?? ((time + actor * 1.3) % blinkPeriod < .11);
            var frame = transitioning ? 4 : looking ? mouth == 0 ? 5 : mouth + 5
                : mouth != 0 ? mouth : blink ? 3 : 0;
            if (actor == 0 && speaker == 0 && speaking && !looking && !transitioning)
            {
                if (speechAge >= 2.85 && speechAge < 3.18) frame = 9;
                else if (speechAge >= 3.18 && speechAge < 3.4) frame = 10;
            }
            if (!looking && !transitioning && attention is not null && frame is not (9 or 10))
            {
                var rinkFrame = attention.Frame(actor, speaking && speaker == actor, speechAge);
                if (blink) rinkFrame += 8;
                Frames[actor] = new("rink", rinkFrame, PortraitGaze.Board);
            }
            else Frames[actor] = new("conversation", frame, looking ? PortraitGaze.Partner : PortraitGaze.Board);
        }
    }

    public static (bool Gerard, bool Rene) ConversationLooks(int pattern, int speaker, double age)
    {
        bool first, other;
        switch (pattern)
        {
            case 0: first = age >= .7 && age < 1.7; other = false; break;
            case 1: first = age >= .65 && age < 2.1; other = age >= 1.35 && age < 2.8; break;
            case 2: first = false; other = age >= .5 && age < 1.8; break;
            default: first = age >= 1.15 && age < 2.6; other = age >= .45 && age < 1.9; break;
        }
        return speaker == 0 ? (first, other) : (other, first);
    }

    private static int MouthAt(double age)
    {
        // Includes closed-mouth beats: punctuation does not read as a constant flap.
        ReadOnlySpan<int> shapes = [0, 1, 2, 1, 0, 1, 1, 2, 0];
        return shapes[(int)(age * 8) % shapes.Length];
    }

    public static int FrameAt(IReadOnlyList<double> durations, double age)
    {
        for (var i = 0; i < durations.Count - 1; i++)
        {
            if (age < durations[i]) return i;
            age -= durations[i];
        }
        return durations.Count - 1;
    }
}
