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
                return this.File.Path.Replace(this.Owner.SavePath, "");
            }
        }

        public string FileName
        {
            get { return this.File.Path.Split(System.IO.Path.DirectorySeparatorChar).Last(); }
        }

        public double Progress { get { return this.Owner.Torrent.GetFileProgresses()[0]; } }

        public string Priority
        {
            get 
            {
                int a = this.Owner.Torrent.GetFilePriority(this.FileIndex);
                return a.ToString();
            }
        }

        public void ChangePriority(int pr)
        {
            this.Owner.Torrent.SetFilePriority(this.FileIndex, pr);
            UpdateList("Priority");
        }

        public bool DownloadFile
        {
            get { return this.Owner.Torrent.GetFilePriority(this.FileIndex) != 0; }
            set
            {
                if (value)
                {
                    this.Owner.Torrent.SetFilePriority(this.FileIndex, 3);
                }
                else
                {
                    this.Owner.Torrent.SetFilePriority(this.FileIndex, 0);
                }
                UpdateList("DownloadFile");
            }
        }

        public string Size { get { return Utility.PrettifyAmount(this.File.Size); } }

        public Ragnar.FileEntry File { get; private set; }

        public long RawSize { get { return this.File.Size; } }

        public FileInfo() { }

        public TorrentInfo Owner { get; private set; }

        public int FileIndex { get; private set; }

        public FileInfo(TorrentInfo owner, Ragnar.FileEntry file, int file_index)
        {
            this.File = file;
            this.Owner = owner;
            this.FileIndex = file_index;
            if (this.Owner != null)
            {
                this.Owner.FileInfoList.Add(this);
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

    public class DirectoryKey : System.Collections.Hashtable, Aga.Controls.Tree.ITreeModel
    {

        public DirectoryKey(string name, TorrentInfo ti)
        {
            this.Name = name; this.OwnerTorrent = ti;
        }

        public string Name { get; private set; }
        public TorrentInfo OwnerTorrent { get; private set; }
        public System.Collections.IEnumerable GetChildren(object parent)
        {
            if (parent == null)
            {
                return this.Values;
            }
            else if (parent is DirectoryKey)
            {
                return (parent as DirectoryKey).Values;
            }
            return null;
        }

        public bool HasChildren(object parent)
        {
            if (parent is DirectoryKey)
            {
                return ((DirectoryKey)parent).Count > 0;
            }

            return false;
        }

        /// <summary>
        /// No use outside of TorrentInfo.PopulateFileList()
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="trunk"></param>
        public static void ProcessFile(string branch, DirectoryKey trunk, TorrentInfo owner, Ragnar.FileEntry f, int index)
        {
            string[] parts = branch.Split('\\');
            if (parts.Length == 1)
            {
                //((FileList)trunk[DirectoryKey.FILE_MARKER]).Add(new FileInfo(owner, f));
                trunk.Add(f.Path, new FileInfo(owner, f, index));
            }
            else
            {
                string node = parts[0];
                string other = branch.Substring(node.Length + 1);

                if (!trunk.ContainsKey(node))
                {
                    trunk[node] = new DirectoryKey(node, owner);
                }
                ProcessFile(other, (DirectoryKey)trunk[node], owner, f, index);
            }
        }


    }

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