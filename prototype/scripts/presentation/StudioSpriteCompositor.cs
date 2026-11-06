using Godot;

namespace EvolutionTwo.Presentation;

/// <summary>Removes only edge-connected matte pixels when loading a sprite sheet.
/// Original RGB values and the source assets remain untouched.</summary>
internal static class StudioSpriteCompositor
{
    public static ImageTexture Create(Texture2D source, int tileWidth, int tileHeight, int matteTolerance = 0,
        IReadOnlySet<int>? mattePalette = null)
    {
        using var image = source.GetImage();
        image.Convert(Image.Format.Rgba8);
        var width = image.GetWidth();
        var height = image.GetHeight();
        var pixels = image.GetData();
        for (var oy = 0; oy < height; oy += tileHeight)
        for (var ox = 0; ox < width; ox += tileWidth)
        {
            var key = (oy * width + ox) * 4;
            var red = pixels[key]; var green = pixels[key + 1]; var blue = pixels[key + 2];
            var visited = new bool[tileWidth * tileHeight];
            var queue = new Queue<(int X, int Y)>();
            void Visit(int x, int y)
            {
                if (x < 0 || x >= tileWidth || y < 0 || y >= tileHeight) return;
                var local = y * tileWidth + x;
                if (visited[local]) return;
                visited[local] = true;
                var index = ((oy + y) * width + ox + x) * 4;
                var paletteMatch = mattePalette?.Contains((pixels[index] << 16) | (pixels[index + 1] << 8) | pixels[index + 2]) == true;
                if (!paletteMatch && (Math.Abs(pixels[index] - red) > matteTolerance ||
                    Math.Abs(pixels[index + 1] - green) > matteTolerance ||
                    Math.Abs(pixels[index + 2] - blue) > matteTolerance)) return;
                pixels[index + 3] = 0;
                queue.Enqueue((x, y));
            }
            for (var x = 0; x < tileWidth; x++) { Visit(x, 0); Visit(x, tileHeight - 1); }
            for (var y = 0; y < tileHeight; y++) { Visit(0, y); Visit(tileWidth - 1, y); }
            while (queue.TryDequeue(out var p))
            {
                Visit(p.X - 1, p.Y); Visit(p.X + 1, p.Y);
                Visit(p.X, p.Y - 1); Visit(p.X, p.Y + 1);
            }
        }
        using var cutout = Image.CreateFromData(width, height, false, Image.Format.Rgba8, pixels);
        return ImageTexture.CreateFromImage(cutout);
    }
}
