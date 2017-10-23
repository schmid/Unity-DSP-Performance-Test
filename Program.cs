using System;
using System.Diagnostics;
using System.Collections.Generic;

interface ITest
{
    string name { get; }
    void Init();
    float PerformTest(long iterations);
}

class TestMathSin : ITest
{
    public string name { get; }
    public TestMathSin(string name) { this.name = name; }
    public void Init() { }
    public float PerformTest(long iterations)
    {
        double sum = 0.0f;
        double angle_per_iteration = Math.PI * 2.0 / iterations;
        for (long i = 0; i < iterations; ++i)
        {
            double angle = i * angle_per_iteration;
            sum += Math.Sin(angle); // make sure that code isn't optimized away
        }
        return (float)sum;
    }
}

class TestTable : ITest
{
    public string name { get; }
    private float[] sine;
    private int tableSize;

    public TestTable(string name, int tableSize)
    {
        this.name = name;
        this.tableSize = tableSize;
    }
    public void Init()
    {
        sine = new float[tableSize];
        float anglePerIteration = (float)(Math.PI * 2.0 / tableSize);
        for (int i = 0; i < tableSize; ++i)
        {
            sine[i] = (float)Math.Sin(i * anglePerIteration);
        }
    }
    public float PerformTest(long iterations)
    {
        float sum = 0.0f;
        float periods_per_iteration = 0.9999f / iterations;
        for (long i = 0; i < iterations; ++i)
        {
            float periods = i * periods_per_iteration;
            int idx = (int)(periods * tableSize);
            sum += sine[idx];
        }
        return sum;
    }
}

class TestTable_unsafe : ITest
{
    public string name { get; }
    private float[] sine;
    private int tableSize;

    public TestTable_unsafe(string name, int tableSize)
    {
        this.name = name;
        this.tableSize = tableSize;
    }
    public void Init()
    {
        sine = new float[tableSize];
        float anglePerIteration = (float)(Math.PI * 2.0 / tableSize);
        for (int i = 0; i < tableSize; ++i)
        {
            sine[i] = (float)Math.Sin(i * anglePerIteration);
        }
    }
    public float PerformTest(long iterations)
    {
        float sum = 0.0f;
        unsafe
        {
            float periods_per_iteration = 0.9999f / iterations;
            fixed (float* ptr = sine)
            {
                for (long i = 0; i < iterations; ++i)
                {
                    float periods = i * periods_per_iteration;
                    int idx = (int)(periods * tableSize);
                    //sum += ptr[idx];
                    sum += *(ptr +idx);
                }
            }
        }
        return sum;
    }
}

class TestParabolic : ITest
{
    public string name { get; }
    public TestParabolic(string name) { this.name = name; }
    public void Init() { }
    public float PerformTest(long iterations)
    {
        float sum = 0.0f;
        float periods_per_iteration = 1.0f / iterations;
        const float PI2 = (float)(Math.PI * 2.0);
        const float F = (float)(-8.0 * Math.PI);
        for (long i = 0; i < iterations; ++i)
        {
            float p = i * periods_per_iteration;
            float sine = 8 * p + F * p * Math.Abs(p * PI2);
            sum += sine;
        }
        return sum;
    }
}

public class Program
{
    const long ITERATIONS = 48000L * 3000L;
    private Stopwatch stopWatch = new Stopwatch();

    static void Main(string[] args)
    {
        string output = new Program().RunTests();
        Console.WriteLine(output);
    }

    private string Run(ITest test, long iterations)
    {
        string output = string.Empty;
        test.Init();
        output += string.Format("{0}:", test.name);
        stopWatch.Reset();

        stopWatch.Start();
        float results = test.PerformTest(iterations);
        stopWatch.Stop();

        long time_ms = stopWatch.ElapsedMilliseconds;
        float time_s = time_ms * 0.001f;
        // Output sum just make absolutely sure it's not optimized away
        output += string.Format("  {0} sines/smp ({1} iterations) (sum = {2}, stopWatch time = {3} ms)\n", (iterations / 48000) / time_s, iterations, results, time_ms);
        return output;
    }

    private List<ITest> GetTests()
    {
        return new List<ITest> {
            new TestMathSin("Library Sine Test"),
            new TestParabolic("Polynomial Approximation Test"),
            new TestTable("Array Test (2048 samples)", 2048),
            //new TestTable("Array Test (64K samples)", 1 << 16),
            new TestTable("Array Test (16M samples)", 1 << 24),
            new TestTable_unsafe("Array Test [unsafe] (2048 samples)", 2048),
            //new TestTable_unsafe("Array Test [unsafe] (64K samples)", 1 << 16),
            new TestTable_unsafe("Array Test [unsafe] (16M samples)", 1 << 24),
        };
    }

    public string RunTestsFast()
    {
        List<ITest> tests = GetTests();

        string output = string.Empty;
        foreach (var test in tests)
        {
            output += Run(test, 100);
        }
        return output;
    }

    public string RunTests()
    {
        List<ITest> tests = GetTests();

        string output = string.Empty;
        foreach (var test in tests)
        {
            output += Run(test, ITERATIONS);
        }
        return output;
    }
}
