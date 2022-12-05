namespace WinterProjectAPIV61.DataTransferObjects
{
    public class CreateShareGroupDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool HasConcluded { get; set; }

        public int UserID { get; set; }
        public bool? IsPublic { get; set; }
        
        public DateTime? CreationDate { get; set; }

        public DateTime? ConclusionDate { get; set; }

        public DateTime? LastActiveDate { get; set; }
    }
}
