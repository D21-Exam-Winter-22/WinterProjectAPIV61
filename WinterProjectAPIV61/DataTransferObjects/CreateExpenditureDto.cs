namespace WinterProjectAPIV61.DataTransferObjects
{
    public class CreateExpenditureDto
    {
        public int UserID { get; set; }
        public int GroupID { get; set; }
        public double? Amount { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
        
        public DateTime? DatePaid { get; set; }

        public string? ReceiptPicture { get; set; }
    }
}
