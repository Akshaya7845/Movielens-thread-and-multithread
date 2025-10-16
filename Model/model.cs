using System;
using System.Collections.Generic;

namespace MovieLensMVC
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string[] Genres { get; set; } = Array.Empty<string>();
    }

    public class User
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Occupation { get; set; }
        public string Zip { get; set; }
    }


    public class Rating
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double Score { get; set; }
    }
}
