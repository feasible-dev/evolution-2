using Godot;

namespace EvolutionTwo.Presentation;

public static class MarqueeChecks
{
    public static void Run(BroadcastScene scene)
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Marquee: " + message);
            checks++;
        }
        Check(scene.Billboard.GetChildren().OfType<TickerStrip>().Count()==1, "one uninterrupted message strip");
        Check(!scene.Billboard.GetChildren().OfType<Label>().Any(), "central score and caption removed");
        var program = new MarqueeProgram();
        var session = new LevelOneSession();
        var lines = new HashSet<string>();
        for(var i=0;i<6;i++)
        {
            program.Update(i==0?0:100,session,1008);
            lines.Add(program.Cue.Text);
            Check(program.Cue.Palette!=MarqueePalette.Red && program.Impact==0, "ordinary routines never flood red");
        }
        Check(lines.Count==6, "six distinct broadcast routines");
        var live = new MarqueeProgram();
        session.SetAllocation(BehaviorChoice.Direction,10);
        Check(session.Start(new BudgetPlan(0,0,10)), "live-clock fixture starts with a complete budget");
        live.Update(0,session,1008);live.Update(10,session,1008);
        var sequence = live.Sequence;var clockLine = live.Cue.Text;
        session.Tick(.2f);live.Update(1,session,1008);
        Check(live.Cue.Text!=clockLine && live.Sequence==sequence,
            "current clock updates without restarting the scrolling cue");
        program.TriggerImpact("100% !");
        Check(program.Cue.Centered && program.Cue.Palette==MarqueePalette.Red, "strong result uses a centered red headline");
        program.Update(.3,session,1008);
        Check(program.Impact>.99f, "red attack reaches full strength");
        program.TriggerImpact("SECOND RESULT");
        program.Update(3.6,session,1008);
        Check(program.Impact==0, "a second alert does not restart the red screen inside cooldown");
        program.Update(1,session,1008);
        Check(program.Cue.Palette!=MarqueePalette.Red, "normal program resumes after the headline");
        program.Reset();
        Check(program.Impact==0, "restart clears all red effects");
        Check(MarqueeProgram.ImpactEnvelope(-1)==0 && MarqueeProgram.ImpactEnvelope(4)==0,
            "impact is bounded and ends completely");
        Check(new MarqueeCue("BOLD",MarqueePalette.Blue).Bold,"bold is the default type weight");
        var motions = new HashSet<MarqueeMotion>();
        program.Reset();
        for(var i=0;i<6;i++) { program.Update(i==0?0:100,session,1600); motions.Add(program.Cue.Motion); }
        Check(motions.Count==4,"routine selection includes scrolling, still, three-flash and burst performances");
        program.Reset(); program.Update(0,session,100);
        Check(program.Cue.Motion==MarqueeMotion.Scroll,"long messages scroll instead of being clipped while centered");
        var stopped=MarqueePerformance.Sample(MarqueeMotion.StopFlash,2.5,160,400);
        Check(stopped.ScrollColumns==-120 && !stopped.Repeat,"short message stops at the center without duplicates");
        var pulses=0; var wasLit=false; var remainsStill=true;
        for(var tick=0;tick<180;tick++)
        {
            var pose=MarqueePerformance.Sample(MarqueeMotion.StopFlash,MarqueePerformance.FlashAt+tick*.01,160,400);
            var lit=pose.TextVisibility>0;
            if(lit&&!wasLit)pulses++;
            wasLit=lit;
            remainsStill &= pose.ScrollColumns==stopped.ScrollColumns;
        }
        Check(remainsStill,"message stays still throughout the flash sequence");
        Check(pulses==3,"exactly three visible flash pulses");
        var resumed=MarqueePerformance.Sample(MarqueeMotion.StopFlash,6.7,160,400);
        Check(resumed.ScrollColumns>stopped.ScrollColumns && resumed.TextVisibility==1,"scrolling resumes to the left after the flashes");
        Check(MarqueePerformance.Sample(MarqueeMotion.Hold,1,160,400)==MarqueePerformance.Sample(MarqueeMotion.Hold,20,160,400),
            "stationary routines never drift");
        var burst=MarqueePerformance.Sample(MarqueeMotion.Burst,.3,160,400);
        Check(burst.BurstProgress>0 && burst.TextVisibility==0,"burst intro is a non-letter motif");
        var afterBurst=MarqueePerformance.Sample(MarqueeMotion.Burst,.7,160,400);
        Check(afterBurst.BurstProgress<0 && afterBurst.TextVisibility==1 && afterBurst.Centered,"burst gives way to a readable still message");
        GD.Print($"MARQUEE PASS: {checks} checks");
    }
}
