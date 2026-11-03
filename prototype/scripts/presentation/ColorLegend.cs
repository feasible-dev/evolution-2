using Godot;
using static EvolutionTwo.Presentation.BroadcastUi;

namespace EvolutionTwo.Presentation;

public partial class ColorLegend : Control
{
    public override void _Draw()
    {
        var font = GetThemeDefaultFont();
        DrawRect(new Rect2(0, 8, 15, 15), Colors.White);
        DrawString(font, new Vector2(25, 23), L10n.Text("legend.always"), fontSize: 16, modulate: Muted);
        var x = 265f;
        foreach (var d in BehaviorCatalog.All)
        {
            DrawRect(new Rect2(x, 8, 15, 15), d.Color);
            DrawString(font, new Vector2(x + 24, 23), d.Name, fontSize: 16, modulate: Paper);
            x += d.Id == BehaviorChoice.Target ? 225 : 170;
        }
        DrawRect(new Rect2(x, 8, 15, 15), Muted, false, 2);
        DrawString(font, new Vector2(x + 25, 23), L10n.Text("plan.no_rule"), fontSize: 15, modulate: Muted);
        DrawString(font, new Vector2(x + 190, 23), L10n.Text("legend.key"), fontSize: 14, modulate: Muted);
    }
}
