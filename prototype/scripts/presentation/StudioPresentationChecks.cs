using Godot;

namespace EvolutionTwo.Presentation;

internal static class StudioPresentationChecks
{
    public static void Run(BroadcastScene scene)
    {
        var checks = 0;
        void Check(bool condition, string description)
        {
            if (!condition) throw new InvalidOperationException("Studio: " + description);
            checks++;
        }
        using (var source = Image.CreateEmpty(6, 6, false, Image.Format.Rgba8))
        {
            source.Fill(new Color("04132d"));
            for (var y = 1; y < 5; y++) for (var x = 1; x < 5; x++) source.SetPixel(x, y, Colors.Beige);
            source.SetPixel(3, 3, new Color("04132d"));
            using var texture = ImageTexture.CreateFromImage(source);
            using var sprite = StudioSpriteCompositor.Create(texture, 6, 6);
            using var cutout = sprite.GetImage();
            Check(cutout.GetPixel(0, 0).A == 0, "external matte becomes transparent");
            Check(cutout.GetPixel(3, 3).A == 1, "enclosed same-color facial/clothing pixels remain opaque");
            Check(cutout.GetPixel(1, 1) == source.GetPixel(1, 1), "foreground color is unchanged");
            Check(source.GetPixel(0, 0).A == 1, "source pixels remain intact");
            source.SetPixel(0, 1, new Color("081423"));
            using var mixedTexture = ImageTexture.CreateFromImage(source);
            using var mixedSprite = StudioSpriteCompositor.Create(mixedTexture, 6, 6,
                mattePalette: new HashSet<int> { 0x04132d, 0x081423 });
            using var mixedImage = mixedSprite.GetImage();
            Check(mixedImage.GetPixel(0, 1).A == 0 && mixedImage.GetPixel(3, 3).A == 1,
                "mixed legacy mattes clear outside while enclosed shadows survive");
        }
        using var assets = new BroadcastAnimationAssets();
        foreach (var actor in new[] { 0, 1 })
        foreach (var pose in new[] { new BroadcastPortraitFrame("conversation", 0, PortraitGaze.Board),
            new("stand", 3, PortraitGaze.Board), new("stand", 8, PortraitGaze.Board), new("slap", 4, PortraitGaze.Board),
            new("rink", 2, PortraitGaze.Board), new("excited", 2, PortraitGaze.Partner) })
        {
            using var original = assets.Get(actor, pose).GetImage();
            original.Convert(Image.Format.Rgba8);
            using var cutout = assets.GetCutout(actor, pose).GetImage();
            var source = original.GetData(); var shown = cutout.GetData();
            var identicalColor = source.Length == shown.Length;
            var cleared = 0; var opaque = 0;
            for (var i = 0; i < source.Length; i += 4)
            {
                identicalColor &= source[i] == shown[i] && source[i + 1] == shown[i + 1] && source[i + 2] == shown[i + 2];
                if (shown[i + 3] == 0) cleared++; else opaque++;
            }
            Check(identicalColor, "sprite RGB, proportions and native dimensions are preserved: " + actor + "/" + pose.Clip);
            Check(cleared > 100 && opaque > 1000, "real sprite silhouette survives matte removal: " + actor + "/" + pose.Clip);
        }
        var hold = SpeechBubble.ReadingSeconds(160);
        using (var nico = scene.OpponentIdentity.DisplayedPortrait!.GetImage())
        {
            Check(nico.GetPixel(0, 64).A == 0 && nico.GetPixel(127, 64).A == 0,
                "Nico's varying matte is transparent on both sides");
            Check(nico.GetPixel(64, 64).A == 1, "Nico's face remains opaque");
        }
        Check(hold > SpeechBubble.ReadingSeconds(30), "longer commentary gets more reading time");
        Check(SpeechBubble.Opacity(hold, 160) == 1, "bubble remains fully readable through its hold");
        Check(SpeechBubble.Opacity(hold + .3, 160) is > 0 and < 1, "bubble fades instead of vanishing abruptly");
        Check(SpeechBubble.Opacity(hold + 1, 160) == 0, "stale commentary reveals the studio");
        Check(scene.BoardArea.Position.Y == 244, "studio decoration does not move the arena down");
        Check(TeamPalette.Opponent == new Color("f58b23") && TeamPalette.Us == new Color("24d0ff"), "team palette is orange versus cyan");
        GD.Print($"STUDIO PRESENTATION PASS: {checks} alpha, timing and layout checks");
    }
}
