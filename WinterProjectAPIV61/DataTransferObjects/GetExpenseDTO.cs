namespace WinterProjectAPIV61.DataTransferObjects;

public class GetExpenseDTO
{
    public int ExpenseId { get; set; }

    public int? UserGroupId { get; set; }

    public double? Amount { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateTime? DatePaid { get; set; }

    public string? ReceiptPicture { get; set; }
}