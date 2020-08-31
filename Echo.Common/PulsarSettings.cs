
namespace Echo.Common
{
    //https://stackoverflow.com/questions/58183920/how-to-setup-app-settings-in-a-net-core-3-worker-service
    public class PulsarSettings
    {
        public string ServiceUrl { get; set; }
        public string ProxyServiceUrl { get; set; }
        public bool UseProxy { get; set; }
        public int OperationTimeout { get; set; }
        public string Topic { get; set; }
    }
}
