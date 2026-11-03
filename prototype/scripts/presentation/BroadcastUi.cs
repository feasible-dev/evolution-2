using Godot;

namespace EvolutionTwo.Presentation;

internal static class BroadcastUi
{
    public static readonly Color Paper = new("e0edff");
    public static readonly Color Muted = new("82a4cf");
    public static readonly Color Accent = new("37a4ff");
    public static readonly Color Orange = new("ff9d31");
    public static readonly Color Pink = new("ed66ba");
    public static readonly Color Red = new("ff5c61");
    public static readonly Color Green = new("55deaa");
    public static Label Text(Node parent, string text, int size = 18, Color? color = null)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore,
            VerticalAlignment = VerticalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color ?? Paper);
        parent.AddChild(label);
        return label;
    }
    public static Button Button(Node parent, string text, Action pressed, int size = 18)
    {
        var button = new Button { Text = text, AutowrapMode = TextServer.AutowrapMode.Off,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand };
        button.AddThemeFontSizeOverride("font_size", size);
        button.AddThemeColorOverride("font_color", Paper);
        button.AddThemeColorOverride("font_pressed_color", Paper);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeStyleboxOverride("normal", Style("061122", "214a79"));
        button.AddThemeStyleboxOverride("hover", Style("102a4d", "4bb3ff"));
        button.AddThemeStyleboxOverride("pressed", Style("123f70", "63cbff"));
        button.AddThemeStyleboxOverride("disabled", Style("030916", "132b49"));
        button.AddThemeColorOverride("font_disabled_color", new Color("466488"));
        button.AddThemeStyleboxOverride("focus", new StyleBoxFlat { BgColor = Colors.Transparent, BorderColor = Accent,
            BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2 });
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }
    public static StyleBoxFlat Style(string fill, string border) => new()
    {
        BgColor = new Color(fill), BorderColor = new Color(border),
        BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
        ContentMarginLeft = 15, ContentMarginRight = 15, ContentMarginTop = 8, ContentMarginBottom = 8
    };
    public static void Place(Control c, float x, float y, float w, float h)
    {
        c.Position = new Vector2(x, y);
        c.Size = new Vector2(MathF.Max(1, w), MathF.Max(1, h));
    }
}
