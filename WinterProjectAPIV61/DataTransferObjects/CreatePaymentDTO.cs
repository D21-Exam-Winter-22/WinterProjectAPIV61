namespace WinterProjectAPIV61.DataTransferObjects;

public class CreatePaymentDTO
{
    public int? GroupId { get; set; }
    public int? UserId { get; set; }

    public double? Amount { get; set; }
}