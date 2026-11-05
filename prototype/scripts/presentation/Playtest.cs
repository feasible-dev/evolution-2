using System.Diagnostics;
using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;

namespace EvolutionTwo.Presentation;

public static class Playtest
{
    private static int _checks;
    public static async Task ReviewPortraits(BroadcastScene scene)
    {
        try
        {
            var portraits = scene.Presenters;
            var names = new[] { "gerard", "rene" };
            var bodies = new uint[2][];
            var closedFaces = new uint[2][];
            var colors = new HashSet<uint>();
            foreach (var gaze in Enum.GetValues<PortraitGaze>())
                foreach (var talking in new[] { false, true })
                {
                    foreach (var portrait in portraits)
                    {
                        portrait.SetGaze(gaze); portrait.SetFrame(0, talking, true);
                    }
                    await scene.ToSignal(scene.GetTree().CreateTimer(.35), SceneTreeTimer.SignalName.Timeout);
                    await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    for (var i = 0; i < portraits.Count; i++)
                    {
                        using var rendered = portraits[i].Texture.GetImage();
                        var body = new List<uint>(); var face = new List<uint>();
                        for (var y = 0; y < rendered.GetHeight(); y++)
                            for (var x = 0; x < rendered.GetWidth(); x++)
                            {
                                var pixel = rendered.GetPixel(x, y).ToRgba32();
                                colors.Add(pixel);
                                if (y >= 108) body.Add(pixel);
                                else face.Add(pixel);
                            }
                        if (gaze == PortraitGaze.Board && !talking) bodies[i] = body.ToArray();
                        Check(body.SequenceEqual(bodies[i]), "body pixels remain fixed through every gaze and mouth change");
                        if (!talking) closedFaces[i] = face.ToArray();
                        else Check(!face.SequenceEqual(closedFaces[i]), "speech visibly animates the face");
                        GD.Print($"PORTRAIT_CAPTURE:{names[i]}-{gaze.ToString().ToLowerInvariant()}-{talking}.png:"
                            + Convert.ToBase64String(rendered.SavePngToBuffer()));
                    }
                }
            Check(colors.Count <= 16, "composed portraits retain the shared sixteen inks");
            GD.Print($"PORTRAITS PASS: {_checks} continuity checks; {colors.Count} shared colors");
            scene.GetTree().Quit(0);
        }
        catch (Exception ex) { GD.PushError(ex.ToString()); scene.GetTree().Quit(1); }
    }
    public static async Task Run(BroadcastScene scene, bool capture)
    {
        try
        {
            if (capture) { await Capture(scene); scene.GetTree().Quit(0); return; }
            var sw = Stopwatch.StartNew();
            if (OS.GetCmdlineUserArgs().Contains("--duel-audit"))
            {
                var duel = new LevelOneSession();
                duel.ResizeArena(new Vector2(1280, 756));
                duel.SetAllocation(BehaviorChoice.Throttle, 3); duel.SetAllocation(BehaviorChoice.Direction, 7);
                duel.Start(new BudgetPlan(2, 2, 6)); duel.SetSpeed(10);
                AdvanceUntil(duel, () => duel.Period == 12, 50);
                duel.SetSpeed(1); duel.Tick(9);
                foreach (var team in new[] { duel, duel.Opponent! })
                {
                    var directions = team.Simulation.WorldState.Agents.Where(a => BehaviorCatalog.For(a)?.Id == BehaviorChoice.Direction).ToArray();
                    Check(directions.All(a => MathF.Abs(AgentVisualPalette.GetDisplayRule(a.Genome.Program).Scalar - 1) < .0001f),
                        "direction strength cannot evolve into a team-specific acceleration bonus");
                    Check(team.Simulation.WorldState.Agents.All(a => a.Genome.DecisionIntervalSeconds == .2f
                        && a.Genome.DecisionIntervalJitterSeconds == 0), "all agents share the same decision cadence after twelve generations");
                    Check(directions.Where(a => a.CurrentIntent.WorldAccelerationWish.LengthSquared() > .000001f)
                        .All(a => MathF.Abs(a.CurrentIntent.WorldAccelerationWish.Length() - 44) < .001f),
                        "actual direction push has identical magnitude despite different cards and inherited directions");
                    GD.Print(FormattableString.Invariant($"DUEL {(ReferenceEquals(team, duel) ? "BLUE" : "RED")}: G{team.Period} speed={directions.Average(a => a.Velocity.Length()):0.00}; direction strength={directions.Average(a => AgentVisualPalette.GetDisplayRule(a.Genome.Program).Scalar):0.000}; world push={directions.Average(a => a.CurrentIntent.WorldAccelerationWish.Length()):0.000}; decision interval={directions.Average(a => a.Genome.DecisionIntervalSeconds):0.000}; energy={directions.Average(a => a.Energy):0.00}"));
                }
                scene.GetTree().Quit(0); return;
            }
            if (OS.GetCmdlineUserArgs().Contains("--motion-audit"))
            {
                var session = new LevelOneSession(); var plan = new BudgetPlan(7, 3, 0);
                session.SetAllocation(BehaviorChoice.Turn, plan.Turn);
                session.SetAllocation(BehaviorChoice.Throttle, plan.Throttle);
                session.SetAllocation(BehaviorChoice.Direction, plan.Direction);
                session.SetSpeed(1); session.Start(plan);
                var rival = session.Opponent!;
                var sample = MotionSummary.Measure(session);
                Check(Math.Abs(sample.Speed - session.Simulation.WorldState.Agents.Average(a => a.Velocity.Length())) < .001
                    && Math.Abs(sample.EnergyPercent - 100) < .001,
                    "displayed speed uses physical velocity and fresh reserves are full");
                var first = session.Simulation.WorldState.Agents[0];
                var savedEnergy = first.Energy; first.Energy = 0;
                Check(MotionSummary.Measure(session).Exhausted == 1, "energy diagnostics count exhaustion");
                first.Energy = savedEnergy;
                Check(session.Active == rival.Active, "audit compares identical cards");
                void Report(string name, LevelOneSession team)
                {
                    var agents = team.Simulation.WorldState.Agents;
                    GD.Print(FormattableString.Invariant($"MOTION {name} t={team.SimulatedSeconds:0.0}: mass={agents.Average(a => a.PhysicsProfile.Mass):0.000}; thrust={agents.Average(a => a.PhysicsProfile.MaxForwardAcceleration):0.000}; thrust/mass={agents.Average(a => a.PhysicsProfile.MaxForwardAcceleration / a.PhysicsProfile.Mass):0.000}; drag={agents.Average(a => a.PhysicsProfile.LinearDrag):0.000}; energy={agents.Average(a => a.Energy):0.000}; speed={agents.Average(a => a.Velocity.Length()):0.000}; exhausted={agents.Count(a => a.Energy <= 0)}; maxSpeed={team.Simulation.Settings.MaxSpeed}"));
                }
                GD.Print($"IDENTICAL CARDS: {plan}");
                Report("US", session); Report("OPPONENT", rival);
                foreach (var seconds in new[] { 3, 6, 9 })
                {
                    session.Tick(seconds / LevelOneSession.BaseTimeScale);
                    Report("US", session); Report("OPPONENT", rival);
                }
                var a = new Simulation(session.Simulation.Settings, seed: 999);
                var b = new Simulation(rival.Simulation.Settings, seed: 999);
                a.InitializeAgents(); b.InitializeAgents();
                Check(a.WorldState.Agents.Zip(b.WorldState.Agents).All(p => p.First.PhysicsProfile == p.Second.PhysicsProfile
                    && p.First.Genome == p.Second.Genome), "same cards and seed produce identical traits for either side");
                for (var step = 0; step < 360; step++) { a.Update(1f / 60); b.Update(1f / 60); }
                Check(a.WorldState.Agents.Zip(b.WorldState.Agents).All(p => p.First.Position == p.Second.Position
                    && p.First.Velocity == p.Second.Velocity && p.First.Energy == p.Second.Energy),
                    "same cards and seed produce identical movement for either side");
                session.SetSpeed(10);
                for (var generation = 1; generation <= 3; generation++)
                {
                    AdvanceUntil(session, () => session.Period > generation, 10);
                    Check(session.Simulation.WorldState.Agents.Zip(rival.Simulation.WorldState.Agents).All(p =>
                        p.First.Position == p.Second.Position && p.First.Velocity == p.Second.Velocity
                        && p.First.Genome == p.Second.Genome && p.First.Energy == p.Second.Energy
                        && p.First.PhysicsProfile == PhysicsProfile.Default && p.Second.PhysicsProfile == PhysicsProfile.Default),
                        "7/3/0 stays identical through selection, mutation and reproduction");
                    Check(session.LastResult!.Survivors == rival.LastResult!.Survivors, "matched decks have matched survival");
                    GD.Print($"MATCHED LIVE TEAMS G{generation}: {session.LastResult.Survivors} = {rival.LastResult.Survivors}; exact DNA, positions, velocities and energy");
                }
                GD.Print($"MOTION AUDIT PASS: {_checks} checks. No side multiplier in matched-seed control.");
                scene.GetTree().Quit(0); return;
            }
            if (OS.GetCmdlineUserArgs().Contains("--collision-test"))
            {
                var simulation = new Simulation(new SimulationSettings { WorldSize = LevelOneSession.WorldSize,
                    BoardShape = BoardShapeKind.RoundedRectangle, AgentCount = 240, AgentRadius = 2 }, seed: 71);
                simulation.InitializeAgents();
                long pairs = 0;
                var timer = Stopwatch.StartNew();
                for (var step = 0; step < 240; step++)
                {
                    for (var i = 0; i < 240; i++)
                        simulation.WorldState.Agents[i].Position = new Vector2(1250 + i % 16 * .5f, 260 + i / 16 * .5f);
                    simulation.Update(1f / 60);
                    pairs += simulation.LastUpdateMetrics.CheckedPairs;
                    Check(simulation.WorldState.Agents.All(a => float.IsFinite(a.Position.X)
                        && BoardGeometry.ContainsPoint(a.Position, LevelOneSession.WorldSize, BoardShapeKind.RoundedRectangle)),
                        "dense contacts keep agents finite and within the arena");
                }
                GD.Print($"COLLISION BENCH: {timer.Elapsed.TotalMilliseconds:0} ms; {pairs} candidate pairs; 240 dense steps");
                CheckGeometryParity();
                foreach (var size in new[] { new Vector2(996, 580), new Vector2(700, 900), new Vector2(1600, 500) })
                {
                    var arena = new Rect2(new Vector2(7, 11), size);
                    var world = LevelOneSession.WorldSize;
                    var r = MathF.Min(world.X, world.Y) * BoardGeometry.RoundedRectangleCornerRadiusFraction;
                    var sr = r * MathF.Min(size.X / world.X, size.Y / world.Y);
                    var displacement = new Vector2(12, 7);
                    foreach (var origin in new[] { new Vector2(5, 5), world / 2, world - new Vector2(30, 30) })
                    {
                        var shown = SimulationBoard.MapToArena(origin + displacement, arena) - SimulationBoard.MapToArena(origin, arena);
                        Check(shown.DistanceTo(displacement * (sr / r)) < .001f,
                            "equal physical motion has equal screen displacement everywhere");
                    }
                    var corners = new[] { new Vector2(world.X - r, r), new Vector2(world.X - r, world.Y - r),
                        new Vector2(r, world.Y - r), new Vector2(r, r) };
                    for (var corner = 0; corner < 4; corner++)
                    for (var sample = 0; sample <= 20; sample++)
                    {
                        var angle = (-1 + corner) * Mathf.Pi / 2 + sample / 20f * Mathf.Pi / 2;
                        var center = SimulationBoard.MapToArena(corners[corner], arena);
                        var edge = SimulationBoard.MapToArena(corners[corner] + Vector2.FromAngle(angle) * r, arena);
                        Check(MathF.Abs(edge.DistanceTo(center) - sr) < .001f,
                            "all four corners have constant screen radius at every window aspect ratio");
                    }
                }
                foreach (var p in BoardGeometry.GetOutline(LevelOneSession.WorldSize, BoardShapeKind.RoundedRectangle))
                {
                    var inset = BoardGeometry.ClampPoint(p, LevelOneSession.WorldSize, BoardShapeKind.RoundedRectangle, 2.5f);
                    Check(inset.X >= 2.5f && inset.Y >= 2.5f && inset.X <= 1277.5f && inset.Y <= 557.5f,
                        "agent radius remains inset along the physical contour");
                }
                var live = new LevelOneSession(); FillBalanced(live); live.Start();
                live.Tick(1, .0001);
                Check(live.SimulatedSeconds < .1 && live.SimulatedSeconds == live.Opponent!.SimulatedSeconds,
                    "frame budget bounds catch-up work without desynchronizing teams");
                GD.Print($"GEOMETRY AND FRAME BUDGET PASS: {_checks} checks");
                foreach (var viewport in new[] { new Vector2(996, 588), new Vector2(850, 700) })
                {
                    var oldVelocity = live.Simulation.WorldState.Agents[0].Velocity;
                    var oldTime = live.SimulatedSeconds;
                    live.ResizeArena(new Vector2(1280, 1280 * viewport.Y / viewport.X));
                    var fitted = SimulationBoard.FitArena(new Rect2(Vector2.Zero, viewport), live.ArenaSize);
                    Check(fitted.Size.DistanceTo(viewport) < .001f, "arena fills both dimensions without blank padding");
                    Check(live.ArenaSize == live.Opponent!.ArenaSize && live.Simulation.WorldState.Size == live.ArenaSize,
                        "both teams and world state use the resized physical arena");
                    Check(oldVelocity == live.Simulation.WorldState.Agents[0].Velocity && oldTime == live.SimulatedSeconds,
                        "resize preserves speed and simulation clock");
                    Check(live.Simulation.WorldState.Agents.All(a => BoardGeometry.ContainsPoint(a.Position,
                        live.ArenaSize, BoardShapeKind.RoundedRectangle)), "resize keeps agents inside the actual boundary");
                }
                GD.Print("ADAPTIVE ARENA PASS: full height, uniform scale, synchronized teams");
                scene.GetTree().Quit(0); return;
            }
            CheckLocalization();
            var s = scene.Session;
            Check(scene.Preparation.Cards.Length == 3, "three approved cards are in the playable scene");
            Check(MathF.Abs(scene.Presenters[1].Position.X - scene.Presenters[0].Position.X - 192) < .01f,
                "presenters sit together at the center");
            Check(scene.BubbleArea(0).End.X < scene.Presenters[0].Position.X
                && scene.BubbleArea(1).Position.X > scene.Presenters[1].Position.X + 192,
                "Gerard's bubble stays left and Rene's bubble stays right");
            var gerardLine = scene.SpeechFor(0);
            s.Tick(4); s.Tick(1); scene.SyncUi();
            Check(scene.SpeechFor(0) == gerardLine && scene.SpeechFor(1) == s.Speech && s.Speaker == 1,
                "each presenter keeps his own side of the conversation");
            if (scene.AnimationDirector is not null) BroadcastAnimationChecks.Run(scene);
            else Check(scene.Presenters.All(p => p.Gaze == PortraitGaze.Partner && p.HasGazeAtlas),
                "legacy portraits briefly look at each other during Rene's explanation");
            s.Tick(2); scene.SyncUi();
            Check(scene.Presenters.All(p => p.Gaze == PortraitGaze.Board), "eyes return to the action after the conversational glance");
            scene.ResetButton.EmitSignal(Button.SignalName.Pressed);
            scene.GetViewport().NotifyMouseEntered();
            async Task ClickAt(Control control, Vector2 local)
            {
                var point = control.GetGlobalTransformWithCanvas() * local;
                scene.GetViewport().PushInput(new InputEventMouseMotion { Position = point }, true);
                scene.GetViewport().PushInput(new InputEventMouseButton { Position = point,
                    ButtonIndex = MouseButton.Left, Pressed = true }, true);
                scene.GetViewport().PushInput(new InputEventMouseButton { Position = point,
                    ButtonIndex = MouseButton.Left, Pressed = false }, true);
                await scene.ToSignal(scene.GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            var clickable = scene.Preparation.Cards[0];
            await ClickAt(clickable, new Vector2(150, 180));
            Check(clickable.IsFlipped, "mouse click on front flips the card");
            await ClickAt(clickable, new Vector2(150, 180));
            Check(!clickable.IsFlipped, "mouse click on back flips the card back");
            await ClickAt(scene.Preparation.Sliders[0], new Vector2(scene.Preparation.Sliders[0].Size.X - 1, 9));
            Check(s.Draft == BudgetPlan.Focus(BehaviorChoice.Turn), "external slider can allocate all ten credits with pointer input");
            scene.Preparation.ClearButton.EmitSignal(Button.SignalName.Pressed);
            Check(s.Draft == new BudgetPlan(0, 0, 0) && s.Draft.Remaining == 10 && scene.Primary.Disabled,
                "all sliders start empty and ten unspent points block start");
            scene.Preparation.Sliders[0].Value = 3;
            scene.Preparation.Sliders[1].Value = 4;
            Check(s.Draft == new BudgetPlan(3, 4, 0) && s.Draft.Remaining == 3,
                "each slider consumes only its own points");
            scene.Preparation.Sliders[2].Value = 10;
            Check(s.Draft == new BudgetPlan(3, 4, 3) && scene.Preparation.Sliders[2].Value == 3,
                "requested increase stops at remaining capacity without moving other sliders");
            scene.Preparation.Sliders[0].Value = 8;
            Check(s.Draft == new BudgetPlan(3, 4, 3) && scene.Preparation.Sliders[0].Value == 3,
                "full budget blocks increases and restores the requested slider");
            scene.Preparation.Sliders[1].Value = 2;
            Check(s.Draft == new BudgetPlan(3, 2, 3) && s.Draft.Remaining == 2 && scene.Primary.Disabled,
                "lowering a slider returns points without moving the others");
            var propulsion = scene.Preparation.Cards[1];
            propulsion.Flip();
            scene.Preparation.Sliders[2].Value = 5;
            Check(s.Draft == new BudgetPlan(3, 2, 5) && !scene.Primary.Disabled,
                "freed points can be assigned independently while cards are flipped");
            propulsion.Flip();
            var beforeInvalid = s.Draft;
            Check(!s.SetAllocation(BehaviorChoice.Direction, 11) && s.Draft == beforeInvalid, "out of range is rejected");
            scene.Preparation.ClearButton.EmitSignal(Button.SignalName.Pressed);
            scene.Preparation.Sliders[2].Value = 10;
            Check(s.Draft == new BudgetPlan(0, 0, 10), "ten available points can be assigned to one card");
            CheckIndependentAllocation();
            CheckPerfectSurvival();
            CheckOpponent();
            scene.Primary.EmitSignal(Button.SignalName.Pressed);
            Check(s.HasStarted && s.Active.Direction == 10 && s.Simulation.WorldState.Agents.Count == LevelOneSession.Population, "start uses actual budget");
            Check(scene.BoardVisible && !scene.Preparation.Visible,
                "starting hides the large cards and reveals the arena");
            var expectedArena = MathF.Min(scene.Size.X * .7f, 1008);
            Check(MathF.Abs(scene.BoardArea.Size.X - expectedArena) < .01f
                && MathF.Abs(scene.BoardArea.Position.X - (scene.Size.X - expectedArena) / 2) < .01f,
                "widescreen surplus is assigned equally to both player panels");
            var lockedPlan = s.Draft;
            Check(!s.SetAllocation(BehaviorChoice.Direction, 0) && !s.ApplyPlan(), "running round rejects edits and revalidation");
            s.ClearPlan();
            Check(s.Draft == lockedPlan && s.Active == lockedPlan && s.Pending is null, "clear cannot change a running plan");
            scene.SpeedSlider.Value = 7;
            Check(s.Speed == 7, "slider accepts intermediate speeds");
            scene.SpeedSlider.Value = 10;
            Check(s.Speed == 10 && !s.SetSpeed(20) && !s.SetSpeed(0), "speed is capped at ten");
            scene._UnhandledKeyInput(new InputEventKey { Keycode = Key.Key4, Pressed = true });
            Check(s.Speed == 10, "old speed shortcut cannot bypass slider cap");
            Check(MathF.Abs(scene.BubbleArea(0).Position.X - scene.BoardArea.Position.X) < .01f
                && MathF.Abs(scene.BubbleArea(1).End.X - scene.BoardArea.End.X) < .01f,
                "commentary panels align with the arena edges");
            Check(scene.SpeedSlider.Position.X > scene.BoardArea.End.X && scene.PauseButton.Position.X > scene.BoardArea.End.X,
                "speed and pause sit in our sidebar");
            Check(MathF.Abs(scene.BoardArea.End.Y - (scene.Size.Y - 8 - BroadcastScene.ReservedFooterHeight)) < .01f && scene.BoardArea.Size.Y > scene.Size.Y * .65f,
                "arena occupies the lower scene down to the bottom margin");
            Check(scene.Billboard.Position.Y > scene.BubbleArea(0).End.Y
                && scene.Billboard.Position.Y + scene.Billboard.Size.Y < scene.BoardArea.Position.Y,
                "score strip fits between commentary and arena");
            Check(scene.Billboard.GetChildren().OfType<TickerStrip>().Count(t => t.ClipContents) == 2,
                "both scrolling message lanes clip their content");
            scene.PauseButton.EmitSignal(Button.SignalName.Pressed);
            var still = s.Simulation.WorldState.Agents.Select(a => a.Position).ToArray();
            var speechBeforePause = s.SpeechAge;
            s.Tick(1);
            Check(s.Paused && still.SequenceEqual(s.Simulation.WorldState.Agents.Select(a => a.Position)) && s.SpeechAge > speechBeforePause,
                "pause freezes physics while commentary remains readable");
            Check(!s.SetAllocation(BehaviorChoice.Turn, 3) && !s.ApplyPlan() && s.Paused,
                "pausing never unlocks cards");
            if (OS.GetCmdlineUserArgs().Contains("--cards-test"))
            { GD.Print($"CARDS PASS: {_checks} checks including locked round and sidebars"); scene.GetTree().Quit(0); return; }
            scene.ResetButton.EmitSignal(Button.SignalName.Pressed);
            s.SetAllocation(BehaviorChoice.Direction, 10); scene.SyncUi(); s.SetSpeed(1); scene.Primary.EmitSignal(Button.SignalName.Pressed);
            Advance(s, LevelOneSession.PeriodSeconds / LevelOneSession.BaseTimeScale - .1f);
            Check(!s.Discovered && s.Completed == 0, "5-percent criterion remains hidden before first selection");
            Advance(s, .22f);
            Check(s.Completed == 1 && s.Stage == BroadcastStage.Selection, "first cutoff records once and holds briefly");
            Advance(s, .12f);
            Check(s.Discovered, "first selection reveals the narrow zone");
            var zone = (RectangleSurvivalZone)SurvivalRegionFactory.CreateZones(s.ArenaSize, LevelOneSession.RightEdge())[0];
            Check(MathF.Abs(zone.Bounds.Size.X / s.ArenaSize.X - .05f) < .00001f
                && MathF.Abs(zone.Bounds.Position.X / s.ArenaSize.X - .95f) < .00001f,
                "survival band is exactly the rightmost 5 percent");
            var rule = new RegionSurvivalRule(new ISurvivalZone[] { zone });
            foreach (var agent in s.Simulation.WorldState.Agents)
                Check(s.Survivors.Contains(agent) == rule.Evaluate(agent, s.Simulation.WorldState, new EvolutionSettings()).Survives,
                    "visible survivors equal original geographical selection");
            Check(s.Survivors.All(a => a.Position.X > s.ArenaSize.X * .95f && a.MaxDistanceFromBirth >= 6),
                "survivors are inside narrow strip and have moved");
            Advance(s, .7f);
            Check(s.Period == 2 && s.Stage == BroadcastStage.Running, "generation advances automatically without a review click");
            var parentDNA = s.Simulation.WorldState.Agents.Select(a => a.Genome).ToArray();
            Check(!s.SetAllocation(BehaviorChoice.Turn, 10) && !s.ApplyPlan(), "generation boundaries keep the round plan locked");
            Check(parentDNA.SequenceEqual(s.Simulation.WorldState.Agents.Select(a => a.Genome)), "rejected edits preserve live DNA");
            AdvanceUntil(s, () => s.Period >= 3, 25);
            Check(s.Active == new BudgetPlan(0, 0, 10) && s.Pending is null, "same plan remains active throughout the round");

            CheckSpeed();
            CheckFramePartition();
            CheckFactoryBudget();
            CheckIndependentAllocation();
            CheckGeometryParity();
            CheckObservationParity();
            s.SetSpeed(10);
            for (var generation = 10; generation <= 40; generation += 10)
            {
                AdvanceUntil(s, () => s.Completed >= generation, 60);
                GD.Print($"PROGRESS: G{s.Completed}; latest={s.LastResult!.Survivors}; best={s.Best}/240");
            }
            Check(s.Completed >= 40 && s.Period >= 40, "continuous loop reaches forty real generations");

            GD.Print("SURVIVAL TRACE: " + string.Join(",", s.History.Select(h => h.Survivors)));
            foreach (var h in s.History)
                Check(h.Rules.Values.Sum(r => r.Total) + h.Neutral.Total == LevelOneSession.Population
                    && h.Rules.Values.Sum(r => r.Survivors) + h.Neutral.Survivors == h.Survivors,
                    "every generation's plotted proportions and survival counts sum correctly");
            Check(s.Composition.Sum() == LevelOneSession.Population, "live proportions sum to current population");
            GD.Print($"CONTINUOUS: {s.Completed} generations; latest={s.LastResult!.Survivors}/{LevelOneSession.Population}; extinctions={s.Extinctions}");
            scene.ResetButton.EmitSignal(Button.SignalName.Pressed);
            Check(!s.HasStarted && !s.Discovered && s.Completed == 0 && s.Pending is null && s.Speed == 5 && s.Extinctions == 0,
                "reset clears history, speed, queued plans and disclosure");
            GD.Print($"PLAYTEST PASS: {_checks} checks in {sw.Elapsed.TotalSeconds:0.00}s. Budget, speed, 5% selection, 40 generations, history and queued DNA policy.");
            if (capture) await Capture(scene);
            scene.GetTree().Quit(0);
        }
        catch (Exception ex)
        {
            GD.PrintErr("PLAYTEST FAIL: " + ex);
            scene.GetTree().Quit(1);
        }
    }
    private static void CheckSpeed()
    {
        var slow = new LevelOneSession(); FillBalanced(slow); slow.SetSpeed(1); slow.Start(); slow.Tick(.25f);
        var fast = new LevelOneSession(); FillBalanced(fast); fast.SetSpeed(10); fast.Start(); fast.Tick(.25f);
        Check(Math.Abs(slow.SimulatedSeconds - .5) < .001, "base speed now simulates two seconds per real second");
        Check(Math.Abs(fast.SimulatedSeconds - slow.SimulatedSeconds * 10) < .001, "10x performs ten times the physics before cutoff");
    }
    private static void CheckFramePartition()
    {
        var a = new LevelOneSession(); var b = new LevelOneSession();
        FillBalanced(a); FillBalanced(b); a.SetSpeed(1); b.SetSpeed(1); a.Start(); b.Start();
        for (var i = 0; i < 120; i++) a.Tick(1f / 120);
        for (var i = 0; i < 30; i++) b.Tick(1f / 30);
        Check(a.Simulation.WorldState.Agents.Zip(b.Simulation.WorldState.Agents)
            .All(p => p.First.Position.DistanceTo(p.Second.Position) < .0001f), "fixed physics gives equal positions at 30 and 120 FPS");
    }
    private static void CheckFactoryBudget()
    {
        var plan = new BudgetPlan(1, 1, 8);
        var profile = BehaviorCatalog.Profile(plan); var pool = BehaviorCatalog.Pool(plan);
        var random = new Random(70); var hits = 0;
        for (var i = 0; i < 3000; i++)
            if (GeneRuleFactory.CreateRandomRule(random, pool, LevelOneSession.PeriodSeconds, 0, profile).ActionKind == GeneActionKind.AccelerateDirection) hits++;
        Check(MathF.Abs(hits / 3000f - .8f) < .03f, "8/10 budget matches 80 percent conditional action draws");
    }
    private static void CheckLocalization()
    {
        var en = L10n.ReadCatalog("en");
        var fr = L10n.ReadCatalog("fr");
        Check(en.Keys.Order().SequenceEqual(fr.Keys.Order()), "English and French catalogs have identical keys");
        foreach (var key in en.Keys)
        {
            var count = System.Text.CompositeFormat.Parse(en[key]).MinimumArgumentCount;
            Check(count == System.Text.CompositeFormat.Parse(fr[key]).MinimumArgumentCount,
                "translation placeholders match across languages");
            var args = Enumerable.Repeat<object>(1, count).ToArray();
            Check(!string.IsNullOrWhiteSpace(string.Format(L10n.Culture, en[key], args))
                && !string.IsNullOrWhiteSpace(string.Format(L10n.Culture, fr[key], args)), "both translations format successfully");
        }
        Check(L10n.Text("card.turn.title") == (L10n.Language == "en" ? "Turn" : "Virage"), "selected language reaches the cards");
        Check(L10n.Text("speech.perfect_rene").Length > 0, "perfect-survival dialogue is translated");
    }

    private static void FillBalanced(LevelOneSession s)
    {
        s.ClearPlan(); s.SetAllocation(BehaviorChoice.Turn, 3);
        s.SetAllocation(BehaviorChoice.Throttle, 3); s.SetAllocation(BehaviorChoice.Direction, 4);
    }
    private static void CheckIndependentAllocation()
    {
        for (var turn = 0; turn <= 10; turn++)
            for (var push = 0; push <= 10 - turn; push++)
                for (var target = 0; target <= 10 - turn - push; target++)
                    foreach (var choice in Enum.GetValues<BehaviorChoice>())
                        for (var value = 0; value <= 10; value++)
                        {
                            var plan = new BudgetPlan(turn, push, target);
                            var next = plan.Allocate(choice, value);
                            Check(next.Total <= 10 && next[choice] == Math.Min(value, plan[choice] + plan.Remaining)
                                && Enum.GetValues<BehaviorChoice>().Where(c => c != choice).All(c => next[c] == plan[c]),
                                "every partial and complete plan leaves other allocations unchanged");
                        }
    }
    private static void CheckPerfectSurvival()
    {
        var s = new LevelOneSession(); FillBalanced(s); s.SetSpeed(1); s.Start();
        s.Tick(LevelOneSession.PeriodSeconds / LevelOneSession.BaseTimeScale - .01f);
        // Deliberately arrange a boundary fixture; production simulation never forces survivors.
        for (var i = 0; i < s.Simulation.WorldState.Agents.Count; i++)
        {
            var agent = s.Simulation.WorldState.Agents[i];
            agent.Position = new Vector2(1250, 30 + i * 2);
            agent.Velocity = Vector2.Zero; agent.MaxDistanceFromBirth = 10;
        }
        s.Tick(.02f);
        Check(s.LastResult?.Survivors == 240 && s.Paused && (s.PerfectSurvivalReached || s.Opponent?.PerfectSurvivalReached == true) && s.Discovered,
            "perfect survival pauses at selection with the full population counted");
        var elapsed = s.SimulatedSeconds;
        s.Tick(5);
        Check(s.SimulatedSeconds == elapsed && s.Period == 1 && s.Speaker == 1,
            "celebration dialogue continues while physics and generation remain paused");
        s.TogglePause(); s.Tick(1);
        Check(!s.Paused && s.Period == 2 && s.PerfectSurvivalReached, "player can resume after the first perfect result");
        s.Reset();
        Check(!s.PerfectSurvivalReached && !s.Paused && s.Draft.Total == 0, "reset clears achievement and budget");
    }

    private static void CheckOpponent()
    {
        var s = new LevelOneSession(); FillBalanced(s); s.SetSpeed(10); s.Start();
        var rival = s.Opponent!;
        Check(rival.HasStarted && rival.Active.Total == 10 && rival.Speed == 10, "opponent commits a full budget and shares speed");
        Check(!rival.SetAllocation(BehaviorChoice.Direction, 0), "opponent cards lock during the round");
        Check(!ReferenceEquals(s.Simulation, rival.Simulation)
            && s.Simulation.WorldState.Agents[0].Position == rival.Simulation.WorldState.Agents[0].Position,
            "teams have separate populations with matched starting positions");
        AdvanceUntil(s, () => s.Completed >= 2, 10);
        Check(s.Completed == rival.Completed && s.Period == rival.Period && s.SimulatedSeconds == rival.SimulatedSeconds,
            "both teams remain synchronized across selection and reproduction");
        Check(rival.History.Count == 2 && rival.Composition.Sum() == 240 && rival.LastResult!.Survivors == rival.Survivors.Count,
            "opponent results and composition come from its own population");
        s.TogglePause(); var elapsed = rival.SimulatedSeconds; s.Tick(2);
        Check(elapsed == rival.SimulatedSeconds, "pause stops both teams");
        s.Reset(); FillBalanced(s); s.SetSpeed(1); s.Start();
        s.Tick(LevelOneSession.PeriodSeconds / LevelOneSession.BaseTimeScale - .01f);
        foreach (var agent in rival.Simulation.WorldState.Agents)
        {
            agent.Position = new Vector2(1250, 280); agent.Velocity = Vector2.Zero; agent.MaxDistanceFromBirth = 10;
        }
        s.Tick(.02f);
        Check(rival.LastResult?.Survivors == 240 && rival.PerfectSurvivalReached && s.Paused,
            "opponent perfect survival pauses the shared round");
        s.TogglePause(); s.Tick(1);
        Check(s.Period == rival.Period && !s.Paused, "both teams resume after opponent victory");
        s.Reset();
        Check(!rival.HasStarted && rival.History.Count == 0 && !rival.PerfectSurvivalReached, "restart clears the opponent");
        Check(Enumerable.Range(1, 10).Select(OpponentStrategy.Choose).Distinct().Count() > 1,
            "opponent chooses varied strategies between rounds");
    }

    private static void CheckGeometryParity()
    {
        var size = LevelOneSession.WorldSize;
        var polygon = BoardGeometry.GetOutline(size, BoardShapeKind.RoundedRectangle);
        var random = new Random(920);
        for (var i = 0; i < 3000; i++)
        {
            var p = new Vector2((float)random.NextDouble() * (size.X + 30) - 15, (float)random.NextDouble() * (size.Y + 30) - 15);
            var inside = Geometry2D.IsPointInPolygon(p, polygon);
            Check(BoardGeometry.ContainsPoint(p, size, BoardShapeKind.RoundedRectangle) == inside, "optimized containment matches Godot polygon");
            var closest = p; var best = float.MaxValue; var normal = Vector2.Zero;
            for (var j = 0; j < polygon.Length; j++)
            {
                var a = polygon[j]; var b = polygon[(j + 1) % polygon.Length];
                var candidate = Geometry2D.GetClosestPointToSegment(p, a, b); var distance = p.DistanceSquaredTo(candidate);
                if (distance < best) { best = distance; closest = candidate; normal = (b - a).Orthogonal().Normalized(); }
            }
            Check(BoardGeometry.ClampPoint(p, size, BoardShapeKind.RoundedRectangle).DistanceTo(inside ? p : closest) < .002f,
                "optimized clamping matches native closest segments");
            if ((p - size / 2).Dot(normal) < 0) normal = -normal;
            Check(BoardGeometry.GetBoundaryNormal(p, size, BoardShapeKind.RoundedRectangle).Dot(normal) > .999f,
                "optimized normals match native polygon edges");
        }
        var original = polygon[0]; polygon[0] = new Vector2(-999, -999);
        Check(BoardGeometry.GetOutline(size, BoardShapeKind.RoundedRectangle)[0] == original, "public outlines cannot mutate the private cache");
    }
    private static void CheckObservationParity()
    {
        var settings = new SimulationSettings();
        var full = new AgentObservation(12, 50, 35, new Vector2(4, 8), true, 7, 20, true);
        var minimal = new AgentObservation(0, 0, 0, Vector2.Zero, false, 0, 20, true);
        foreach (var condition in Enum.GetValues<GeneConditionKind>())
        foreach (var action in Enum.GetValues<GeneActionKind>())
        {
            var rule = new GeneRule(true, condition, .3f, action, .8f, new Vector2(.8f, .4f));
            var program = new GeneProgram(rule, GeneRule.Disabled, GeneRule.Disabled);
            Check(Simulation.RequiresNeighborhood(program) == (condition == GeneConditionKind.NeighborCountGreaterOrEqual),
                "neighborhood fast path follows actual inherited conditions");
            if (condition == GeneConditionKind.NeighborCountGreaterOrEqual) continue;
            var a = new Agent { Genome = AgentGenome.Default with { Program = program }, PhysicsProfile = PhysicsProfile.Default,
                Position = settings.WorldSize * .6f, BrainClock = 10, IsTouchingWall = true };
            Check(AgentBrain.Decide(a, full, settings) == AgentBrain.Decide(a, minimal, settings)
                && AgentBrain.DecideRuleIntent(a, full, settings) == AgentBrain.DecideRuleIntent(a, minimal, settings),
                "minimal observation preserves every non-neighbor action intent");
        }
    }
    private static void Advance(LevelOneSession s, float seconds)
    {
        for (var i = 0; i < (int)MathF.Ceiling(seconds * 30); i++) s.Tick(1f / 30);
    }
    private static void AdvanceUntil(LevelOneSession s, Func<bool> done, float limit)
    {
        for (var i = 0; i < limit * 30 && !done(); i++)
        {
            if (s.Paused && (s.PerfectSurvivalReached || s.Opponent?.PerfectSurvivalReached == true)) s.TogglePause(); // Simulate choosing to continue after victory.
            s.Tick(1f / 30);
        }
        Check(done(), "requested simulation milestone is reached within its time budget");
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message); _checks++;
    }
    private static async Task Capture(BroadcastScene scene)
    {
        scene.Session.Reset(); scene.SetProcess(true);
        async Task Save(string name)
        {
            scene.SyncUi();
            await scene.ToSignal(scene.GetTree().CreateTimer(.25), SceneTreeTimer.SignalName.Timeout);
            await scene.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var image = scene.GetViewport().GetTexture().GetImage();
            GD.Print("GAME_CAPTURE:" + name + ":" + Convert.ToBase64String(image.SavePngToBuffer()));
        }
        scene.Session.Tick(2);
        await Save("01-preparation.png");
        scene.Session.Tick(4); scene.Session.Tick(1.2f);
        foreach (var card in scene.Preparation.Cards) card.FlipButton.EmitSignal(Button.SignalName.Pressed);
        await Save("02-versos-et-dialogue.png");
        foreach (var card in scene.Preparation.Cards) card.FlipButton.EmitSignal(Button.SignalName.Pressed);
        scene.Preparation.Sliders[1].Value = 8;
        await Save("03-budget-automatique.png");
        if (OS.GetCmdlineUserArgs().Contains("--capture-cards")) return;
        scene.Session.ClearPlan(); scene.Session.SetAllocation(BehaviorChoice.Direction, 10); scene.Preparation.Refresh();
        scene.Primary.EmitSignal(Button.SignalName.Pressed);
        scene.SpeedSlider.Value = 10;
        AdvanceUntil(scene.Session, () => scene.Session.Completed >= 1 && scene.Session.Discovered, 4);
        scene.Session.TogglePause();
        scene.Session.Tick(2.5f);
        await Save("03-selection.png");
        scene.Session.TogglePause();
        AdvanceUntil(scene.Session, () => scene.Session.Period >= 2 && scene.Session.Stage == BroadcastStage.Running, 4);
        Advance(scene.Session, .25f);
        scene.Session.TogglePause();
        scene.Session.Tick(3);
        await Save("04-sidebars.png");
        if (OS.GetCmdlineUserArgs().Contains("--capture-layout"))
        {
            await scene.ToSignal(scene.GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);
            await Save("05-billboard-motion.png");
            return;
        }
        scene.Session.TogglePause();
        AdvanceUntil(scene.Session, () => scene.Session.Completed >= 40, 100);
        if (!scene.Session.Paused) scene.Session.TogglePause();
        scene.Session.Tick(2.5f);
        await Save("05-forty-generations.png");
        GD.Print("CAPTURE TRACE: " + string.Join(",", scene.Session.History.Select(h => h.Survivors)));
    }
}
