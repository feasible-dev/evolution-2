namespace EvolutionTwo.Presentation;

public enum OpponentReaction { None, Win, Lose }
public readonly record struct OpponentReactionPose(OpponentReaction Kind, int Frame);
public readonly record struct ReactionBeat(int Frame, double Seconds);

/// <summary>One real-time reaction to the first completed 100% survival result.</summary>
public sealed class OpponentReactionDirector
{
    public static readonly ReactionBeat[] WinBeats =
    [new(0,.15), new(1,.35), new(2,.25), new(3,.4), new(4,.25),
     new(3,.25), new(4,.3), new(5,1.5), new(6,.55), new(7,.4)];
    public static readonly ReactionBeat[] LoseBeats =
    [new(0,.15), new(1,.4), new(2,.65), new(3,.6), new(4,.8),
     new(5,.65), new(6,.5), new(7,.4)];

    private double _startedAt;
    public bool Resolved { get; private set; }
    public OpponentReaction Outcome { get; private set; }
    public OpponentReactionPose Pose { get; private set; }

    public void Reset()
    {
        Resolved = false;
        Outcome = OpponentReaction.None;
        Pose = default;
        _startedAt = 0;
    }

    public void Update(double realTime, bool hasStarted, PeriodOutcome? player, PeriodOutcome? opponent)
    {
        if (!hasStarted) { Reset(); return; }
        if (!Resolved && player is not null && opponent is not null && player.Period == opponent.Period)
        {
            var playerPerfect = player.Survivors == LevelOneSession.Population;
            var opponentPerfect = opponent.Survivors == LevelOneSession.Population;
            if (playerPerfect || opponentPerfect)
            {
                Resolved = true;
                // A simultaneous perfect result is a tie, not a fabricated loss.
                Outcome = playerPerfect == opponentPerfect ? OpponentReaction.None
                    : opponentPerfect ? OpponentReaction.Win : OpponentReaction.Lose;
                _startedAt = realTime;
            }
        }
        Pose = Sample(Outcome, realTime - _startedAt);
    }

    public static OpponentReactionPose Sample(OpponentReaction reaction, double age)
    {
        if (reaction == OpponentReaction.None || age < 0) return default;
        var beats = reaction == OpponentReaction.Win ? WinBeats : LoseBeats;
        foreach (var beat in beats)
        {
            if (age < beat.Seconds) return new(reaction, beat.Frame);
            age -= beat.Seconds;
        }
        return default;
    }
}
