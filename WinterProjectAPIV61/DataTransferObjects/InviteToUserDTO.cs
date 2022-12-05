namespace WinterProjectAPIV61.DataTransferObjects;

public class InviteToUserDTO
{
    public int InviteID { get; set; }
    public bool? IsPending { get; set; }
    public DateTime? InviteTime { get; set; }
    public int? RecieverID { get; set; }
    public string? Message { get; set; }
    public int GroupID { get; set; }
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public bool? HasConcluded { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? LastActiveDate { get; set; }
}