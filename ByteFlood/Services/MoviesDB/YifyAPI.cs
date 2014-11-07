using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Jayrock;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.Net;
using System.Web;
namespace ByteFlood.Services.MoviesDatabases
{
    public class YifyAPI : IMovieDB
    {
        public IMovieDBSearchResult[] Search(string queryText)
        {
            using (WebClient nc = new WebClient())
            {
                string data = nc.DownloadString(string.Format("https://yts.re/api/list.json?keywords={0}",
                    HttpUtility.UrlEncode(clean_query(queryText))));

                JsonObject jo = JsonConvert.Import<JsonObject>(data);

                if (jo["error"] != null)
                {
                    return new IMovieDBSearchResult[0];
                }
                else
                {
                    int count = Convert.ToInt32(jo["MovieCount"]);
                    List<YifyAPIResult> list = new List<YifyAPIResult>(count);
                    JsonArray movies = (JsonArray)jo["MovieList"];

                    for (int i = 0; i < count; i++)
                    {
                        JsonObject movie = (JsonObject)movies[i];
                        list.Add(new YifyAPIResult()
                        {
                            MovieID = Convert.ToInt32(movie["MovieID"]),
                            CoverImage = Convert.ToString(movie["CoverImage"]),
                            Downloaded = Convert.ToString(movie["Downloaded"]),
                            Genre = Convert.ToString(movie["Genre"]),
                            ImdbCode = Convert.ToString(movie["ImdbCode"]),
                            MovieRating = float.Parse(Convert.ToString(movie["MovieRating"]), CultureInfo.InvariantCulture.NumberFormat),
                            DateUploaded = new DateTime(1970, 1, 1).AddSeconds(Convert.ToInt32(movie["DateUploadedEpoch"])),
                            ImdbLink = Convert.ToString(movie["ImdbLink"]),
                            MovieTitle = Convert.ToString(movie["MovieTitle"]),
                            MovieTitleClean = Convert.ToString(movie["MovieTitleClean"]),
                            Size = Convert.ToString(movie["Size"]),
                            MovieUrl = Convert.ToString(movie["MovieUrl"]),
                            Quality = Convert.ToString(movie["Quality"]),
                            State = Convert.ToString(movie["State"]),
                            TorrentHash = Convert.ToString(movie["TorrentHash"]),
                            TorrentPeers = Convert.ToString(movie["TorrentPeers"]),
                            UploaderUID = Convert.ToString(movie["UploaderUID"]),
                            MovieYear = Convert.ToInt32(movie["MovieYear"]),
                            TorrentSeeds = Convert.ToString(movie["TorrentSeeds"]),
                            Uploader = Convert.ToString(movie["Uploader"]),
                            TorrentUrl = Convert.ToString(movie["TorrentUrl"]),
                            SizeByte = Convert.ToInt64(movie["SizeByte"]),
                            TorrentMagnetUrl = Convert.ToString(movie["TorrentMagnetUrl"])
                        });
                    }
                    return list.ToArray();
                }
            }
        }

        private string clean_query(string a)
        {
            StringBuilder sb = new StringBuilder(a.Length);

            foreach (char c in a)
            {
                if (c == '(' ||
                    c == ')' ||
                    c == '.')
                {
                    continue;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }


    public class YifyAPIResult : IMovieDBSearchResult
    {
        #region IMovieDBSearchResult
        public string Title
        {
            get { return this.MovieTitleClean; }
        }

        public int Year
        {
            get { return this.MovieYear; }
        }

        public Uri ThumbImageUri
        {
            get { return new Uri(this.CoverImage); }
        }

        public Uri PosterImageUri
        {
            get { return new Uri(this.CoverImage); }
        }

        public float Rating
        {
            get
            {
                return this.MovieRating;
            }
        }
        public IMovieDBSearchResultType IMediaType
        {
            get
            {
                return IMovieDBSearchResultType.Movie;
            }
        }
        public Uri ServiceIcon 
        {
            get
            {
                return new Uri("http://yify-torrents.com/favicon.ico");
            }
        }
        #endregion

        public int MovieID { get; set; }
        public string State { get; set; }
        public string MovieUrl { get; set; }
        public string MovieTitle { get; set; }
        public string MovieTitleClean { get; set; }
        public int MovieYear { get; set; }
        public DateTime DateUploaded { get; set; }

        public string Quality { get; set; }
        public string CoverImage { get; set; }
        public string ImdbCode { get; set; }
        public string ImdbLink { get; set; }
        public string Size { get; set; }
        public long SizeByte { get; set; }
        public float MovieRating { get; set; }
        public string Genre { get; set; }
        public string Uploader { get; set; }
        public string UploaderUID { get; set; }

        public string Downloaded { get; set; }
        public string TorrentSeeds { get; set; }
        public string TorrentPeers { get; set; }
        public string TorrentUrl { get; set; }
        public string TorrentHash { get; set; }
        public string TorrentMagnetUrl { get; set; }
    }
}
