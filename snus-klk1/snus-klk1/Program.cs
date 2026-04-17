using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using snus_klk1;
using snus_klk1.model;
using snus_klk1.service;

class Program
{
    static void Main(string[] args)
    {
        var config = XMLParser.Parse("SystemConfig.xml");
        int workerCount = Config.workerCount;
        int maxQueueSize = Config.maxQueueSize;
        if (Config.setupFromFile)
        {
            workerCount = (int)config["WorkerCount"];
            maxQueueSize = (int)config["MaxQueueSize"];
        }
        List<Job> jobs = (List<Job>)config["Jobs"];
        var queue = new ConcurrentPriorityQueue(maxQueueSize);
        var system = new ProcessingSystem(queue, workerCount, 60);
        system.JobCompleted += async (sender, e) =>
        {
            await Logger.LogAsync("COMPLETED", e.JobId, e.Result.ToString());
        };
        system.JobFailed += async (sender, e) =>
        {
            await Logger.LogAsync("ABORT", e.JobId, "FAILED");
        };
        var producer = new JobProducer(workerCount, queue, system, 100000000, 3000, 10);
        _ = Task.Run(() => producer.ParallelProduceJobs());
        foreach (var job in jobs)
        {
            system.Submit(job);
        }

        Console.WriteLine("System running... press ENTER to exit");
        Console.ReadLine();
    }
}