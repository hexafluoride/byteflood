using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ByteFlood.Formatters
{
    // formatter for plural/singular forms of
    // seconds/hours/days
    //http://stackoverflow.com/questions/16689468/how-to-produce-human-readable-strings-to-represent-a-timespan
    public class HMSFormatter : ICustomFormatter, IFormatProvider
    {
        string _plural, _singular;

        public HMSFormatter() { }

        private HMSFormatter(string plural, string singular)
        {
            _plural = plural;
            _singular = singular;
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                string fmt;
                switch (format)
                {
                    case "S": // second
                        fmt = String.Format(new HMSFormatter("{0} seconds", "{0} second"), "{0}", arg);
                        break;
                    case "M": // minute
                        fmt = String.Format(new HMSFormatter("{0} minutes", "{0} minute"), "{0}", arg);
                        break;
                    case "H": // hour
                        fmt = String.Format(new HMSFormatter("{0} hours", "{0} hour"), "{0}", arg);
                        break;
                    case "D": // day
                        fmt = String.Format(new HMSFormatter("{0} days", "{0} day"), "{0}", arg);
                        break;
                    default:
                        // plural/ singular             
                        fmt = String.Format((int)arg > 1 ? _plural : _singular, arg);  // watch the cast to int here...
                        break;
                }
                return fmt;
            }
            return String.Format(format, arg);
        }

        public static string GetReadableTimespan(TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedList<long, string> 
            { 
                {60, "{3:S}" },
                {60*60, "{2:M}, {3:S}"},
                {24*60*60, "{1:H}, {2:M}"},
                {Int64.MaxValue , "{0:D}, {1:H}"}
            };

            // find nearest best match
            var find = cutoff.Keys.ToList()
                          .BinarySearch((long)ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return String.Format(
                new HMSFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }
    }

}
