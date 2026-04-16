using System;
using System.IO;
using System.Threading.Tasks;
using snus_klk1.model;

internal static class Logger
{
    private static readonly string path = "log.txt";
    private static readonly SemaphoreSlim _mutex = new(1, 1);
    public static async Task LogAsync(string status, Guid jobId, string message)
    {
        string line = $"[{DateTime.Now}] [{status}] {jobId}, {message}\n";

        await _mutex.WaitAsync();
        try
        {
            lock (GlobalMutex.consoleMutex)
            {
                Console.Write(line);
            }
            await File.AppendAllTextAsync(path, line);
        }
        finally
        {
            _mutex.Release();
        }
    }
}