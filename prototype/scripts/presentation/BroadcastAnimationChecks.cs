using Godot;

namespace EvolutionTwo.Presentation;

public static class BroadcastAnimationChecks
{
    public static void Run(BroadcastScene scene)
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Broadcast: " + message);
            checks++;
        }
        Check(scene.Presenters.All(p => p.UsesAnimation), "both actors use the new runtime frames");
        for (var speaker = 0; speaker < 2; speaker++)
        {
            var solitary = BroadcastAnimationDirector.ConversationLooks(0, speaker, 1);
            Check(speaker == 0 ? solitary == (true, false) : solitary == (false, true), "solitary glance works in both directions");
            var early = BroadcastAnimationDirector.ConversationLooks(1, speaker, 1);
            var together = BroadcastAnimationDirector.ConversationLooks(1, speaker, 1.6);
            var after = BroadcastAnimationDirector.ConversationLooks(1, speaker, 2.4);
            Check(early == solitary && together == (true, true), "listener responds after the initiating glance");
            Check(speaker == 0 ? after == (false, true) : after == (true, false), "initiator returns to rink before partner");
            var listenerOnly = BroadcastAnimationDirector.ConversationLooks(2, speaker, 1);
            Check(speaker == 0 ? listenerOnly == (false, true) : listenerOnly == (true, false), "listener can glance without interrupting analyst");
            Check(BroadcastAnimationDirector.ConversationLooks(1, speaker, 3) == (false, false), "exchange ends at rink");
        }
        using var assets = new BroadcastAnimationAssets();
        var director = new BroadcastAnimationDirector(assets.EventDurations);
        void Sample(double time, int completed=0, int cue=0, string? gesture=null)
            => director.Update(time,1,0,time,false,completed,cue,gesture);
        Sample(0);
        Sample(1,1);
        Check(director.CurrentEvent is null, "ordinary selection does not trigger a gesture");
        Sample(2,2);
        Check(director.CurrentEvent is null, "ordinary matches can remain seated");
        Sample(19,10,1,"stand");
        Check(director.CurrentEvent == "stand", "exceptional spoken cue triggers standing");
        Sample(20.36,10,1,"stand");
        Check(director.Frames[0].Frame == 3, "Gerard stands before partner notices");
        Sample(20.5,10,1,"stand");
        Check(director.Frames[1].Frame == 4, "partner notices after the 490 ms delay");
        Sample(24,10,2,"slap");
        Check(director.CurrentEvent == "stand", "reaction continues while match state is paused");
        Sample(26,10,2,"slap");
        Check(director.CurrentEvent is null, "standing settles back to conversation");
        director.Reset(); Sample(0);
        Check(director.CurrentEvent is null && director.Frames.All(f=>f.Clip=="conversation"), "restart clears pending gestures");
        Sample(.1,1,1,"stand");
        Check(director.CurrentEvent == "stand", "perfect-result spoken cue triggers celebration");
        Sample(30,2,1,"stand");
        Check(director.CurrentEvent is null,"an old cue never replays after the cooldown");
        director.Reset();
        director.Update(0,1,0,.2,true,1,1,"excited");
        Check(director.IsExcited && director.Frames[0].Clip=="excited", "spoken aftermath starts a seated performance");
        director.Update(1.4,1,0,1.4,true,1,1,"excited");
        Check(director.Frames[0].Frame/2==1, "excited torso shifts toward the left rink");
        director.Update(3.1,1,0,3.1,true,1,1,"excited");
        Check(director.Frames[0].Gaze==PortraitGaze.Partner && director.Frames[1].Gaze==PortraitGaze.Board, "Gerard invites his colleague before the response");
        director.Update(3.8,1,0,3.8,true,1,1,"excited");
        Check(director.Frames.All(f=>f.Gaze==PortraitGaze.Partner), "Rene answers the excited glance after a delay");
        director.Update(8.1,1,0,8.1,false,1,1,"excited");
        Check(!director.IsExcited && director.Frames[0].Clip!="excited", "aftermath settles without looping forever");
        director.Reset();
        Check(!director.IsExcited, "restart clears the seated aftermath");
        Check(TickerStrip.ScrollColumn(1)==11 && TickerStrip.ScrollColumn(1.01)==11 && TickerStrip.ScrollColumn(2)==22,
            "LED messages step through fixed columns at the preserved reading speed");
        DirectionAndAttentionChecks.Run();
        MarqueeChecks.Run(scene);

        // Verify actual pixel resources, not only scheduling labels.
        for (var actor=0;actor<2;actor++)
        {
            using var board = assets.Get(actor,new("conversation",0,PortraitGaze.Board)).GetImage();
            using var speech = assets.Get(actor,new("conversation",1,PortraitGaze.Board)).GetImage();
            using var partner = assets.Get(actor,new("conversation",5,PortraitGaze.Partner)).GetImage();
            using var partnerSpeech = assets.Get(actor,new("conversation",6,PortraitGaze.Partner)).GetImage();
            Check(board.GetWidth()==192 && board.GetHeight()==128, "native portrait size is preserved");
            Check(!board.GetData().SequenceEqual(speech.GetData()), "each commentator has visible board-facing speech");
            Check(!partner.GetData().SequenceEqual(partnerSpeech.GetData()), "each commentator can speak toward his colleague");
            using var body = board.GetRegion(new Rect2I(0,100,192,28));
            using var speakingBody = speech.GetRegion(new Rect2I(0,100,192,28));
            Check(body.GetData().SequenceEqual(speakingBody.GetData()), "syllables do not jitter the seated body");
            foreach (var clip in new[]{"stand","slap"})
            {
                using var entry=assets.Get(actor,new(clip,0,PortraitGaze.Board)).GetImage();
                using var exit=assets.Get(actor,new(clip,assets.EventDurations[clip].Length-1,PortraitGaze.Board)).GetImage();
                Check(board.GetData().SequenceEqual(entry.GetData()), "event entry matches the seated master exactly");
                Check(board.GetData().SequenceEqual(exit.GetData()), "event exit matches the seated master exactly");
            }
            using var rinkNeutral=assets.Get(actor,new("rink",4,PortraitGaze.Board)).GetImage();
            Check(board.GetData().SequenceEqual(rinkNeutral.GetData()), "rink and conversation share the same neutral portrait");
            for (var gaze=0;gaze<8;gaze++)
            {
                using var eyesOpen=assets.Get(actor,new("rink",gaze,PortraitGaze.Board)).GetImage();
                using var eyesClosed=assets.Get(actor,new("rink",gaze+8,PortraitGaze.Board)).GetImage();
                Check(!eyesOpen.GetData().SequenceEqual(eyesClosed.GetData()), "each rink gaze has a visible blink");
                using var stillBody=eyesOpen.GetRegion(new Rect2I(0,80,192,48));
                using var blinkBody=eyesClosed.GetRegion(new Rect2I(0,80,192,48));
                Check(stillBody.GetData().SequenceEqual(blinkBody.GetData()), "rink blinks preserve the seated body");
            }
        }
        // Speech must not swap a whole generated jaw or resize the face.
        var speechPairs = new (string Clip, int Closed, int Speaking)[]
        {
            ("conversation",0,1), ("conversation",0,2), ("conversation",5,6),
            ("conversation",5,7), ("conversation",0,9),
            ("rink",0,1), ("rink",2,3), ("rink",4,5),
            ("rink",8,9), ("rink",10,11), ("rink",12,13),
            ("excited",0,1), ("excited",2,3), ("excited",4,5), ("excited",6,7)
        };
        foreach (var pair in speechPairs)
        {
            using var closed = assets.Get(0,new(pair.Clip,pair.Closed,PortraitGaze.Board)).GetImage();
            using var speaking = assets.Get(0,new(pair.Clip,pair.Speaking,PortraitGaze.Board)).GetImage();
            var minX=192; var minY=128; var maxX=-1; var maxY=-1;
            for (var y=0;y<128;y++) for (var x=0;x<192;x++)
            {
                if (closed.GetPixel(x,y)==speaking.GetPixel(x,y)) continue;
                minX=Math.Min(minX,x); maxX=Math.Max(maxX,x);
                minY=Math.Min(minY,y); maxY=Math.Max(maxY,y);
            }
            Check(maxX>=minX && maxY-minY<6 && maxX-minX<20,
                $"{pair.Clip}/{pair.Speaking}: visible syllables stay confined to the lips");
            using var jaw = closed.GetRegion(new Rect2I(0,70,192,58));
            using var talkingJaw = speaking.GetRegion(new Rect2I(0,70,192,58));
            Check(jaw.GetData().SequenceEqual(talkingJaw.GetData()),
                $"{pair.Clip}/{pair.Speaking}: speech keeps chin, neck and torso stable");
        }
        using var before = assets.Get(1,new("stand",0,PortraitGaze.Board)).GetImage();
        using var held = assets.Get(1,new("stand",3,PortraitGaze.Board)).GetImage();
        Check(before.GetData().SequenceEqual(held.GetData()), "partner pixels stay unchanged throughout initial rise");
        GD.Print($"BROADCAST PASS: {checks} scheduling and pixel checks");
    }

    public static async Task Review(BroadcastScene scene)
    {
        try
        {
            Run(scene);
            if (DisplayServer.GetName() == "headless") { scene.GetTree().Quit(); return; }
            var folder=ProjectSettings.GlobalizePath("res://../art-direction/v41/captures");
            Directory.CreateDirectory(folder);
            await scene.ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var cards=scene.GetViewport().GetTexture().GetImage())cards.SavePng(Path.Combine(folder,"00-direction-cards.png"));
            scene.Session.Tick(4); scene.Session.Tick(1.2f); scene.SyncUi();
            scene.Session.SetAllocation(BehaviorChoice.Throttle,3);
            scene.Session.SetAllocation(BehaviorChoice.Direction,7);
            scene.Session.ApplyPlan(); scene.Session.TogglePause(); scene.Session.Tick(2.8f); scene.SyncUi();
            using var assets=new BroadcastAnimationAssets();
            var samples=new (string Name,BroadcastPortraitFrame G,BroadcastPortraitFrame R)[]
            {
                ("01-level",new("conversation",0,PortraitGaze.Board),new("conversation",0,PortraitGaze.Board)),
                ("02-gerard-glance",new("conversation",6,PortraitGaze.Partner),new("conversation",0,PortraitGaze.Board)),
                ("03-rene-analysis",new("conversation",0,PortraitGaze.Board),new("conversation",6,PortraitGaze.Partner)),
                ("04-reply",new("conversation",5,PortraitGaze.Partner),new("conversation",6,PortraitGaze.Partner)),
                ("05-standing-unnoticed",new("stand",3,PortraitGaze.Board),new("stand",3,PortraitGaze.Board)),
                ("06-standing-reaction",new("stand",8,PortraitGaze.Board),new("stand",8,PortraitGaze.Partner)),
                ("07-table-impact",new("slap",4,PortraitGaze.Board),new("slap",4,PortraitGaze.Board)),
                ("08-rink-left",new("rink",0,PortraitGaze.Board),new("rink",0,PortraitGaze.Board)),
                ("09-rink-right",new("rink",2,PortraitGaze.Board),new("rink",3,PortraitGaze.Board)),
                ("10-independent",new("rink",6,PortraitGaze.Board),new("rink",2,PortraitGaze.Board)),
                ("11-relaxed",new("excited",0,PortraitGaze.Board),new("conversation",0,PortraitGaze.Board)),
                ("12-excited-left",new("excited",3,PortraitGaze.Board),new("conversation",0,PortraitGaze.Board)),
                ("13-excited-partner",new("excited",7,PortraitGaze.Partner),new("conversation",5,PortraitGaze.Partner))
            };
            foreach(var sample in samples)
            {
                scene.Presenters[0].ShowAnimation(sample.G); scene.Presenters[1].ShowAnimation(sample.R);
                await scene.ToSignal(scene.GetTree(),SceneTree.SignalName.ProcessFrame);
                await scene.ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
                using var screen=scene.GetViewport().GetTexture().GetImage();
                var file=Path.Combine(folder,sample.Name+".png");
                if(screen.SavePng(file)!=Error.Ok) throw new IOException("Could not save "+file);
                GD.Print("BROADCAST_CAPTURE: "+file);
            }
            scene.Billboard.Program.TriggerImpact(L10n.Text("marquee.perfect",L10n.Text("ui.us")));
            await scene.ToSignal(scene.GetTree().CreateTimer(.5),SceneTreeTimer.SignalName.Timeout);
            await scene.ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var red=scene.GetViewport().GetTexture().GetImage())red.SavePng(Path.Combine(folder,"14-red-impact.png"));
            await scene.ToSignal(scene.GetTree().CreateTimer(4),SceneTreeTimer.SignalName.Timeout);
            await scene.ToSignal(RenderingServer.Singleton,RenderingServer.SignalName.FramePostDraw);
            using(var settled=scene.GetViewport().GetTexture().GetImage())settled.SavePng(Path.Combine(folder,"15-settled.png"));
            scene.GetTree().Quit();
        }
        catch(Exception ex) { GD.PushError(ex.ToString()); scene.GetTree().Quit(1); }
    }
}
