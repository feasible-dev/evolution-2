namespace EvolutionTwo.Presentation;

public enum MarqueePalette { OrangeBlue, Orange, Blue, Red }
public readonly record struct MarqueeCue(string Text, MarqueePalette Palette, bool Centered = false);

/// <summary>Broadcast routines and one-shot result reactions; never changes the match.</summary>
public sealed class MarqueeProgram
{
    private int _completed, _routine;
    private int _activeRoutine = -1, _lastStatsSecond = -1;
    private bool _started;
    private readonly bool[] _perfect = new bool[2], _high = new bool[2];
    private double _time, _nextCue, _impactStart = -100, _nextImpact;
    public MarqueeCue Cue { get; private set; }
    public int Revision { get; private set; }
    public int Sequence { get; private set; }
    public float Impact => ImpactEnvelope(_time - _impactStart);
    public static float ImpactEnvelope(double age)
    {
        if (age < 0 || age >= 3.8) return 0;
        if (age < .3) return (float)(age / .3);
        if (age < 1.2) return 1;
        return (float)((3.8 - age) / 2.6);
    }

    public void Reset()
    {
        _completed = _routine = 0; _started = false;
        _time = _nextCue = _nextImpact = 0; _impactStart = -100;
        Array.Clear(_perfect); Array.Clear(_high);
        _activeRoutine = _lastStatsSecond = -1;
        Cue = default; Revision++; Sequence++;
    }

    public void Update(double delta, LevelOneSession session, float width)
    {
        if (session.Completed < _completed || (_started && !session.HasStarted)) Reset();
        _started = session.HasStarted;
        _time += delta;
        if (session.Completed > _completed)
        {
            _completed = session.Completed;
            var teams = new[] {session, session.Opponent!};
            for (var side = 0; side < 2; side++)
            {
                var count = teams[side].LastResult?.Survivors ?? -1;
                var name = L10n.Text(side == 0 ? "ui.us" : "ui.opponent");
                if ((count == LevelOneSession.Population && !_perfect[side]) || count == 0)
                {
                    _perfect[side] |= count == LevelOneSession.Population;
                    TriggerImpact(L10n.Text(count == 0 ? "marquee.wipeout" : "marquee.perfect", name));
                    return;
                }
            }
            for (var side = 0; side < 2; side++)
                if (!_high[side] && teams[side].LastResult?.SurvivalRate >= .95f)
                {
                    _high[side] = true;
                    if (_time >= _nextCue)
                        Show(new(L10n.Text("marquee.high", L10n.Text(side == 0 ? "ui.us" : "ui.opponent")), MarqueePalette.Orange), 6);
                }
        }
        if (_time < _nextCue)
        {
            if (_activeRoutine is >= 0 and <= 3 && (int)_time != _lastStatsSecond)
            {
                _lastStatsSecond = (int)_time;
                var updated = BuildRoutine(session, _activeRoutine);
                if (updated != Cue) { Cue = updated; Revision++; }
            }
            return;
        }
        var routine = _routine++ % 6;
        var cue = BuildRoutine(session, routine);
        Show(cue, Math.Max(7, (cue.Text.Length * 8 * TickerStrip.DotPitch - width) / 28 + 7), routine);
    }

    private static MarqueeCue BuildRoutine(LevelOneSession session, int routine)
    {
        var own = session.LastResult;
        var other = session.Opponent!.LastResult;
        var a = own is null ? "—" : (own.SurvivalRate * 100).ToString("0.0", L10n.Culture) + "%";
        var b = other is null ? "—" : (other.SurvivalRate * 100).ToString("0.0", L10n.Culture) + "%";
        string text;
        var palette = MarqueePalette.OrangeBlue;
        switch (routine)
        {
            case 0: text = L10n.Text("marquee.results", session.Completed, b, a); break;
            case 1:
                text = L10n.Text("marquee.clock", session.Period, session.Cycle.GenerationElapsedSeconds,
                    LevelOneSession.PeriodSeconds, L10n.Text(session.Paused ? "ui.pause" : "marquee.live"));
                palette = MarqueePalette.Blue; break;
            case 2:
                var left = MotionSummary.Measure(session.Opponent!); var right = MotionSummary.Measure(session);
                text = L10n.Text("marquee.energy", left.EnergyPercent, right.EnergyPercent);
                palette = MarqueePalette.Orange; break;
            case 3:
                text = own is null || other is null ? L10n.Text("marquee.waiting")
                    : L10n.Text("marquee.survivors", other.Survivors, own.Survivors, LevelOneSession.Population);
                break;
            case 4:
                // A short readable excerpt; full dialogue remains beside the commentators.
                text = session.Speech.Split(new[]{'!', '?'}, 2)[0].Trim().TrimEnd('.') + "!";
                palette = MarqueePalette.Blue; break;
            default: text = L10n.Text("billboard.audience"); palette = MarqueePalette.Orange; break;
        }
        return new(text, palette);
    }

    public void TriggerImpact(string text)
    {
        // Separate the readable alert from its rare full-screen performance.
        if (_time >= _nextImpact) { _impactStart = _time; _nextImpact = _time + 20; }
        Show(new(text, MarqueePalette.Red, true), 4.2);
    }

    private void Show(MarqueeCue cue, double seconds, int routine = -1)
    {
        Cue = cue; Revision++; Sequence++; _nextCue = _time + seconds;
        _activeRoutine = routine; _lastStatsSecond = (int)_time;
    }
}
