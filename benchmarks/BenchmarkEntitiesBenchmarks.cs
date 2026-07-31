[MemoryDiagnoser]
public class BenchmarkEntitiesBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // setup test data in [GlobalSetup]
        var testList = new List<BenchmarkEntity>();
        for (int i = 0; i < 100; i++)
        {
            testList.Add(new BenchmarkEntity());
        }
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // use inputSize to set up test data
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // include a [Params] for input size where relevant
    }
}