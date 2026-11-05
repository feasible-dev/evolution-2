using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class GenerationTimeline : Control
{
    public LevelOneSession Session { get; set; } = null!;
    public bool IsOpponent { get; set; }
    private LevelOneSession Team => IsOpponent ? Session.Opponent! : Session;
    private Color TeamColor => IsOpponent ? TeamPalette.Opponent : TeamPalette.Us;
    private const float Left = 32;
    private float PlotWidth => MathF.Max(1, Size.X - Left - 8);
    private float Step => PlotWidth / 40;
    private float SampleX(int index) => Left + (index + .5f) * Step;

    public override void _Draw()
    {
        var font = GetThemeDefaultFont();
        var rows = Team.History.TakeLast(40).ToArray();
        DrawString(font, new Vector2(0, 15), L10n.Text("chart.survival"), fontSize: 13, modulate: Muted);
        var latest = Team.LastResult is { } last ? (last.SurvivalRate * 100).ToString("0.0", L10n.Culture) + "%" : "—";
        var latestText = L10n.Text("chart.latest", latest);
        DrawString(font, new Vector2(0, 35), latestText, fontSize: 13, modulate: TeamColor);
        const float top = 49;
        const float height = 76;
        var baseline = top + height;
        var right = Left + PlotWidth;
        var axis = new Color("476488");
        var guide = new Color("29415b");
        DrawLine(new Vector2(Left, top), new Vector2(Left, baseline), axis, 1);
        DrawLine(new Vector2(Left, baseline), new Vector2(right, baseline), axis, 1);
        foreach (var fraction in new[] { 0f, .5f, 1f })
        {
            var y = baseline - fraction * height;
            var label = $"{fraction * 100:0}%";
            DrawString(font, new Vector2(Left - 6 - font.GetStringSize(label, fontSize: 10).X, y + 3), label, fontSize: 10, modulate: Muted);
            DrawLine(new Vector2(Left - 3, y), new Vector2(Left, y), axis, 1);
            if (fraction == 1)
                DrawDashedLine(new Vector2(Left + 1, y), new Vector2(right, y), guide, 1, 5);
            else if (fraction == .5f)
                DrawLine(new Vector2(Left + 1, y), new Vector2(right, y), new Color("14273c"), 1);
        }
        var points = rows.Select((row, i) => new Vector2(SampleX(i), baseline - row.SurvivalRate * height)).ToArray();
        if (points.Length > 1) DrawPolyline(points, TeamColor, 2, true);
        for (var i = 0; i < rows.Length; i++)
        {
            DrawCircle(points[i], rows.Length == 1 ? 2 : 1, TeamColor);
            if (i > 0 && rows[i].Plan != rows[i - 1].Plan)
                DrawLine(new Vector2(SampleX(i), top), new Vector2(SampleX(i), baseline), new Color(Pink, .35f), 1);
            if (i == 0 || i == rows.Length - 1)
            {
                var label = $"G{rows[i].Period}";
                DrawLine(new Vector2(SampleX(i), baseline), new Vector2(SampleX(i), baseline + 3), axis, 1);
                DrawString(font, new Vector2(SampleX(i) - font.GetStringSize(label, fontSize: 10).X / 2, baseline + 14), label, fontSize: 10, modulate: Muted);
            }
        }
        DrawString(font, new Vector2(0, 282), L10n.Text("chart.mix"), fontSize: 12, modulate: Muted);
        var colors = new[] { BehaviorCatalog.All[0].Color, BehaviorCatalog.All[1].Color, BehaviorCatalog.All[2].Color, new Color("476488") };
        for (var j = 0; j < 4; j++)
        {
            var y = 303 + j * 21;
            var name = j == 3 ? L10n.Text("plan.no_rule") : BehaviorCatalog.All[j].Name;
            var share = Team.Composition[j] / (float)LevelOneSession.Population;
            var value = (share * 100).ToString("0.0", L10n.Culture) + "%";
            DrawString(font, new Vector2(0, y), name, fontSize: 11, modulate: colors[j]);
            DrawString(font, new Vector2(Size.X - font.GetStringSize(value, fontSize: 11).X, y), value, fontSize: 11, modulate: Muted);
            DrawRect(new Rect2(0, y + 5, Size.X, 4), new Color("14273c"));
            if (share > 0) DrawRect(new Rect2(0, y + 5, Size.X * share, 4), colors[j]);
        }
        if (rows.Length == 0)
            DrawString(font, new Vector2(Left + 10, top + height * .6f), "—", fontSize: 16, modulate: Muted);

    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion mouse) return;
        var rows = Team.History.TakeLast(40).ToArray();
        if (mouse.Position.Y > 148) { TooltipText = ""; return; }
        var index = (int)MathF.Floor((mouse.Position.X - Left) / Step);
        TooltipText = index >= 0 && index < rows.Length
            ? L10n.Text("chart.tooltip", rows[index].Period, rows[index].Survivors, LevelOneSession.Population,
                rows[index].SurvivalRate, rows[index].Plan.Turn, rows[index].Plan.Throttle, rows[index].Plan.Direction) : "";
    }
}
