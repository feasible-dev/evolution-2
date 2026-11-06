using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class BroadcastScene : Control
{
    public LevelOneSession Session { get; } = new();
    public RulePreparationPanel Preparation { get; private set; } = null!;
    public Button Primary => Preparation.CommitButton;
    public Button ResetButton { get; private set; } = null!;
    public Button PauseButton { get; private set; } = null!;
    public HSlider SpeedSlider { get; private set; } = null!;
    public bool BoardVisible => _board.Visible;
    public const float ReservedFooterHeight = 48; // Half an inch at the reference 96-DPI UI scale.
    public Rect2 BoardArea => new(_board.Position, _board.Size);
    // Extra widescreen space belongs to the player panels, rather than black margins.
    private float SideWidth => (Size.X - MathF.Min(Size.X * .70f, 1008)) / 2;
    public CompetitorIdentity OpponentIdentity { get; private set; } = null!;
    private SimulationBoard _board = null!;
    public ArenaBillboard Billboard { get; private set; } = null!;
    private Label _opponentMetrics = null!;
    private Label _ownPhysics = null!, _opponentPhysics = null!;
    private Label _ownStrategy = null!, _opponentStrategy = null!;
    private ActiveBehaviorCard[] _opponentCards = Array.Empty<ActiveBehaviorCard>();
    private GenerationTimeline _timeline = null!, _opponentTimeline = null!;
    private Label _speedLabel = null!;
    private PortraitView[] _portraits = null!;
    private Label _brand = null!, _status = null!, _metrics = null!, _rate = null!;
    private SpeechBubble[] _bubbles = Array.Empty<SpeechBubble>();
    private StudioBackdrop _studio = null!;
    private Label[] _bubbleTexts = Array.Empty<Label>(), _bubbleNames = Array.Empty<Label>();
    private readonly string[] _bubbleLines = { "", "" };
    private string _lastBubbleText = "";
    private int _lastBubbleSpeaker = -1;
    private float _lastSpeechAge = -1;
    private double _broadcastTime;
    private readonly ArenaAttention _attention = new();
    private readonly OpponentReactionDirector _opponentReaction = new();
    private BroadcastAnimationAssets? _animationAssets;
    public BroadcastAnimationDirector? AnimationDirector { get; private set; }
    public IReadOnlyList<PortraitView> Presenters => _portraits;
    public string SpeechFor(int speaker) => _bubbleLines[speaker];
    public Rect2 BubbleArea(int speaker) => new(_bubbles[speaker].Position, _bubbles[speaker].Size);
    private Label _opponentTitle = null!, _ownTitle = null!, _opponentState = null!, _ownState = null!;
    private ActiveBehaviorCard[] _deckButtons = Array.Empty<ActiveBehaviorCard>();
    private Button _sound = null!;
    private readonly List<CanvasItem> _retainedCanvas = new();
    private bool _layoutRunning;
    private int _revision = -1;
    private double _rateReal, _rateSim;
    private float _measuredSpeed;
    private AudioStreamPlayer _click = null!;
    public bool AudioEnabled { get; private set; } = true;

    public override void _Ready()
    {
        Theme = new Theme { DefaultFont = ThemeDB.FallbackFont, DefaultFontSize = 18 };
        Theme.SetStylebox("panel", "TooltipPanel", Style("06152b", "2f8ae8"));
        Theme.SetColor("font_color", "TooltipLabel", Paper);
        _studio = new StudioBackdrop(); AddChild(_studio);
        OpponentIdentity = new CompetitorIdentity { Profile = CompetitorProfile.Opponent, TeamColor = TeamPalette.Opponent };
        AddChild(OpponentIdentity);
        _brand = Text(this, "EVOLUTION\nWARS", 28, BroadcastHardware.Cream);
        _brand.AddThemeFontOverride("font", GD.Load<Font>("res://assets/fonts/RacingSansOne-Regular.ttf"));
        _brand.HorizontalAlignment = HorizontalAlignment.Center;
        _status = Text(this, "", 14, Muted); _status.HorizontalAlignment = HorizontalAlignment.Right;
        _bubbles = new SpeechBubble[2]; _bubbleTexts = new Label[2]; _bubbleNames = new Label[2];
        for (var i = 0; i < 2; i++)
        {
            var panel = new SpeechBubble { PointsRight = i == 0 }; AddChild(panel);
            _bubbles[i] = panel;
            _bubbleNames[i] = Text(panel, i == 0 ? "Gerard" : "René", 14, new Color("53616a"));
            _bubbleNames[i].AddThemeFontOverride("font", GD.Load<Font>("res://assets/fonts/RacingSansOne-Regular.ttf"));
            _bubbleTexts[i] = Text(panel, "", 17, new Color("172932"));
            _bubbleTexts[i].AutowrapMode = TextServer.AutowrapMode.WordSmart;
        }
        _portraits = new[] { new PortraitView(), new PortraitView() };
        foreach (var p in _portraits) AddChild(p);
        if (OS.GetCmdlineUserArgs().Contains("--portrait-review") || OS.GetCmdlineUserArgs().Contains("--classic-portraits"))
        {
            _portraits[0].Initialize("res://assets/gerard-mccautry-v13b.png", "res://assets/gerard-gazes-v14.png");
            _portraits[1].Initialize("res://assets/rene-le-chevalier-v13.png", "res://assets/rene-gazes-v14.png");
        }
        else
        {
            _animationAssets = new BroadcastAnimationAssets();
            AnimationDirector = new BroadcastAnimationDirector(_animationAssets.EventDurations);
            for (var i = 0; i < 2; i++) _portraits[i].InitializeAnimation(_animationAssets, i);
        }
        Preparation = new RulePreparationPanel
        {
            Session = Session, Updated = () => { Click(); SyncUi(); }
        };
        AddChild(Preparation);
        Billboard = new ArenaBillboard { Session = Session }; AddChild(Billboard);
        _board = new SimulationBoard { Session = Session, TextureFilter = TextureFilterEnum.Nearest }; AddChild(_board);
        _timeline = new GenerationTimeline { Session = Session }; AddChild(_timeline);
        _opponentTimeline = new GenerationTimeline { Session = Session, IsOpponent = true }; AddChild(_opponentTimeline);
        _opponentMetrics = Text(this, "", 13);
        _ownPhysics = Text(this, "", 11, TeamPalette.Us);
        _opponentPhysics = Text(this, "", 11, TeamPalette.Opponent);
        _ownPhysics.AddThemeConstantOverride("line_spacing", -3);
        _opponentPhysics.AddThemeConstantOverride("line_spacing", -3);
        _ownPhysics.MouseFilter = _opponentPhysics.MouseFilter = MouseFilterEnum.Stop;
        _opponentMetrics.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _metrics = Text(this, "", 13);
        _metrics.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _rate = Text(this, "", 13, Muted);
        PauseButton = Button(this, L10n.Text("ui.pause"), () => { Session.TogglePause(); Click(); SyncUi(); }, 15);
        _speedLabel = Text(this, "", 13, Muted);
        SpeedSlider = new HSlider { MinValue = 1, MaxValue = LevelOneSession.MaxSpeed, Step = 1, Value = Session.Speed };
        foreach (var style in new[] { "slider", "grabber_area", "grabber_area_highlight" })
            SpeedSlider.AddThemeStyleboxOverride(style, Preparation.Sliders[0].GetThemeStylebox(style));
        foreach (var icon in new[] { "grabber", "grabber_highlight" })
            SpeedSlider.AddThemeIconOverride(icon, Preparation.Sliders[0].GetThemeIcon(icon));
        SpeedSlider.ValueChanged += value => { Session.SetSpeed((int)value); SyncUi(); };
        AddChild(SpeedSlider);
        _opponentTitle = Text(this, L10n.Text("ui.opponent"), 14, TeamPalette.Opponent);
        _ownTitle = Text(this, L10n.Text("ui.us"), 14, TeamPalette.Us);
        _ownStrategy = Text(this, L10n.Text("ui.active_cards"), 13, Muted);
        _opponentStrategy = Text(this, L10n.Text("ui.active_cards"), 13, Muted);
        _opponentState = Text(this, L10n.Text("opponent.short"), 12, Muted);
        _opponentState.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _ownState = Text(this, "", 13, Muted); _ownState.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _deckButtons = new ActiveBehaviorCard[3];
        _opponentCards = new ActiveBehaviorCard[3];
        for (var i = 0; i < 3; i++)
        {
            _deckButtons[i] = new ActiveBehaviorCard { Choice = (BehaviorChoice)i, TeamColor = TeamPalette.Us };
            _opponentCards[i] = new ActiveBehaviorCard { Choice = (BehaviorChoice)i, TeamColor = TeamPalette.Opponent };
            AddChild(_deckButtons[i]); AddChild(_opponentCards[i]);
        }
        ResetButton = Button(this, L10n.Text("ui.restart"), ResetRun, 13);
        _sound = Button(this, L10n.Text("ui.sound_on"), () => { AudioEnabled = !AudioEnabled; _sound.Text = AudioEnabled ? L10n.Text("ui.sound_on") : L10n.Text("ui.sound_off"); }, 12);
        _sound.TooltipText = L10n.Text("ui.sound_tip");
        MakeClickSound(); CollectCanvasItems(this);
        var impactLayer = new CanvasLayer { Layer = 100 };
        AddChild(impactLayer);
        impactLayer.AddChild(new BroadcastImpactOverlay { Billboard = Billboard });
        Arrange(); SyncUi();
        var args = OS.GetCmdlineUserArgs();
        if (args.Contains("--broadcast-review"))
        {
            SetProcess(false);
            Callable.From(() => { _ = BroadcastAnimationChecks.Review(this); }).CallDeferred();
        }
        else if (args.Contains("--portrait-review"))
        {
            SetProcess(false);
            Callable.From(() => { _ = Playtest.ReviewPortraits(this); }).CallDeferred();
        }
        else if (args.Contains("--smoke-test") || args.Contains("--cards-test") || args.Contains("--capture-flow") || args.Contains("--capture-cards") || args.Contains("--capture-layout"))
        {
            SetProcess(false);
            Callable.From(() => { _ = Playtest.Run(this, args.Contains("--capture-flow") || args.Contains("--capture-cards") || args.Contains("--capture-layout")); }).CallDeferred();
        }
    }

    private void ResetRun()
    {
        ClearBubbles(); Session.Reset(); _rateReal = _rateSim = 0; Click(); SyncUi();
    }

    public override void _Process(double delta)
    {
        _broadcastTime += delta;
        var before = Session.SimulatedSeconds;
        Session.Tick(MathF.Min((float)delta, .1f), frameBudgetMilliseconds: 16);
        _attention.Update(_broadcastTime, delta, Session);
        Session.TryArenaComment(_attention.Concentration, _attention.TrackingAgent);
        _rateReal += delta; _rateSim += Session.SimulatedSeconds - before;
        if (_rateReal >= .5) { _measuredSpeed = (float)(_rateSim / _rateReal / LevelOneSession.BaseTimeScale); _rateReal = _rateSim = 0; }
        SyncUi();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized && _board is not null) Arrange();
        if (what == NotificationPredelete)
        {
            _animationAssets?.Dispose();
        }
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;
        if (key.Keycode == Key.Space) Session.TogglePause();
        else if (key.Keycode == Key.Enter && !Session.HasStarted) Session.ApplyPlan();
        else if (key.Keycode == Key.R) ResetRun();
        else return;
        GetViewport().SetInputAsHandled(); SyncUi();
    }

    public void SyncUi()
    {
        _opponentReaction.Update(_broadcastTime, Session.HasStarted, Session.LastResult, Session.Opponent?.LastResult);
        OpponentIdentity.ShowReaction(_opponentReaction.Pose);
        if (_revision != Session.Revision)
        {
            _revision = Session.Revision;
            if (_layoutRunning != Session.HasStarted)
            {
                _layoutRunning = Session.HasStarted; Arrange();
            }
            _status.Text = !Session.HasStarted ? L10n.Text("ui.plan_status") : Session.Paused ? (Session.PerfectSurvivalReached && Session.LastResult?.Survivors == LevelOneSession.Population ? L10n.Text("ui.perfect_status") : L10n.Text("ui.pause")) : L10n.Text("ui.running", Session.Period);
            Preparation.Refresh();
            SpeedSlider.SetValueNoSignal(Session.Speed);
            _speedLabel.Text = L10n.Text("ui.speed", Session.Speed);
            PauseButton.Disabled = !Session.HasStarted;
            PauseButton.Text = Session.Paused ? L10n.Text("ui.resume") : L10n.Text("ui.pause");
            for (var i = 0; i < _deckButtons.Length; i++)
            {
                _deckButtons[i].SetAllocation(Session.Active[(BehaviorChoice)i]);
                _opponentCards[i].SetAllocation(Session.Opponent!.Active[(BehaviorChoice)i]);
            }
            _ownState.Text = L10n.Text("plan.locked");
        }
        if (AnimationDirector is not null)
        {
            AnimationDirector.Update(_broadcastTime, Session.SpeechSequence, Session.Speaker, Session.SpeechAge, Session.IsSpeaking,
                Session.Completed, Session.GestureSequence, Session.Gesture, _attention);
            for (var i = 0; i < 2; i++) _portraits[i].ShowAnimation(AnimationDirector.Frames[i]);
        }
        else
        {
            var mouth = Session.IsSpeaking && (int)(Session.SpeechAge * 7) % 3 != 0;
            for (var i = 0; i < 2; i++)
                _portraits[i].SetFrame(Session.Speaker == i ? Session.Expression : 0, Session.Speaker == i && mouth, Session.Speaker == i);
            var looks = PresenterMotion.At(_broadcastTime, Session.Speaker, Session.SpeechAge, Session.IsSpeaking);
            _portraits[0].SetGaze(looks.Gerard); _portraits[1].SetGaze(looks.Rene);
        }
        UpdateBubbles();
        _metrics.Text = Session.LastResult is { } last
            ? L10n.Text("ui.side_compact", last.Survivors, LevelOneSession.Population, Session.Completed)
            : L10n.Text("ui.population", LevelOneSession.Population);
        var rival = Session.Opponent!;
        _opponentMetrics.Text = rival.LastResult is { } rivalLast
            ? L10n.Text("ui.side_compact", rivalLast.Survivors, LevelOneSession.Population, rival.Completed)
            : L10n.Text("ui.population", LevelOneSession.Population);
        UpdatePhysics(_ownPhysics, Session);
        UpdatePhysics(_opponentPhysics, rival);
        _rate.Text = Session.Paused ? L10n.Text("ui.rate_paused", Session.SimulatedSeconds)
            : Session.HasStarted ? $"SIM. {Session.SimulatedSeconds:0}s · ×{_measuredSpeed:0.0}" : "";
        _board.QueueRedraw(); _timeline.QueueRedraw(); _opponentTimeline.QueueRedraw();
        foreach (var c in _retainedCanvas) c.QueueRedraw();
        QueueRedraw();
    }

    private static void UpdatePhysics(Label label, LevelOneSession team)
    {
        var stats = MotionSummary.Measure(team);
        label.Text = L10n.Text("physics.summary", stats.Speed, stats.EnergyPercent,
            stats.AccelerationCapacity, stats.Mass, stats.Drag, team.Simulation.Settings.FixedWorldRuleAcceleration ?? 0);
        label.TooltipText = L10n.Text("physics.help", team.Simulation.Settings.MaxSpeed, stats.Exhausted);
    }

    private void UpdateBubbles()
    {
        if (_lastBubbleSpeaker != Session.Speaker || _lastBubbleText != Session.Speech || Session.SpeechAge < _lastSpeechAge)
        {
            _bubbleLines[Session.Speaker] = Session.Speech;
            _bubbleTexts[Session.Speaker].Text = Session.Speech;
            _bubbles[Session.Speaker].ShowLine(Session.Speech);
            FitBubbleText(Session.Speaker);
            _lastBubbleText = Session.Speech; _lastBubbleSpeaker = Session.Speaker;
        }
        _lastSpeechAge = Session.SpeechAge;
        for (var i = 0; i < 2; i++)
        {
            var active = i == Session.Speaker;
            _bubbleTexts[i].VisibleCharacters = active && Session.IsSpeaking ? Math.Max(1, (int)(Session.SpeechAge * 48)) : -1;
            _bubbleTexts[i].Modulate = Colors.White;
        }
    }

    private void ClearBubbles()
    {
        Array.Fill(_bubbleLines, "");
        foreach (var bubble in _bubbles) bubble.Clear();
        _lastBubbleSpeaker = -1; _lastBubbleText = ""; _lastSpeechAge = -1; _broadcastTime = 0;
        AnimationDirector?.Reset();
        _attention.Reset();
        _opponentReaction.Reset();
    }

    private void Arrange()
    {
        var w = Size.X; var h = Size.Y;
        var side = SideWidth; var middle = w - side * 2;
        var running = Session.HasStarted;
        Place(_studio, side, 0, w - side, 184);
        Place(_brand, w - side + 24, 48, side - 48, 92);
        Place(_status, w - 350, 8, 324, 24);
        Place(OpponentIdentity, 16, 28, side - 32, 201);
        Place(_portraits[0], w * .5f - 192, 36, 192, 128);
        Place(_portraits[1], w * .5f, 36, 192, 128);
        var bubbleWidth = w * .5f - 202 - side;
        Place(_bubbles[0], side, 36, bubbleWidth, 128);
        Place(_bubbles[1], w * .5f + 202, 36, bubbleWidth, 128);
        for (var i = 0; i < 2; i++)
        {
            Place(_bubbleNames[i], 12, 6, bubbleWidth - 24, 21);
            Place(_bubbleTexts[i], 12, 29, bubbleWidth - 24, 94);
            FitBubbleText(i);
        }
        Place(Preparation, (w - 660) * .5f, 190, 660, h - 205);
        Preparation.Visible = !running;
        Place(Billboard, side, 168, middle, 70);
        var boardHeight = MathF.Max(200, h - 252 - ReservedFooterHeight);
        Place(_board, side, 244, middle, boardHeight);
        Session.ResizeArena(new Vector2(1280, 1280 * (boardHeight - 12) / (middle - 12)));
        var ownX = w - side + 16;
        var sidebarWidth = side - 32;
        Place(_opponentTitle, 16, 244, sidebarWidth, 28);
        Place(_ownTitle, ownX, 244, sidebarWidth, 28);
        Place(_opponentState, 16, 544, sidebarWidth, 26);
        Place(_ownStrategy, ownX, 420, sidebarWidth, 20);
        Place(_opponentStrategy, 16, 420, sidebarWidth, 20);
        for (var i = 0; i < _deckButtons.Length; i++)
        {
            var cardWidth = (sidebarWidth - 12) / 3;
            Place(_deckButtons[i], ownX + i * (cardWidth + 6), 444, cardWidth, 110);
            Place(_opponentCards[i], 16 + i * (cardWidth + 6), 444, cardWidth, 110);
            _opponentCards[i].Visible = running;
            _deckButtons[i].Visible = running;
        }
        Place(_ownState, ownX, 544, sidebarWidth, 26);
        Place(_timeline, ownX, 270, sidebarWidth, 144);
        Place(_opponentTimeline, 16, 270, sidebarWidth, 144);
        Place(_metrics, ownX, 562, sidebarWidth, 24);
        Place(_opponentMetrics, 16, 562, sidebarWidth, 24);
        Place(_ownPhysics, ownX, 598, sidebarWidth, 70);
        Place(_opponentPhysics, 16, 598, sidebarWidth, 70);
        Place(_speedLabel, ownX, h - 177, sidebarWidth, 22);
        Place(SpeedSlider, ownX, h - 152, sidebarWidth, 20);
        Place(_rate, ownX, h - 130, sidebarWidth, 22);
        Place(PauseButton, ownX, h - 105, sidebarWidth, 31);
        Place(ResetButton, ownX, h - 69, sidebarWidth, 31);
        Place(_sound, ownX, h - 33, sidebarWidth, 29);
        foreach (var c in new Control[] { _board, _timeline, _opponentTimeline,
            Billboard, _ownPhysics, _opponentPhysics, _ownStrategy, _opponentStrategy, _opponentMetrics, _metrics, PauseButton, _rate, _speedLabel, SpeedSlider, _opponentTitle, _ownTitle, _opponentState, _ownState })
            c.Visible = running;
        // The disabled strategy cards already explain the lock via their tooltip.
        _ownState.Visible = _opponentState.Visible = false;
        _opponentTitle.TooltipText = L10n.Text("opponent.ai_tip");
    }

    private void FitBubbleText(int speaker)
    {
        var label = _bubbleTexts[speaker];
        if (label.Size.X <= 0) return;
        var font = label.GetThemeDefaultFont();
        var size = 17;
        while (size > 12 && font.GetMultilineStringSize(label.Text, width: label.Size.X, fontSize: size).Y > 90) size--;
        label.AddThemeFontSizeOverride("font_size", size);
    }

    public override void _Draw()
    {
        var side = SideWidth;
        if (Session.HasStarted)
        {
            DrawRect(new Rect2(side, Size.Y - ReservedFooterHeight, Size.X - side * 2, ReservedFooterHeight - 8), new Color("040a12"));
            DrawRect(new Rect2(8, 240, side - 16, Size.Y - 248), new Color("111310"));
            DrawRect(new Rect2(Size.X - side + 8, 240, side - 16, Size.Y - 248), new Color("111310"));
            foreach (var x in new[] { side - 5, Size.X - side + 5 })
            {
                DrawLine(new Vector2(x, 240), new Vector2(x, Size.Y - 8), new Color("3c4446"), 2);
                DrawLine(new Vector2(x + 2, 240), new Vector2(x + 2, Size.Y - 8), new Color("11171a"), 2);
            }
        }
    }
    private void CollectCanvasItems(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is CanvasItem item) _retainedCanvas.Add(item);
            CollectCanvasItems(child);
        }
    }
    private void MakeClickSound()
    {
        const int rate = 22050; var data = new byte[rate / 28 * 2];
        for (var i = 0; i < data.Length / 2; i++)
        {
            var value = (short)(MathF.Sin(i * 2 * MathF.PI * 480 / rate) * 2800 * MathF.Exp(-i / 130f));
            data[2 * i] = (byte)(value & 255); data[2 * i + 1] = (byte)((value >> 8) & 255);
        }
        _click = new AudioStreamPlayer { Stream = new AudioStreamWav { Data = data, Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = rate }, VolumeDb = -15 }; AddChild(_click);
    }
    private void Click() { if (AudioEnabled && _click is not null) _click.Play(); }
}
