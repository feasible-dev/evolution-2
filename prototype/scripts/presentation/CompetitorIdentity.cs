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
    private Font _displayFont = null!;
    private readonly List<ImageTexture> _cutouts = new();
    private readonly Dictionary<OpponentReaction, AtlasTexture[]> _reactions = new();
    public OpponentReactionPose ReactionPose { get; private set; }
    public Texture2D? DisplayedPortrait => _portrait?.Texture;
    // Keep the waist-up participant at native pixel scale; captions live beside it.
    private const float Tile = 128;
    private bool Compact => Size.X < Tile + 122;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
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
                var sheet = GD.Load<Texture2D>($"res://assets/studio-v52/nico-{reaction.ToString().ToLowerInvariant()}.png");
                if (sheet is null || sheet.GetSize() != new Vector2(128, 1536))
                    throw new InvalidOperationException($"Invalid Nico reaction sheet: {reaction}");
                _reactions[reaction] = Enumerable.Range(0,8).Select(i => new AtlasTexture
                {
                    Atlas = CutoutSheet(sheet), Region = new Rect2(0, i * 192, 128, 192), FilterClip = true
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
        var cutout = StudioSpriteCompositor.Create(texture, 128, 192, matteTolerance: 12);
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
        _name.AutowrapMode = TextServer.AutowrapMode.Word;
        var textX = Compact ? 108 : 138;
        var textWidth = MathF.Max(62, Size.X - textX - 4);
        _name.AddThemeFontSizeOverride("font_size", Compact ? 17 : 24);
        _name.HorizontalAlignment = _character.HorizontalAlignment = HorizontalAlignment.Left;
        Place(_portrait, Compact ? -8 : 0, 18, Tile, 192);
        Place(_name, textX, 56, textWidth, 56);
        Place(_character, textX, 110, textWidth, 32);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var x = Size.X - 88;
        DrawColoredPolygon(new[] { new Vector2(x + 4, 6), new Vector2(Size.X - 2, 2),
            new Vector2(Size.X - 6, 23), new Vector2(x, 27) }, TeamColor);
        DrawString(_displayFont, new Vector2(x + 9, 20), L10n.Text("profile.level", 1), fontSize: 13, modulate: BroadcastHardware.Ink);
    }
}
