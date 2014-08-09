using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteFlood
{
    public enum Theme
    {
        Aero2, Aero, Classic, Luna
    }
    public class Settings
    {
        public Theme Theme { get; set; }
        public bool DrawGrid { get; set; }
        public Color DownloadColor { get; set; }
        public Color UploadColor { get; set; }
        public string DefaultDownloadPath { get; set; }
        public bool PreferEncryption { get; set; }
        public int ListeningPort { get; set; }
        public string Path;

        [XmlIgnore]
        public Brush DownloadBrush { get { return new SolidColorBrush(DownloadColor); } }
        [XmlIgnore]
        public Brush UploadBrush { get { return new SolidColorBrush(UploadColor); } }

        public static Settings DefaultSettings = new Settings() { 
            Theme = Theme.Aero2,
            DrawGrid = true,
            DownloadColor = Colors.Green,
            UploadColor = Colors.Red,
            DefaultDownloadPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads"),
            PreferEncryption = true,
            ListeningPort = 1025
        };

        public Settings()
        {
        }
        public static void Save(Settings s, string path)
        {
            XmlWriter xw = XmlWriter.Create(path, new XmlWriterSettings() {
                Indent = true
            });
            new XmlSerializer(typeof(Settings)).Serialize(xw, s);
            xw.Flush();
        }

        public static Settings Load(string path)
        {
            if (!File.Exists(path))
                return Settings.DefaultSettings;
            string s = File.ReadAllText(path);
            XmlReader x = XmlReader.Create(new StringReader(s));
            return (Settings)new XmlSerializer(typeof(Settings)).Deserialize(x);
        }
    }
}
