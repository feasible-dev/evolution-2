public sealed class TestHarness
{
    private readonly List<string> _failures = new();

    public void Run(string testName, Action testAction)
    {
        try
        {
            testAction();
            Console.WriteLine($"PASS: {testName}");
        }
        catch (Exception exception)
        {
            _failures.Add($"FAIL: {testName} -> {exception.Message}");
        }
    }

    public int Finish()
    {
        if (_failures.Count == 0)
        {
            Console.WriteLine("All simulation tests passed.");
            return 0;
        }

        foreach (var failure in _failures)
        {
            Console.Error.WriteLine(failure);
        }

        return 1;
    }
}
