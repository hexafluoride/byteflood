using System;
using System.Linq;
using System.Runtime.CompilerServices;
using MonoTorrent.Common;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ByteFlood
{
    public class FileInfo : INotifyPropertyChanged
    {
        public static LanguageEngine Language { get { return App.CurrentLanguage; } }

        /// <summary>
        /// This is the relative file name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.File.Path.Replace(this.Owner.RootDownloadDirectory, "");
            }
        }

        public string FullPath 
        {
            get 
            {
                // even thought libtorrent documentation specify that the Path property is the
                // full path, sometimes this isn't true (returns the relative path),
                // so we need to workaround it
                if (System.IO.Path.IsPathRooted(this.File.Path)) 
                {
                    return this.File.Path;
                }
                else 
                {
                    return System.IO.Path.Combine(this.Owner.SavePath, this.File.Path);
                }
            }
        }

        public string FileName
        {
            get { return System.IO.Path.GetFileName(this.File.Path); }
        }

        private long _downloaded_bytes = 0;
        public long DownloadedBytes 
        {
            get { return this._downloaded_bytes; }
            set 
            {
                if (value != this._downloaded_bytes) 
                {
                    this._downloaded_bytes = value;
                    UpdateList("DownloadedBytes", "Progress");
                }
            }
        }

        public double Progress 
        {
            get 
            {
                if (this.RawSize > 0) 
                {
                    return Convert.ToDouble(this.DownloadedBytes) / Convert.ToDouble(this.RawSize);
                }
                return 100d;
            } 
        }

        public string Priority
        {
            get 
            {
                int a = this.Owner.Torrent.GetFilePriority(this.FileIndex);
                switch (a) 
                {
                    case 0:
                       return Language.FilePriority_Skip;
                    case 1:
                       return Language.FilePriority_Lowest;
                    case 2:
                       return Language.FilePriority_Low;
                    case 3:
                       return Language.FilePriority_Normal;
                    case 4:
                       return Language.FilePriority_High;
                    case 5:
                       return Language.FilePriority_Highest;
                    case 6:
                       return Language.FilePriority_Immediate;
                    default :
                       return string.Format("{0}: {1}", Language.FilePriority_Custom, a);
                }
            }
        }

        public void ChangePriority(int pr)
        {
            this.Owner.Torrent.SetFilePriority(this.FileIndex, pr);
            this.Owner.UpdateSingle("WantedBytes");
            UpdateSingle("Priority");
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
                UpdateSingle();
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

        #region INotifyPropertyChanged implementation

        public void UpdateSingle([CallerMemberName]string name = null)
        {
	        var handler = PropertyChanged;
            if (handler != null)
				handler(this, new PropertyChangedEventArgs(name));
        }

	    public void UpdateList(params string[] str)
	    {
		    foreach (string s in str)
			    UpdateSingle(s);
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
}