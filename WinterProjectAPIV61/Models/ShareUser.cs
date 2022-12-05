using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinterProjectAPIV61.Models;

public partial class ShareUser
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public bool? IsAdmin { get; set; }

    public string? Address { get; set; }

    public int? QuestionId { get; set; }

    public string? SecurityAnswer { get; set; }

    public bool? IsDisabled { get; set; }

    public bool? IsBlacklisted { get; set; }

    [JsonIgnore]
    public virtual ICollection<Invite> Invites { get; } = new List<Invite>();

    public virtual SecurityQuestion? Question { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserGroup> UserGroups { get; } = new List<UserGroup>();
}
