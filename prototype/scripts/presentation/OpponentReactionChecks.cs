using Godot;

namespace EvolutionTwo.Presentation;

public static class OpponentReactionChecks
{
    public static void Run(CompetitorIdentity identity)
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Opponent reaction: " + message);
            checks++;
        }
        static PeriodOutcome Result(int generation, int survivors) => new(generation, survivors,
            new Dictionary<BehaviorChoice, RuleOutcome>(), new(0,0), BudgetPlan.Default);
        var director = new OpponentReactionDirector();
        director.Update(0,true,Result(1,180),Result(1,200));
        Check(!director.Resolved && director.Pose.Kind == OpponentReaction.None,
            "an ordinary lead is not a match win");
        director.Update(1,true,Result(2,240),Result(1,200));
        Check(!director.Resolved,"wait for both results from the same generation");
        director.Update(2,true,Result(2,240),Result(2,240));
        Check(director.Resolved && director.Outcome == OpponentReaction.None,"simultaneous perfect survival is a tie");
        director.Update(3,true,Result(3,200),Result(3,240));
        Check(director.Outcome == OpponentReaction.None,"resuming after a tie does not invent a winner later");
        director.Update(0,false,null,null);
        Check(!director.Resolved && director.Pose == default,"preparation clears a previous outcome");

        foreach (var expected in new[] { OpponentReaction.Win, OpponentReaction.Lose, OpponentReaction.None })
        {
            var match = new LevelOneSession();
            match.SetAllocation(BehaviorChoice.Direction,10);
            Check(match.Start(new BudgetPlan(0,0,10)),"controlled match starts with complete budgets");
            match.SetSpeed(1);
            match.Tick(LevelOneSession.PeriodSeconds / LevelOneSession.BaseTimeScale - .01f);
            var counts = expected == OpponentReaction.Win ? new[] { 200,240 }
                : expected == OpponentReaction.Lose ? new[] { 240,200 } : new[] { 240,240 };
            for (var side=0;side<2;side++)
            {
                var team = side == 0 ? match : match.Opponent!;
                for (var i=0;i<team.Simulation.WorldState.Agents.Count;i++)
                {
                    var agent = team.Simulation.WorldState.Agents[i];
                    agent.Position = new Vector2((i<counts[side]?1240:300)+i/40*5,180+i%40*4);
                    agent.Velocity = Vector2.Zero; agent.MaxDistanceFromBirth = 10;
                }
            }
            match.Tick(.02f);
            Check(match.LastResult?.Survivors==counts[0] && match.Opponent!.LastResult?.Survivors==counts[1],
                "actual selection records the controlled result for both teams");
            Check(match.Paused,"first perfect result pauses the live simulation");
            director.Reset();
            director.Update(10,match.HasStarted,match.LastResult,match.Opponent!.LastResult);
            Check(director.Outcome==expected,"measured outcome selects the correct Nico reaction");
            director.Update(11,match.HasStarted,match.LastResult,match.Opponent.LastResult);
            Check(expected==OpponentReaction.None ? director.Pose.Kind==OpponentReaction.None : director.Pose.Frame>0,
                "animation advances in real time even while simulation is paused");
            director.Update(20,match.HasStarted,match.LastResult,match.Opponent.LastResult);
            Check(director.Pose==default,"reaction settles back to neutral instead of looping");
            director.Update(30,true,Result(2,counts[1]),Result(2,counts[0]));
            Check(director.Outcome==expected && director.Pose==default,"later generations cannot restart or reverse the reaction");
        }

        identity.ShowReaction(default);
        using var neutral=StudioSpriteCompositor.CopyImage(identity.DisplayedPortrait!);
        foreach(var reaction in new[] { OpponentReaction.Win, OpponentReaction.Lose })
        {
            for(var frame=0;frame<8;frame++)
            {
                identity.ShowReaction(new(reaction,frame));
                using var pixels=StudioSpriteCompositor.CopyImage(identity.DisplayedPortrait!);
                Check(pixels.GetSize()==new Vector2I(128,192),"every reaction preserves native pixel dimensions");
                var same=pixels.GetData().SequenceEqual(neutral.GetData());
                Check(frame is 0 or 7 ? same : !same,"neutral endpoints are exact; gestures have actual drawn motion");
            }
        }
        identity.ShowReaction(default);
        GD.Print($"OPPONENT REACTION PASS: {checks} outcome, timing and asset checks");
    }
}
