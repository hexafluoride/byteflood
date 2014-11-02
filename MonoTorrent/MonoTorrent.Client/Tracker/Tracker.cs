//
// Tracker.cs
//
// Authors:
//   Alan McGovern alan.mcgovern@gmail.com
//
// Copyright (C) 2006 Alan McGovern
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections.Generic;
using System.Text;
using MonoTorrent.Common;

namespace MonoTorrent.Client.Tracker
{
    public abstract class Tracker : ITracker
    {
        public event EventHandler BeforeAnnounce;
        public event EventHandler<AnnounceResponseEventArgs> AnnounceComplete;
        public event EventHandler BeforeScrape;
        public event EventHandler<ScrapeResponseEventArgs> ScrapeComplete;

        bool canAnnounce;
        bool canScrape;
        int complete;
        int downloaded;
        int incomplete;

        TimeSpan minUpdateInterval;

        TrackerState status;
        TimeSpan updateInterval;
        Uri uri;
        string warningMessage;

        public bool CanAnnounce
        {
            get { return canAnnounce; }
            protected set { canAnnounce = value; }
        }
        public bool CanScrape
        {
            get { return canScrape; }
            set { canScrape = value; }
        }
        public int Complete
        {
            get { return complete; }
            protected set { complete = value; }
        }
        public int Downloaded
        {
            get { return downloaded; }
            protected set { downloaded = value; }
        }

        public string FailureMessage
        {
            get
            {
                if (this._fail_messages.Count > 1)
                {
                    return this._fail_messages[_fail_messages.Count - 1].Value;
                }
                return string.Empty;
            }
            protected set 
            {
                this.AppendFailure(value, false);
            }
        }

        private List<KeyValuePair<DateTime, string>> _fail_messages = new List<KeyValuePair<DateTime, string>>();

        public int FailureCount
        {
            get;
            private set;
        }

        public void AppendFailure(string message, bool count)
        {
            if (count) { FailureCount++; }
            _fail_messages.Add(new KeyValuePair<DateTime, string>(DateTime.Now, message));
        }

        public bool IsUpdating { get; set; }

        public int Incomplete
        {
            get { return incomplete; }
            protected set { incomplete = value; }
        }
        public TimeSpan MinUpdateInterval
        {
            get { return minUpdateInterval; }
            protected set { minUpdateInterval = value; }
        }
        public TrackerState Status
        {
            get { return status; }
            protected set { status = value; }
        }

        public TimeSpan UpdateInterval
        {
            get { return updateInterval; }
            set { updateInterval = value; }
        }

        public DateTime LastUpdated { get; set; }

        public Uri Uri
        {
            get { return uri; }
        }
        public string WarningMessage
        {
            get { return warningMessage ?? ""; }
            protected set { warningMessage = value; }
        }

        protected Tracker(Uri uri)
        {
            Check.Uri(uri);
            this.UpdateInterval = TimeSpan.FromMinutes(20);
            this.MinUpdateInterval = TimeSpan.FromMinutes(3);
            this.uri = uri;
            this.LastUpdated = DateTime.Now.Subtract(TimeSpan.FromHours(1));
        }

        public abstract void Announce(AnnounceParameters parameters, object state);
        public abstract void Scrape(ScrapeParameters parameters, object state);

        protected virtual void RaiseBeforeAnnounce()
        {
            EventHandler h = BeforeAnnounce;
            if (h != null)
                h(this, EventArgs.Empty);
        }
        protected virtual void RaiseAnnounceComplete(AnnounceResponseEventArgs e)
        {
            EventHandler<AnnounceResponseEventArgs> h = AnnounceComplete;
            if (h != null)
                h(this, e);
        }
        protected virtual void RaiseBeforeScrape()
        {
            EventHandler h = BeforeScrape;
            if (h != null)
                h(this, EventArgs.Empty);
        }
        protected virtual void RaiseScrapeComplete(ScrapeResponseEventArgs e)
        {
            EventHandler<ScrapeResponseEventArgs> h = ScrapeComplete;
            if (h != null)
                h(this, e);
        }
    }
}
