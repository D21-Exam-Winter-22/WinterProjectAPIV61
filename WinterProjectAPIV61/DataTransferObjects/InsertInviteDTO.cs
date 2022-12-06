namespace WinterProjectAPIV61.DataTransferObjects;

public class InsertInviteDTO
{
    public int? ToUserId { get; set; }
    public int? FromUserId { get; set; }
    public int? FromGroupId { get; set; }
    public string? Message { get; set; }
}