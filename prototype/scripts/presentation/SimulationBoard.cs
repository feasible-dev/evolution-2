using EvoPrototype.Core;
using EvoPrototype.Evolution;
using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class SimulationBoard : Control
{
    public LevelOneSession Session { get; set; } = null!;
    public Rect2 PlayRect { get; private set; }
    private Agent? _inspected;
    private LevelOneSession? _inspectedTeam;
    private IEnumerable<(LevelOneSession Team, int Index)> Teams
    {
        get { yield return (Session, 0); if (Session.Opponent is { } rival) yield return (rival, 1); }
    }
    private readonly Dictionary<int, Texture2D> _textures = new();
    public string Inspection
    {
        get
        {
            if (_inspected is null || _inspectedTeam is null || !_inspectedTeam.Simulation.WorldState.Agents.Contains(_inspected))
                return L10n.Text("board.inspect_tip");
            var rule = AgentVisualPalette.GetDisplayRule(_inspected.Genome.Program);
            var def = BehaviorCatalog.For(_inspected);
            var teamName = L10n.Text(ReferenceEquals(_inspectedTeam, Session) ? "ui.us" : "ui.opponent");
            return teamName + " / " + (def is null ? L10n.Text("board.no_rule")
                : L10n.Text("board.rule", def.Name, rule.Scalar)
                + (def.Id == BehaviorChoice.Direction ? L10n.Text("board.direction", rule.DirectionAngleNormalized * 360) : ""));
        }
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse) return;
        var found = Teams.SelectMany(t => t.Team.Simulation.WorldState.Agents.Select(a => (t.Team, Agent: a)))
            .Where(t => t.Team.Cycle.Phase != EvolutionPhase.Selection || t.Team.Survivors.Contains(t.Agent))
            .OrderBy(t => (MapToArena(t.Agent.Position, PlayRect, Session.ArenaSize)).DistanceSquaredTo(mouse.Position))
            .FirstOrDefault(t => (MapToArena(t.Agent.Position, PlayRect, Session.ArenaSize)).DistanceTo(mouse.Position) < 12);
        _inspected = found.Agent; _inspectedTeam = found.Team;
        AcceptEvent();
        TooltipText = Inspection;
    }
    public override void _Draw()
    {
        if (Session is null) return;
        // Fill the arena allocated by the layout; world-space physics remain unchanged.
        var extent = Size - Vector2.One * 12;
        PlayRect = FitArena(new Rect2((Size - extent) / 2, extent), Session.ArenaSize);
        var outline = BoardGeometry.GetOutline(Session.ArenaSize, BoardShapeKind.RoundedRectangle)
            .Select(p => MapToArena(p, PlayRect, Session.ArenaSize)).ToArray();
        DrawColoredPolygon(outline, new Color("01040a"));
        var closedOutline = outline.Append(outline[0]).ToArray();
        DrawPolyline(closedOutline, new Color("174578"), 1.5f, true);
        if (!Session.HasStarted)
        {
            DrawString(GetThemeDefaultFont(), PlayRect.GetCenter() - new Vector2(115, 0), L10n.Text("board.start"), fontSize: 17, modulate: Accent);
            return;
        }
        if (Session.Discovered)
        {
            var boundary = MapToArena(new Vector2(Session.ArenaSize.X * (1 - LevelOneSession.ZoneFraction), 0), PlayRect, Session.ArenaSize).X;
            var strip = new[] { new Vector2(boundary, PlayRect.Position.Y - 1),
                new Vector2(PlayRect.End.X + 1, PlayRect.Position.Y - 1),
                PlayRect.End + Vector2.One, new Vector2(boundary, PlayRect.End.Y + 1) };
            foreach (var clipped in Geometry2D.IntersectPolygons(outline, strip))
            {
                DrawColoredPolygon(clipped, new Color(.12f, .45f, .94f, .22f));
                var edge = clipped.Where(p => MathF.Abs(p.X - boundary) < .01f).OrderBy(p => p.Y).ToArray();
                if (edge.Length >= 2) DrawLine(edge[0], edge[^1], Accent, 1);
            }
            DrawString(GetThemeDefaultFont(), new Vector2(boundary + 5, PlayRect.Position.Y + 40), "5 %", fontSize: 11, modulate: Accent);
        }
        var ownPositions = Session.Simulation.WorldState.Agents
            .Where(a => Session.Cycle.Phase != EvolutionPhase.Selection || Session.Survivors.Contains(a))
            .Select(a => a.Position).ToHashSet();
        foreach (var (team, key) in Teams)
        {
            var color = key == 0 ? TeamPalette.Us : TeamPalette.Opponent;
            if (!_textures.TryGetValue(key, out var texture))
            {
                texture = MakePoint(color); _textures[key] = texture;
            }
            foreach (var a in team.Simulation.WorldState.Agents)
            {
                if (team.Cycle.Phase == EvolutionPhase.Selection && !team.Survivors.Contains(a)) continue;
                var center = MapToArena(a.Position, PlayRect, Session.ArenaSize);
                // Identical decks can coincide exactly: keep both team colors visible.
                if (key == 1 && ownPositions.Contains(a.Position))
                    DrawTextureRectRegion(texture, new Rect2(center + new Vector2(0, -2.5f), new Vector2(2.5f, 5)),
                        new Rect2(2.5f, 0, 2.5f, 5));
                else
                    DrawTextureRect(texture, new Rect2(center - Vector2.One * 2.5f, Vector2.One * 5), false);
                if (ReferenceEquals(a, _inspected)) DrawArc(center, 6, 0, Mathf.Tau, 12, color, 1);
            }
        }
    }
    public static Rect2 FitArena(Rect2 available, Vector2? worldSize = null)
    {
        var world = worldSize ?? LevelOneSession.WorldSize;
        var scale = MathF.Min(available.Size.X / world.X,
            available.Size.Y / world.Y);
        var size = world * scale;
        return new Rect2(available.Position + new Vector2((available.Size.X - size.X) / 2, 0), size);
    }

    // One scale everywhere: physical distance and speed have the same visual meaning.
    public static Vector2 MapToArena(Vector2 point, Rect2 available, Vector2? worldSize = null)
    {
        var world = worldSize ?? LevelOneSession.WorldSize;
        var arena = FitArena(available, world);
        return arena.Position + point * (arena.Size.X / world.X);
    }

    private static Texture2D MakePoint(Color color)
    {
        using var image = Image.CreateEmpty(5, 5, false, Image.Format.Rgba8);
        image.Fill(Colors.Transparent);
        for (var y = -2; y <= 2; y++)
            for (var x = -2; x <= 2; x++)
                if (x * x + y * y <= 5) image.SetPixel(x + 2, y + 2, color);
        return ImageTexture.CreateFromImage(image);
    }
}
