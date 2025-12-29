using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class Invoice
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid BranchId { get; set; }

    public Guid MemberId { get; set; }

    public string? InvoiceNumber { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? Version { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

    public virtual Member Member { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Tenant Tenant { get; set; } = null!;
}
