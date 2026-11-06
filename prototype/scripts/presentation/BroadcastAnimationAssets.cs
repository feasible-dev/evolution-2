using System.Text.Json;
using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Loads native-size pixel frames without the older portrait palette shader.</summary>
public sealed class BroadcastAnimationAssets : IDisposable
{
    private const string Root = "res://assets/broadcast-v39/";
    private readonly Dictionary<(int Actor, string Clip), AtlasTexture[]> _frames = new();
    private readonly Dictionary<(int Actor, string Clip), AtlasTexture[]> _cutoutFrames = new();
    private readonly Dictionary<Texture2D, ImageTexture> _cutoutSheets = new();
    private readonly Dictionary<(int Actor, string Clip, int Frame), ImageTexture> _displayFrames = new();
    private ImageTexture? _standing;
    private ImageTexture? _sitting, _excited;
    // The older generated poses use three exact matte colors. Flood only from
    // the outside so enclosed clothing and facial shadows keep their pixels.
    private static readonly HashSet<int> MattePalette = new() { 0x04132d, 0x031332, 0x081423 };
    public Dictionary<string, double[]> EventDurations { get; } = new();

    public BroadcastAnimationAssets()
    {
        using var manifest = JsonDocument.Parse(Godot.FileAccess.GetFileAsString(Root + "manifest.json"));
        var count = manifest.RootElement.GetProperty("conversationFrames").GetInt32();
        Add(0, "conversation", GD.Load<Texture2D>("res://assets/broadcast-v41/gerard-conversation.png"), count, 0);
        Add(1, "conversation", GD.Load<Texture2D>(Root + "rene-conversation.png"), count, 0);
        var rink=GD.Load<Texture2D>("res://assets/broadcast-v41/rink.png");
        Add(0,"rink",rink,16,0);Add(1,"rink",rink,16,192);
        var excited = GD.Load<Texture2D>("res://assets/broadcast-v41/excited.png");
        Add(0,"excited",excited,8,0); Add(1,"excited",excited,8,192);
        foreach (var entry in manifest.RootElement.GetProperty("events").EnumerateObject())
        {
            var durations = entry.Value.GetProperty("durationsMs").EnumerateArray().Select(v => v.GetDouble() / 1000).ToArray();
            EventDurations.Add(entry.Name, durations);
            var sheet = GD.Load<Texture2D>(Root + entry.Name + ".png");
            Add(0, entry.Name, sheet, durations.Length, 0);
            Add(1, entry.Name, sheet, durations.Length, 192);
        }
    }

    private void Add(int actor, string clip, Texture2D sheet, int count, int x)
    {
        if (sheet is null || sheet.GetWidth() < x + 192 || sheet.GetHeight() != count * 128)
            throw new InvalidOperationException($"Invalid broadcast sheet: {actor}/{clip}");
        _frames.Add((actor, clip), Enumerable.Range(0, count).Select(i => new AtlasTexture
        {
            Atlas = sheet, Region = new Rect2(x, i * 128, 192, 128), FilterClip = true
        }).ToArray());
        if (!_cutoutSheets.TryGetValue(sheet, out var cutout))
        {
            cutout = StudioSpriteCompositor.Create(sheet, 192, 128, mattePalette: MattePalette);
            _cutoutSheets.Add(sheet, cutout);
        }
        _cutoutFrames.Add((actor, clip), Enumerable.Range(0, count).Select(i => new AtlasTexture
        {
            Atlas = cutout, Region = new Rect2(x, i * 128, 192, 128), FilterClip = true
        }).ToArray());
    }

    public AtlasTexture Get(int actor, BroadcastPortraitFrame pose) => _frames[(actor, pose.Clip)][pose.Frame];
    public AtlasTexture GetCutout(int actor, BroadcastPortraitFrame pose) => _cutoutFrames[(actor, pose.Clip)][pose.Frame];
    public ImageTexture GetDisplay(int actor, BroadcastPortraitFrame pose)
    {
        var key = (actor, pose.Clip, pose.Frame);
        if (_displayFrames.TryGetValue(key, out var cached)) return cached;
        using var canvas = Image.CreateEmpty(192, 192, false, Image.Format.Rgba8);
        canvas.Fill(Colors.Transparent);
        if (actor == 0 && (pose.Clip == "excited" || pose.Clip == "stand" && pose.Frame >= 16))
        {
            _excited ??= StudioSpriteCompositor.Create(
                GD.Load<Texture2D>("res://assets/studio-v52/gerard-excited.png"), 192, 128, matteTolerance: 14);
            using var sheet = StudioSpriteCompositor.CopyImage(_excited);
            var frame = pose.Clip == "stand" ? (pose.Frame == 16 ? 1 : 0) : pose.Frame;
            var baseFrame = frame / 2 * 2;
            var origin = new Vector2I(baseFrame % 4 * 192, baseFrame / 4 * 128);
            canvas.BlitRect(sheet, new Rect2I(origin, new Vector2I(192, 120)), new Vector2I(0, 64));
            if (frame % 2 == 1)
            {
                // Share the closed pose's skull, chin and torso. A syllable
                // replaces only the generated lip pixels, never the whole face.
                var mouths = new[] { new Rect2I(100, 59, 14, 8), new Rect2I(87, 62, 14, 8),
                    new Rect2I(120, 56, 14, 8), new Rect2I(124, 56, 14, 8) };
                var mouth = mouths[baseFrame / 2];
                var source = new Rect2I(origin + new Vector2I(192, 0) + mouth.Position, mouth.Size);
                canvas.BlitRect(sheet, source, mouth.Position + new Vector2I(0, 64));
            }
        }
        else if (actor == 0 && pose.Clip == "stand" && pose.Frame is 14 or 15)
        {
            _sitting ??= StudioSpriteCompositor.Create(
                GD.Load<Texture2D>("res://assets/studio-v52/gerard-sit.png"), 192, 192, matteTolerance: 14);
            using var sheet = StudioSpriteCompositor.CopyImage(_sitting);
            canvas.BlitRect(sheet, new Rect2I((pose.Frame - 14) * 192, 0, 192, 180), new Vector2I(0, 12));
        }
        else if (actor == 0 && pose.Clip == "stand" && pose.Frame is >= 2 and <= 13)
        {
            _standing ??= StudioSpriteCompositor.Create(
                GD.Load<Texture2D>("res://assets/studio-v51/gerard-standing.png"), 192, 192, matteTolerance: 14);
            using var sheet = StudioSpriteCompositor.CopyImage(_standing);
            // Larger presenters still reveal the excited eyes below the screen
            // edge. Translate the standing body; never shrink its head to fit.
            var drop = pose.Frame == 2 ? 8 : 18;
            canvas.BlitRect(sheet, new Rect2I(pose.Frame % 6 * 192, pose.Frame / 6 * 192, 192, 192 - drop), new Vector2I(0, drop));
        }
        else
        {
            using var original = StudioSpriteCompositor.CopyImage(GetCutout(actor, pose));
            // The bottom eight source rows are the obsolete flat blue desk,
            // not character pixels. The painted gallery supplies the desk now.
            canvas.BlitRect(original, new Rect2I(0, 0, 192, 120), new Vector2I(0, 64));
        }
        var display = ImageTexture.CreateFromImage(canvas);
        _displayFrames.Add(key, display);
        return display;
    }
    public void Dispose()
    {
        foreach (var frames in _frames.Values) foreach (var frame in frames) frame.Dispose();
        _frames.Clear();
        foreach (var frames in _cutoutFrames.Values) foreach (var frame in frames) frame.Dispose();
        foreach (var sheet in _cutoutSheets.Values) sheet.Dispose();
        _cutoutFrames.Clear(); _cutoutSheets.Clear();
        foreach (var frame in _displayFrames.Values) frame.Dispose();
        _displayFrames.Clear(); _standing?.Dispose(); _sitting?.Dispose(); _excited?.Dispose();
    }
}
