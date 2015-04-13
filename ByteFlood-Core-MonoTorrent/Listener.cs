using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Common;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Xml;
using Jayrock.JsonRpc;

using Logger = ByteFlood.Core.MonoTorrent.Logger;
using ByteFlood.Core.MonoTorrent;

namespace ByteFlood
{
    public enum LogMessageType
    {
        Error, Info, Warning
    }
    public class Listener
    {
        public State State;
        public Thread Thread;
        public bool Running = true;
        public TcpListener TcpListener;
        public StateRpcHandler Handler;
        public Listener(State state)
        {
            Thread = new Thread(new ThreadStart(MainLoop));
            Thread.SetApartmentState(ApartmentState.STA);
            Thread.Start();
            State = state;
            Handler = new StateRpcHandler(State);
        }

        public void MainLoop()
        {
            TcpListener = new TcpListener(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 65432));
            TcpListener.Start();
            while (Running)
            {
                try
                {
                    HandleConnection(TcpListener.AcceptTcpClient());
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception occurred in listener thread!", "LISTENER", -1, MessageType.Error); 
                    Logger.Log(ex.ToString(), "LISTENER", -1, MessageType.Error); 
                }
            }
        }

        public void HandleConnection(TcpClient tcp)
        {
            NetworkStream ns = tcp.GetStream();
            StreamWriter sw = new StreamWriter(ns) { AutoFlush = true };
            StreamReader sr = new StreamReader(ns);
            Logger.Log(string.Format("Incoming connection from {0}", tcp.Client.RemoteEndPoint.ToString()), "LISTENER", 2);
            JsonRpcDispatcher dispatcher = JsonRpcDispatcherFactory.CreateDispatcher(Handler);
            while (true)
            {
                dispatcher.Process(sr, sw);
            }
            //Logger.Log("Connection closed.", "LISTENER", 2);
        }

        public void Shutdown()
        {
            Running = false;
            TcpListener.Stop();
            Thread.Abort();
        }
    }
}
