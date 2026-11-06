using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>The warehouse and a vertical gallery fascia, with undistorted hardware.</summary>
public partial class StudioBackdrop : Control
{
    public const float FasciaTop = 150;
    public const float FasciaHeight = 70;
    public const float ArenaTop = 224;
    private const float SourceHeight = 126;
    private const float EndWidth = 220;
    public bool ForegroundOnly { get; set; }
    private Texture2D _rear = null!;

    public static float SideWidth(float width) => (width - MathF.Min(width * .70f, 1008)) / 2;

    public static Rect2 MatrixRect(float width)
    {
        var scale = FasciaHeight / SourceHeight;
        var side = SideWidth(width);
        var inset = (496 - 310) * scale;
        return new Rect2(side + inset, FasciaTop + 32 * scale,
            width - side * 2 - inset - (1740 - 1550) * scale, 68 * scale);
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        TextureFilter = TextureFilterEnum.Nearest;
        _rear = GD.Load<Texture2D>("res://assets/studio-v50/press-gallery.png");
        Resized += QueueRedraw;
    }

    private void DrawRegion(Rect2 destination, Rect2 source)
    {
        // Coordinates are authored on the 2048-wide review image; the generated
        // source may have a different resolution. Apply one factor to both axes.
        var sourceScale = _rear.GetWidth() / 2048f;
        DrawTextureRectRegion(_rear, destination, new Rect2(source.Position * sourceScale, source.Size * sourceScale));
    }

    public override void _Draw()
    {
        if (_rear is null) return;
        if (!ForegroundOnly)
        {
            // Crop above the old tabletop. Uniform scaling preserves lamps,
            // columns and bolts at every window size.
            var scale = MathF.Max(Size.X / 2048, Size.Y / 389);
            var sourceSize = Size / scale;
            DrawRegion( new Rect2(Vector2.Zero, Size),
                new Rect2((2048 - sourceSize.X) / 2, 0, sourceSize.X, sourceSize.Y));
            return;
        }

        // A vertical front masks the bottom of the hands. Both rigid end caps
        // scale uniformly; only the unadorned horizontal middle changes length.
        var side = SideWidth(Size.X);
        var scaleEnd = FasciaHeight / SourceHeight;
        var end = EndWidth * scaleEnd;
        var middle = Size.X - 2 * side - 2 * end;
        DrawRegion( new Rect2(side, FasciaTop, end, FasciaHeight),
            new Rect2(310, 496, EndWidth, SourceHeight));
        DrawRegion( new Rect2(side + end, FasciaTop, middle, FasciaHeight),
            new Rect2(310 + EndWidth, 496, 1430 - 2 * EndWidth, SourceHeight));
        DrawRegion( new Rect2(Size.X - side - end, FasciaTop, end, FasciaHeight),
            new Rect2(1740 - EndWidth, 496, EndWidth, SourceHeight));
    }
}
