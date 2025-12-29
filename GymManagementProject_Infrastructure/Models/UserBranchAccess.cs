using System;
using System.Collections.Generic;

namespace GymManagementProject_Infrastructure.Models;

public partial class UserBranchAccess
{
    public Guid UserId { get; set; }

    public Guid BranchId { get; set; }

    public bool? IsPrimary { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
