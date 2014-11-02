namespace ByteFlood
{
    public class TorrentProperties
    {
        public int MaxConnections { get; set; }
        public int MaxDownloadSpeed { get; set; }
        public int MaxUploadSpeed { get; set; }
        public int UploadSlots { get; set; }
        public bool UseDHT { get; set; }
        public bool EnablePeerExchange { get; set; }
        public string OnFinish { get; set; }
        public float RatioLimit { get; set; }

        public static readonly TorrentProperties DefaultTorrentProperties = new TorrentProperties()
        {
            MaxConnections = 60,
            MaxDownloadSpeed = 0,
            MaxUploadSpeed = 0,
            UploadSlots = 4,
            UseDHT = true,
            EnablePeerExchange = true,
            RatioLimit = 0, 
            OnFinish = null
        };
    }
}