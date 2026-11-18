using Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public class Peripheral : CampusAsset, IBorrowable
    {
        public bool IsCheckedOut { get; private set; }
        public string BorrowerId { get; private set; }

        public Peripheral(string assetTag, decimal purchasePrice, string modelName)
            : base(assetTag, purchasePrice, modelName)
        {
        }

        public override decimal CalculateDepreciation() => PurchasePrice / 3m;

        public void CheckOut(string borrowerId)
        {
            if (Status == "In Repair")
                throw new System.Exception("Cannot checkout asset in repair.");
            if (Status == "Unrepairable")
                throw new System.Exception("Cannot checkout asset that is unrepairable.");
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

        public void RestoreCheckoutState(bool isCheckedOut, string borrowerId)
        {
            IsCheckedOut = isCheckedOut;
            BorrowerId = borrowerId;
        }
    }
}