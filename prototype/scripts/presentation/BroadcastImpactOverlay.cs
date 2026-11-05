using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>One brief red color wash, with a smooth return and no repeated flashing.</summary>
public partial class BroadcastImpactOverlay : ColorRect
{
    public ArenaBillboard Billboard { get; set; } = null!;
    private ShaderMaterial _effect = null!;
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Material = _effect = new ShaderMaterial { Shader = GD.Load<Shader>("res://assets/broadcast-v40/red-impact.gdshader") };
    }
    public override void _Process(double delta)
    {
        Size = GetViewportRect().Size;
        var impact = Billboard.Visible ? Billboard.RedImpact : 0;
        Visible = impact > 0;
        _effect.SetShaderParameter("strength", impact);
    }
    public override void _ExitTree() => _effect?.Dispose();
}
