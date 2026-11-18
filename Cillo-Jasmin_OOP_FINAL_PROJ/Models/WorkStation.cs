using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public class Workstation : CampusAsset, IBorrowable, INetworkable
    {
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }

        public bool IsCheckedOut { get; private set; }
        public string BorrowerId { get; private set; }

        public Workstation(string assetTag, decimal purchasePrice, string modelName)
            : base(assetTag, purchasePrice, modelName)
        {
        }

        public void CheckOut(string borrowerId)
        {
            if (Status == "In Repair")
                throw new System.Exception("Cannot checkout an item that is in repair.");
            if (Status == "Unrepairable")
                throw new System.Exception("Cannot checkout an item that is unrepairable.");
            if (IsCheckedOut)
                throw new System.Exception("Asset is already checked out.");
            if (string.IsNullOrWhiteSpace(borrowerId))
                throw new System.Exception("Borrower ID is required.");

            IsCheckedOut = true;
            BorrowerId = borrowerId;
            UpdateStatus("Borrowed");
        }

        public void CheckIn()
        {
            if (!IsCheckedOut)
                throw new System.Exception("Asset is not checked out.");

            IsCheckedOut = false;
            BorrowerId = null;
            UpdateStatus("Available");
        }

        public override decimal CalculateDepreciation() => PurchasePrice / 5;

        public void AssignNetworkDetails(string ip, string mac)
        {
            IpAddress = ip;
            MacAddress = mac;
        }

        public void RestoreCheckoutState(bool isCheckedOut, string borrowerId)
        {
            IsCheckedOut = isCheckedOut;
            BorrowerId = borrowerId;
        }
    }
}