using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public class NetworkingHardware : CampusAsset, INetworkable
    {
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }

        public NetworkingHardware(string assetTag, decimal purchasePrice, string modelName)
            : base(assetTag, purchasePrice, modelName)
        {
        }

        public override decimal CalculateDepreciation() => PurchasePrice / 10m;

        public void AssignNetworkDetails(string ip, string mac)
        {
            IpAddress = ip;
            MacAddress = mac;
        }

        public void RestoreNetworkState(string ip, string mac)
        {
            IpAddress = ip;
            MacAddress = mac;
        }
    }
}