using System;
using System.Collections.Generic;

namespace WinterProjectAPIV61.Models;

public partial class Invite
{
    public int InviteId { get; set; }

    public bool? IsPending { get; set; }

    public DateTime? InviteTime { get; set; }

    public int? ToUserId { get; set; }

    public int? FromUserGroupId { get; set; }

    public string? Message { get; set; }

    public virtual UserGroup? FromUserGroup { get; set; }

    public virtual ShareUser? ToUser { get; set; }
}
