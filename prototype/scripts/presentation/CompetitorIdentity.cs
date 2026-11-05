using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class CompetitorIdentity : Control
{
    private CompetitorProfile _profile = CompetitorProfile.Opponent;
    public CompetitorProfile Profile
    {
        get => _profile;
        set { _profile = value; if (IsNodeReady()) Refresh(); }
    }
    public Color TeamColor { get; set; } = TeamPalette.Us;
    private TextureRect _portrait = null!;
    private Label _name = null!, _character = null!;
    // One imported portrait pixel occupies one logical UI pixel, just like
    // the commentators' 192x128 frames. Never squeeze it into a thumbnail.
    private const float Tile = 128;
    private bool Compact => Size.X < Tile + 122;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _portrait = new TextureRect { ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore,
            TextureFilter = TextureFilterEnum.Nearest };
        AddChild(_portrait);
        _name = Text(this, "", 15, TeamColor);
        _character = Text(this, "", 12, Muted);
        _name.AutowrapMode = _character.AutowrapMode = TextServer.AutowrapMode.Word;
        Resized += Arrange;
        Refresh();
    }

    private void Refresh()
    {
        _name.Text = L10n.Text(Profile.NameKey);
        _character.Text = L10n.Text(Profile.CharacterKey);
        _portrait.Texture = Profile.PortraitPath is { } path && ResourceLoader.Exists(path)
            ? GD.Load<Texture2D>(path) : null;
        _portrait.Visible = _portrait.Texture is not null;
        Arrange();
        QueueRedraw();
    }

    private void Arrange()
    {
        if (_portrait is null) return;
        _name.HorizontalAlignment = _character.HorizontalAlignment = Compact
            ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        if (Compact)
        {
            Place(_portrait, MathF.Floor((Size.X - Tile) / 2), 0, Tile, Tile);
            Place(_name, 0, 130, Size.X, 22);
            Place(_character, 0, 152, Size.X, 20);
        }
        else
        {
            Place(_portrait, 0, 0, Tile, Tile);
            Place(_name, Tile + 12, 20, Size.X - Tile - 20, 44);
            Place(_character, Tile + 12, 68, Size.X - Tile - 20, 40);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(0, 0, Size.X, Compact ? 172 : Tile), new Color("071320"));
    }
}
