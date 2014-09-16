using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ByteFlood.Services.MoviesDatabases
{
    public interface IMovieDB
    {
        IMovieDBSearchResult[] Search(string queryText);
    }

    public interface IMovieDBSearchResult
    {
        string Title { get; }
        int Year { get; }
        Uri ThumbImageUri { get; }
        Uri PosterImageUri { get; }
        Uri ServiceIcon { get; }
        float Rating { get; }
        IMovieDBSearchResultType IMediaType { get; }
    }

    public enum IMovieDBSearchResultType
    {
        TVSeries,
        Movie,
        TVEpisode,
        Other
    }

    /// <summary>
    /// Since interfaces are not XmlSerializable, this class solve the problem.
    /// Credits: https://stackoverflow.com/a/1376358
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class IMDBSRSerializeable<T> : IXmlSerializable where T : IMovieDBSearchResult
    {
        public IMDBSRSerializeable() { }
        public IMDBSRSerializeable(T t) { this.Value = t; }
        public T Value { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            if (Value == null)
            {
                writer.WriteAttributeString("type", "null");
                return;
            }
            Type type = this.Value.GetType();
            XmlSerializer serializer = new XmlSerializer(type);
            writer.WriteAttributeString("type", type.FullName);
            serializer.Serialize(writer, this.Value);
        }

        public void ReadXml(XmlReader reader)
        {
            if (!reader.HasAttributes)
                return;
            string type = reader.GetAttribute("type");
            reader.Read(); // consume the value
            Type t = null;
            switch (type)
            {
                case "null":
                    return;
                case "ByteFlood.Services.MoviesDatabases.ImdbSearchResult":
                    t = typeof(ImdbSearchResult); break;
                case "ByteFlood.Services.MoviesDatabases.YifyAPIResult":
                    t = typeof(YifyAPIResult); break;
                default:
                    return;
            }

            XmlSerializer serializer = new XmlSerializer(t);
            this.Value = (T)serializer.Deserialize(reader);
            reader.ReadEndElement();
        }

        public XmlSchema GetSchema() { return (null); }
    }
}
