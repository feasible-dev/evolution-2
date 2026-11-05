using System.Text.Json;
using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Loads native-size pixel frames without the older portrait palette shader.</summary>
public sealed class BroadcastAnimationAssets : IDisposable
{
    private const string Root = "res://assets/broadcast-v39/";
    private readonly Dictionary<(int Actor, string Clip), AtlasTexture[]> _frames = new();
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
    }

    public AtlasTexture Get(int actor, BroadcastPortraitFrame pose) => _frames[(actor, pose.Clip)][pose.Frame];
    public void Dispose()
    {
        foreach (var frames in _frames.Values) foreach (var frame in frames) frame.Dispose();
        _frames.Clear();
    }
}
