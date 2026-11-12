namespace Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces
{
    public interface INetworkable
    {
        string IpAddress { get; set; }
        string MacAddress { get; set; }
        void AssignNetworkDetails(string ip, string mac);
    }
};