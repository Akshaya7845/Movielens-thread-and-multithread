using System;
using System.Collections.Generic;
using System.IO;

namespace MovieLensMVC
{
    public class ReportView
    {
        public static void DisplayReportSummary(string title, double seconds)
        {
            Console.WriteLine($"{title} completed in {seconds:F2} seconds");
        }

        public static void SaveReportsToCsv(string folder, Dictionary<string, List<(string Movie, double Avg)>> reports)
        {
            Directory.CreateDirectory(folder);

            foreach (var kv in reports)
            {
                string filePath = Path.Combine(folder, $"{kv.Key}_Top10.csv");
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Rank,Movie,AverageRating");
                    int rank = 1;
                    foreach (var item in kv.Value)
                    {
                        writer.WriteLine($"{rank},{item.Movie},{item.Avg:F2}");
                        rank++;
                    }
                }
            }
        }

    }
}
