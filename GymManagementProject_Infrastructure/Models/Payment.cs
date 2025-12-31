using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Payment
{
    public Guid Id { get; set; }

    public Guid? InvoiceId { get; set; }

    public string? Method { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Version { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
