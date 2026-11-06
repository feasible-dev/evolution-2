using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>One continuous studio image behind the analysts and the right column.</summary>
public partial class StudioBackdrop : Control
{
    private Texture2D _rear = null!;
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        _rear = GD.Load<Texture2D>("res://assets/studio-v49/warehouse.png");
        Resized += QueueRedraw;
    }
    public override void _Draw()
    {
        if (_rear is null) return;
        // A single panoramic surface keeps the perspective continuous across columns.
        DrawTextureRect(_rear, new Rect2(Vector2.Zero, Size), false);
    }
}
