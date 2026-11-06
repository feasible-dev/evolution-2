using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

/// <summary>Three live cards sharing the existing ten-credit budget.</summary>
public partial class RulePreparationPanel : Control
{
    public LevelOneSession Session { get; set; } = null!;
    public Action? Updated { get; set; }
    public Action? Committed { get; set; }
    public EvolutionCard[] Cards { get; private set; } = Array.Empty<EvolutionCard>();
    public HSlider[] Sliders { get; private set; } = new HSlider[3];
    public Button CommitButton { get; private set; } = null!;
    public Button ClearButton { get; private set; } = null!;
    private Label _budget = null!, _remaining = null!, _notice = null!, _context = null!;
    private readonly List<Label> _chances = new();
    private const float PanelWidth = 660;

    public override void _Ready()
    {
        Text(this, L10n.Text("plan.title"), 14, Muted).Position = Vector2.Zero;
        _budget = Text(this, "", 26, Accent); Place(_budget, 0, 25, 210, 42);
        _remaining = Text(this, "", 16, Muted); Place(_remaining, 223, 29, 300, 36);
        ClearButton = Button(this, L10n.Text("plan.clear"), () => { Session.ClearPlan(); Refresh(); Updated?.Invoke(); }, 14);
        Place(ClearButton, 552, 32, 108, 32);
        var sub = Text(this, L10n.Text("plan.help"), 13, Muted);
        Place(sub, 0, 70, PanelWidth, 22);
        Cards = new EvolutionCard[3];
        for (var i = 0; i < 3; i++)
        {
            var definition = BehaviorCatalog.All[i];
            var x = i * 225;
            var card = new EvolutionCard { Choice = definition.Id, Position = new Vector2(x, 94), Scale = Vector2.One * .7f };
            Cards[i] = card; AddChild(card);
            var budgetBox = new Panel { MouseFilter = MouseFilterEnum.Ignore };
            budgetBox.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color("09192b"),
                CornerRadiusTopLeft = 6, CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6, CornerRadiusBottomRight = 6 });
            AddChild(budgetBox); Place(budgetBox, x, 398, 210, 53);
            var caption = Text(budgetBox, L10n.Text("plan.budget"), 11, Muted); Place(caption, 12, 4, 60, 21);
            var chance = Text(budgetBox, "", 14, definition.Color);
            chance.HorizontalAlignment = HorizontalAlignment.Right;
            Place(chance, 70, 4, 128, 21); _chances.Add(chance);
            Sliders[i] = CreateSlider(definition.Id);
            budgetBox.AddChild(Sliders[i]); Place(Sliders[i], 12, 28, 186, 18);
        }
        CommitButton = Button(this, "", () => { var applied = Session.ApplyPlan(); Refresh(); Updated?.Invoke(); if (applied) Committed?.Invoke(); });
        Place(CommitButton, 0, 463, PanelWidth, 40);
        _notice = Text(this, "", 14, Muted); _notice.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        Place(_notice, 0, 509, PanelWidth, 41);
        _context = Text(this, "", 13, Muted); Place(_context, 0, 557, PanelWidth, 30);
        Refresh();
    }

    private HSlider CreateSlider(BehaviorChoice choice)
    {
        var slider = new HSlider
        {
            Position = new Vector2(100, 374), Size = new Vector2(144, 20),
            MinValue = 0, MaxValue = BudgetPlan.Limit, Step = 1, Value = 0,
            TooltipText = L10n.Text("plan.slider_tip")
        };
        var track = new StyleBoxFlat { BgColor = new Color("38546b"), ContentMarginTop = 2, ContentMarginBottom = 2,
            CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2, CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2 };
        var fill = (StyleBoxFlat)track.Duplicate(); fill.BgColor = new Color("72d3e8");
        slider.AddThemeStyleboxOverride("slider", track);
        slider.AddThemeStyleboxOverride("grabber_area", fill);
        slider.AddThemeStyleboxOverride("grabber_area_highlight", fill);
        using var knobImage = new Image();
        knobImage.LoadSvgFromString("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\"><circle cx=\"8\" cy=\"8\" r=\"6\" fill=\"#edf8f4\" stroke=\"#72d3e8\" stroke-width=\"2\"/></svg>");
        var knob = ImageTexture.CreateFromImage(knobImage);
        slider.AddThemeIconOverride("grabber", knob);
        slider.AddThemeIconOverride("grabber_highlight", knob);
        slider.ValueChanged += value => ChangeAllocation(choice, (int)Math.Round(value));
        return slider;
    }

    private void ChangeAllocation(BehaviorChoice choice, int requested)
    {
        Session.SetAllocation(choice, requested);
        Refresh(); Updated?.Invoke();
    }

    public void Refresh()
    {
        if (_budget is null) return;
        _budget.Text = L10n.Text("plan.total", Session.Draft.Total, BudgetPlan.Limit);
        _remaining.Text = Session.Draft.Remaining == 0 ? L10n.Text("plan.full") : L10n.Text("plan.remaining", Session.Draft.Remaining);
        for (var i = 0; i < Cards.Length; i++)
        {
            var id = BehaviorCatalog.All[i].Id;
            Sliders[i].SetValueNoSignal(Session.Draft[id]);
            Sliders[i].Editable = !Session.HasStarted;
            _chances[i].Text = L10n.Text("plan.allocation", Session.Draft[id], Session.Draft.Chance(id) * 100);
        }
        CommitButton.Text = Session.HasStarted ? L10n.Text("plan.apply") + (Session.Period + 1).ToString("00") : L10n.Text("plan.start");
        CommitButton.Disabled = Session.HasStarted || !Session.Draft.IsValid;
        ClearButton.Disabled = Session.HasStarted;
        _notice.Text = Session.Notice;
        _notice.Modulate = Session.Pending.HasValue ? Pink : Colors.White;
        _context.Text = !Session.HasStarted ? L10n.Text("plan.empty_help") : Session.Pending.HasValue ? L10n.Text("plan.pending", Session.Period + 1)
            : L10n.Text("plan.active", Session.Active.Turn, Session.Active.Throttle, Session.Active.Direction, Session.AppliedAtGeneration);
        QueueRedraw();
    }

}
