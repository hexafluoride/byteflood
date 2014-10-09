using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoTorrent.Common
{
    public class GetRightHttpSeed
    {
        public string Url { get; set; }

        /// <summary>
        /// Indicate if this webseed has been added to the peer list and is being used,
        /// so we don't add it twice.
        /// </summary>
        public bool HasBeenAdded { get; set; }

        public GetRightHttpSeed() 
        {
            this.HasBeenAdded = false;
        }
    }
}
