using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieLensMVC
{
    public class ReportController
    {
        private readonly List<Movie> _movies;
        private readonly List<User> _users;
        private readonly List<Rating> _ratings;

        public ReportController(List<Movie> movies, List<User> users, List<Rating> ratings)
        {
            _movies = movies;
            _users = users;
            _ratings = ratings;
        }

        public Dictionary<string, List<(string Movie, double Avg)>> GenerateReportsSingleThread()
        {
            return GenerateReportsInternal(_ratings);
        }

        public Dictionary<string, List<(string Movie, double Avg)>> GenerateReportsMultiThread(int chunkSize = 10000)
        {
            int total = _ratings.Count;
            int numChunks = (int)Math.Ceiling(total / (double)chunkSize);

            var tasks = new List<Task<Dictionary<string, List<(string Movie, double Avg)>>>>();

            for (int i = 0; i < numChunks; i++)
            {
                int start = i * chunkSize;
                int end = Math.Min(total, start + chunkSize);
                var subset = _ratings.GetRange(start, end - start);
                tasks.Add(Task.Run(() => GenerateReportsInternal(subset)));
            }

            Task.WaitAll(tasks.ToArray());
            var merged = new Dictionary<string, List<(string Movie, double Avg)>>();

            foreach (var task in tasks)
            {
                foreach (var kv in task.Result)
                {
                    if (!merged.ContainsKey(kv.Key))
                        merged[kv.Key] = new List<(string Movie, double Avg)>();

                    merged[kv.Key].AddRange(kv.Value);
                }
            }
            var final = new Dictionary<string, List<(string Movie, double Avg)>>();

            foreach (var kv in merged)
            {
                var grouped = kv.Value
                    .GroupBy(x => x.Movie)
                    .Select(g => (Movie: g.Key, Avg: g.Average(x => x.Avg)))
                    .OrderByDescending(x => x.Avg)
                    .Take(10)
                    .ToList();

                final[kv.Key] = grouped;
            }

            return final;
        }

        private Dictionary<string, List<(string Movie, double Avg)>> GenerateReportsInternal(List<Rating> subset)
        {
            var result = new Dictionary<string, List<(string Movie, double Avg)>>();

            var joined = from r in subset
                         join u in _users on r.UserId equals u.Id
                         join m in _movies on r.MovieId equals m.Id
                         select new { r, u, m };

            var all = joined
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["General"] = all;

            var male = joined.Where(x => x.u.Gender == "M")
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["Male"] = male;

            var female = joined.Where(x => x.u.Gender == "F")
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["Female"] = female;

            string[] genres = { "Action", "Drama", "Comedy", "Fantasy" };
            foreach (var genre in genres)
            {
                var genreList = joined.Where(x => x.m.Genres.Contains(genre))
                    .GroupBy(x => x.m.Title)
                    .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                    .OrderByDescending(x => x.Avg)
                    .Take(10)
                    .ToList();

                result[genre] = genreList;
            }
            var under18 = joined.Where(x => x.u.Age < 18)
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["Age_Under18"] = under18;

            var between18_30 = joined.Where(x => x.u.Age >= 18 && x.u.Age < 30)
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["Age_18_30"] = between18_30;

            var above30 = joined.Where(x => x.u.Age >= 30)
                .GroupBy(x => x.m.Title)
                .Select(g => (Movie: g.Key, Avg: g.Average(x => x.r.Score)))
                .OrderByDescending(x => x.Avg)
                .Take(10)
                .ToList();
            result["Age_Above30"] = above30;

            return result;
        }
    }
}
