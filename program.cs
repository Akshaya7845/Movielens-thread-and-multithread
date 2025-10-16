using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MovieLensMVC;

namespace MovieLensMVC
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("--------------- MovieLens 100k Report Generator --------------");

            string basePath = Directory.GetCurrentDirectory();
            string uData = Path.Combine(basePath, "u.data");
            string uItem = Path.Combine(basePath, "u.item");
            string uUser = Path.Combine(basePath, "u.user");

            Console.WriteLine("Loading data...");
            var movies = LoadMovies(uItem);
            var users = LoadUsers(uUser);
            var ratings = LoadRatings(uData);

            var controller = new ReportController(movies, users, ratings);

            var sw = Stopwatch.StartNew();
            var reportsSingle = controller.GenerateReportsSingleThread();
            sw.Stop();
            double singleTime = sw.Elapsed.TotalSeconds;
            ReportView.DisplayReportSummary("Without Threads", singleTime);
            ReportView.SaveReportsToCsv(Path.Combine(basePath, "output_singlethread"), reportsSingle);
            sw.Restart();
            var reportsMulti = controller.GenerateReportsMultiThread();
            sw.Stop();
            double multiTime = sw.Elapsed.TotalSeconds;
            ReportView.DisplayReportSummary("With Threads", multiTime);
            ReportView.SaveReportsToCsv(Path.Combine(basePath, "output_multithread"), reportsMulti);
            Console.WriteLine("\nReports generated successfully!");
        }

        static List<Movie> LoadMovies(string path)
        {
            var list = new List<Movie>();

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 6)
                    continue; 

                string title = parts[1].Trim();
                int id = 0;

                if (!int.TryParse(parts[0], out id))
                    continue;

                var genres = parts.Skip(5)
                                  .Select((v, i) => v == "1" ? GenreName(i) : null)
                                  .Where(g => g != null)
                                  .ToArray();

                list.Add(new Movie
                {
                    Id = id,
                    Title = title,
                    Genres = genres
                });
            }

            Console.WriteLine($"Loaded {list.Count} movies.");
            return list;
        }


        static string GenreName(int index)
        {
            string[] genres = { "unknown", "Action", "Adventure", "Animation", "Children's", "Comedy", "Crime", "Documentary", "Drama", "Fantasy", "Film-Noir", "Horror", "Musical", "Mystery", "Romance", "Sci-Fi", "Thriller", "War", "Western" };
            return index < genres.Length ? genres[index] : "unknown";
        }

        static List<User> LoadUsers(string path)
        {
            var list = new List<User>();
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 4)
                    continue;

                if (!int.TryParse(parts[0], out int id)) continue;
                if (!int.TryParse(parts[1], out int age)) continue;

                list.Add(new User
                {
                    Id = id,
                    Age = age,
                    Gender = parts[2],
                    Occupation = parts[3]
                });
            }

            Console.WriteLine($"Loaded {list.Count} users.");
            return list;
        }


        static List<Rating> LoadRatings(string path)
        {
            var list = new List<Rating>();

            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('\t');
                if (parts.Length < 3)
                    continue;

                if (!int.TryParse(parts[0], out int userId)) continue;
                if (!int.TryParse(parts[1], out int movieId)) continue;
                if (!double.TryParse(parts[2], out double score)) continue;

                list.Add(new Rating
                {
                    UserId = userId,
                    MovieId = movieId,
                    Score = score
                });
            }

            Console.WriteLine($"Loaded {list.Count} ratings.");
            return list;
        }

    }
}
