using System;
using System.Collections.Generic;

namespace WinterProjectAPIV61.Models;

public partial class TermsOfService
{
    public int ToSid { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? LastModificationDate { get; set; }

    public string? Content { get; set; }
}
