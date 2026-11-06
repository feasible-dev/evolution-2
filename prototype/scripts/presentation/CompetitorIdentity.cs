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
    private Texture2D? _neutral;
    private Texture2D _stage = null!;
    private Font _displayFont = null!;
    private readonly List<ImageTexture> _cutouts = new();
    private readonly Dictionary<OpponentReaction, AtlasTexture[]> _reactions = new();
    public OpponentReactionPose ReactionPose { get; private set; }
    public Texture2D? DisplayedPortrait => _portrait?.Texture;
    // One imported portrait pixel occupies one logical UI pixel, just like
    // the commentators' 192x128 frames. Never squeeze it into a thumbnail.
    private const float Tile = 128;
    private bool Compact => Size.X < Tile + 122;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _stage = GD.Load<Texture2D>("res://assets/studio-v49/opponent-stage.png");
        _displayFont = GD.Load<Font>("res://assets/fonts/RacingSansOne-Regular.ttf");
        _portrait = new TextureRect { ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore,
            TextureFilter = TextureFilterEnum.Nearest };
        AddChild(_portrait);
        _name = Text(this, "", 24, BroadcastHardware.Cream);
        _name.AddThemeFontOverride("font", _displayFont);
        _character = Text(this, "", 11, new Color("b9b6a4"));
        _name.AutowrapMode = _character.AutowrapMode = TextServer.AutowrapMode.Word;
        Resized += Arrange;
        Refresh();
    }

    private void Refresh()
    {
        _name.Text = L10n.Text(Profile.NameKey).ToUpper(L10n.Culture);
        _character.Text = L10n.Text(Profile.CharacterKey);
        ClearReactions();
        _neutral = Profile.PortraitPath is { } path && ResourceLoader.Exists(path)
            ? Cutout(GD.Load<Texture2D>(path)) : null;
        if (Profile.Id == "nico-valdes")
        {
            foreach (var reaction in new[] { OpponentReaction.Win, OpponentReaction.Lose })
            {
                var sheet = GD.Load<Texture2D>($"res://assets/opponents/nico-v44/{reaction.ToString().ToLowerInvariant()}.png");
                if (sheet is null || sheet.GetSize() != new Vector2(128, 1024))
                    throw new InvalidOperationException($"Invalid Nico reaction sheet: {reaction}");
                _reactions[reaction] = Enumerable.Range(0,8).Select(i => new AtlasTexture
                {
                    Atlas = CutoutSheet(sheet), Region = new Rect2(0, i * 128, 128, 128), FilterClip = true
                }).ToArray();
            }
        }
        ShowReaction(default);
        _portrait.Visible = _neutral is not null;
        Arrange();
        QueueRedraw();
    }

    public void ShowReaction(OpponentReactionPose pose)
    {
        ReactionPose = pose;
        // Exact approved rest pixels bookend every clip; no generated near-copy.
        _portrait.Texture = pose.Frame is > 0 and < 7 && _reactions.TryGetValue(pose.Kind, out var frames)
            ? frames[pose.Frame] : _neutral;
    }

    private void ClearReactions()
    {
        foreach (var frames in _reactions.Values) foreach (var frame in frames) frame.Dispose();
        _reactions.Clear();
        foreach (var cutout in _cutouts) cutout.Dispose();
        _cutouts.Clear(); _sheetCache.Clear();
    }

    private readonly Dictionary<Texture2D, ImageTexture> _sheetCache = new();
    private ImageTexture Cutout(Texture2D texture)
    {
        // Nico's approved source has a subtly varying purple matte, unlike the
        // flat commentator sheets. Limit tolerance to this portrait family.
        var cutout = StudioSpriteCompositor.Create(texture, 128, 128, matteTolerance: 8);
        _cutouts.Add(cutout); return cutout;
    }
    private ImageTexture CutoutSheet(Texture2D sheet)
    {
        if (!_sheetCache.TryGetValue(sheet, out var cutout))
            _sheetCache.Add(sheet, cutout = Cutout(sheet));
        return cutout;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) ClearReactions();
    }

    private void Arrange()
    {
        if (_portrait is null) return;
        _name.AutowrapMode = Compact ? TextServer.AutowrapMode.Off : TextServer.AutowrapMode.Word;
        var nameSize = 24;
        if (Compact)
            while (nameSize > 14 && _displayFont.GetStringSize(_name.Text, fontSize: nameSize).X > Size.X - 12) nameSize--;
        _name.AddThemeFontSizeOverride("font_size", nameSize);
        _name.HorizontalAlignment = _character.HorizontalAlignment = Compact
            ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        if (Compact)
        {
            Place(_portrait, MathF.Floor((Size.X - Tile) / 2), 28, Tile, Tile);
            Place(_name, 6, 158, Size.X - 12, 24);
            Place(_character, 6, 182, Size.X - 12, 16);
        }
        else
        {
            Place(_portrait, 6, 46, Tile, Tile);
            Place(_name, Tile + 16, 68, Size.X - Tile - 24, 64);
            Place(_character, Tile + 16, 136, Size.X - Tile - 24, 30);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_stage is null) return;
        DrawTextureRect(_stage, new Rect2(0, 0, Size.X, 201), false);
        var x = Size.X - 88;
        DrawColoredPolygon(new[] { new Vector2(x + 4, 6), new Vector2(Size.X - 2, 2),
            new Vector2(Size.X - 6, 23), new Vector2(x, 27) }, TeamColor);
        DrawString(_displayFont, new Vector2(x + 9, 20), L10n.Text("profile.level", 1), fontSize: 13, modulate: BroadcastHardware.Ink);
    }
}
