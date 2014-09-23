using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PhotoLoader.ImageLoaders
{
    class ExternalLoader : ILoader
    {
        #region ILoader Members

        public System.IO.Stream Load(string source)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    byte[] html = webClient.DownloadData(source);

                    if (html == null || html.Length == 0) return null;

                    return new MemoryStream(html);
                }
                catch { return null; }
            }
        }

        #endregion
    }
}