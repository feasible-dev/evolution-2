using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class CompetitorIdentity : Control
{
    private CompetitorProfile _profile = CompetitorProfile.Player;
    public CompetitorProfile Profile
    {
        get => _profile;
        set { _profile = value; if (IsNodeReady()) Refresh(); }
    }
    public Color TeamColor { get; set; } = TeamPalette.Us;
    private TextureRect _portrait = null!;
    private Label _name = null!, _character = null!;
    private const float Tile = 84;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _portrait = new TextureRect { ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_portrait);
        _name = Text(this, "", 15, TeamColor);
        _character = Text(this, "", 12, Muted);
        _name.AutowrapMode = _character.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        Refresh();
    }

    private void Refresh()
    {
        _name.Text = L10n.Text(Profile.NameKey);
        _character.Text = L10n.Text(Profile.CharacterKey);
        _portrait.Texture = Profile.PortraitPath is { } path && ResourceLoader.Exists(path)
            ? GD.Load<Texture2D>(path) : null;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var tile = new Rect2(0, (Size.Y - Tile) / 2, Tile, Tile);
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("071320"));
        DrawRect(tile, new Color("0d2235"));
        DrawRect(tile, new Color(TeamColor, .65f), false, 1);
        if (_portrait is null) return;
        Place(_portrait, tile.Position.X + 3, tile.Position.Y + 3, Tile - 6, Tile - 6);
        Place(_name, Tile + 12, 22, Size.X - Tile - 20, 32);
        Place(_character, Tile + 12, 56, Size.X - Tile - 20, 42);
        if (_portrait.Texture is not null) return;
        // Neutral character placeholder, deliberately not a likeness of the user.
        var center = tile.GetCenter();
        var ink = new Color("8da8bc");
        DrawCircle(center + new Vector2(0, -13), 14, ink);
        DrawColoredPolygon(new[] { center + new Vector2(-27, 31), center + new Vector2(-23, 14),
            center + new Vector2(-12, 6), center + new Vector2(12, 6),
            center + new Vector2(23, 14), center + new Vector2(27, 31) }, ink);
        DrawLine(tile.Position + new Vector2(5, Tile - 5), tile.End - new Vector2(5, 5), TeamColor, 2);
    }
}
