using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Jayrock;
using Jayrock.Json;

namespace ByteFloodUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = null;
            string destination_dir = null;

            if (args.Length < 2)
            {
                Console.WriteLine("Bad program usage");
                return;
            }

            url = args[0].Replace("\"", "");
            destination_dir = args[1].Replace("\"", "");

            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(destination_dir))
            {
                Console.WriteLine("Bad program usage");
                return;
            }

            Console.Title = "ByteFlood Updater";

            //Wait for byteflood to exit
            System.Threading.Thread.Sleep(1500);

            #region Terminate byteflood
            bool shutdown_signal_sent = false;

            while (Process.GetProcessesByName("byteflood").Length > 0)
            {
                if (shutdown_signal_sent)
                {
                    Console.WriteLine("ByteFlood did not respond for the shutdown signal or it does not support it.");
                    Console.WriteLine("Please close it manually and press any key to continue.");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("ByteFlood is still running. Sending shutdown signal...");
                    try
                    {
                        send_shutdown_signal();
                        //wait to exit
                        System.Threading.Thread.Sleep(5000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not send shutdown signal for the following reason: {0}", ex.Message);
                    }
                    finally
                    {
                        shutdown_signal_sent = true;
                    }
                }
            }
            #endregion
            Console.WriteLine("ByteFlood is terminated. Commencing download");

            string temp_extract_dir = Path.Combine(Path.GetTempPath(), "extracted");

            string temp_extract_rm = Path.Combine(Path.GetTempPath(), "rm");

            check_dir(temp_extract_dir);
            check_dir(temp_extract_rm);

            bool file_downloaded = false;

            MemoryStream memIO = null;

            while (!file_downloaded)
            {
                try
                {
                    memIO = new MemoryStream();
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);

                    int length = 1;
                    int downloaded = 0;
                    using (var response = wr.GetResponse())
                    {
                        length = Convert.ToInt32(response.ContentLength);

                        using (var response_stream = response.GetResponseStream())
                        {
                            byte[] buffer = new byte[4096];

                            int bs = 0;

                            while ((bs = response_stream.Read(buffer, 0, 4096)) > 0)
                            {
                                memIO.Write(buffer, 0, bs);
                                downloaded += bs;
                                update_progress(downloaded, length);
                            }
                            Console.WriteLine();
                            Console.WriteLine("Download finished");
                            file_downloaded = true;
                            System.Threading.Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not download the update.");
                    Console.WriteLine("Error was:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("Retry? Y/N");
                    if (!Console.ReadLine().ToUpper().StartsWith("Y"))
                    {
                        Console.WriteLine("Exiting...");
                        return;
                    }
                }
            }

            Console.WriteLine("Decompressing file...");

            Unzip f = new Unzip(memIO);
            f.ExtractToDirectory(temp_extract_dir);
            f.Dispose();

            Console.WriteLine("Copying files...");

            //Move new files
            List<MoveFileAction> actions = new List<MoveFileAction>();

            DirectoryInfo temp_dir = new DirectoryInfo(temp_extract_dir);
            DirectoryInfo dest_dir = new DirectoryInfo(destination_dir);

            bool updater_need_update = false;

            foreach (FileInfo fi in temp_dir.GetFiles("*", SearchOption.AllDirectories))
            {
                if (fi.Name == "ByteFloodUpdater.exe") { updater_need_update = true; continue; }
                string relative_name = fi.FullName.Remove(0, temp_dir.FullName.Length + 1);
                string dst_file = Path.Combine(dest_dir.FullName, relative_name);
                FileInfo iiiii = new FileInfo(dst_file);
                Directory.CreateDirectory(iiiii.DirectoryName);
                actions.Add(new MoveFileAction(fi.FullName, dst_file, temp_extract_rm));
            }

            bool err = false;
            int files_copied = 0;

            foreach (var action in actions)
            {
                try
                {
                    action.PerformAction();
                    files_copied++;
                    update_progress(files_copied, actions.Count);
                }
                catch
                {
                    err = true;
                    break;
                }
                finally
                {
                    action.CleanUp();
                }
            }

            if (err)
            {
                Console.WriteLine("An error has been occured. Rolling back actions...");
                foreach (var action in actions)
                {
                    action.Undo();
                }
            }
            else
            {
                Console.WriteLine("All OK.");
                Process.Start(Path.Combine(dest_dir.FullName, "byteflood.exe"));
                if (updater_need_update)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("taskkill /IM ByteFloodUpdater.exe");
                    sb.AppendFormat("copy /Y /B \"{0}\" \"{1}\"", Path.Combine(temp_dir.FullName, "ByteFloodUpdater.exe"),
                                                                  Path.Combine(dest_dir.FullName, "ByteFloodUpdater.exe"));
                    sb.AppendLine();
                    sb.AppendFormat("del /Q \"{0}\"", temp_extract_dir);
                    sb.AppendLine();

                    sb.AppendFormat("del /Q \"{0}\"", temp_extract_rm);
                    sb.AppendLine();

                    string cmd_file = Path.Combine(Path.GetTempPath(), "a.bat");
                    sb.AppendFormat("del \"{0}\"", cmd_file);

                    File.WriteAllText(cmd_file, sb.ToString());
                    ProcessStartInfo psi = new ProcessStartInfo(cmd_file);
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
            }

            Directory.Delete(temp_extract_dir, true);
            Directory.Delete(temp_extract_rm, true);
        }

        private static void check_dir(string dir)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
            }
            else { Directory.CreateDirectory(dir); }
        }

        public class MoveFileAction
        {
            private string source;
            private string dest;
            private string temp_dir;

            private string temp_rm;
            private bool executed = false;

            public MoveFileAction(string source, string destination, string temp_dir)
            {
                this.source = source;
                this.dest = destination;
                this.temp_dir = temp_dir;
                Directory.CreateDirectory(temp_dir);
            }

            public void PerformAction()
            {
                FileInfo i = new FileInfo(dest);
                if (i.Exists)
                {
                    temp_rm = Path.Combine(temp_dir, i.Name);
                    File.Move(dest, temp_rm);
                    File.Move(source, dest);
                    executed = true;
                }
                else
                {
                    File.Move(source, dest);
                    executed = true;
                }
            }

            public void Undo()
            {
                if (executed)
                {
                    if (string.IsNullOrEmpty(temp_rm))
                    {
                        //Simply move back
                        if (File.Exists(dest))
                        {
                            File.Move(dest, source);
                        }
                    }
                    else
                    {
                        File.Move(dest, source);
                        File.Move(temp_rm, dest);
                    }
                }
            }

            public void CleanUp()
            {
                try
                {
                    if (!string.IsNullOrEmpty(temp_rm))
                    {
                        if (File.Exists(temp_rm)) { File.Delete(temp_rm); }
                    }
                }
                catch { }
            }
        }

        private static void update_progress(double downloaded, double total)
        {
            Console.CursorLeft = 0;

            Console.Write("[");

            double percent = (downloaded / total);

            int ticks = Convert.ToInt32(percent * 36f);

            for (int i = 0; i < 36; i++)
            {
                if (i <= ticks)
                {
                    Console.Write("*");
                }
                else
                {
                    Console.Write(" ");
                }
            }

            Console.Write("] {0}%  ", (percent * 100).ToString("0.00"));
        }

        private static void send_shutdown_signal()
        {
            TcpClient tcp = new TcpClient();
            tcp.Connect("127.0.0.1", 65432);
            NetworkStream ns = tcp.GetStream();
            StreamWriter sw = new StreamWriter(ns);
            JsonObject jo = new JsonObject();
            jo.Add("id", 0);
            jo.Add("method", "shutdown");
            sw.WriteLine(jo.ToString());
            sw.Flush();
            tcp.Close();
        }

    }
}
