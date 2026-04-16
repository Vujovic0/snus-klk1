using System;
using System.Collections.Generic;
using System.Text;

namespace snus_klk1.service
{
    internal static class JobHandler
    {
        static bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;
            for (int i = 3; i*i <= number; i += 2){
                if (number % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        static int SerialCountPrimes(int start, int end)
        {
            int primesCount = 0;

            for (int i = start; i <= end; i++)
            {
                if (IsPrime(i))
                {
                    primesCount++;
                }
            }

            return primesCount;
        }

        static async Task<int> ParallelCountPrimes(int limit, int threadNum)
        {
            threadNum = Math.Clamp(threadNum, 1, 8);
            int chunkSize = limit / threadNum;
            List<Task<int>> tasks = new();
            for (int i = 0; i < threadNum; i ++)
            {
                int start = i*chunkSize + 1;
                int end = (i+1 == threadNum) ? limit : (i + 1) * chunkSize;
                tasks.Add(Task.Run(() =>
                {
                    return SerialCountPrimes(start, end);
                }));
            }
            int[] results = await Task.WhenAll(tasks);
            int total = 0;

            foreach(int result in results){
                total += result;
            }

            return total;
        }

        static async Task<int> SimulateIO(int ms){
            Thread.Sleep(ms);
            Random rnd = new Random();
            return rnd.Next(101);
        }


        static Dictionary<string, int> ParsePayload(string payload){
            Dictionary<string, int> result = new ();
            if (string.IsNullOrWhiteSpace(payload))
                return result;
            string[] parts = payload.Split(',');
            foreach (var part in parts)
            {
                string[] kvpair = part.Split(':');
                if (kvpair.Length != 2)
                    continue;
                string key = kvpair[0].Trim();
                int value = int.Parse(kvpair[1].Replace("_", ""));
                result[key] = value;
            }

            return result;
        }
    }
}
