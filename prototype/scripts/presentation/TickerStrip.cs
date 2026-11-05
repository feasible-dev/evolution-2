using System.Text.Json;
using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>A stationary LED lattice. Only each bulb's sampled color changes.</summary>
public partial class TickerStrip : Control
{
    private const string AlphabetRoot = "res://assets/broadcast-v38/";
    private string _message = "", _characters = "";
    private Image? _alphabet;
    private ImageTexture? _messageTexture;
    private ImageTexture? _whiteTexture;
    private ShaderMaterial? _ledMaterial;
    private TextureRect _panel = null!;
    private int _columns, _cellWidth, _cellHeight;
    private Color[] _palette = [];
    private double _time;
    private MarqueePalette _paletteMode;
    private bool _centered;
    public float Impact { get; set; }
    public void SetCue(MarqueeCue cue, bool restart = true)
    {
        _message = cue.Text ?? ""; _paletteMode = cue.Palette; _centered = cue.Centered;
        if (restart) _time = 0;
        RebuildMessage();
    }
    public double EffectPhase { get; set; }
    public const float DotPitch = 2.5f;
    public static int ScrollColumn(double seconds) => (int)Math.Floor(seconds * 28 / DotPitch);
    public string Message
    {
        get => _message;
        set { if (_message == value) return; _message = value; RebuildMessage(); }
    }

    public override void _Ready()
    {
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Ignore;
        _alphabet = GD.Load<Texture2D>(AlphabetRoot + "ticker-alphabet.png").GetImage();
        using var json = JsonDocument.Parse(Godot.FileAccess.GetFileAsString(AlphabetRoot + "ticker-alphabet.json"));
        var data = json.RootElement;
        _characters = data.GetProperty("characters").GetString()!;
        _columns = data.GetProperty("columns").GetInt32();
        _cellWidth = data.GetProperty("cellWidth").GetInt32();
        _cellHeight = data.GetProperty("cellHeight").GetInt32();
        _palette = [new Color("ff941f"), new Color("369dff"), new Color("f52824")];
        using var white = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        white.Fill(Colors.White);
        _whiteTexture = ImageTexture.CreateFromImage(white);
        _ledMaterial = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/broadcast-v40/led-matrix.gdshader") };
        _panel = new TextureRect
        {
            Texture = _whiteTexture, Material = _ledMaterial,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore
        };
        AddChild(_panel);
        RebuildMessage();
    }

    private void RebuildMessage()
    {
        if (_alphabet is null || _ledMaterial is null) return;
        var text = _message.ToUpperInvariant() + (_centered ? "" : "   •   ");
        if (text.Length == 0) text = " ";
        using var raster = Image.CreateEmpty(text.Length * _cellWidth, _cellHeight, false, Image.Format.Rgba8);
        raster.Fill(Colors.Transparent);
        var word = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (i > 0 && text[i - 1] == ' ' && text[i] != ' ') word++;
            var glyph = _characters.IndexOf(text[i]);
            if (glyph < 0) glyph = _characters.IndexOf('?');
            var color = _palette[_paletteMode switch
            {
                MarqueePalette.Orange => 0, MarqueePalette.Blue => 1, MarqueePalette.Red => 2, _ => word % 2
            }];
            for (var y = 0; y < _cellHeight; y++)
                for (var x = 0; x < _cellWidth; x++)
                    if (_alphabet.GetPixel(glyph % _columns * _cellWidth + x, glyph / _columns * _cellHeight + y).A > .5f)
                        raster.SetPixel(i * _cellWidth + x, y, color);
        }
        var previous = _messageTexture;
        _messageTexture = ImageTexture.CreateFromImage(raster);
        _ledMaterial.SetShaderParameter("message_texture", _messageTexture);
        _ledMaterial.SetShaderParameter("message_size", new Vector2(raster.GetWidth(), raster.GetHeight()));
        previous?.Dispose();
    }

    public override void _Process(double delta)
    {
        _time += delta;
        if (_ledMaterial is null) return;
        _panel.Size = Size;
        _ledMaterial.SetShaderParameter("panel_size", Size);
        _ledMaterial.SetShaderParameter("pitch", DotPitch);
        _ledMaterial.SetShaderParameter("scroll_columns", (float)ScrollColumn(_time));
        _ledMaterial.SetShaderParameter("effect_time", (float)(_time + EffectPhase));
        var fit = MathF.Min(1.38f, Size.X / MathF.Max(1, _message.Length * _cellWidth * DotPitch));
        _ledMaterial.SetShaderParameter("glyph_scale", _centered ? MathF.Max(.65f, 1 + (fit - 1) * Impact) : 1f);
        _ledMaterial.SetShaderParameter("bold_amount", _centered ? 1f : 0f);
        _ledMaterial.SetShaderParameter("centered", _centered);
        _ledMaterial.SetShaderParameter("red_amount", Impact);
    }

    public override void _ExitTree()
    {
        _alphabet?.Dispose();
        _messageTexture?.Dispose();
        _whiteTexture?.Dispose();
        _ledMaterial?.Dispose();
    }
}
