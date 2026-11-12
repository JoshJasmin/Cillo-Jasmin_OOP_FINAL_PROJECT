using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;
using System.Reflection.Metadata.Ecma335;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{

    public class Workstation : CampusAsset, IBorrowable, INetworkable
    {
   
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }

      
        public bool IsCheckedOut { get; private set; }
        public string BorrowerId { get; private set; }

        public Workstation(string tag, decimal price, string model)
            : base(tag, price, model) 
        {
        }

        public void CheckOut(string borrowerId)
        {
            if (Status == "In Repair")
                throw new Exception("Cannot checkout an item that is in repair.");

            IsCheckedOut = true;
            BorrowerId = borrowerId;
            Status = "Borrowed";
        }

        public void CheckIn()
        {
            IsCheckedOut = false;
            BorrowerId = null;
            Status = "Available";
        }

        public override decimal CalculateDepreciation() => PurchasePrice / 5;


        public void AssignNetworkDetails(string ip, string mac)
        {
            IpAddress = ip;
            MacAddress = mac;
        }

 
    }
};