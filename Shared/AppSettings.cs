namespace Network
{
    public class AppSettings
    {
        public string HostName { get; set; }

        public int Port { get; set; }

        public int Timeout { get; set; } = 15;
    }
}