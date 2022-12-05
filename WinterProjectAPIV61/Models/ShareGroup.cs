using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinterProjectAPIV61.Models;

public partial class ShareGroup
{
    public int GroupId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public bool? HasConcluded { get; set; }

    public bool? IsPublic { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ConclusionDate { get; set; }

    public DateTime? LastActiveDate { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserGroup> UserGroups { get; } = new List<UserGroup>();
}
