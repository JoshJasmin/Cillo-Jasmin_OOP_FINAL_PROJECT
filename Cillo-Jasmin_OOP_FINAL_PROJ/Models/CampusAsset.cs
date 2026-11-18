using System;

namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public abstract class CampusAsset
    {
        public string AssetTag { get; protected set; }
        public decimal PurchasePrice { get; protected set; }
        public string ModelName { get; protected set; }
        public string Status { get; protected set; }

        protected CampusAsset(string assetTag, decimal purchasePrice, string modelName)
        {
            if (string.IsNullOrWhiteSpace(assetTag))
                throw new ArgumentException("Asset tag is required.");
            if (purchasePrice < 0)
                throw new ArgumentException("Price cannot be negative.");

            AssetTag = assetTag;
            PurchasePrice = purchasePrice;
            ModelName = modelName;
            Status = "Available";
        }

        public void UpdateStatus(string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.");
            Status = newStatus;
        }

        public abstract decimal CalculateDepreciation();

        public override string ToString()
        {
            return $"Tag: {AssetTag} | Model: {ModelName} | Type: {GetType().Name} | Status: {Status} | Price: {PurchasePrice:C}";
        }
    }
}