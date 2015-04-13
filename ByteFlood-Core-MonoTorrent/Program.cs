using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ByteFlood.Core.MonoTorrent;

namespace ByteFlood
{
    class Program
    {
        public Dispatcher Dispatcher;
        public static State State = new State();
        public static Settings Settings;

        static void Main(string[] args)
        {
            Logger.Sinks.Add(Console.Out);

            Settings = Settings.Load("./settings.xml");

            State.Initialize();
        }
    }
}
