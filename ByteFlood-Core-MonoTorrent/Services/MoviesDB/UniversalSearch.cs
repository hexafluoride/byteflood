using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteFlood.Services.MoviesDatabases
{
    /// <summary>
    /// Class that handle movie information searching using various search engines
    /// </summary>
    public static class UniversalSearch
    {
        private static IMovieDB[] Engines = new IMovieDB[] 
        {
            new YifyAPI(),
            new Imdb()
        };

        public static IMovieDBSearchResult[] Search(string queryText)
        {
            List<IMovieDBSearchResult> l = new List<IMovieDBSearchResult>();
         
            foreach (IMovieDB engine in Engines)
            {
                var results = engine.Search(queryText);

                if (results.Length > 0)
                {
                    l.AddRange(results);
                }
            }

            return l.ToArray();
        }

    }
}
