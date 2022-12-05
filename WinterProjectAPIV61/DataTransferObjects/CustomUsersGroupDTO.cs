namespace WinterProjectAPIV61.DataTransferObjects;

public class CustomUsersGroupDTO
{
    public int GroupId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? HasConcluded { get; set; }
    public bool? IsPublic { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? ConclusionDate { get; set; }
    public DateTime? LastActiveDate { get; set; }
}