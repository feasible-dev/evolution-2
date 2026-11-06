namespace EvolutionTwo.Presentation;

public enum MarqueeMotion { Scroll, Hold, StopFlash, Burst }
public readonly record struct MarqueeFrame(int ScrollColumns, bool Centered, bool Repeat,
    float TextVisibility = 1, float BurstProgress = -1);

/// <summary>Real-time bus/stadium sign choreography on a stationary dot grid.</summary>
public static class MarqueePerformance
{
    public const double StopAt = 2.4, FlashAt = 3.5, FlashPeriod = .6, ResumeAt = 5.7;
    public const int FlashCount = 3;
    public static double MinimumDuration(MarqueeMotion motion) => motion switch
    {
        MarqueeMotion.StopFlash => 9, MarqueeMotion.Hold => 6,
        MarqueeMotion.Burst => 4.2, _ => 7
    };

    public static MarqueeFrame Sample(MarqueeMotion motion, double age, int messageColumns, int panelColumns)
    {
        age = Math.Max(0, age);
        var center = (int)Math.Floor((messageColumns - panelColumns) * .5);
        switch (motion)
        {
            case MarqueeMotion.Hold:
                return new(0,true,false);
            case MarqueeMotion.Burst:
                return new(0,true,false,age < .65 ? 0 : 1,age < .65 ? (float)(age/.65) : -1);
            case MarqueeMotion.StopFlash:
                var offset = age < StopAt ? center - TickerStrip.ScrollColumn(StopAt) + TickerStrip.ScrollColumn(age)
                    : age < ResumeAt ? center : center + TickerStrip.ScrollColumn(age - ResumeAt);
                var visibility = 1f;
                if (age >= FlashAt && age < FlashAt + FlashPeriod * FlashCount)
                {
                    var phase = (age - FlashAt) % FlashPeriod;
                    visibility = phase >= .18 && phase < .36 ? 1 : 0;
                }
                return new(offset,false,false,visibility);
            default:
                return new(TickerStrip.ScrollColumn(age),false,true);
        }
    }
}
