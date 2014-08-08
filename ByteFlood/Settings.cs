using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace ByteFlood
{
    public enum Theme
    {
        Aero2, Aero, Classic, Luna
    }
    public class Settings
    {
        public Theme Theme { get; set; }
        public string Path;
        public Settings()
        {}
        public static void Save(Settings s, string path)
        {
            XmlWriter xw = XmlWriter.Create(path);
            new XmlSerializer(typeof(Settings)).Serialize(xw, s);
            xw.Flush();
        }

        public static Settings Load(string path)
        {
            if (!File.Exists(path))
                return new Settings();
            string s = File.ReadAllText(path);
            XmlReader x = XmlReader.Create(new StringReader(s));
            return (Settings)new XmlSerializer(typeof(Settings)).Deserialize(x);
        }
    }
}
