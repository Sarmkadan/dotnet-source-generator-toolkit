using BenchmarkDotNet; using System; using System.Collections.Generic; using System.Linq; using System.Text; using System.Threading; using System.Threading.Tasks;

namespace BenchmarkDotNet {
    [MemoryDiagnoser]
    public class ValidationExceptionBenchmarks {
        [GlobalSetup]
        public void Setup() {
            // Set up realistic test data here
        }

        [Benchmark]
        public void Benchmark_ValidationException_PublicMethod1() {
            // Test ValidationException public method 1
        }

        [Benchmark]
        [Params(10, 100, 1000)]
        public void Benchmark_ValidationException_PublicMethod2() {
            // Test ValidationException public method 2 with input size
        }

        [Benchmark]
        public void Benchmark_ValidationException_PublicMethod3() {
            // Test ValidationException public method 3
        }
    }
}