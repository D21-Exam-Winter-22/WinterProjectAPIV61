using System;
using System.Collections.Generic;

namespace WinterProjectAPIV61.Models;

public partial class InPayment
{
    public int TransactionId { get; set; }

    public int? UserGroupId { get; set; }

    public double? Amount { get; set; }

    public DateTime? DatePaid { get; set; }

    public virtual UserGroup? UserGroup { get; set; }
}
