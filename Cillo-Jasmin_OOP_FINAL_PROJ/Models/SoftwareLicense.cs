namespace Cillo_Jasmin_OOP_FINAL_PROJ.Models
{
    public class SoftwareLicense : CampusAsset
    {
        public int MonthsRemaining { get; set; }

        // THIS IS THE FIX: A constructor that takes 4 arguments
        public SoftwareLicense(string assetTag, decimal purchasePrice, string modelName, int monthsRemaining)
            : base(assetTag, purchasePrice, modelName) // Pass the first 3 to CampusAsset
        {
            MonthsRemaining = monthsRemaining;
        }

        public override decimal CalculateDepreciation()
        {
            if (MonthsRemaining <= 0) return PurchasePrice;
            return PurchasePrice / MonthsRemaining;
        }
    }
}