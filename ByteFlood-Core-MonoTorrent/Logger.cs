using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ByteFlood.Core.MonoTorrent
{
    class Logger
    {
        public static string DefaultFromString = "MAIN";

        private static int Verbosity = 3;

        public static List<TextWriter> Sinks = new List<TextWriter>();

        public static Dictionary<MessageType, string> Tags = new Dictionary<MessageType, string>()
        {
            {MessageType.Error, "ERROR"},
            {MessageType.Info, "INFO"},
            {MessageType.Warning, "WARNING"}
        };

        public static void Log(string message, string from = "DEFAULT", int verbosity = 0, MessageType type = MessageType.Info)
        {
            if (verbosity > Verbosity)
                return;

            if (from == "DEFAULT")
                from = DefaultFromString;

            string final = GetTag(type, from) + message;
            Sinks.ForEach(sink =>
            {
                sink.WriteLine(final);
                sink.Flush();
            });
        }

        public static string GetTag(MessageType msg, string source)
        {
            string tag = Tags[msg];
            return "[ " + source + " : " + tag + " ] ";
        }
    }

    public enum MessageType
    {
        Info, Error, Warning
    }
}
