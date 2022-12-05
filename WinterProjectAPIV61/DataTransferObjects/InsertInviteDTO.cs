namespace WinterProjectAPIV61.DataTransferObjects;

public class InsertInviteDTO
{
    public int? ToUserId { get; set; }
    public int? FromUserID { get; set; }
    public int? FromGroupID { get; set; }
    public string? Message { get; set; }
}