using ConsoleApp.ConsoleObject.Table;
using ConsoleApp.ConsoleObject.Table.TableRows;
using Laba2.ConsoleObject.Table.TableRows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

public class Program
{
    static BigInteger[] cache;
    static bool[] cached;

    static ITableRow Row(params object[] cells)
    {
        return new TableRow(new List<object>(cells));
    }

    static long Factorial(int n)
    {
        return n == 0 ? 1 : n * Factorial(n - 1);
    }

    static long SumTo(int n)
    {
        return n == 0 ? 0 : n + SumTo(n - 1);
    }

    static int DigitSum(long n)
    {
        n = Math.Abs(n);

        return n < 10 ? (int)n : (int)(n % 10) + DigitSum(n / 10);
    }

    static int DigitalRoot(long n)
    {
        int s = DigitSum(n);

        return s < 10 ? s : DigitalRoot(s);
    }

    static void PrintUp(int n)
    {
        if (n > 1)
        {
            PrintUp(n - 1);
        }

        Console.Write(n + " ");
    }

    static void PrintDown(int n)
    {
        if (n < 1)
        {
            return;
        }

        Console.Write(n + " ");
        PrintDown(n - 1);
    }

    static void PrintMinusNToN(int cur, int n)
    {
        if (cur > n)
        {
            return;
        }

        Console.Write(cur + " ");
        PrintMinusNToN(cur + 1, n);
    }

    static void PrintNToMinusN(int cur, int n)
    {
        if (cur < -n)
        {
            return;
        }

        Console.Write(cur + " ");
        PrintNToMinusN(cur - 1, n);
    }

    static void PrintEven(int cur, int n)
    {
        if (cur > n)
        {
            return;
        }

        Console.Write(cur + " ");
        PrintEven(cur + 2, n);
    }

    static void PrintOdd(int cur, int n)
    {
        if (cur > n)
        {
            return;
        }

        Console.Write(cur + " ");
        PrintOdd(cur + 2, n);
    }

    static long Power(long x, int n)
    {
        return n == 0 ? 1 : x * Power(x, n - 1);
    }

    static long GcdRec(long a, long b)
    {
        return b == 0 ? a : GcdRec(b, a % b);
    }

    static BigInteger FibBasic(int n)
    {
        return n < 2 ? n : FibBasic(n - 1) + FibBasic(n - 2);
    }

    static void ResetCache(int n)
    {
        cache = new BigInteger[Math.Max(n + 1, 2)];
        cached = new bool[cache.Length];
        cache[0] = 0;
        cache[1] = 1;
        cached[0] = true;
        cached[1] = true;
    }

    static BigInteger FibCacheRec(int n)
    {
        if (!cached[n])
        {
            cache[n] = FibCacheRec(n - 1) + FibCacheRec(n - 2);
            cached[n] = true;
        }

        return cache[n];
    }

    static BigInteger FibCache(int n)
    {
        ResetCache(n);

        return FibCacheRec(n);
    }

    static BigInteger FibArray(int n)
    {
        BigInteger[] fibs = new BigInteger[Math.Max(n + 1, 2)];
        fibs[0] = 0;
        fibs[1] = 1;

        for (int i = 2; i <= n; i++)
        {
            fibs[i] = fibs[i - 1] + fibs[i - 2];
        }

        return fibs[n];
    }

    static BigInteger FibOnTheFly(int n)
    {
        if (n < 2)
        {
            return n;
        }

        BigInteger f0 = 0;
        BigInteger f1 = 1;

        for (int i = 2; i <= n; i++)
        {
            BigInteger f2 = f0 + f1;
            f0 = f1;
            f1 = f2;
        }

        return f1;
    }

    static string Measure(Func<int, BigInteger> func, int n, int trials)
    {
        Stopwatch sw = Stopwatch.StartNew();

        for (int t = 0; t < trials; t++)
        {
            func(n);
        }

        sw.Stop();

        return (sw.Elapsed.TotalMilliseconds / trials).ToString("F4");
    }

    static string MeasureLimited(Func<int, BigInteger> func, int n, int maxN, int trials)
    {
        if (n > maxN)
        {
            return "-";
        }

        return Measure(func, n, trials);
    }

    static string MeasureMemory(Func<int, BigInteger> func, int n, int maxN)
    {
        if (n > maxN)
        {
            return "-";
        }

        GC.Collect();
        long before = GC.GetAllocatedBytesForCurrentThread();
        func(n);
        long after = GC.GetAllocatedBytesForCurrentThread();

        return ((after - before) / 1024.0).ToString("F2");
    }

    static ITableRow MaxNRow(string name, Func<int, BigInteger> func, int start, bool doubling, int maxN, double limitMs)
    {
        int best = 0;
        double bestMs = 0;
        bool exceeded = false;

        for (int n = start; n <= maxN; n = doubling ? n * 2 : n + 1)
        {
            Stopwatch sw = Stopwatch.StartNew();
            func(n);
            sw.Stop();

            if (sw.Elapsed.TotalMilliseconds > limitMs)
            {
                exceeded = true;
                break;
            }

            best = n;
            bestMs = sw.Elapsed.TotalMilliseconds;
        }

        string result = exceeded ? best.ToString() : ">= " + best + " (предел поиска)";

        return Row(name, result, bestMs.ToString("F4"));
    }

    static string MeasurePeak(Func<int, BigInteger> func, int n, int maxN)
    {
        if (n > maxN)
        {
            return "-";
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long baseline = GC.GetTotalMemory(true);
        long peak = baseline;
        bool running = true;

        Thread sampler = new Thread(() =>
        {
            while (running)
            {
                long m = GC.GetTotalMemory(false);

                if (m > peak)
                {
                    peak = m;
                }

                Thread.Sleep(1);
            }
        });

        sampler.Start();
        func(n);
        running = false;
        sampler.Join();

        return ((peak - baseline) / 1048576.0).ToString("F2");
    }

    static void Task1()
    {
        Console.WriteLine("\nЗадание 1 (рекурсивные функции)");
        var rows = new List<ITableRow>();
        rows.Add(Row("Функция", "Аргумент", "Результат"));
        rows.Add(Row("Факториал", 10, Factorial(10)));
        rows.Add(Row("Сумма от 1 до N", 100, SumTo(100)));
        rows.Add(Row("Сумма цифр", 987654321, DigitSum(987654321)));
        rows.Add(Row("Цифровой корень", 987654321, DigitalRoot(987654321)));

        new Table("task1", rows).Print();
    }

    static void Task2()
    {
        Console.WriteLine("\nЗадание 2 (рекурсивные процедуры)");

        Console.Write("От 1 до 10:      ");
        PrintUp(10);
        Console.WriteLine();

        Console.Write("От 10 до 1:      ");
        PrintDown(10);
        Console.WriteLine();

        Console.Write("От -5 до 5:      ");
        PrintMinusNToN(-5, 5);
        Console.WriteLine();

        Console.Write("От 5 до -5:      ");
        PrintNToMinusN(5, 5);
        Console.WriteLine();

        Console.Write("Чётные до 20:    ");
        PrintEven(2, 20);
        Console.WriteLine();

        Console.Write("Нечётные до 20:  ");
        PrintOdd(1, 20);
        Console.WriteLine();
    }

    static void Task3()
    {
        Console.WriteLine("\nЗадание 3 (функции от двух аргументов)");
        var rows = new List<ITableRow>();
        rows.Add(Row("Функция", "Аргументы", "Результат"));
        rows.Add(Row("Степень", "3, 5", Power(3, 5)));
        rows.Add(Row("Степень", "2, 10", Power(2, 10)));
        rows.Add(Row("Степень", "5, 2", Power(5, 2)));
        rows.Add(Row("НОД (Евклид)", "48, 18", GcdRec(48, 18)));
        rows.Add(Row("НОД (Евклид)", "1071, 462", GcdRec(1071, 462)));

        new Table("task3", rows).Print();
    }

    static void Task4()
    {
        Console.WriteLine("\nЗадание 4 (проверка: все реализации Фибоначчи дают одинаковый результат)");
        var rows = new List<ITableRow>();
        rows.Add(Row("N", "Без оптимизации", "С кеш-памятью", "С массивом", "\"На лету\"", "Совпадают"));

        int[] sizes = { 1, 3, 7, 20, 30 };

        foreach (int n in sizes)
        {
            BigInteger a = FibBasic(n);
            BigInteger b = FibCache(n);
            BigInteger c = FibArray(n);
            BigInteger d = FibOnTheFly(n);

            rows.Add(Row(n, a, b, c, d, a == b && b == c && c == d ? "да" : "нет"));
        }

        new Table("task4", rows).Print();
    }

    static void Task5()
    {
        Console.WriteLine("\nЗадание 5a (максимальное N за 1 секунду на одно вычисление)");
        var rowsA = new List<ITableRow>();
        rowsA.Add(Row("Реализация", "Макс. N", "Время, мс"));
        rowsA.Add(MaxNRow("Без оптимизации", FibBasic, 1, false, 60, 1000));
        rowsA.Add(MaxNRow("С кеш-памятью", FibCache, 100, true, 102_400, 1000));
        rowsA.Add(MaxNRow("С массивом", FibArray, 100, true, 102_400, 1000));
        rowsA.Add(MaxNRow("\"На лету\"", FibOnTheFly, 100, true, 1_638_400, 1000));

        new Table("task5a", rowsA).Print();

        Console.WriteLine("\nЗадание 5b (время выполнения, мс)");
        var rowsB = new List<ITableRow>();
        rowsB.Add(Row("N", "Без оптимизации", "С кеш-памятью", "С массивом", "\"На лету\""));

        int[] sizes = { 10, 20, 30, 35, 40, 100, 1_000, 5_000, 10_000 };
        int basicLimit = 35;

        foreach (int n in sizes)
        {
            int trials = n <= 1_000 ? 20 : 3;

            rowsB.Add(Row(
                n,
                MeasureLimited(FibBasic, n, basicLimit, n <= 20 ? 20 : 1),
                Measure(FibCache, n, trials),
                Measure(FibArray, n, trials),
                Measure(FibOnTheFly, n, trials)));
        }

        new Table("task5b", rowsB).Print();

        Console.WriteLine("\nЗадание 5c (выделенная память за один вызов, КБ)");
        var rowsC = new List<ITableRow>();
        rowsC.Add(Row("N", "Без оптимизации", "С кеш-памятью", "С массивом", "\"На лету\""));

        foreach (int n in sizes)
        {
            rowsC.Add(Row(
                n,
                MeasureMemory(FibBasic, n, basicLimit),
                MeasureMemory(FibCache, n, int.MaxValue),
                MeasureMemory(FibArray, n, int.MaxValue),
                MeasureMemory(FibOnTheFly, n, int.MaxValue)));
        }

        new Table("task5c", rowsC).Print();

        Console.WriteLine("\nЗадание 5c (пиковая память управляемой кучи во время вычисления, МБ)");
        var rowsP = new List<ITableRow>();
        rowsP.Add(Row("N", "Без оптимизации", "С кеш-памятью", "С массивом", "\"На лету\""));

        int[] peakSizes = { 30, 35, 1_000, 10_000, 50_000, 100_000 };

        foreach (int n in peakSizes)
        {
            rowsP.Add(Row(
                n,
                MeasurePeak(FibBasic, n, basicLimit),
                MeasurePeak(FibCache, n, int.MaxValue),
                MeasurePeak(FibArray, n, int.MaxValue),
                MeasurePeak(FibOnTheFly, n, int.MaxValue)));
        }

        new Table("task5c_peak", rowsP).Print();
    }

    static void RunAll()
    {
        Task1();
        Task2();
        Task3();
        Task4();
        Task5();
    }

    public static void Main(string[] args)
    {
        Thread thread = new Thread(RunAll, 512 * 1024 * 1024);

        thread.Start();
        thread.Join();

        Console.ReadKey();
    }
}