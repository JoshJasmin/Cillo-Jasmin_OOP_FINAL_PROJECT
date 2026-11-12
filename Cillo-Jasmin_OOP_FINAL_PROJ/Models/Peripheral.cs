using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;
using System.Net.NetworkInformation;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public class Peripheral : CampusAsset, IBorrowable
    {
        public bool IsCheckedOut { get; private set; }
        public string BorrowerId { get; private set; }

        // THIS IS THE FIX: The constructor must take 3 arguments 
        // and pass them to the base class using ": base(...)"
        public Peripheral(string assetTag, decimal purchasePrice, string modelName)
            : base(assetTag, purchasePrice, modelName)
        {
        }

        public override decimal CalculateDepreciation()
        {
            return PurchasePrice / 3m;
        }

        public void CheckOut(string borrowerId)
        {
            if (Status == "In Repair") throw new System.Exception("Cannot checkout asset in repair.");
            if (IsCheckedOut) throw new System.Exception("Asset is already checked out.");

            IsCheckedOut = true;
            BorrowerId = borrowerId;
            UpdateStatus("Borrowed");
        }

        public void CheckIn()
        {
            if (!IsCheckedOut) throw new System.Exception("Asset is not checked out.");

            IsCheckedOut = false;
            BorrowerId = null;
            UpdateStatus("Available");
        }
    }
}