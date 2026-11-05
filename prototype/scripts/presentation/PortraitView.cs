using Godot;

namespace EvolutionTwo.Presentation;

public enum PortraitGaze { Board, Outer, Partner, Across }

/// <summary>Expression and gaze atlases share the same 192x128 viewport and finite palette.</summary>
public partial class PortraitView : TextureRect
{
    private TextureRect _frame = null!;
    private AtlasTexture[] _poses = null!;
    private AtlasTexture[]? _gazePoses;
    private int _index = -1, _pose;
    private bool _speaking;
    private ShaderMaterial _material = null!;
    private float _mouth, _gazeBlend = 1;
    private int _gazeFrom, _gazeTo;
    private bool _continuous;
    private BroadcastAnimationAssets? _animations;
    private int _actor;
    public bool UsesAnimation => _animations is not null;
    public BroadcastPortraitFrame AnimationFrame { get; private set; }
    public PortraitGaze Gaze { get; private set; }
    public bool HasGazeAtlas => _gazePoses is not null;

    public void InitializeAnimation(BroadcastAnimationAssets assets, int actor)
    {
        _animations = assets; _actor = actor;
        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;
        TextureFilter = TextureFilterEnum.Nearest;
        MouseFilter = MouseFilterEnum.Ignore;
        ShowAnimation(new("conversation", 0, PortraitGaze.Board));
    }

    public void ShowAnimation(BroadcastPortraitFrame pose)
    {
        if (_animations is null) return;
        AnimationFrame = pose; Gaze = pose.Gaze;
        Texture = _animations.Get(_actor, pose);
        Modulate = Colors.White;
    }

    public void Initialize(string asset, string? gazeAsset = null)
    {
        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;
        TextureFilter = TextureFilterEnum.Nearest;
        MouseFilter = MouseFilterEnum.Ignore;
        var viewport = new SubViewport { Size = new Vector2I(192, 128), Disable3D = true,
            TransparentBg = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
        AddChild(viewport);
        _poses = Frames(GD.Load<Texture2D>(asset));
        if (gazeAsset is not null) _gazePoses = Frames(GD.Load<Texture2D>(gazeAsset));
        _frame = new TextureRect { Size = new Vector2(192, 128), ExpandMode = ExpandModeEnum.IgnoreSize,
            StretchMode = StretchModeEnum.Scale, TextureFilter = TextureFilterEnum.Linear,
            Material = _material = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/portrait-palette-v05.gdshader") } };
        // A single seated body remains fixed across speech and gaze changes.
        // Separate illustrated gesture sheets are unsuitable as animation in-betweens.
        _continuous = gazeAsset is not null;
        if (_continuous)
        {
            _frame.Texture = GD.Load<Texture2D>(gazeAsset!);
            _material.SetShaderParameter("portrait_atlas", _frame.Texture);
        }
        _material.SetShaderParameter("continuous_portrait", _continuous);
        viewport.AddChild(_frame);
        Texture = viewport.GetTexture();
        SetFrame(0, false, true);
    }

    private static AtlasTexture[] Frames(Texture2D atlas)
    {
        var cell = atlas.GetSize() / new Vector2(4, 2);
        return Enumerable.Range(0, 8).Select(i => new AtlasTexture { Atlas = atlas,
            Region = new Rect2(new Vector2(i % 4, i / 4) * cell, cell), FilterClip = true }).ToArray();
    }

    public void SetFrame(int pose, bool speaking, bool active)
    {
        if (UsesAnimation) return;
        _pose = Math.Clamp(pose, 0, 3); _speaking = speaking;
        UpdateFrame();
        Modulate = Colors.White;
    }

    public void SetGaze(PortraitGaze gaze)
    {
        if (UsesAnimation) return;
        Gaze = gaze;
        UpdateFrame();
    }

    private void UpdateFrame()
    {
        if (_continuous) return;
        var directed = Gaze != PortraitGaze.Board && _gazePoses is not null;
        var frame = (directed ? (int)Gaze : _pose) + (_speaking ? 4 : 0);
        var key = frame + (directed ? 16 : 0);
        if (_index == key) return;
        _frame.Texture = directed ? _gazePoses![frame] : _poses[frame];
        _index = key;
    }

    public override void _Process(double delta)
    {
        if (!_continuous) return;
        // Finish a turn before accepting the next one, so rapid state changes cannot snap.
        if (_gazeBlend >= 1 && _gazeTo != (int)Gaze)
        {
            _gazeFrom = _gazeTo; _gazeTo = (int)Gaze; _gazeBlend = 0;
        }
        _gazeBlend = Math.Min(1, _gazeBlend + (float)delta / .22f);
        _mouth = Mathf.MoveToward(_mouth, _speaking ? 1 : 0, (float)delta / .07f);
        _material.SetShaderParameter("gaze_from", (float)_gazeFrom);
        _material.SetShaderParameter("gaze_to", (float)_gazeTo);
        _material.SetShaderParameter("gaze_blend", Mathf.SmoothStep(0, 1, _gazeBlend));
        _material.SetShaderParameter("mouth_open", _mouth);
    }

    public override void _Notification(int what)
    {
        if (what != NotificationPredelete) return;
        if (_poses is not null) foreach (var pose in _poses) pose.Dispose();
        if (_gazePoses is not null) foreach (var pose in _gazePoses) pose.Dispose();
    }
}

internal static class PresenterMotion
{
    // Real time, independent of the simulation's x1..x20 clock.
    public static (PortraitGaze Gerard, PortraitGaze Rene) At(double time, int speaker, float speechAge, bool speaking)
    {
        if (speaking && speechAge >= (speaker == 1 ? .8f : 1.1f) && speechAge < 1.8f)
            return (PortraitGaze.Partner, PortraitGaze.Partner);
        var phase = time % 18;
        if (phase >= 9.2 && phase < 10.4) return (PortraitGaze.Partner, PortraitGaze.Partner);
        var gerard = phase >= 6 && phase < 6.8 ? PortraitGaze.Outer
            : phase >= 12 && phase < 13.2 ? PortraitGaze.Across : PortraitGaze.Board;
        var rene = phase >= 7.1 && phase < 7.9 ? PortraitGaze.Outer
            : phase >= 13.5 && phase < 14.7 ? PortraitGaze.Across : PortraitGaze.Board;
        return (gerard, rene);
    }
}
