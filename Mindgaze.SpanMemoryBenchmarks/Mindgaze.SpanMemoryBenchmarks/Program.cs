using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Mindgaze.SpanMemoryBenchmarks
{
    class Program
    {
        private static long GetUserMemory()
        {
            // In MB
            return Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;
        }

        private static void ExecuteMemoryOperation(String operationName, Action operation)
        {
            var memoryBefore = GetUserMemory();
            operation();
            var memoryAfter = GetUserMemory();

            Console.WriteLine($"Operation {operationName} needed {memoryAfter - memoryBefore}MB of memory!");
        }

        private static string GenerateBigString(int length)
        {
            var sb = new StringBuilder(length);

            foreach (var i in Enumerable.Range(0, length))
            {
                sb.Append((char)('A' + i % 26));
            }

            return sb.ToString();
        }

        private static string GenerateCommaSeparatedNumbers(int length)
        {
            var sb = new StringBuilder(length);

            foreach (var i in Enumerable.Range(0, length))
            {
                sb.Append(i);

                if (i < length - 1)
                {
                    sb.Append(',');
                }
            }

            return sb.ToString();
        }

        static void Main(string[] args)
        {
            var bigString = GenerateBigString(1000000);

            ExecuteMemoryOperation("Substring NOT optimized", () =>
            {
                var substring1 = bigString.Substring(0, bigString.Length / 2);
                var substring2 = bigString.Substring(bigString.Length / 2, bigString.Length / 2);
                var substring3 = substring1.Substring(0, substring1.Length / 2);
                var substring4 = substring1.Substring(substring2.Length / 2, substring2.Length / 2);
                var substring5 = substring2.Substring(0, substring2.Length / 2);
                var substring6 = substring2.Substring(substring2.Length / 2, substring2.Length / 2);

                // Perform some operations ...
            });

            ExecuteMemoryOperation("Substring optimized", () =>
            {
                var bigSpan = bigString.AsSpan();

                var subspan1 = bigSpan.Slice(0, bigSpan.Length / 2);
                var subspan2 = bigSpan.Slice(bigSpan.Length / 2, bigSpan.Length / 2);
                var subspan3 = subspan1.Slice(0, subspan1.Length / 2);
                var subspan4 = subspan1.Slice(subspan1.Length / 2, subspan1.Length / 2);
                var subspan5 = subspan2.Slice(0, subspan2.Length / 2);
                var subspan6 = subspan2.Slice(subspan2.Length / 2, subspan2.Length / 2);

                // Perform some operations ...
            });

            var commaSeparatedNumbers = GenerateCommaSeparatedNumbers(1000000);

            ExecuteMemoryOperation("Split NOT optimized", () =>
            {
                foreach (var token in commaSeparatedNumbers.Split(','))
                {
                    int.Parse(token);
                }
            });

            ExecuteMemoryOperation("Split optimized", () =>
            {
                var span = commaSeparatedNumbers.AsSpan();
                var index = -1;

                do
                {
                    index = span.IndexOf(',');
                    if (index >= 0)
                    {
                        var slicedToken = span.Slice(0, index);
                        int.Parse(slicedToken);
                        span = span.Slice(index + 1);
                    }
                    else
                    {
                        int.Parse(span);
                    }
                }
                while (index >= 0);
            });

            Console.ReadLine();
        }
    }
}
