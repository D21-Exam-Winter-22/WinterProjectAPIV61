namespace WinterProjectAPIV61.DataTransferObjects
{
    public class UpdateGroupDetailsDto
    {
        public int? GroupID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        
        public bool? IsPublic { get; set; }
        
        public DateTime? CreationDate { get; set; }

        public DateTime? ConclusionDate { get; set; }

        public DateTime? LastActiveDate { get; set; }
    }
}
