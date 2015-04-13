using System;
using System.Linq;
using MonoTorrent.Common;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ByteFlood
{
    public class FileInfo : INotifyPropertyChanged
    {
        public string Name
        {
            get
            {
                return Program.Settings.ShowRelativePaths ? this.File.Path : this.File.FullPath;
            }
        }

        public string FileName
        {
            get { return this.File.Path.Split(System.IO.Path.DirectorySeparatorChar).Last(); }
        }

        public double Progress { get { return this.File.BitField.PercentComplete; } }

        public string Priority
        {
            get { return this.File.Priority.ToString(); }
        }

        public void ChangePriority(Priority pr)
        {
            if (this.File.Priority != pr)
            {
                this.File.Priority = pr;
                UpdateList("Priority");
            }
            if (this.Owner != null)
            {
                this.Owner.SetSavedFilePriority(this, pr);
            }
        }

        public bool DownloadFile
        {
            get { return this.File.Priority != MonoTorrent.Common.Priority.Skip; }
            set
            {
                if (value)
                {
                    this.File.Priority = MonoTorrent.Common.Priority.Normal;
                }
                else
                {
                    this.File.Priority = MonoTorrent.Common.Priority.Skip;
                }
                UpdateList("DownloadFile");
            }
        }

        public string Size { get { return Utility.PrettifyAmount(this.File.Length); } }

        public TorrentFile File { get; private set; }

        public long RawSize { get { return this.File.Length; } }

        public FileInfo() { }

        public TorrentInfo Owner { get; private set; }

        public FileInfo(TorrentInfo owner, TorrentFile file)
        {
            this.File = file;
            this.Owner = owner;
            if (this.Owner != null)
            {
                this.Owner.FileInfoList.Add(this);
                var saved_pr = this.Owner.GetSavedFilePriority(this);

                this.ChangePriority(saved_pr);
            }
        }

        public void Update()
        {
            UpdateList("Progress");
        }

        #region INotifyPropertyChanged implementation

        public void UpdateList(string str)
        {
            if (PropertyChanged == null) { return; }
            PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    //public class DirectoryKey : System.Collections.Hashtable, Aga.Controls.Tree.ITreeModel
    //{

    //    public DirectoryKey(string name, TorrentInfo ti)
    //    {
    //        this.Name = name; this.OwnerTorrent = ti;
    //    }

    //    public string Name { get; private set; }
    //    public TorrentInfo OwnerTorrent { get; private set; }
    //    public System.Collections.IEnumerable GetChildren(object parent)
    //    {
    //        if (parent == null)
    //        {
    //            return this.Values;
    //        }
    //        else if (parent is DirectoryKey)
    //        {
    //            return (parent as DirectoryKey).Values;
    //        }
    //        return null;
    //    }

    //    public bool HasChildren(object parent)
    //    {
    //        if (parent is DirectoryKey)
    //        {
    //            return ((DirectoryKey)parent).Count > 0;
    //        }

    //        return false;
    //    }

    //    /// <summary>
    //    /// No use outside of TorrentInfo.PopulateFileList()
    //    /// </summary>
    //    /// <param name="branch"></param>
    //    /// <param name="trunk"></param>
    //    public static void ProcessFile(string branch, DirectoryKey trunk, TorrentInfo owner, TorrentFile f)
    //    {
    //        string[] parts = branch.Split('\\');
    //        if (parts.Length == 1)
    //        {
    //            //((FileList)trunk[DirectoryKey.FILE_MARKER]).Add(new FileInfo(owner, f));
    //            trunk.Add(f.FullPath, new FileInfo(owner, f));
    //        }
    //        else
    //        {
    //            string node = parts[0];
    //            string other = branch.Substring(node.Length + 1);

    //            if (!trunk.ContainsKey(node))
    //            {
    //                trunk[node] = new DirectoryKey(node, owner);
    //            }
    //            ProcessFile(other, (DirectoryKey)trunk[node], owner, f);
    //        }
    //    }


    //}

    [Serializable]
    [XmlType(TypeName = "FilePriority")]
    public struct FilePriority
    {
        public string Key
        { get; set; }

        public MonoTorrent.Common.Priority Value
        { get; set; }

    }
}