using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    // THIS LINE IS THE FIX: It must say ": CampusAsset"
    public class NetworkingHardware : CampusAsset, INetworkable
    {
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }

        // Constructor must pass the base properties to CampusAsset
        public NetworkingHardware(string assetTag, decimal purchasePrice, string modelName)
            : base(assetTag, purchasePrice, modelName)
        {
        }

        public override decimal CalculateDepreciation()
        {
            // Example logic for networking hardware
            return PurchasePrice / 10m;
        }

        public void AssignNetworkDetails(string ip, string mac)
        {
            IpAddress = ip;
            MacAddress = mac;
        }
    }
}