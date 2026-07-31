[MemoryDiagnoser]
public class GenerationStartedEventBenchmarks
{
    [Benchmark]
    public void Benchmark_GenerationStartedEvent_Create()
    {
        // Setup and test data
        var startedEvent = new GenerationStartedEvent();
        startedEvent.Init();
        // Benchmark creation
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var newEvent = new GenerationStartedEvent();
        }
        sw.Stop();
        Console.WriteLine($"Creation took {sw.ElapsedMilliseconds}ms");
    }

    [Benchmark]
    public void Benchmark_GenerationStartedEvent_GetData()
    {
        // Setup and test data
        var startedEvent = new GenerationStartedEvent();
        startedEvent.Init();
        // Benchmark getting data
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            startedEvent.GetData();
        }
        sw.Stop();
        Console.WriteLine($"Getting data took {sw.ElapsedMilliseconds}ms");
    }

    [Benchmark]
    public void Benchmark_GenerationStartedEvent_Process()
    {
        // Setup and test data
        var startedEvent = new GenerationStartedEvent();
        startedEvent.Init();
        // Benchmark processing
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            startedEvent.Process();
        }
        sw.Stop();
        Console.WriteLine($"Processing took {sw.ElapsedMilliseconds}ms");
    }
}