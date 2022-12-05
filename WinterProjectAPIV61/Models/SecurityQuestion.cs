using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinterProjectAPIV61.Models;

public partial class SecurityQuestion
{
    public int QuestionId { get; set; }

    public string? Question { get; set; }

    [JsonIgnore]
    public virtual ICollection<ShareUser> ShareUsers { get; } = new List<ShareUser>();
}
