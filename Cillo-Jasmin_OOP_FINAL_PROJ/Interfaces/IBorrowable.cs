namespace Cillo_Jasmin_OOP_FINAL_PROJ.Interfaces
{
    public interface IBorrowable
    {
        bool IsCheckedOut { get; }

        // THIS IS THE MISSING PROPERTY
        string BorrowerId { get; }

        void CheckOut(string borrowerId);
        void CheckIn();
    }
}