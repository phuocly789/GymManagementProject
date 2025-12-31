using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class RevenueSummary
{
    public DateTime? Month { get; set; }

    public decimal? TotalRevenue { get; set; }
}
