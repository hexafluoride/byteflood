using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace PhotoLoader.ImageLoaders
{
    public class CachedExternalResoucre : ILoader
    {
        static Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

        #region ILoader Members

        public System.IO.Stream Load(string source)
        {
            if (cache.ContainsKey(source)) 
            {
                return new MemoryStream(cache[source]);
            }
            
            using (var webClient = new WebClient())
            {
                try
                {
                    byte[] html = webClient.DownloadData(source);

                    if (html == null || html.Length == 0) return null;

                    try { cache.Add(source, html); }
                    catch { }

                    return new MemoryStream(html);
                }
                catch { return null; }
            }
        }

        #endregion
    }
}
