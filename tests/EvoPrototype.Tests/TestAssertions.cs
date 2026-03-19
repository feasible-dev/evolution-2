public static class TestAssertions
{
    public static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void AssertNearlyEqual(float expected, float actual, float tolerance, string message)
    {
        if (MathF.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException($"{message} Expected {expected}, got {actual}.");
        }
    }
}
