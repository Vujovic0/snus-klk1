using System;
using System.Collections.Generic;
using System.Xml.Linq;
using snus_klk1.model;
using snus_klk1.model.enums;

namespace snus_klk1.service
{
    internal class XMLParser
    {
        public static Dictionary<string, object> Parse(string filePath)
        {
            XDocument doc = XDocument.Load(filePath);

            var root = doc.Element("SystemConfig");
            if (root == null)
            {
                throw new KeyNotFoundException();
            }

            int workerCount = int.Parse(root.Element("WorkerCount").Value);
            int maxQueueSize = int.Parse(root.Element("MaxQueueSize").Value);

            List<Job> jobs = new List<Job>();

            foreach (var jobNode in root.Element("Jobs").Elements("Job"))
            {
                JobType type = Enum.Parse<JobType>(jobNode.Attribute("Type").Value.ToUpper());

                string payload = jobNode.Attribute("Payload").Value;

                int priority = int.Parse(jobNode.Attribute("Priority").Value);
                Job job = new(type, payload, priority);
                jobs.Add(job);
                Console.WriteLine(
                    $"[{DateTime.Now}] [PARSED_JOB] {job.Id}, {job.Type}, Priority={priority}, Payload={payload}"
                );
            }

            return new Dictionary<string, object>
            {
                { "WorkerCount", workerCount },
                { "MaxQueueSize", maxQueueSize },
                { "Jobs", jobs }
            };
        }
    }
}