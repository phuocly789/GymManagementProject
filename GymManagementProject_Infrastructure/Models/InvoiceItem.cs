using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class InvoiceItem
{
    public Guid Id { get; set; }

    public Guid? InvoiceId { get; set; }

    public string? ItemType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Invoice? Invoice { get; set; }
}
