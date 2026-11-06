using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>One uninterrupted physical matrix with a rotating broadcast program.</summary>
public partial class ArenaBillboard : Control
{
    public LevelOneSession Session { get; set; } = null!;
    public MarqueeProgram Program { get; } = new();
    public float RedImpact => Program.Impact;
    public TickerStrip Strip { get; private set; } = null!;
    private int _revision = -1;
    private int _sequence = -1;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Strip = new TickerStrip();
        AddChild(Strip);
        Resized += Arrange;
        Arrange();
    }

    public void Arrange()
    {
        // Share the recess geometry with the foreground renderer.
        var width = GetParent<Control>().Size.X;
        var recess = StudioBackdrop.MatrixRect(width);
        Strip.Position = recess.Position - Position;
        Strip.Size = recess.Size;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        Program.Update(delta, Session, Strip.Size.X);
        if (_revision != Program.Revision)
        {
            _revision = Program.Revision;
            Strip.SetCue(Program.Cue, _sequence != Program.Sequence);
            _sequence = Program.Sequence;
        }
        Strip.Impact = Program.Impact;
    }
}
