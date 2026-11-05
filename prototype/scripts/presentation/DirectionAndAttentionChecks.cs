using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvolutionTwo.Presentation;

public static class DirectionAndAttentionChecks
{
    public static void Run()
    {
        var checks=0;
        void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);checks++;}
        var plan=new BudgetPlan(0,0,10);
        var pool=BehaviorCatalog.Pool(plan);
        Check(pool.AllowDirectionAction && !pool.AllowSteerToTargetAction && !pool.AllowDistanceToPointCondition
            && !pool.AllowHalfSpaceCondition,"direction deck unlocks no target or region perception");
        var random=new Random(37);
        var rules=Enumerable.Range(0,200).Select(_=>GeneRuleFactory.CreateRandomRule(random,pool,36,0,BehaviorCatalog.Profile(plan))).ToArray();
        Check(rules.All(r=>r.ActionKind==GeneActionKind.AccelerateDirection && r.ConditionKind==GeneConditionKind.Always),"direction-only deck generates only unconditional direction actions");
        Check(rules.Select(r=>(int)(r.DirectionAngleNormalized*8)).Distinct().Count()==8,"initial angles explore the full circle, not the safe zone");
        var session=new LevelOneSession();
        var template=session.Simulation.WorldState.Agents[0].Genome;
        foreach(var angle in new[]{0f,.25f,.5f,.75f,.99f})
        {
            var rule=rules[0] with {DirectionAngleNormalized=angle,Scalar=1};
            Agent Make(Vector2 position,float heading,Vector2 target)=>new(){Genome=template with {Program=new GeneProgram(rule with {TargetPointNormalized=target},GeneRule.Disabled,GeneRule.Disabled)},Position=position,Heading=heading,PhysicsProfile=PhysicsProfile.Default};
            var settings=session.Simulation.Settings;
            var a=AgentBrain.DecideRuleIntent(Make(new(25,25),0,Vector2.Zero),default,settings);
            var b=AgentBrain.DecideRuleIntent(Make(new(1200,400),2,Vector2.One),default,settings);
            Check(a.WorldAccelerationWish.DistanceTo(b.WorldAccelerationWish)<.001f,"absolute direction ignores position, heading and target");
            Check(a.WorldAccelerationWish.DistanceTo(Vector2.FromAngle(angle*Mathf.Tau)*44)<.001f,"both teams use the shared fixed world push");
        }
        Check(MathF.Abs(GeneRuleFactory.MutateDirection(.99f,1,.03f)-.02f)<.0001f,"angle mutation wraps across the right-facing seam");
        var source=rules[0] with {DirectionAngleNormalized=.99f};
        var policy=MutationPolicy.Default with {RemoveRuleChance=0,AddRuleChance=0,ConditionKindMutationChance=0,ActionKindMutationChance=0,ActionParameterMutationChance=1};
        var profile=BehaviorCatalog.Profile(plan);
        profile.ActionMutationRates[GeneActionKind.AccelerateDirection]=1;
        var mutants=Enumerable.Range(0,80).Select(_=>GeneRuleFactory.MutateRule(random,source,pool,policy,36,.03f,profile)).ToArray();
        Check(mutants.All(r=>r.DirectionAngleNormalized>=0 && r.DirectionAngleNormalized<1) && mutants.Any(r=>r.DirectionAngleNormalized!=source.DirectionAngleNormalized),"direction actually mutates and remains on the circle");
        var noMutation=policy with {ActionParameterMutationChance=0};
        profile.ActionMutationRates[GeneActionKind.AccelerateDirection]=0;
        var inherited=GeneRuleFactory.MutateRule(random,source,pool,noMutation,36,.03f,profile);
        Check(inherited.DirectionAngleNormalized==source.DirectionAngleNormalized,"direction is inherited unchanged without parameter mutation");

        var cluster=Enumerable.Repeat(new Vector2(.96f,.5f),91).Concat(Enumerable.Repeat(new Vector2(.1f,.1f),9)).ToArray();
        var measured=ArenaAttention.MeasureCluster(cluster);
        Check(measured.Share>=.9f && measured.Point.X>.9f,"90 percent concentration focuses on the real cluster");
        var scattered=Enumerable.Range(0,100).Select(i=>new Vector2((i%10)/9f,(i/10)/9f)).ToArray();
        Check(ArenaAttention.MeasureCluster(scattered).Share<.4f,"scattered population is not reported as concentrated");
        var attention=new ArenaAttention();
        foreach(var team in new[]{session,session.Opponent!})foreach(var a in team.Simulation.WorldState.Agents)a.Position=new Vector2(session.ArenaSize.X*.96f,session.ArenaSize.Y*.5f);
        var agrees=0;
        for(var i=0;i<200;i++){attention.Update(i*.1,.1,session);if(attention.Point(0).X>.8f&&attention.Point(1).X>.8f)agrees++;}
        Check(agrees>170,"both presenters mostly attend to a dominant cluster");
        Check(attention.Point(0).X<=1&&attention.Point(1).X<=1,"tracking stays inside normalized rink coordinates");
        foreach(var team in new[]{session,session.Opponent!})foreach(var a in team.Simulation.WorldState.Agents)a.Position=new Vector2(session.ArenaSize.X*.05f,session.ArenaSize.Y*.4f);
        for(var i=200;i<240;i++)attention.Update(i*.1,.1,session);
        Check(attention.Point(0).X<.2f&&attention.Point(1).X<.2f,"attention follows a moving cluster, not a scripted rightward look");
        foreach(var survivors in new[]{216,228,0})
        {
            var match=new LevelOneSession();match.SetAllocation(BehaviorChoice.Direction,10);match.Start(new BudgetPlan(0,0,10));match.SetSpeed(1);
            match.Tick(LevelOneSession.PeriodSeconds/LevelOneSession.BaseTimeScale-.01f);
            for(var side=0;side<2;side++)
            {
                var team=side==0?match:match.Opponent!;
                for(var i=0;i<team.Simulation.WorldState.Agents.Count;i++)
                {
                    var a=team.Simulation.WorldState.Agents[i];
                    a.Position=i<(side==0?survivors:120)?new Vector2(1240+i/40*5,180+i%40*4):new Vector2(300+i/40*5,180+i%40*4);
                    a.Velocity=Vector2.Zero;a.MaxDistanceFromBirth=10;
                }
            }
            match.Tick(.02f);
            Check(match.LastResult?.Survivors==survivors,"controlled highlight fixture reaches the intended survival count");
            Check(survivors==216?match.GestureSequence==0:match.GestureSequence==1&&match.Gesture==(survivors==0?"slap":"stand"),"only exceptional outcomes emit gesture cues");
            if(survivors!=216)Check(match.SpeechAge<.1f&&match.IsSpeaking,"gesture cue and its commentary begin together");
            var marquee=new MarqueeProgram();marquee.Update(.4,match,1008);
            Check(survivors==0?marquee.Cue.Palette==MarqueePalette.Red:marquee.Cue.Palette!=MarqueePalette.Red,
                "a measured wipeout selects the red routine while ordinary/high results do not");
            if(survivors==228)
            {
                match.TogglePause();
                match.Tick(6.8f);
                Check(match.Gesture=="stand", "standing dialogue waits for the physical gesture to settle");
                match.Tick(.5f);
                Check(match.Gesture=="excited" && match.Speaker==0 && match.IsSpeaking,
                    "milestone queues enthusiastic spoken aftermath while paused");
                match.Tick(8.1f);
                Check(match.Speaker==1 && match.IsSpeaking, "analyst answers after Gerard's seated excitement");
            }
        }
        GD.Print($"DIRECTION / ATTENTION PASS: {checks} checks");
    }
}
