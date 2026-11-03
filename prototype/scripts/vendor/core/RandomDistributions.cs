using System;

namespace EvoPrototype.Core;

/// <summary>
/// Small distribution helpers for bounded and positive parameter sampling.
/// </summary>
public static class RandomDistributions
{
    public static float SampleBeta(Random random, float alpha, float beta)
    {
        var x = SampleGamma(random, alpha);
        var y = SampleGamma(random, beta);
        var sum = x + y;
        if (sum <= 0.00001f)
        {
            return 0.5f;
        }

        return x / sum;
    }

    private static float SampleGamma(Random random, float shape)
    {
        if (shape <= 0.0f)
        {
            return 0.0f;
        }

        if (shape < 1.0f)
        {
            var u = Math.Max((float)random.NextDouble(), 0.00001f);
            return SampleGamma(random, shape + 1.0f) * MathF.Pow(u, 1.0f / shape);
        }

        var d = shape - (1.0f / 3.0f);
        var c = 1.0f / MathF.Sqrt(9.0f * d);

        while (true)
        {
            var x = SampleStandardNormal(random);
            var v = 1.0f + (c * x);
            if (v <= 0.0f)
            {
                continue;
            }

            v = v * v * v;
            var u = (float)random.NextDouble();

            if (u < 1.0f - (0.0331f * x * x * x * x))
            {
                return d * v;
            }

            if (MathF.Log(u) < (0.5f * x * x) + (d * (1.0f - v + MathF.Log(v))))
            {
                return d * v;
            }
        }
    }

    private static float SampleStandardNormal(Random random)
    {
        var u1 = Math.Max((float)random.NextDouble(), 0.00001f);
        var u2 = (float)random.NextDouble();
        return MathF.Sqrt(-2.0f * MathF.Log(u1)) * MathF.Cos(2.0f * MathF.PI * u2);
    }
}
