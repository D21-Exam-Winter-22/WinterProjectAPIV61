namespace WinterProjectAPIV61.DataTransferObjects
{
    public class CreateShareUserDto
    {
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public bool? IsAdmin { get; set; }

        public string? Password { get; set; }
        
        public string? Address { get; set; }
        
        public int? QuestionID { get; set; }
        public string? SecurityAnswer { get; set; }
        public bool? IsDisabled { get; set; }
        public bool? IsBlacklisted { get; set; }
        
    }
}
