using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Web;

namespace ByteFlood.Services.MoviesDatabases
{
    public class Imdb : IMovieDB
    {
        public IMovieDBSearchResult[] Search(string queryText)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(string.Format("http://www.imdb.com/find?q={0}&s=tt&exact=true", HttpUtility.UrlEncode(queryText)));

            if (IsMoviePage(doc))
            {
                //TODO: Fix this.
                return new ImdbSearchResult[0];
            }
            else
            {
                return this.ParseResults(doc);
            }
        }

        private bool IsMoviePage(HtmlDocument doc)
        {
            HtmlNode i = doc.DocumentNode.GetElementById("overview-top");
            return (i != null);
        }

        private ImdbSearchResult[] ParseResults(HtmlDocument doc)
        {
            Regex RESULT_ID_MATCHER = new Regex(@"tt\d+", RegexOptions.Compiled);
            Regex RESULT_TYPE_MATCHER = new Regex(@"\(\D+\)", RegexOptions.Compiled);
            Regex RESULT_YEAR_MATCHER = new Regex(@"\(\d\d\d\d\)", RegexOptions.Compiled);

            HtmlNode[] findResults = doc.DocumentNode.GetElementsByClassName("findResult");

            List<ImdbSearchResult> results = new List<ImdbSearchResult>();

            foreach (HtmlNode node in findResults)
            {
                if (node.Name == "tr")
                {
                    ImdbSearchResult r = new ImdbSearchResult();

                    //first TD is the result image
                    //second TD is the data
                    HtmlNode[] imgs = node.GetElementsByTagName("img");
                    HtmlNode[] result_text = node.GetElementsByClassName("result_text");

                    if (imgs.Length > 0)
                    {
                        r.ThumbImageUrl = imgs[0].GetAttributeValue("src", "");
                    }

                    if (result_text.Length > 0)
                    {
                        HtmlNode result_text_node = result_text[0];

                        HtmlNode[] smalls = result_text_node.GetElementsByTagName("small");
                        if (smalls.Length > 0)
                        {
                            result_text_node.RemoveChild(smalls[0]);
                        }

                        HtmlNode[] links = result_text_node.GetElementsByTagName("a");

                        HtmlNode result_link = links[0];

                        r.Title = HttpUtility.HtmlDecode(result_link.InnerText);

                        string link = result_link.GetAttributeValue("href", "");

                        r.ID = Convert.ToInt32((RESULT_ID_MATCHER.Match(link).Value.Replace("tt", "")));

                        string result_description = HttpUtility.HtmlDecode(result_text_node.InnerText);

                        var year_matches = RESULT_YEAR_MATCHER.Matches(result_description);
                        if (year_matches.Count > 0)
                        {
                            r.Year = Convert.ToInt32(year_matches[0].Value.Substring(1, 4));
                        }

                        var types_matches = RESULT_TYPE_MATCHER.Matches(result_description);
                        if (types_matches.Count > 0)
                        {
                            r.SetMetiaType(types_matches[types_matches.Count - 1].Value);
                        }
                        else
                        {
                            r.Type = MediaType.Unspecified;
                        }
                    }

                    results.Add(r);
                }
            }

            return results.ToArray();
        }

        public enum MediaType
        {
            TV_Series,
            TV_Episode,
            TV_Special,
            TV_Movie,
            Short,
            Film,
            Video,
            VideoGame,
            TV_Mini_Series,
            Unspecified,
            Unknown
        }
    }

    public class ImdbSearchResult : IMovieDBSearchResult
    {
        public int ID;
        public string Title { get; set; }
        public int Year { get; set; }
        public Imdb.MediaType Type { get; set; }
        public Uri ServiceIcon { get { return new Uri("http://www.imdb.com/favicon.ico"); } }
        public IMovieDBSearchResultType IMediaType 
        {
            get 
            {
                switch (this.Type) 
                {
                    case Imdb.MediaType.Film:
                    case Imdb.MediaType.Short:
                    case Imdb.MediaType.TV_Movie:
                    case Imdb.MediaType.Video:
                        return IMovieDBSearchResultType.Movie;
                    case Imdb.MediaType.TV_Episode:
                        return IMovieDBSearchResultType.TVEpisode;
                    case Imdb.MediaType.TV_Mini_Series:
                    case Imdb.MediaType.TV_Series:
                        return IMovieDBSearchResultType.TVSeries;
                    default:
                        return IMovieDBSearchResultType.Other;
                }
            }
        }

        public string ThumbImageUrl { get; set; }
        public Uri ThumbImageUri
        {
            get { return new Uri(this.ThumbImageUrl); }
        }
        public bool IsIndevelopment { get; set; }
        public string MoviePageLink
        {
            get
            {
                return string.Format("http://www.imdb.com/title/tt{0}/", this.ID);
            }
        }

        #region Secondary Properties

        private string _genre;
        public string Genre
        {
            get
            {
                if (this.secondary_properties_loaded)
                {
                    return this._genre;
                }
                else
                {
                    this.LoadSecondaryProperties();
                    return this._genre;
                }
            }
        }

        private string _plot;
        public string Plot
        {
            get
            {
                if (this.secondary_properties_loaded)
                {
                    return this._plot;
                }
                else
                {
                    this.LoadSecondaryProperties();
                    return this._plot;
                }
            }
        }

        private string _director;
        public string Director
        {
            get
            {
                if (this.secondary_properties_loaded)
                {
                    return this._director;
                }
                else
                {
                    this.LoadSecondaryProperties();
                    return this._director;
                }
            }
        }

        private float _rating;
        public float Rating
        {
            get
            {
                if (this.secondary_properties_loaded)
                {
                    return this._rating;
                }
                else
                {
                    this.LoadSecondaryProperties();
                    return this._rating;
                }
            }
        }

        private string _poster_image_link;
        public string PosterImageLink
        {
            get
            {
                if (this.secondary_properties_loaded)
                {
                    return this._poster_image_link;
                }
                else
                {
                    this.LoadSecondaryProperties();
                    return this._poster_image_link;
                }
            }
        }

        public Uri PosterImageUri 
        {
            get { return new Uri(this.PosterImageLink); }
        }

        #endregion

        public void SetMetiaType(string type)
        {
            switch (type.ToLower())
            {
                case "(tv series)":
                    this.Type = Imdb.MediaType.TV_Series; break;
                case "(tv episode)":
                    this.Type = Imdb.MediaType.TV_Episode; break;
                case "(short)":
                    this.Type = Imdb.MediaType.Short; break;
                case "(film)":
                    this.Type = Imdb.MediaType.Film; break;
                case "(tv special)":
                    this.Type = Imdb.MediaType.TV_Special; break;
                case "(tv mini-series)":
                    this.Type = Imdb.MediaType.TV_Mini_Series; break;
                case "(video)":
                    this.Type = Imdb.MediaType.Video; break;
                case "(video game)":
                    this.Type = Imdb.MediaType.VideoGame; break;
                case "(tv movie)":
                    this.Type = Imdb.MediaType.TV_Movie; break;
                case "(in development)":
                    this.IsIndevelopment = true; break;
                case "":
                    this.Type = Imdb.MediaType.Unspecified; break;
                default:
                    this.Type = Imdb.MediaType.Unknown; break;
            }
        }

        bool secondary_properties_loaded = false;

        private void LoadSecondaryProperties()
        {
            if (this.secondary_properties_loaded) { return; }

            HtmlWeb web = new HtmlWeb();
            HtmlDocument _markup = web.Load(this.MoviePageLink);

            HtmlNode img_primary = _markup.DocumentNode.GetElementById("img_primary");

            if (img_primary != null)
            {
                var img_elements = img_primary.GetElementsByTagName("img");
                if (img_elements.Length > 0)
                {
                    this._poster_image_link = img_elements[0].GetAttributeValue("src", "");
                }
            }

            HtmlNode overview_top = _markup.DocumentNode.GetElementById("overview-top");

            var genres = overview_top.GetElementsByAttributeValue("itemprop", "genre");

            if (genres.Length > 0)
            {
                this._genre = genres[0].InnerText.Trim();
            }

            var ratings = overview_top.GetElementsByClassName("titlePageSprite star-box-giga-star");

            if (ratings.Length > 0)
            {
                float.TryParse(ratings[0].InnerText.Trim(), out  this._rating);
            }

            var plots = overview_top.GetElementsByAttributeValue("itemprop", "description");

            if (plots.Length > 0)
            {
                this._plot = HttpUtility.HtmlDecode(plots[0].InnerText.Trim());
            }

            var director = overview_top.GetElementsByAttributeValue("itemprop", "creator");

            if (director.Length > 0)
            {
                var a = director[0].GetElementsByAttributeValue("itemprop", "name");
                this._director = HttpUtility.HtmlDecode(a[0].InnerText);
            }

            this.secondary_properties_loaded = true;
        }
    }
}
