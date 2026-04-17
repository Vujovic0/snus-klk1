using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using snus_klk1.model;

namespace snus_klk1.service
{
    internal class Reporter
    {
        private int _reportIndex = 0;
        private const int MaxReports = 10;
        ConcurrentDictionary<Guid, JobRecord> _jobs;

        public Reporter(ConcurrentDictionary<Guid, JobRecord> jobs, int reportDelaySeconds)
        {
            _jobs = jobs;
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(reportDelaySeconds));
                        RotateReport();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
        }

        private List<JobReport> GenerateReport()
        {
            return _jobs.Values
                .GroupBy(j => j.Job.Type)
                .Select(g => new JobReport
                {
                    JobType = g.Key,
                    Count = g.Count(),
                    AvgDurationMs = g.Where(x => x.Success)
                                      .DefaultIfEmpty()
                                      .Average(x => x?.Duration.TotalMilliseconds ?? 0),
                    FailedCount = g.Count(x => !x.Success)
                })
                .OrderByDescending(r => r.JobType)
                .ToList();
        }

        private void SaveReport(List<JobReport> report, int index)
        {
            var doc = new System.Xml.Linq.XDocument(
                new System.Xml.Linq.XElement("Report",
                new System.Xml.Linq.XAttribute("Id", _reportIndex),
                    new System.Xml.Linq.XAttribute("Timestamp", DateTime.Now),
                    report.Select(r =>
                        new System.Xml.Linq.XElement("JobType",
                            new System.Xml.Linq.XAttribute("type", r.JobType),
                            new System.Xml.Linq.XElement("Count", r.Count),
                            new System.Xml.Linq.XElement("AvgDurationMs", r.AvgDurationMs),
                            new System.Xml.Linq.XElement("FailedCount", r.FailedCount)
                        )
                    )
                )
            );

            string file = $"report_{index}.xml";
            doc.Save(file);
        }

        private void RotateReport()
        {
            var report = GenerateReport();

            int fileIndex = _reportIndex % MaxReports;

            SaveReport(report, fileIndex);

            _reportIndex++;
        }
    }
}
