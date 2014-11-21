
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoTorrent
{
    public class MagnetLink
    {
        public InfoHash InfoHash
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public MagnetLink(string url)
        {
            ParseMagnetLink(url);
        }

        void ParseMagnetLink(string url)
        {
            string[] splitStr = url.Split('?');
            if (splitStr.Length == 0 || splitStr[0] != "magnet:")
                throw new FormatException("The magnet link must start with 'magnet:?'.");

            if (splitStr.Length == 1)
                return;//no parametter

            string[] parameters = splitStr[1].Split('&', ';');

            for (int i = 0; i < parameters.Length; i++)
            {
                string[] keyval = parameters[i].Split('=');
                if (keyval.Length != 2)
                    throw new FormatException("A field-value pair of the magnet link contain more than one equal'.");
                switch (keyval[0].Substring(0, 2))
                {
                    case "xt"://exact topic
                        if (InfoHash != null)
                            throw new FormatException("More than one infohash in magnet link is not allowed.");

                        string val = keyval[1].Substring(9);
                        //TODO: Support more hash encoding scheme
                        //https://en.wikipedia.org/wiki/Magnet_URI_scheme#URN.2C_containing_hash_.28xt.29
                        switch (keyval[1].Substring(0, 9))
                        {
                            case "urn:sha1:"://base32 hash
                            case "urn:btih:":
                                if (val.Length == 32)
                                    InfoHash = InfoHash.FromBase32(val);
                                else if (val.Length == 40)
                                    InfoHash = InfoHash.FromHex(val);
                                else
                                    throw new FormatException("Infohash must be base32 or hex encoded.");
                                break;
                        }
                        break;
                    case "dn"://display name
                        Name = System.Web.HttpUtility.UrlDecode(keyval[1]);
                        break;
                    default:
                        //not supported
                        break;
                }
            }
        }
    }
}
