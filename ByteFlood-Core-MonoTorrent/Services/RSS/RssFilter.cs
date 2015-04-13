using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ByteFlood.Services.RSS
{
    public class RssFilter
    {
        private Regex m;

        private string _ft;

        public string FilterText
        {
            get { return this._ft; }
            set
            {
                if (this._ft != value)
                {
                    this.m = new Regex(Regex.Escape(value), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    this._ft = value;
                }
            }
        }

        public bool IsAllowed(RssTorrent t) 
        {
            if (this.FilterValid) 
            {
                bool regex_match = this.m.IsMatch(t.Name);

                if (this.FilterAction == FilterActionEnum.Download)
                {
                    return regex_match;
                }
                else //In Skip action, we don't want stuffs that match the filter
                {
                    return !regex_match;
                }
            }

            return false;
        }

        public bool FilterValid
        {
            get { return this.m != null; }
        }

        public FilterActionEnum FilterAction { get; set; }

        public enum FilterActionEnum { Download, Skip }

        [XmlIgnore]
        public System.Windows.RoutedEventHandler RemoveAction { get; set; }
    }

}
